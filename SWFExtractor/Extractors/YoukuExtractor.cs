using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWFExtractorNS.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Web;

namespace SWFExtractorNS.Extractors
{
    class YoukuExtractor : IExtractor
    {
        private static readonly String YOUKU = "youku.com";
        // http://k.youku.com/player/getFlvPath/sid/{sid}_00/st/flv/fileid/{fileid}?start=0&K={key}&hd={0,1,2,3}&myp=0&ts=62&ymovie=1&ypp=2&ctype=10&ev=1&token={token}&oip=3663206230&ep={ep}&yxon=1&special=true
        private static readonly String VIDEO_INFO_ADDRESS = "http://play.youku.com/play/get.json?ct=10&&pt=0&ob=1&vid={0}&aid={1}";

        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            SWFExtractor.RegisterExtractor(YOUKU, typeof(YoukuExtractor));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Media Extract(String url)
        {
            String domain = HTTPHelper.ParseDomain(url);
            if (!domain.Equals(YOUKU, StringComparison.OrdinalIgnoreCase))
                throw new DomainNotSupportedException(domain);

            return ExtractCurrentValid(url);
        }

        /// <summary>
        /// 当前有效的爬取策略
        /// </summary>
        /// <param name="url">类似 http://v.youku.com/v_show/id_XMTU4MDgxNTg4OA==.html?f=27301952&from=y1.2-3.2 </param>
        /// <returns></returns>
        private Media ExtractCurrentValid(String url)
        {
            Regex reg = new Regex("http://v.youku.com/v_show/id_([^.]+).html\\?f=(\\d+)");
            if (!reg.IsMatch(url))
            {
                throw new Exception("无效URL");
            }

            Match m = reg.Match(url);
            YoukuVideo youkuVideo = new YoukuVideo(m.Groups[1].Value, m.Groups[2].Value);
            String videoInfoAccessURL = String.Format(VIDEO_INFO_ADDRESS, youkuVideo.vid, youkuVideo.aid);
            String responseString = HTTPHelper.SendHTTPGetRequest(videoInfoAccessURL);
            youkuVideo.Parse(responseString);
            return ExtractURL(youkuVideo);
        }

        private Media ExtractURL(YoukuVideo youkuVideo)
        {
            /*
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            JArray ja = (JArray)jo["data"]["stream"];
            VideoURL videoURL = new VideoURL();
            IDictionary<String, String> urls = new Dictionary<String, String>();
            foreach (JObject stream in ja)
            {
                String url = stream["url"].ToString();
                String responseString = HTTPHelper.SendHTTPGetRequest(url);
                JObject streamJO = (JObject)JsonConvert.DeserializeObject(responseString);

                String streamURL = streamJO["info"].ToString();
                String name = stream["name"].ToString();
                urls.Add(name, streamURL);
                if (videoURL.NormalURL == null)
                    videoURL.NormalURL = streamURL;
            }

            videoURL.URLs = urls;
            videoURL.urlType = URLType.M3U8;
            return videoURL;
             */
            return null;
        }

        /*
        private static string myEncoder(string a, byte[] c, bool isToBase64)
        {
            string result = "";
            List<Byte> bytesR = new List<byte>();
            int f = 0, h = 0, q = 0;
            int[] b = new int[256];
            for (int i = 0; i < 256; i++)
                b[i] = i;
            while (h < 256)
            {
                f = (f + b[h] + a[h % a.Length]) % 256;
                int temp = b[h];
                b[h] = b[f];
                b[f] = temp;
                h++;
            }
            f = 0; h = 0; q = 0;
            while (q < c.Length)
            {
                h = (h + 1) % 256;
                f = (f + b[h]) % 256;
                int temp = b[h];
                b[h] = b[f];
                b[f] = temp;
                byte[] bytes = new byte[] { (byte)(c[q] ^ b[(b[h] + b[f]) % 256]) };
                bytesR.Add(bytes[0]);
                result += System.Text.ASCIIEncoding.ASCII.GetString(bytes);
                q++;
            }
            if (isToBase64)
            {
                Byte[] byteR = bytesR.ToArray();
                result = Convert.ToBase64String(byteR);
            }
            return result;
        }

        public static void getEp(string vid, string ep, ref string epNew, ref string token, ref string sid)
        {
            string template1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            string template2 = "bf7e5f01";
            byte[] bytes = Convert.FromBase64String(ep);
            //string urf8ep = System.Text.UTF8Encoding.UTF8.GetString(bytes);
            //string ascIIep = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
            string temp = myEncoder(template1, bytes, false);
            string[] part = temp.Split('_');
            sid = part[0];
            token = part[1];
            string whole = string.Format("{0}_{1}_{2}", sid, vid, token);
            byte[] newbytes = System.Text.ASCIIEncoding.ASCII.GetBytes(whole);
            epNew = myEncoder(template2, newbytes, true);
        }
         * */

        class YoukuStream
        {
            public String fileID;
            public String key;
            public Int64 videoDuration; // ms
            public Int64 audioDuration; // ms
            public Int64 size;
            public String streamType;
            public int width;
            public int height;

            public String sid;
            public String token;
            public String ep;

            public void GenerateEpSidToken(String encryptedString)
            {
                /*
                this._playListData.oip = data.security.ip;
                if (data.security.encrypt_string)
                {
                    str = PlayListUtil.getInstance().getSize(data.security.encrypt_string);
                    strs = str.split("_");
                    this._playListData.sid = strs[0];
                    this._playListData.tk = strs[1];
                }

                 * */

                // 与下面PlayListUtil.getSize效果相同
                byte[] encryptedBytes = Convert.FromBase64String(encryptedString);
                String encryptedASCIIString = System.Text.Encoding.ASCII.GetString(encryptedBytes);
                
                String orignalASCIIString = encryptedASCIIString;   // TODO: 此处应该把加密字符串解密
                String[] orignalParts = orignalASCIIString.Split('_');
                sid = orignalParts[0];
                token = orignalParts[1];

                String orignalEP = sid + "_" + fileID + "_" + token;
                //orignalEP = orignalEP + ("_" + PlayerConfig.bctime);
                //_loc13_ = PlayListUtil.getInstance().changeSize(orignalEP);
                //orignalEP = PlayListUtil.getInstance().setSize(orignalEP + "_" + _loc13_.substr(0, 4)));
                byte[] orignalEPBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(orignalEP);
                String base64EP = Convert.ToBase64String(orignalEPBytes);
                String urlEncodedEP = HttpUtility.UrlEncode(base64EP);
                ep = urlEncodedEP;

                //urlEncodedEP = "nzFtLpUtlk3NLw4vhlu633sWn0wNfe11DJaH%2BcpJokFHEjPDcEi4pNkCEIt43trVsjbSozqqonIxeb0vwKpoV23uDU4F4ygPlEErIYzk5G%2BFNYPSLKPEv88U8cQlycLlWT1p1rbXWik%3D";
                //base64EP = HttpUtility.UrlDecode(urlEncodedEP);
                //orignalEPBytes = Convert.FromBase64String(base64EP);
                //orignalEP = System.Text.ASCIIEncoding.ASCII.GetString(orignalEPBytes);

                /*
                 * *
                 * public static function getFileParameters(param1:int, param2:IPlayListData, param3:VideoSegmentData, param4:Boolean, param5:int = 0) : String
                      {
                         var _loc13_:String = null;
                         var _loc6_:* = "";
                         var _loc7_:String = param1.toString(16);
                         if(_loc7_.length == 1)
                         {
                            _loc7_ = "0" + _loc7_;
                         }
                         _loc6_ = _loc6_ + ("/sid/" + param2.sid + "_" + _loc7_);
                         var _loc8_:String = param2.fileType;
                         if(PlayerConfig.isHoutaiPlayer)
                         {
                            _loc8_ = param3.type;
                         }
                         if(_loc8_ == "hd2" || _loc8_ == "hd3")
                         {
                            _loc8_ = "flv";
                         }
                         if(param2.drm)
                         {
                            _loc8_ = "f4v";
                         }
                         _loc6_ = _loc6_ + ("/st/" + _loc8_);
                         _loc6_ = _loc6_ + ("/fileid/" + param3.fileId);
                         if(param4)
                         {
                            if(param2.drm)
                            {
                               _loc6_ = _loc6_ + "?K=";
                            }
                            else
                            {
                               _loc6_ = _loc6_ + ("?start=" + int(param5) + "&K=");
                            }
                         }
                         else
                         {
                            _loc6_ = _loc6_ + "?K=";
                         }
                         if(param3.key == "")
                         {
                            _loc6_ = _loc6_ + (param2.key2 + param2.key1);
                         }
                         else
                         {
                            _loc6_ = _loc6_ + param3.key;
                         }
                         var _loc9_:String = "";
                         var _loc10_:String = param2.fileType;
                         if(PlayerConfig.isHoutaiPlayer)
                         {
                            _loc10_ = param3.type;
                         }
                         if(_loc10_ == "flv" || _loc10_ == "flvhd")
                         {
                            _loc9_ = "0";
                         }
                         else if(_loc10_ == "mp4")
                         {
                            _loc9_ = "1";
                         }
                         else if(_loc10_ == "hd2")
                         {
                            _loc9_ = "2";
                         }
                         else if(_loc10_ == "hd3")
                         {
                            _loc9_ = "3";
                         }
                         _loc6_ = _loc6_ + ("&hd=" + _loc9_);
                         var _loc11_:int = CoreContext.playerProxy.playerData.IkuFlag;
                         _loc6_ = _loc6_ + ("&myp=" + _loc11_.toString());
                         _loc6_ = _loc6_ + ("&ts=" + int(param3.seconds));
                         if(param2.show)
                         {
                            if(param2.show.isVideoPaid == 1)
                            {
                               _loc6_ = _loc6_ + "&ypremium=1";
                            }
                            else
                            {
                               _loc6_ = _loc6_ + "&ymovie=1";
                            }
                         }
                         _loc6_ = _loc6_ + ("&ypp=" + P2PConfig.ypp);
                         _loc6_ = _loc6_ + ("&ctype=" + PlayerConstant.CTYPE);
                         _loc6_ = _loc6_ + ("&ev=" + PlayerConstant.EV);
                         _loc6_ = _loc6_ + ("&token=" + param2.tk);
                         _loc6_ = _loc6_ + ("&oip=" + param2.oip);
                         var _loc12_:String = param2.sid + "_" + param3.fileId + "_" + param2.tk;
                         _loc12_ = _loc12_ + ("_" + PlayerConfig.bctime);
                         _loc13_ = PlayListUtil.getInstance().changeSize(_loc12_);
                         _loc6_ = _loc6_ + ("&ep=" + PlayListUtil.getInstance().setSize(_loc12_ + "_" + _loc13_.substr(0,4)));
                         return _loc6_;
                      }
                   }
                 * */
            }
        }

        class YoukuVideo
        {
            public String vid;
            public String aid;
            public String ip;
            public String encryptString;
            public List<YoukuStream> streams = new List<YoukuStream>();

            public YoukuVideo(String vid, String aid)
            {
                this.vid = vid;
                this.aid = aid;
            }

            public void Parse(String json)
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(json);
                this.ip = jo["data"]["security"]["ip"].ToString();
                this.encryptString = jo["data"]["security"]["encrypt_string"].ToString();

                JArray ja = (JArray)jo["data"]["stream"];
                foreach (JObject stream in ja)
                {
                    YoukuStream youkuStream = new YoukuStream();
                    youkuStream.streamType = stream["stream_type"].ToString();
                    youkuStream.width = Convert.ToInt32(stream["width"].ToString());
                    youkuStream.height = Convert.ToInt32(stream["height"].ToString());
                    youkuStream.size = Convert.ToInt64(stream["size"].ToString());
                    youkuStream.videoDuration = Convert.ToInt64(stream["milliseconds_video"].ToString());
                    youkuStream.audioDuration = Convert.ToInt64(stream["milliseconds_audio"].ToString());
                    JObject seg = ((JObject)((JArray)stream["segs"])[0]);
                    youkuStream.fileID = seg["fileid"].ToString();
                    youkuStream.key = seg["key"].ToString();
                    youkuStream.GenerateEpSidToken(encryptString);
                    this.streams.Add(youkuStream);
                }
            }
        }

        public class PlayListUtil
        {
            public static String getSize(String encryptString)
            {
               byte[] _loc2_ = Base64.decodeToByteArray(encryptString);
               return System.Text.Encoding.ASCII.GetString(_loc2_);
            }
        }

        public class Base64
        {
          public const String version = "1.0.0";
          private const String BASE64_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
           
          
          public static String encode(String param1)
          {
             char[] _loc2_ = param1.ToArray();
             return encodeByteArray(_loc2_);
          }

          public static String encodeByteArray(char[] param1)
          {
             List<char> _loc3_ = null;
             uint _loc5_ = 0;
             uint _loc6_ = 0;
             uint _loc7_ = 0;
             String _loc2_ = "";
             byte[] _loc4_ = new byte[4];
             int readOffset = 0;
             while (readOffset < param1.Length)
             {
                _loc3_ = new List<char>();
                _loc5_ = 0;
                while (_loc5_ < 3 && readOffset < param1.Length)
                {
                    _loc3_.Add(param1[readOffset]);
                   _loc5_++;
                   readOffset++;
                }
                _loc4_[0] = (byte)((_loc3_[0] & 252) >> 2);
                _loc4_[1] = (byte)((_loc3_[0] & 3) << 4 | _loc3_[1] >> 4);
                _loc4_[2] = (byte)((_loc3_[1] & 15) << 2 | _loc3_[2] >> 6);
                _loc4_[3] = (byte)(_loc3_[2] & 63);
                _loc6_ = (uint)_loc3_.Count;
                while(_loc6_ < 3)
                {
                   _loc4_[_loc6_ + 1] = 64;
                   _loc6_++;
                }
                _loc7_ = 0;
                while(_loc7_ < _loc4_.Length)
                {
                   _loc2_ = _loc2_ + BASE64_CHARS[_loc4_[_loc7_]];
                   _loc7_++;
                }
             }
             return _loc2_;
          }
      
          public static String decode(String param1)
          {
             byte[] _loc2_ = decodeToByteArray(param1);
             return System.Text.Encoding.UTF8.GetString(_loc2_);
          }
      
          public static byte[] decodeToByteArray(String param1)
          {
             uint _loc6_ = 0;
             uint _loc7_ = 0;
             List<byte> _loc2_ = new List<byte>();
             byte[] _loc3_ = new byte[4];
             byte[] _loc4_ = new byte[3];
             uint _loc5_ = 0;
             while(_loc5_ < param1.Length)
             {
                _loc6_ = 0;
                while (_loc6_ < 4 && _loc5_ + _loc6_ < param1.Length)
                {
                    _loc3_[_loc6_] = (byte)BASE64_CHARS.IndexOf(param1[(int)(_loc5_ + _loc6_)]);
                   _loc6_++;
                }
                _loc4_[0] = (byte)((_loc3_[0] << 2) + ((_loc3_[1] & 48) >> 4));
                _loc4_[1] = (byte)(((_loc3_[1] & 15) << 4) + ((_loc3_[2] & 60) >> 2));
                _loc4_[2] = (byte)(((_loc3_[2] & 3) << 6) + _loc3_[3]);
                _loc7_ = 0;
                while(_loc7_ < _loc4_.Length)
                {
                   if(_loc3_[_loc7_ + 1] == 64)
                   {
                      break;
                   }
                   _loc2_.Add(_loc4_[_loc7_]);
                   _loc7_++;
                }
                _loc5_ = _loc5_ + 4;
             }
             return _loc2_.ToArray();
          }
       }
    }
}
