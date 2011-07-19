using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure;

namespace Samba.Presentation.ViewModels
{
    public class TicketTagFilterViewModel : IStringCompareable
    {
        public string ButtonDisplay
        {
            get
            {
                var result = "Geri";
                if (TagValue == "*") return "Tümü";
                if (TagValue == " ") result = "Boş";
                if (!string.IsNullOrEmpty(TagValue.Trim())) { result = TagValue; }
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

        public string GetStringValue()
        {
            return TagValue;
        }
    }
}
