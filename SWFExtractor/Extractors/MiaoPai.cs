using Newtonsoft.Json.Linq;
using SWFExtractorNS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SWFExtractorNS.Extractors
{
    class MiaoPai :IExtractor, IYouGetExtractor
    {

        private static Dictionary<string, string> fake_headers_mobile = new Dictionary<string, string>
        {
            {"Accept","text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"},
            { "Accept-Charset","UTF-8,*;q=0.5"},
            { "Accept-Encoding","gzip,deflate,sdch"},
            { "Accept-Language","en-US,en;q=0.8"},
            { "User-Agent","Mozilla/5.0 (Linux; Android 4.4.2; Nexus 4 Build/KOT49H) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.114 Mobile Safari/537.36"}
        };

        public string Miaopai_load_bg_fid(string fid)
        {
            string page_url = "http://video.weibo.com/show?fid=" + fid + "&type=mp4";
            string mobile_page = YouGetCommon.Get_content(page_url, fake_headers_mobile);
            if (mobile_page != null) {
                string p = @"<video id=.*?src=[\'""](.*?)[\'""]\W";
                Console.WriteLine(p);
                string url = YouGetCommon.MatchOne(mobile_page, p);
                string title = YouGetCommon.MatchOne(mobile_page, @"<title>((.|\n)+?)</title>");
                if (title == null || title.Equals("")) {
                    title = fid;

                }
                title = title.Replace("\n", "_");

                return YouGetCommon.Url_info_to_json(url, null);
            }
            return null;
        }

        public string Miaopai_load(string url)
        {
            Console.WriteLine("miao pai start ----- url = " + url);
            string fid = YouGetCommon.MatchOne(url, @"\?fid=(\d{4}:\w{32})");
            if (fid != null && !fid.Equals(""))
            {
                return Miaopai_load_bg_fid(fid);
            }
            else if (url.Contains("/p/230444"))
            {
                fid = YouGetCommon.MatchOne(url, @"/p/230444(\w+)");
                return Miaopai_load_bg_fid("1034:"+fid);
            }
            else
            {
                string mobile_page = YouGetCommon.Get_content(url, fake_headers_mobile);
                Regex reg = new Regex(@"""page_url""\s*:\s*""([^""]+)""");
                Match m = reg.Match(mobile_page);
                string videoUrl = YouGetCommon.MatchOne(mobile_page, @"<video.*src=['""](.*?)['""]");
                if (m != null && m.Success)
                {//网页内容里面没有数据
                    string escaped_url = m.Groups[1].Value;
                    string webUrl = HttpUtility.UrlEncode(escaped_url);
                    return Miaopai_load(escaped_url);
                } else if (videoUrl != null)//网页内容里直接有video信息
                {
                    Console.WriteLine("web page content video === ");
                    return YouGetCommon.Url_info_to_json(videoUrl, null);
                }
                else {//尝试header中是否有Location
                    Dictionary<string, string> mobile_header = YouGetCommon.GetHTTPResponseHeaders(url, fake_headers_mobile);
                    string key = "Location";
                    if (mobile_header.ContainsKey(key)) {
                        string locationUrl = mobile_header[key];
                        Console.WriteLine("go other link == " + locationUrl);
                        if (locationUrl != null && !locationUrl.Equals("")) {
                            string locaHost = getExtractUrlHost(locationUrl);
                            if (locaHost != null && !locaHost.Equals("")) {//可以转条连接
                                return new YouGetExtractor().YoutGetExtract(locationUrl);
                            }
                        }
                    }
                }
            }

            return null;
        }

        private string getExtractUrlHost(string url) {

            return YouGetCommon.MatchOne(url, @"(https?://[^/]+)/");
        }

        private string getHostName(string host) {

            return YouGetCommon.MatchOne(host, @"//([^.]+)\.");
        }

        public Media Extract(string url)
        {

            string jsonStr = YoutGetExtract(url);
            Console.WriteLine("miao pai ---- success " + jsonStr);

            return YouGetCommon.SingleVideoInfo_To_Media(url, jsonStr);
        }

        public string YoutGetExtract(string url)
        {
            return Miaopai_load(url);
        }
    }
}
