using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWFExtractorNS;

namespace SWFExtratorClient
{
    class StreamAddress
    {
        public int StreamIndex { get; set; }
        public StreamType StreamType { get; set; }
        public int AddressIndex { get; set; }
        public String Address { get; set; }
        public int SourceStreamIndex { get; set; }
    }
}
