using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.TicketModule
{
    public enum FilterType
    {
        OpenTickets,
        AllTickets,
        Customer,
        Location
    }

    public class TicketExplorerFilter : ObservableObject
    {
        public TicketExplorerFilter()
        {
            FilterValues = new List<string>();
        }

        private readonly string[] _filterTypes = { Resources.OnlyOpenTickets, Resources.AllTickets, Resources.Account, Resources.Table };
        public int FilterTypeIndex
        {
            get { return (int)FilterType; }
            set
            {
                FilterType = (FilterType)value;
                FilterValue = "";
                RaisePropertyChanged("IsTextBoxEnabled");
            }
        }

        public bool IsTextBoxEnabled { get { return FilterType != FilterType.OpenTickets; } }
        public string[] FilterTypes { get { return _filterTypes; } }

        public FilterType FilterType { get; set; }

        private string _filterValue;
        public string FilterValue
        {
            get { return _filterValue; }
            set
            {
                _filterValue = value;
                RaisePropertyChanged("FilterValue");
                //UpdateFilterValues();
            }
        }

        private List<string> _filterValues;
        public List<string> FilterValues
        {
            get { return _filterValues; }
            set { _filterValues = value; }
        }

        //public void UpdateFilterValues()
        //{
        //    FilterValues.Clear();
        //    if (FilterType == FilterType.Customer && FilterValue.Length >= 2)
        //    {
        //        FilterValues.AddRange(Dao.Select<Customer, String>(x => x.Name, x => x.Name.StartsWith(FilterValue)).ToList());
        //    }
        //    if (FilterType == FilterType.Location && FilterValue.Length == 1)
        //    {
        //        FilterValues.AddRange(Dao.Select<Table, String>(x => x.Name, x => x.Name.StartsWith(FilterValue)).ToList());
        //    }
        //    RaisePropertyChanged("FilterValues");
        //}

        public Expression<Func<Ticket, bool>> GetExpression()
        {
            Expression<Func<Ticket, bool>> result = null;

            if (FilterType == FilterType.OpenTickets)
                result = x => !x.IsPaid;

            if (FilterType == FilterType.Location)
            {
                if (FilterValue == "*")
                    result = x => !string.IsNullOrEmpty(x.LocationName);
                else if (!string.IsNullOrEmpty(FilterValue))
                    result = x => x.LocationName.ToLower() == FilterValue.ToLower();
                else result = x => string.IsNullOrEmpty(x.LocationName);
            }

            if (FilterType == FilterType.Customer)
            {
                if (FilterValue == "*")
                    result = x => !string.IsNullOrEmpty(x.CustomerName);
                else if (!string.IsNullOrEmpty(FilterValue))
                    result = x => x.CustomerName.ToLower().Contains(FilterValue.ToLower());
                else
                    result = x => string.IsNullOrEmpty(x.CustomerName);
            }

            return result;
        }
    }
}
