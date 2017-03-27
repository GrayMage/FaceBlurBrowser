using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

public class WebPageProcessor
{
    private readonly HtmlDocument _doc;
    private readonly string _targetUrl;

    public WebPageProcessor(string targetUrl)
    {
        _targetUrl = targetUrl;

        var web = new HtmlWeb();

        try
        {
            _doc = web.Load(_targetUrl);
            _targetUrl = web.ResponseUri.ToString();
            var head = _doc.DocumentNode.SelectSingleNode("//head");
            head.InnerHtml = "<script type='text/javascript' src='/Scripts/Proxy.js'></script>" + head.InnerHtml;
        }
        catch (Exception)
        {
            _doc = null;
        }
    }

    private static bool IsAbsoluteUri(string uri)
    {
        Uri result;
        return Uri.TryCreate(uri, UriKind.Absolute, out result);
    }

    private static bool IsEmbeddedImage(string url)
    {
        return url.StartsWith("data:image");
    }

    private void ProcessNodes(string tag, string attributeName, string handler)
    {
        if (!_doc.DocumentNode.HasChildNodes) return;

        var htmlNodes = _doc.DocumentNode.SelectNodes(string.Format("//{0}[@{1}]", tag, attributeName));
        if (htmlNodes == null) return;

        foreach (var link in htmlNodes)
        {
            var attribute = link.Attributes[attributeName];

            var url = attribute.Value;
            var updatedUrl = GetGlobalUrl(url);

            if (tag == "img")
            {
                updatedUrl += IsEmbeddedImage(url) ? "&type=embedded" : "&type=url";
            }

            attribute.Value = string.Format("/{0}?q={1}", handler, updatedUrl);
        }
    }

    private string GetGlobalUrl(string url)
    {
        var updatedUrl = Uri.EscapeUriString(IsAbsoluteUri(url) ? url : new Uri(new Uri(_targetUrl), url).ToString());
        if (updatedUrl.StartsWith("//")) updatedUrl = "http:" + updatedUrl;
        return updatedUrl;
    }

    private void ProcessStyles()
    {
        if (!_doc.DocumentNode.HasChildNodes) return;

        foreach (var node in _doc.DocumentNode.Descendants())
        {
            var attr = node.Attributes["style"];
            if(attr == null) continue;

            attr.Value = new Regex(@"background-image\s*?:\s*?url\s*?\(\s*?(?<url>.*)\s*?\)").Replace(attr.Value,
                m => m.ToString().Replace(m.Groups["url"].Value, string.Format("/GetImage?type=url&q={0}", GetGlobalUrl(m.Groups["url"].Value))));
        }
    }

    public string Process()
    {
        if (_doc == null)
            return "<h2>No such site...</h2>";

        ProcessNodes("a", "href", "GetPage");
        ProcessNodes("img", "src", "GetImage");
        ProcessStyles();

        var sb = new StringBuilder();
        var sw = new StringWriter(sb);

        _doc.Save(sw);
        return sw.ToString();
    }
}