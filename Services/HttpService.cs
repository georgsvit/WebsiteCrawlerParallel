using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteCrawlerParallel.Services
{
    public static class HttpService
    {
        public static async Task<string> GetFileDataByUri(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader reader = string.IsNullOrWhiteSpace(response.CharacterSet) switch
                    {
                        false => new(receiveStream, Encoding.GetEncoding(response.CharacterSet)),
                        _ => new(receiveStream)
                    };

                    string data = await reader.ReadToEndAsync();

                    response.Close();
                    reader.Close();

                    if (String.IsNullOrWhiteSpace(data))
                        throw new Exception("File is empty");

                    return data;

                default:
                    response.Close();
                    throw new Exception($"Response status code: {response.StatusCode}");
            }
        }

        public static async Task<TimeSpan> GetPageResponseTimeByUri(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
            Stopwatch timer = new();

            timer.Start();
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            timer.Stop();

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return timer.Elapsed;

                default:
                    response.Close();
                    throw new Exception($"Response status code: {response.StatusCode}");
            }
        }

        public static async Task<(string, TimeSpan)> GetFileDataAndResponseTimeByUri(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
            Stopwatch timer = new();

            timer.Start();
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            timer.Stop();

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader reader = string.IsNullOrWhiteSpace(response.CharacterSet) switch
                    {
                        false => new(receiveStream, Encoding.GetEncoding(response.CharacterSet)),
                        _ => new(receiveStream)
                    };

                    string data = await reader.ReadToEndAsync();

                    response.Close();
                    reader.Close();

                    if (String.IsNullOrWhiteSpace(data))
                        throw new Exception("File is empty");

                    return (data, timer.Elapsed);

                default:
                    response.Close();
                    throw new Exception($"Response status code: {response.StatusCode}");
            }
        }
    }
}
