using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWFExtractorNS.Utils;


namespace SWFExtractorNS.Extractors
{
    public class BallbarExtractor :IExtractor
    {
        private static readonly string BALLBAR = "ballbar.cc";

        public static void Register()
        {
            SWFExtractor.RegisterExtractor(BALLBAR, typeof(BallbarExtractor));
        }


      

     
        /// <summary>
        /// 输入网址 例如:
        /// http://v.ballbar.cc/watch/D1817D7942B4A0E3927B4DA4780B8BB18B723420BA6B1E47246AD38FCB141D9F14803128001480327200.html
        /// </summary>
        /// <returns>
        /// 得到 D1817D7942B4A0E3927B4DA4780B8BB18B723420BA6B1E47246AD38FCB141D9F14803128001480327200 这部分
        /// </returns>
        /// 
        private  string getPageName(string url)
        {
         
            string[] splitor1 = {"/"};
            string[] temps = url.Split(splitor1,StringSplitOptions.RemoveEmptyEntries);
            string[] splitor2 = {"."};
            temps = temps[temps.Length - 1].Split(splitor2,StringSplitOptions.RemoveEmptyEntries);
            if (temps[0].Length != 84)
                throw new Exception("pageName 不是 84位");
            return temps[0];
        }

        /// <summary>
        /// 拼接url,用于向服务器请求视频流信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private  string getNextUrl(string str)
        {
            string str1 = "//live2.ballbar.cc";
            string str2 = "//cdn.b8b8.tv:8888";
            long time = getDate();
            return "http:"+str1 + "/watch/getdata.php?k=" + str + "&d=" + str2 + "&&t=" + time;
        }

        /// <summary>
        /// 从服务器获取视频流的相关信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private  string getThirdUrl(string url)
        {
            String responseString = HTTPHelper.SendHTTPGetRequest(url);
            responseString = System.Web.HttpUtility.UrlDecode(responseString);
            return responseString;
        }

        /// <summary>
        /// 从服务器返回的信息中提取流地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private  string getFinalUrl(string url)
        {
            string[] splitor1 = { "$$$", "?str=", "&type=" };
            string[] temps = url.Split(splitor1, StringSplitOptions.RemoveEmptyEntries);
            return temps[2];

        }


        /// <summary>
        /// 返回 如 1480316902132 这种类型的时间
        /// </summary>
        /// <returns></returns>
        private  long getDate()
        {
            TimeSpan ts = DateTime.Now - DateTime.Parse("1970-1-1");
            long time = (long)ts.TotalMilliseconds;
            return time;
        }

        /// <summary>
        /// 根据网页的地址获取播放的视频的流地址
        /// </summary>
        /// <param name="url">网页的地址</param>
        /// <returns>播放的视频的流地址</returns>
        private  string getVedioUrl(string url)
        {
            string res = getPageName(url);
            res = getNextUrl(res);
            res = getThirdUrl(res);
            res = getFinalUrl(res);
            return res;
        }

        public Media Extract(string url)
        {
           
            string vedioUrl = getVedioUrl(url);
            List<Stream> streams = new List<Stream>();
            if(vedioUrl.StartsWith("rtmp"))
                streams.Add(new Stream(StreamType.RTMP, new List<string> { vedioUrl }));
            else
                streams.Add(new Stream(StreamType.M3U8, new List<string> { vedioUrl }));
            if (streams.Count > 0)
                return new Media(url, streams.ToArray(), 0);
            else
                return null;
        }

    }
}
