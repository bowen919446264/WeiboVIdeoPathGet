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
    class SohuExtractor: IExtractor
    {
        private static readonly String SOHU = "sohu.com";
        private static readonly String VIDEO_INFO_ADDRESS1 = "http://hot.vrs.sohu.com/vrs_flash.action?vid={0}"; // for http://tv.sohu.com/ and http://film.sohu.com/
        private static readonly String VIDEO_INFO_ADDRESS2 = "http://my.tv.sohu.com/play/videonew.do?vid={0}";  // for http://my.tv.sohu.com/

        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            SWFExtractor.RegisterExtractor(SOHU, typeof(SohuExtractor));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Media Extract(String url)
        {
            String domain = HTTPHelper.ParseDomain(url);
            if (!domain.Equals(SOHU, StringComparison.OrdinalIgnoreCase))
                throw new DomainNotSupportedException(domain);

            return ExtractCurrentValid(url);
        }

        /// <summary>
        /// 当前有效的爬取策略
        /// </summary>
        /// <param name="url"> 
        /// 类似 http://tv.sohu.com/20160525/n451290140.shtml 
        /// 或者 http://my.tv.sohu.com/pl/9136343/84137803.shtml
        /// </param>
        /// <returns></returns>
        private Media ExtractCurrentValid(String url)
        {
            String videoID = ExtractVideoID(url);
            if (url.IndexOf("http://tv.sohu.com/") >= 0 || url.IndexOf("http://film.sohu.com/") >= 0)
            {
                String videoAPIAccessURL = String.Format(VIDEO_INFO_ADDRESS1, videoID);
                String responseString = HTTPHelper.SendHTTPGetRequest(videoAPIAccessURL);
                return ExtractURL(url, responseString, videoID);
            }
            else if (url.IndexOf("http://my.tv.sohu.com") >= 0)
            {
                String videoAPIAccessURL = String.Format(VIDEO_INFO_ADDRESS2, videoID);
                String responseString = HTTPHelper.SendHTTPGetRequest(videoAPIAccessURL);
                return ExtractURL(url, responseString, videoID);
            }
            else
                return null;
        }

        private String ExtractVideoID(String url)
        {
            String responseString = HTTPHelper.SendHTTPGetRequest(url);
            Regex regex = new Regex("var\\s+vid\\s*=\\s*[\"']?(\\d+)[\"']?");
            if (regex.IsMatch(responseString))
            {
                Match m = regex.Match(responseString);
                return m.Groups[1].Value;
            }

            Regex regex2 = new Regex("data-vid\\s*=\\s*['\"](\\d+)['\"]");
            if (regex2.IsMatch(responseString))
            {
                Match m = regex2.Match(responseString);
                return m.Groups[1].Value;
            }
            return null;
        }

        #region 历史爬取策略
        private Media ExtractURLBefore20170503(String url, String json)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            String serverIP = jo["allot"].ToString();
            JArray addressArrayObject = (JArray)jo["data"]["su"];
            List<Stream> streams = new List<Stream>();
            List<String> addresses = new List<String>();
            foreach (String shortAddress in addressArrayObject)
            {
                String fullAddress = String.Format("http://{0}/fmp4?new={1}", serverIP, shortAddress);
                addresses.Add(fullAddress);
            }
            if (addresses.Count > 0)
            {
                streams.Add(new Stream(StreamType.MP4, addresses));
            }
            if (streams.Count > 0)
                return new Media(url, streams.ToArray(), 0);
            return null;
        }
        #endregion

        private Media ExtractURL(String url, String json, String vid)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            String serverIP = jo["allot"].ToString();
            JArray suArray = (JArray)jo["data"]["su"];
            JArray clipsURLArray = (JArray)jo["data"]["clipsURL"];
            JArray ckArray = (JArray)jo["data"]["ck"];
            if (suArray.Count != clipsURLArray.Count || suArray.Count != ckArray.Count)
            {
                return null;
            }

            List<Stream> streams = new List<Stream>();
            List<String> addresses = new List<String>();
            for (int i = 0; i < suArray.Count; i++)
            {
                String strClipUrl = clipsURLArray[i].ToString();
                int index = strClipUrl.IndexOf('/', 9);
                String subClipUrl = strClipUrl.Substring(index, strClipUrl.Length - index);
                String fullAddress = String.Format("http://{0}/?prot=9&prod=flash&pt=1&file={1}&new={2}&key={3}&vid={4}&rb=1", serverIP, subClipUrl, suArray[i].ToString(), ckArray[i].ToString(), vid);
                
                String response = HTTPHelper.SendHTTPGetRequest(fullAddress);
                JObject mediaJo = (JObject)JsonConvert.DeserializeObject(response);
                String mediaAddress = mediaJo["url"].ToString();
                addresses.Add(mediaAddress);
            }
            if (addresses.Count > 0)
            {
                streams.Add(new Stream(StreamType.MP4, addresses));
            }
            if (streams.Count > 0)
                return new Media(url, streams.ToArray(), 0);
            return null;
        }
    }
}
