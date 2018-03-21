using System;
using System.Collections.Generic;
using System.Text;

namespace SWFExtractorNS
{
    class InvalidURLException : Exception
    {
        private String _url;

        public InvalidURLException(String url)
        {
            _url = url;
        }

        public override string ToString()
        {
            return String.Format("地址" + _url + "不是合法的URL");
        }
    }
}
