using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SWFExtractorNS.Utils
{
    class YouGetCommon
    {
        public static Dictionary<string, string> VideoMap = new Dictionary<string, string>()
    {
        {"video/3gpp", "3gp" },
        { "video/mp4", "mp4"},
        {"video/MP2T", "ts" },
        {"video/quicktime", "mov" },
        { "video/webm", "webm"},
        { "video/x-flv", "flv"},
        {"video/x-ms-asf", "asf" }
    };

        public static string[] Match1(String url, params string[] patterns) {
            if (patterns != null) {
                if (patterns.Length == 1)
                {
                    string regStr = patterns[0];
                    Regex reg = new Regex(regStr);
                    Match match = reg.Match(url);
                    if (match.Success)
                    {
                        string[] strs = new string[1];
                        strs[0] = match.Groups[1].Value;
                        return strs;
                    }
                    else
                    {
                        return null;
                    }

                }
                else {
                    List<string> list = new List<string>();
                    foreach (string p in patterns) {
                        Regex reg = new Regex(p);
                        Match match = reg.Match(url);
                        if (match.Success) {
                            string mStr = match.Groups[1].Value;
                            list.Add(mStr);
                        }
                    }
                    return list.ToArray<string>();
                }
            }

            return null;
        }

        public static string MatchOne(String url, string pattern)
        {
            return GetStringArr0(Match1(url, pattern));
        }

        public static string GetStringArr0(string[] arr) {
            if (arr == null || arr.Length < 1)
            {
                return null;
            }
            else {
                return arr[0];
            }
        }


        public static string Get_content(string url, Dictionary<string, string> headers)
        {
            HttpWebResponse response = Urlopen_with_retry(url, headers);
            if (response != null) {
                String responseString = null;
                string charset = "utf-8";
                if (response.CharacterSet != null && response.CharacterSet.Length > 0)
                {
                    charset = response.CharacterSet;
                }
                if (charset.CompareTo("utf8") == 0)
                {
                    charset = "utf-8";
                }
                Encoding responseEncoding = Encoding.GetEncoding(charset);

                System.IO.Stream streamReceive = response.GetResponseStream();
                if (response.ContentEncoding.ToLower().Contains("gzip"))
                {
                    streamReceive = new GZipStream(streamReceive, CompressionMode.Decompress);
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(streamReceive, responseEncoding))
                    {
                        responseString = sr.ReadToEnd();
                    }
                }
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                {
                    using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(stream, responseEncoding))
                        {
                            responseString = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), responseEncoding))
                    {
                        responseString = sr.ReadToEnd();
                    }
                }
                response.Close();
                return responseString;
            }
            return null;
        }

        public static HttpWebResponse Urlopen_with_retry(string url, Dictionary<string, string> headers)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            
            if (headers != null)
            {
                WebHeaderCollection header = request.Headers;
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    SetHeaderValue(header, pair.Key, pair.Value);
                }
            }
            int retry_time = 3;
            for (int i = 0; i < retry_time; i++)
            {
                try
                {
                    HttpWebResponse res = request.GetResponse() as HttpWebResponse;
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        return res;
                    }
                    else {
                        Console.Write("URL HTTP请求失败!返回代码:" + res.StatusCode);
                    }
                }
                catch (TimeoutException te) {
                    Console.Write(te.Message);
                }
                catch (Exception e) {
                    Console.Write(e.Message);
                }
            }
            return null;
        }
       

        public static void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }


        public static string Url_to_key(string url)
        {
            string video_host = MatchOne(url, @"https?://([^/]+)/");
            string video_url = MatchOne(url, @"https?://[^/]+(.*)");

            string key = null;
            string temp = MatchOne(video_host, @"(\.[^.]+\.[^.]+)$");
            if (temp != null)
            {
                key = MatchOne(temp, @"([^.]+)");
            }
            else {
                key = MatchOne(video_host, @"([^.]+)");
            }
            
            return key;
        }


        public static string Url_info_to_json(string url, Dictionary<string, string> headers)
        {
            HttpWebResponse response = Urlopen_with_retry(url, headers);
            string ext = "";
            if (response != null) {
                WebHeaderCollection resheaders = response.Headers;
                string type = resheaders["content-type"];
                string conten_dispos = resheaders["content-disposition"];
                if (VideoMap.ContainsKey(type))
                {
                    ext = VideoMap[type];
                }
                else {
                    if (conten_dispos != null && !conten_dispos.Equals("")) {
                        try
                        {
                            string name = MatchOne(conten_dispos, @"filename="" ? ([^ ""]+)"" ? ");
                            string fileName = HttpUtility.UrlEncode(name);
                            string[] names = fileName.Split(',');
                            if (names.Length > 1)
                            {
                                ext = names[1];
                            }
                            else {
                                ext = "";
                            }
                        }
                        catch (Exception e) {
                            ext = "";
                        }
                    }
                }
            }
            JObject json = new JObject();
            json.Add("video_url", url);
            json.Add("video_type", ext);

            response.Close();
            return json.ToString();
        }


        public static Dictionary<string, string> GetHTTPResponseHeaders(string Url, Dictionary<string, string> header)
        {
            Dictionary<string, string> HeaderList = new Dictionary<string, string>();

            HttpWebResponse ResponseObject = Urlopen_with_retry(Url, header);
            string resUrl = ResponseObject.ResponseUri.ToString();
            if (!Url.Equals(resUrl)) {
                HeaderList.Add("Location", resUrl);
            }

            foreach (string HeaderKey in ResponseObject.Headers)
                HeaderList.Add(HeaderKey, ResponseObject.Headers[HeaderKey]);

            ResponseObject.Close();

            return HeaderList;
        }


        public static Media SingleVideoInfo_To_Media(string sourceUrl, string jsonstr) {
            if (jsonstr == null) {
                return null;
            }
            List<Stream> streams = new List<Stream>();
            JObject json = JObject.Parse(jsonstr);
            StreamType stype = StreamType.Unknown;
            List<string> videoList = new List<string>();

            if (json != null)
            {
                string type = json["video_type"].ToString();
                string vUrl = json["video_url"].ToString();
                if ("mp4".Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    stype = StreamType.MP4;
                }
                else if ("m3u8".Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    stype = StreamType.M3U8;
                }
                videoList.Add(vUrl);
            }

            streams.Add(new Stream(stype, videoList));

            return new Media(sourceUrl, streams.ToArray(), 0);
        }


        public static bool isEmptyStr(string str)
        {
            return str == null || "".Equals(str);
        }
    }
}
