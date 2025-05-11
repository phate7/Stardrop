using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Stardrop.Utilities
{
    public static class NexusHelper
    {
        // Get Session_cookie and place here. (Click on Slow Download and check network tab in browser)
        private static string session_cookie = "";
        private static string stardew_game_id = "1303";
        private static string nexus_download_url = "https://www.nexusmods.com/Core/Libs/Common/Managers/Downloads?GenerateDownloadUrl";



        public static string MakeDownloadLink(string refererUrl, string fileId)
        {
            session_cookie = File.ReadAllText("session.txt");
            string downloadUrl = null;
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var request = (HttpWebRequest)WebRequest.Create(nexus_download_url);
            request.Accept = "*/*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";

            request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            request.Headers.Add("Accept-Language", "en,de;q=0.9,de-DE;q=0.8,en-US;q=0.7");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Origin", "https://www.nexusmods.com");
            request.Referer = refererUrl;
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Cookie", session_cookie);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            var postData = "fid=" + fileId;
            postData += "&game_id=" + stardew_game_id;
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseBody = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    try
                    {
                        var responseData = JsonConvert.DeserializeObject<dynamic>(responseBody);

                        downloadUrl = responseData?.url;

                        if (string.IsNullOrEmpty(downloadUrl))
                        {
                            throw new Exception("No download URL found in the response.");
                        }
                    }
                    catch (Newtonsoft.Json.JsonException)
                    {
                        throw new Exception($"Failed to parse the response as JSON from {refererUrl}");
                    }
                }
                else
                {
                    throw new Exception($"Request failed for {refererUrl} with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during POST request: {ex.Message}");
            }

            return downloadUrl;
        }

    }
}
