using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace SWFExtractorNS.Utils
{
    class HTTPHelper
    {
        /// <summary>
        /// 分析URL，获得域名
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static String ParseDomain(String url)
        {
            const String domainPattern = @"http://(?:[a-zA-Z0-9_][a-zA-Z0-9\-_]*\.)+?(([a-zA-Z0-9_][a-zA-Z0-9\-_]*)(\.(com|edu|gov|int|mil|net|org|biz|info|pro|name|museum|coop|aero|xxx|idv|cn|cc))+).*";
            const String IPPattern = "http://([0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}).*";

            try
            {
                Regex reg = new Regex(domainPattern);
                if (!reg.IsMatch(url))
                {
                    reg = new Regex(IPPattern);
                    if (!reg.IsMatch(url))
                        throw new InvalidURLException(url);
                }

                Match match = reg.Match(url);
                return match.Groups[1].Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获得HTTP URL的指定参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static String GetParameterValue(String url, String paramName)
        {
            try
            {
                IDictionary<String, String> parameters = GetParameters(url);
                if (!parameters.ContainsKey(paramName)) return null;

                String paramValue = null;
                parameters.TryGetValue(paramName, out paramValue);
                return paramValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获得 HTTP URL的所有参数
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IDictionary<String, String> GetParameters(String url)
        {
            const String pattern = @"[^?&]*[[?&]([^=]+?)=([^&]+)]*";
            Regex reg = new Regex(pattern);
            MatchCollection mc = reg.Matches(url);
            IDictionary<String, String> dict = new Dictionary<String, String>();
            foreach (Match match in mc)
            {
                dict.Add(match.Groups[1].Value, match.Groups[2].Value);
            }
            return dict;
        }

        /// <summary>
        /// 发送HTTP Get请求，返回请求内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static String SendHTTPGetRequest(String url)
        {
            const string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("URL[" + url + "] HTTP请求失败!返回代码:" + response.StatusCode);

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
            
         
            String responseString;
            using (StreamReader sr = new StreamReader(response.GetResponseStream(), responseEncoding))
            {
                responseString = sr.ReadToEnd();
            }
            return responseString;
        }



        public static byte[] getBytesFromUrl(String url)
        {
            const string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("URL[" + url + "] HTTP请求失败!返回代码:" + response.StatusCode);

            List<byte> list = new List<byte>();
            byte[] bs;
            
            using (BinaryReader br = new BinaryReader(response.GetResponseStream()))
            {
                while (true)
                { 
                    bs = br.ReadBytes(4096);
                    if (bs.Length == 0)
                        break;
                    list.AddRange(bs);
                }
                br.Close();
            }
            

            return list.ToArray();
        }


    }
}
