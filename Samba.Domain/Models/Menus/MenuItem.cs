using System;
using System.Collections.Generic;
using Samba.Domain.Foundation;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class MenuItem : IEntity
    {
        public MenuItem()
            : this(string.Empty)
        {

        }

        public MenuItem(string name)
        {
            Name = name;
            _portions = new List<MenuItemPortion>();
            _propertyGroups = new List<MenuItemPropertyGroup>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] LastUpdateTime { get; set; }
        public string GroupCode { get; set; }
        public string Barcode { get; set; }
        public string Tag { get; set; }

        private IList<MenuItemPortion> _portions;
        public virtual IList<MenuItemPortion> Portions
        {
            get { return _portions; }
            set { _portions = value; }
        }

        private IList<MenuItemPropertyGroup> _propertyGroups;
        public virtual IList<MenuItemPropertyGroup> PropertyGroups
        {
            get { return _propertyGroups; }
            set { _propertyGroups = value; }
        }

        private static MenuItem _all;
        public static MenuItem All { get { return _all ?? (_all = new MenuItem { Name = "*" }); } }

        public MenuItemPortion AddPortion(string portionName, decimal price, string currencyCode)
        {
            var mip = new MenuItemPortion
            {
                Name = portionName,
                Price = new Price(price, currencyCode),
                MenuItemId = Id
            };
            Portions.Add(mip);
            return mip;
        }

        internal MenuItemPortion GetPortion(string portionName)
        {
            foreach (var portion in Portions)
            {
                if (portion.Name == portionName)
                    return portion;
            }
            throw new Exception("Porsiyon Tanımlı Değil.");
        }

        public string UserString
        {
            get { return string.Format("{0} [{1}]", Name, GroupCode); }
        }

        public static MenuItemPortion AddDefaultMenuPortion(MenuItem item)
        {
            return item.AddPortion("Normal", 0, CurrencyContext.DefaultCurrency);
        }

        public static MenuItemProperty AddDefaultMenuItemProperty(MenuItemPropertyGroup item)
        {
            return item.AddProperty("", 0, CurrencyContext.DefaultCurrency);
        }

        public static MenuItem Create()
        {
            var result = new MenuItem();
            AddDefaultMenuPortion(result);
            return result;
        }
    }
}
