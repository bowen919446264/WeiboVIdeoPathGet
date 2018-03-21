using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWFExtractorNS.Utils;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SWFExtractorNS.Extractors
{
    class CCTVExtractor: IExtractor
    {
        private static readonly String CCTV = "cctv.com";
        private static readonly String VIDEO_INFO_ADDRESS = "http://vdn.apps.cntv.cn/api/getHttpVideoInfo.do?pid={0}";

        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            SWFExtractor.RegisterExtractor(CCTV, typeof(CCTVExtractor));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Media Extract(String url)
        {
            String domain = HTTPHelper.ParseDomain(url);
            if (!domain.Equals(CCTV, StringComparison.OrdinalIgnoreCase))
                throw new DomainNotSupportedException(domain);

            return ExtractCurrentValid(url);
        }

        /// <summary>
        /// 当前有效的爬取策略
        /// </summary>
        /// <param name="url"> 类似 http://tv.cctv.com/2016/06/14/VIDEhR6TQPty6NwjkQAV7bmN160614.shtml </param>
        /// <returns></returns>
        private Media ExtractCurrentValid(String url)
        {
            String pid = ExtractPID(url);
            String videoAPIAccessURL = String.Format(VIDEO_INFO_ADDRESS, pid);
            String responseString = HTTPHelper.SendHTTPGetRequest(videoAPIAccessURL);
            return ExtractURL(url, responseString);
        }

        private String ExtractPID(String url)
        {
            String responseString = HTTPHelper.SendHTTPGetRequest(url);
            Regex regex = new Regex("var\\s+guid\\s*=\\s*[\"']?(\\w+)[\"']?");
            if (regex.IsMatch(responseString))
            {
                Match m = regex.Match(responseString);
                return m.Groups[1].Value;
            }
            return null;
        }

        private Media ExtractURL(String url, String json)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            List<Stream> streams = new List<Stream>();
            String[] chapterArray = { "lowChapters", "chapters", "chapters2" };
            foreach(String chapter in chapterArray)
            {
                try
                {
                    JArray chapterObject = (JArray)jo["video"][chapter];
                    List<String> addresses = new List<String>();
                    foreach (JObject urlPart in chapterObject)
                    {
                        addresses.Add(urlPart["url"].ToString());
                    }
                    if (addresses.Count > 0)
                        streams.Add(new Stream(StreamType.MP4, addresses));
                }
                catch (Exception)
                {
                }
            }

            try
            {
                String hlsAddress = jo["hls_url"].ToString();
                if (hlsAddress != null)
                {
                    streams.Add(new Stream(StreamType.M3U8, new List<String>{hlsAddress}));
                }
            }
            catch (Exception)
            {
            }

            if (streams.Count > 0)
                return new Media(url, streams.ToArray(), 1);
            return null;
        }

        public String M3U8UrlPrefix(String m3u8Url)
        {
            return null;
        }
    }
}
