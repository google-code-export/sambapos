using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Modules.BasicReports.Reports
{
    internal class TicketTagInfo
    {
        public decimal Amount { get; set; }
        public int TicketCount { get; set; }
        private string _tagName;
        public string TagName
        {
            get { return !string.IsNullOrEmpty(_tagName) ? _tagName.Trim() : "[Adisyon]"; }
            set { _tagName = value; }
        }
    }
}
