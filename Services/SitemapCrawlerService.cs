using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace WebsiteCrawlerParallel.Services
{
    public static class SitemapCrawlerService
    {
        public static async IAsyncEnumerable<string> Crawl(string uri)
        {
            var temp = await GetSitemapLinksFromRobots(uri).ToListAsync();
            Queue<string> sitemapLinks = new(temp);
            List<string> pageLinks = new();

            while (sitemapLinks.Any())
            {
                string currentLink = sitemapLinks.Dequeue();
                var (links, sitemaps) = await GetLinksFromSitemap(currentLink);

                if (sitemaps.Any())
                    foreach (string sitemap in sitemaps)
                        sitemapLinks.Enqueue(sitemap);

                if (links.Any())
                    pageLinks.AddRange(links);
            }

            foreach (string item in pageLinks)
            {
                yield return item;
            }
        }

        private static async IAsyncEnumerable<string> GetSitemapLinksFromRobots(string uri)
        {
            string robotsData = await HttpService.GetFileDataByUri($"https://{new Uri(uri).Host}/robots.txt");

            Regex rule = new(@"Sitemap: (.*\.xml)\b");
            MatchCollection matchCollection = rule.Matches(robotsData);

            foreach (Match item in matchCollection)
            {
                yield return item.Groups[1].Value;
            }
        }

        private static async Task<(IEnumerable<string>, IEnumerable<string>)> GetLinksFromSitemap(string uri)
        {
            string sitemapData = await HttpService.GetFileDataByUri(uri);

            XmlDocument xml = new();
            xml.LoadXml(sitemapData);

            XmlNodeList nodesList = xml.GetElementsByTagName("loc");

            IEnumerable<string> links = nodesList.Cast<XmlElement>().Select(element => element.InnerText);
            IEnumerable<string> sitemaps = links.Where(link => link.Contains("sitemap") && link.EndsWith("xml"));
            links = links.Except(sitemaps);

            return (links, sitemaps);
        }
    }
}
