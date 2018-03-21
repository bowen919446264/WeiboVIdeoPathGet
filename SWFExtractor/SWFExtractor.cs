using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SWFExtractorNS.Utils;
using SWFExtractorNS.Extractors;
using System.IO;

namespace SWFExtractorNS
{
    /// <summary>
    /// 流类型
    /// </summary>
    public enum StreamType
    {
        Unknown,
        M3U8,
        MP4,
        RTMP
    }

    /// <summary>
    /// 流
    /// </summary>
    public class Stream
    {
        // 流类型
        public StreamType Type
        {
            get
            {
                return _type;
            }
        }

        // 流地址
        public List<String> Addresses
        {
            get
            {
                return _addresses;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="addresses"></param>
        public Stream(StreamType type, List<String> addresses)
        {
            _type = type;
            _addresses = addresses;
        }

        private StreamType _type;
        private List<String> _addresses;
    }

    /// <summary>
    /// media
    /// </summary>
    public class Media
    {
        // 源地址栏地址
        public String SourceURL
        {
            get
            {
                return _sourceURL;
            }
        }

        // 默认流
        public Stream DefaultStream
        {
            get
            {
                return _defaultStream;
            }
        }

        // 全部流
        public Stream[] Streams
        {
            get
            {
                return _streams;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceURL"></param>
        /// <param name="streams"></param>
        /// <param name="defaultStreamIndex"></param>
        public Media(String sourceURL, Stream[] streams, uint defaultStreamIndex)
        {
            _sourceURL = sourceURL;
            _streams = streams;
            if (defaultStreamIndex < _streams.Length)
                _defaultStream = _streams[defaultStreamIndex];
            else if (_streams.Length > 0)
                _defaultStream = _streams[0];
            else
                _defaultStream = null;
        }

        private String _sourceURL;
        private Stream _defaultStream;
        private Stream[] _streams;
    }

    /// <summary>
    /// 流媒体解析类
    /// </summary>
    public class SWFExtractor
    {
        static SWFExtractor()
        {
            HunanTVExtractor.Register();
            YunnanTVExtractor.Register();
            HTML5Extractor.Register();
            //YoukuExtractor.Register();
            //TudouExtractor.Register();
            //LetvExtractor.Register();
            SohuExtractor.Register();
            //IQiyiExtractor.Register();
            CCTVExtractor.Register();
            BallbarExtractor.Register();
            YouGetExtractor.Register();
        }

        /// <summary>
        /// 抽取网页真实视频流地址
        /// </summary>
        /// <param name="url">待抽取的网页地址</param>
        /// <returns>视频流的地址</returns>
        /// <exception cref="SWFExtractorNS.InvalidURLException">非法URL</exception>
        /// <exception cref="SWFExtractorNS.DomainNotSupportedException">域不支持</exception>
        public static Media Extract(String url)
        {
            try
            {
                String domain = "";
                string youGetDomain = YouGetCommon.Url_to_key(url);
                try
                {
                    domain = HTTPHelper.ParseDomain(url);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
                Console.WriteLine("-------------------------------------------------------------");
                if (domain == null || domain.Equals("") || extractors.ContainsKey(youGetDomain))
                {
                    domain = youGetDomain;
                }

                if (domain == null) throw new InvalidURLException(url);
                if (!extractors.ContainsKey(domain))
                {
                    int index = url.LastIndexOf('.');
                    if (index > 0)
                    {
                        String lastComponent = url.ToLower().Substring(index);
                        if (lastComponent.CompareTo(".mp4") == 0 || lastComponent.CompareTo(".flv") == 0)
                        {
                            throw new DomainNotSupportedException(domain);
                        }
                    }

                    HTML5Extractor extractor = new HTML5Extractor();
                    Media videoURL = extractor.Extract(url);
                    if (videoURL != null)
                        return videoURL;
                    throw new DomainNotSupportedException(domain);
                }
                else
                {
                    Type extractorType = extractors[domain];
                    IExtractor extractor = Activator.CreateInstance(extractorType) as IExtractor;
                    return extractor.Extract(url);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 下载m3u8文件到本地
        /// </summary>
        /// <param name="m3u8URL">m3u8地址</param>
        /// <param name="targetPath">本地文件全路径</param>
        /// <returns></returns>
        public static bool DownloadM3U8(String m3u8URL, String targetPath)
        {
            Regex domainRegex = new Regex("^(http://[^/]+)/");
            if (!domainRegex.IsMatch(m3u8URL)) return false;
            Match domainMatch = domainRegex.Match(m3u8URL);
            String domainPrefix = domainMatch.Groups[1].Value;

            int index = m3u8URL.LastIndexOf('/');
            if (index < 0) return false;
            String longPrefix = m3u8URL.Substring(0, index + 1);

            String response = HTTPHelper.SendHTTPGetRequest(m3u8URL);
            StringReader sr = new StringReader(response);
            String line = null;
            StringBuilder sb = new StringBuilder();
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("#EXT"))
                {
                    sb.Append(line);
                }
                else
                {
                    if (line.Length > 0)
                    {
                        if (line[0] == '/')
                        {
                            sb.Append(domainPrefix);
                        }
                        else
                        {
                            sb.Append(longPrefix);
                        }
                    }
                    
                    sb.Append(line);
                }
                sb.Append('\n');
            }

            File.WriteAllText(targetPath, sb.ToString());
            return true;
        }

        /// <summary>
        /// 注册Extractor
        /// </summary>
        /// <param name="domain">域</param>
        /// <param name="extractorType"></param>
        public static void RegisterExtractor(String domain, Type extractorType)
        {
            if (typeof(IExtractor).IsAssignableFrom(extractorType))
            {
                extractors.Add(domain, extractorType);
            }
        }

        #region private fields and functions

        private static IDictionary<String, Type> extractors = new Dictionary<String, Type>();

        #endregion
    }
}
