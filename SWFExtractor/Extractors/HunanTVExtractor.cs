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

namespace SWFExtractorNS.Extractors
{
    class HunanTVExtractor : IExtractor
    {
        private static readonly String HUNANTV = "mgtv.com";
        private static readonly String VIDEO_API_ADDRESS_BEFORE_20170503 = "http://v.api.mgtv.com/player/video?video_id=";
        private static readonly String VIDEO_API_ADDRESS = "http://pcweb.api.mgtv.com/player/video?video_id=";
        

        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            SWFExtractor.RegisterExtractor(HUNANTV, typeof(HunanTVExtractor));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Media Extract(String url)
        {
            String domain = HTTPHelper.ParseDomain(url);
            if (!domain.Equals(HUNANTV, StringComparison.OrdinalIgnoreCase))
                throw new DomainNotSupportedException(domain);

            return ExtractCurrentValid(url);
        }

        /// <summary>
        /// 当前有效的爬取策略
        /// </summary>
        /// <param name="url"> 类似 http://www.mgtv.com/v/2/159041/f/3274837.html </param>
        /// <returns></returns>
        private Media ExtractCurrentValid(String url)
        {
            int nLastStart = url.LastIndexOf('/');
            if (nLastStart < 0) throw new Exception("无效URL");

            int nLastPoint = url.LastIndexOf('.');
            if (nLastPoint < 0) throw new Exception("无效URL");

            if (nLastStart + 1 >= nLastPoint + 1) throw new Exception("无效URL");

            String videoID = url.Substring(nLastStart + 1, nLastPoint - (nLastStart + 1));
            String videoAPIAccessURL = VIDEO_API_ADDRESS + videoID;
            String responseString = HTTPHelper.SendHTTPGetRequest(videoAPIAccessURL);
            return ExtractURL(url, responseString);
        }

        #region 历史爬取策略

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private Media ExtractBefore20151230(String url)
        {
            String videoID = HTTPHelper.GetParameterValue(url, "video_id");
            if (videoID == null) throw new Exception("无法获得URL[" + url + "]的video_id参数");

            String videoAPIAccessURL = VIDEO_API_ADDRESS + videoID;
            String responseString = HTTPHelper.SendHTTPGetRequest(videoAPIAccessURL);
            return ExtractURL(url, responseString);
        }

        #endregion

        private Media ExtractURL(String url, String json)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            JArray ja = (JArray)jo["data"]["stream"];
            JArray domainArray = (JArray)jo["data"]["stream_domain"];
            if (domainArray.Count <= 0)
            {
                throw new Exception("没有发现stream_domain");
            }

            List<Stream> streams = new List<Stream>();
            foreach (JObject stream in ja)
            {
                String streamUrl = stream["url"].ToString();
                if (streamUrl.Trim().Length <= 0) continue;

                String responseString = HTTPHelper.SendHTTPGetRequest(domainArray[0].ToString() + streamUrl);
                JObject streamJO = (JObject)JsonConvert.DeserializeObject(responseString);

                String streamURL = streamJO["info"].ToString();
                String name = stream["name"].ToString();
                streams.Add(new Stream(StreamType.M3U8, new List<string> { streamURL }));
            }

            if (streams.Count > 0)
                return new Media(url, streams.ToArray(), 0);
            else
                return null;
        }
    }
}
