using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWFExtractorNS.Utils;

namespace SWFExtractorNS.Extractors
{
    class IQiyiExtractor : IExtractor
    {
        private static readonly String IQIYI = "iqiyi.com";
        // http://k.youku.com/player/getFlvPath/sid/{sid}_00/st/flv/fileid/{fileid}?start=0&K={key}&hd={0,1,2,3}&myp=0&ts=62&ymovie=1&ypp=2&ctype=10&ev=1&token={token}&oip=3663206230&ep={ep}&yxon=1&special=true
        private static readonly String VIDEO_INFO_ADDRESS = "http://play.youku.com/play/get.json?ct=10&&pt=0&ob=1&vid={0}&aid={1}";

        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            SWFExtractor.RegisterExtractor(IQIYI, typeof(TudouExtractor));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Media Extract(String url)
        {
            String domain = HTTPHelper.ParseDomain(url);
            if (!domain.Equals(IQIYI, StringComparison.OrdinalIgnoreCase))
                throw new DomainNotSupportedException(domain);

            return ExtractCurrentValid(url);
        }

        /// <summary>
        /// 当前有效的爬取策略
        /// </summary>
        /// <param name="url">类似 http://www.iqiyi.com/v_19rrlhvj3s.html </param>
        /// <returns></returns>
        private Media ExtractCurrentValid(String url)
        {
            return null;
        }
    }
}
