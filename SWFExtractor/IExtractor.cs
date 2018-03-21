using System;
using System.Collections.Generic;
using System.Text;

namespace SWFExtractorNS
{
    interface IExtractor
    {
        Media Extract(String url);
    }
}