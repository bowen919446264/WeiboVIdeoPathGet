using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWFExtractorNS.Utils;

namespace SWFExtractorNS.Extractors
{
    class LetvExtractor : IExtractor
    {
        private static readonly String LETV = "letv.com";
        //http://api.le.com/mms/out/video/playJson?id=25505195&platid=1&splatid=101&format=1&tkey=1958086804&domain=www.le.com&dvtype=1000&accessyx=1&devid=104D453E9F81A959767E88B75C52EA521D3694D6
        //http://play.g3proxy.lecloud.com/vod/v2/MTk4LzUxLzY1L2xldHYtdXRzLzE0L3Zlcl8wMF8yMi0xMDQ1Mjc3MDU3LWF2Yy00MTg4MjctYWFjLTMyMDEwLTExNjc0Mi02NzA4NjMwLWQ5ZTFiMjJhYTI4ZWQ0NmEzODk2NzQxOTVkY2M4M2FhLTE0NjQwNTAwNTQ5NzgubXA0?b=459&mmsid=57404585&tm=1464080710&key=2e0b4b0d313ca685f5902fa064351f9c&platid=1&splatid=101&playid=0&tss=ios&vtype=13&cvid=702642841942&payff=0&pip=d94dd5301df06667b90b48619e1ba84e&ctv=pc&m3v=1&termid=1&format=1&hwtype=un&ostype=Windows7&tag=letv&sign=letv&expect=3&p1=1&p2=10&p3=-&vid=25505195&tn=0.12157758139073849&pay=0&uuid=687E21B62761F71B256137F576A7E72A55D3C302&a_idx=undefined&token=null&uid=null&rateid=1000
        private static readonly String VIDEO_INFO_ADDRESS = "http://play.youku.com/play/get.json?ct=10&&pt=0&ob=1&vid={0}&aid={1}";

        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            SWFExtractor.RegisterExtractor(LETV, typeof(LetvExtractor));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Media Extract(String url)
        {
            String domain = HTTPHelper.ParseDomain(url);
            if (!domain.Equals(LETV, StringComparison.OrdinalIgnoreCase))
                throw new DomainNotSupportedException(domain);

            return ExtractCurrentValid(url);
        }

        /// <summary>
        /// 当前有效的爬取策略
        /// </summary>
        /// <param name="url">类似 http://www.le.com/ptv/vplay/25505195.html </param>
        /// <returns></returns>
        private Media ExtractCurrentValid(String url)
        {
            return null;
        }
    }
}
