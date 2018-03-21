using System;
using System.Collections.Generic;
using System.Text;

namespace SWFExtractorNS
{
    class DomainNotSupportedException : Exception
    {
        private String _domain;

        public DomainNotSupportedException(String domain)
        {
            _domain = domain;
        }

        public override string ToString()
        {
            return String.Format("域" + _domain + "不支持");
        }
    }
}
