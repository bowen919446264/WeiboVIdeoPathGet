using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWFExtractorNS.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace SWFExtractorNS.Extractors
{
    class Yixia : IExtractor, IYouGetExtractor
    {

        public string Yixia_miaopai_load_by_scid(string scid)
        {
            string api_endpoint = "http://api.miaopai.com/m/v2_channel.json?fillType=259&scid="+scid+"&vend=miaopai";
            string html = YouGetCommon.Get_content(api_endpoint, null);
            if (html == null) {
                return null;
            }
            JObject api_content = (JObject)JsonConvert.DeserializeObject(html);
            string urlTemp = api_content["result"]["stream"]["base"].ToString();
            string video_url = YouGetCommon.MatchOne(urlTemp, @"(.+)\?vend");
            string title = api_content["result"]["ext"]["t"].ToString();

            string resultInfo = YouGetCommon.Url_info_to_json(video_url, null);
            return resultInfo;
        }

        public string Yixia_xiaokaxiu_load_by_scid(string scid)
        {
            string api_endpoint = "http://api.xiaokaxiu.com/video/web/get_play_video?scid=" + scid;
            string html = YouGetCommon.Get_content(api_endpoint, null);
            if (html == null) {
                return null;
            }
            JObject api_content = (JObject)JsonConvert.DeserializeObject(html);
            string video_url = api_content["data"]["linkurl"].ToString();
            string title = api_content["data"]["title"].ToString();
            string resultInfo = YouGetCommon.Url_info_to_json(video_url, null);
            return resultInfo;
        }


        public string Yixia_load(string url)
        {
            Console.WriteLine("Yixia load start ---- " + url);

            string result_json = null;
            Uri uri = new Uri(url);
            string hostname = uri.Host;
            Console.WriteLine("Yixia load==== host name == " + hostname);
            if (hostname != null) {
                if (hostname.Contains("miaopai.com")) {
                    string scid = Get_miaopai_scid(url);
                    if (scid != null) {
                        result_json = Yixia_miaopai_load_by_scid(scid);
                    } 
                } else if (hostname.Contains("xiaokaxiu.com")) {
                    string scid = null;
                    Regex reg = new Regex(@"http://v.xiaokaxiu.com/v/.+\.html");
                    Match m = reg.Match(url);
                    if (m.Success)
                    {
                        scid = m.Groups[0].Value;
                    }
                    else
                    {
                        reg = new Regex(@"http://m.xiaokaxiu.com/m/.+\.html");
                        m = reg.Match(url);
                        if (m.Success) {
                            scid = m.Groups[0].Value;
                        }
                    }

                    if (scid != null) {
                        result_json =  Yixia_xiaokaxiu_load_by_scid(scid);
                    }

                }
            }
            return result_json;
        }

        private string Get_miaopai_scid(string url) {
            string scid = null;
            string[] regs = new string[] { @"miaopai\.com/show/channel/(.+)\.htm",
               @"miaopai\.com/show/(.+)\.htm",
                @"m\.miaopai\.com/show/channel/(.+)\.htm",
                @"m\.miaopai\.com/show/channel/(.+)" };
            foreach(string reg in regs) {
                scid = YouGetCommon.MatchOne(url, reg);
                if (scid != null && !"".Equals(scid)) {
                    return scid;
                }
            }
            return null;
        }

        public Media Extract(string url)
        {
            //throw new NotImplementedException();
            string jsonStr = YoutGetExtract(url);
            Console.WriteLine("Yixia success-------------" + jsonStr);
            if (jsonStr == null) {
                return null;
            }
            return YouGetCommon.SingleVideoInfo_To_Media(url, jsonStr);
        }

        public string YoutGetExtract(string url)
        {
            return Yixia_load(url);
        }
    }
}
