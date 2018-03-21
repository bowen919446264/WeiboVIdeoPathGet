using Newtonsoft.Json.Linq;
using SWFExtractorNS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SWFExtractorNS.Extractors
{

    class Krcom : IExtractor, IYouGetExtractor
    {
        public string Krcom_load(string url) {
            string page = YouGetCommon.Get_content(url, null);
            if (page != null) {
                try
                {
                    //Console.Write(page);
                    //Console.WriteLine("===================");
                    string videosource = YouGetCommon.MatchOne(page, @"video-sources=\\""fluency=(.*?)\\""");

                    
                    string decodeVideoSource = HttpUtility.UrlDecode(videosource);

                    Console.WriteLine("Krcom_load ---" + decodeVideoSource);
                    string[] videoUrls = Regex.Split(decodeVideoSource, "//", RegexOptions.IgnoreCase); ;
                    JArray jArr = new JArray();
                    foreach (string v in videoUrls)
                    {
                        if (!YouGetCommon.isEmptyStr(v))
                        {
                            string videoUrl = "http://" + v;
                            JObject json = new JObject();
                            json.Add("video_url", videoUrl);
                            if (videoUrl.Contains("map4")) {
                                json.Add("video_type","mp4");
                            }

                            jArr.Add(json);
                        }
                    }
                    return jArr.ToString();
                    
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
                
            }
            return null;
        }



        public Media Extract(string url)
        {
            string jsonStr = YoutGetExtract(url);
            try
            {
                JArray arr = JArray.Parse(jsonStr);
                Console.Write(jsonStr);
                uint length = (uint)(arr.Count - 1);
                List<Stream> list = new List<Stream>();
                foreach (JObject json in arr) {
                    List<String> urls= new List<String>();
                    urls.Add(json["video_url"].ToString());
                    Stream s = new Stream(StreamType.MP4, urls);
                    list.Add(s);
                }
                return new Media(url, list.ToArray(), length - 1);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
           

            return null;
        }

        public string YoutGetExtract(string url)
        {

            return Krcom_load(url);
        }
    }
}
