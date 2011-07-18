using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Presentation.ViewModels
{
    public class TicketTagFilterViewModel
    {
        public string ButtonDisplay
        {
            get
            {
                var result = "Boş";
                if (TagValue == "*") return "Tümü";
                if (!string.IsNullOrEmpty(TagValue)) { result = TagValue; }
                if (Count > 0)
                    result += " [" + Count + "]";
                return result;
            }
        }
        public string TagGroup { get; set; }
        public string TagValue { get; set; }
        public int Count { get; set; }
        public string ButtonColor { get; set; }

        public TicketTagFilterViewModel()
        {
            ButtonColor = "Gray";
        }
    }
}
