using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWFExtractorNS.Utils;
using System.Text.RegularExpressions;

namespace SWFExtractorNS.Extractors
{
    class HTML5Extractor : IExtractor
    {
        private static readonly String HTML5 = "*.*";

        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            SWFExtractor.RegisterExtractor(HTML5, typeof(HunanTVExtractor));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Media Extract(String url)
        {
            String responseString = HTTPHelper.SendHTTPGetRequest(url);
            Regex regex = new Regex("[^<]<video\\s+[^>]*src=['\"]?([^\\s'\"]+)['\"][^>]*>");
            if (regex.IsMatch(responseString))
            {
                Match m = regex.Match(responseString);
                List<Stream> streams = new List<Stream>();
                while (m != null)
                {
                    String src = m.Groups[1].Value;
                    if(src != null)
                    {
                        streams.Add(new Stream(StreamType.Unknown, new List<string>{src}));
                    }

                    m = m.NextMatch();
                }
                if (streams.Count > 0)
                    return new Media(url, streams.ToArray(), 0);
            }
            return null;
        }
    }
}
