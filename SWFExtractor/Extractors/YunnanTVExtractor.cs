using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SWFExtractorNS.Utils;
using System.Text.RegularExpressions;
using System.Xml;

namespace SWFExtractorNS.Extractors
{
    class YunnanTVExtractor : IExtractor
    {
        private static readonly String YUNNANTV = "yntv.cn";
        private static readonly String VIDEO_API_ADDRESS = "http://mediamobile.yntv.cn/api/getCDNByVodId/";


        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            SWFExtractor.RegisterExtractor(YUNNANTV, typeof(YunnanTVExtractor));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Media Extract(String url)
        {
            String domain = HTTPHelper.ParseDomain(url);
            if (!domain.Equals(YUNNANTV, StringComparison.OrdinalIgnoreCase))
                throw new DomainNotSupportedException(domain);

            return ExtractCurrentValid(url);
        }

        /// <summary>
        /// 当前有效的爬取策略
        /// </summary>
        /// <param name="url"> 类似 http://news.yntv.cn/content/20/201606/14/20_1296635.shtml </param>
        /// <returns></returns>
        private Media ExtractCurrentValid(String url)
        {
            String videoID = ExtractVideoID(url);
            String videoAPIAccessURL = VIDEO_API_ADDRESS + videoID;
            String responseString = HTTPHelper.SendHTTPGetRequest(videoAPIAccessURL);
            return ExtractURL(url, responseString);
        }

        private String ExtractVideoID(String url)
        {
            String responseString = HTTPHelper.SendHTTPGetRequest(url);
            Regex regex = new Regex("\\s+urlbody=\"[^/]+/([^/]+)/[^\"]+");
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
            JObject src = (JObject)jo["src"];
            JValue playURL = (JValue)src["0"];
            if (playURL != null)
                return new Media(url, new Stream[] { new Stream(StreamType.MP4, new List<string>{ playURL.ToString() }) }, 0);
            return null; 
        }
    }
}
