using System;
using System.IO;
using System.Text;
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

    private void ProcessNodes(string tag, string attributeName, string handler)
    {
        if (!_doc.DocumentNode.HasChildNodes) return;

        var htmlNodes = _doc.DocumentNode.SelectNodes(string.Format("//{0}[@{1}]", tag, attributeName));
        if (htmlNodes == null) return;

        foreach (var link in htmlNodes)
        {
            var attribute = link.Attributes[attributeName];

            var url = attribute.Value;

            attribute.Value =
                string.Format(
                    "/{0}?q=" + Uri.EscapeUriString(IsAbsoluteUri(url) ? url : new Uri(new Uri(_targetUrl), url).ToString()),
                    handler);
        }
    }

    public string Process()
    {
        if (_doc == null)
            return "<h2>No such site...</h2>";

        ProcessNodes("a", "href", "GetPage");
        ProcessNodes("img", "src", "GetImage");

        var sb = new StringBuilder();
        var sw = new StringWriter(sb);

        _doc.Save(sw);
        return sw.ToString();
    }
}