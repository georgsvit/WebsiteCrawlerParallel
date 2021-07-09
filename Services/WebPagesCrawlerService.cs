using AngleSharp.Html.Parser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteCrawlerParallel.Extensions;

namespace WebsiteCrawlerParallel.Services
{
    public static class WebPagesCrawlerService
    {
        public static Dictionary<string, TimeSpan> Crawl(string uri)
        {
            Dictionary<string, TimeSpan> links = new()
            {
                [uri] = new()
            };

            string pathAndQuery = new Uri(uri).PathAndQuery;

            if (pathAndQuery != "/")
            {
                links.Add(uri.Replace(pathAndQuery, ""), new());
            }

            IEnumerable<string> unprocessedLinks = links.Keys;

            while (unprocessedLinks.Any())
            {
                var foundLinks = new ConcurrentBag<string>();

                Parallel.ForEach(unprocessedLinks, (string link) =>
                {
                    try
                    {
                        var (extractedLinks, responseTime) = ProcessPage(link);
                        links[link] = responseTime;

                        foreach (var item in extractedLinks)
                            foundLinks.Add(item);
                    }
                    catch (Exception e)
                    {
                        links[link] = TimeSpan.MaxValue;

                        // Better to change to logger
                        Console.WriteLine($"URI: {link} Message: {e.Message}");
                    }
                });

                unprocessedLinks = links.CustomConcat(foundLinks.Distinct().ToDictionary(x => x, _ => new TimeSpan()));
            }

            return links;
        }

        private static (IEnumerable<string>, TimeSpan) ProcessPage(string uri)
        {
            var (pageData, responseTime) = HttpService.GetFileDataAndResponseTimeByUri(uri).GetAwaiter().GetResult();

            string host = $"https://{new Uri(uri).Host}";
            var links = GetLinksFromWebPage(pageData, host).GetAwaiter().GetResult();

            return (links, responseTime);
        }

        private static async Task<IEnumerable<string>> GetLinksFromWebPage(string pageData, string host)
        {
            HtmlParser parser = new();
            var page = await parser.ParseDocumentAsync(pageData);

            return page.QuerySelectorAll("a")
                            .Select(element => element.GetAttribute("href"))
                            .Where(link => link is not null && (link.StartsWith(host) || (link.StartsWith("/") && !link.StartsWith("//"))) && !link.Contains("#") && !link.Contains("@"))
                            .Select(link =>
                                link[0] switch
                                {
                                    '/' or '.' => $"{host}{link}",
                                    _ => link
                                }
                            ).Distinct();
        }
    }
}
