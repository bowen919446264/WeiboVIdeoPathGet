using SWFExtractorNS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWFExtractorNS.Extractors
{
    class YouGetExtractor : IExtractor, IYouGetExtractor
    {
        private static Dictionary<string, Type> youGetType = new Dictionary<string, Type>
        {
            {"weibo", typeof(MiaoPai)},
            {"miaopai",  typeof(Yixia)},
            {"xiaokaxiu",typeof(Yixia)},
            { "krcom", typeof(Krcom)}
            
        };

        public static void Register()
        {
            foreach (KeyValuePair<string, Type> pair in youGetType) {
                SWFExtractor.RegisterExtractor(pair.Key, typeof(YouGetExtractor));
            }
            
        }

        public Media Extract(string url)
        {
            String domain = YouGetCommon.Url_to_key(url);
            if (!youGetType.ContainsKey(domain))
                throw new DomainNotSupportedException(domain);

            Type extractorType = youGetType[domain];
            IExtractor extractor = Activator.CreateInstance(extractorType) as IExtractor;
            return extractor.Extract(url);
            
        }

        public string YoutGetExtract(string url)
        {
            String domain = YouGetCommon.Url_to_key(url);
            if (!youGetType.ContainsKey(domain))
                throw new DomainNotSupportedException(domain);

            Type extractorType = youGetType[domain];
            IYouGetExtractor extractor = Activator.CreateInstance(extractorType) as IYouGetExtractor;
            return extractor.YoutGetExtract(url);
        }
    }
}
