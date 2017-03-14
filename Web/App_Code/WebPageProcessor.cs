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

    private void ProcessLinks()
    {
        if (!_doc.DocumentNode.HasChildNodes) return;

        foreach (var link in _doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            var attribute = link.Attributes["href"];

            var url = attribute.Value;

            attribute.Value = "/GetPage?q=" +
                              Uri.EscapeUriString(IsAbsoluteUri(url) ? url : new Uri(new Uri(_targetUrl), url).ToString());
        }
    }

    private void ProcessImages()
    {
        if (!_doc.DocumentNode.HasChildNodes) return;

        foreach (var link in _doc.DocumentNode.SelectNodes("//img[@src]"))
        {
            var attribute = link.Attributes["src"];

            var url = attribute.Value;

            attribute.Value = "/GetImage?q=" +
                              Uri.EscapeUriString(IsAbsoluteUri(url) ? url : new Uri(new Uri(_targetUrl), url).ToString());
        }
    }

    public string Process()
    {
        if (_doc == null)
            return "<h2>No such site...</h2>";

        ProcessLinks();
        ProcessImages();

        var sb = new StringBuilder();
        var sw = new StringWriter(sb);

        _doc.Save(sw);
        return sw.ToString();
    }
}