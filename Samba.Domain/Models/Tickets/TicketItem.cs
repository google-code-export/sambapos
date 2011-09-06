using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Diagnostics;
using Samba.Domain.Foundation;
using Samba.Domain.Models.Menus;

namespace Samba.Domain.Models.Tickets
{
    public class TicketItem
    {
        public TicketItem()
        {
            _properties = new List<TicketItemProperty>();
            CreatedDateTime = DateTime.Now;
            ModifiedDateTime = DateTime.Now;
            _selectedQuantity = 0;
        }

        public int Id { get; set; }
        public int TicketId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string PortionName { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Quantity { get; set; }
        public int PortionCount { get; set; }
        public bool Locked { get; set; }
        public bool Voided { get; set; }
        public int ReasonId { get; set; }
        public bool Gifted { get; set; }
        public int OrderNumber { get; set; }
        public int CreatingUserId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int ModifiedUserId { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        [StringLength(10)]
        public string PriceTag { get; set; }

        private IList<TicketItemProperty> _properties;
        public virtual IList<TicketItemProperty> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        decimal _selectedQuantity;
        public decimal SelectedQuantity { get { return _selectedQuantity; } }

        public void UpdateMenuItem(int userId, MenuItem menuItem, string portionName, decimal price, string priceTag, int quantity, string defaultProperties)
        {
            MenuItemId = menuItem.Id;
            MenuItemName = menuItem.Name;
            var portion = menuItem.GetPortion(portionName);
            Debug.Assert(portion != null);
            UpdatePortion(portion.Name, price > 0 ? price : portion.Price.Amount, priceTag);
            Quantity = quantity;
            _selectedQuantity = quantity;
            PortionCount = menuItem.Portions.Count;
            CreatingUserId = userId;
            CreatedDateTime = DateTime.Now;

            if (!string.IsNullOrEmpty(defaultProperties))
            {
                foreach (var menuItemPropertyGroup in menuItem.PropertyGroups)
                {
                    var properties = defaultProperties.Split(',');
                    foreach (var defaultProperty in properties)
                    {
                        var property = defaultProperty.Trim();
                        var defaultValue = menuItemPropertyGroup.Properties.FirstOrDefault(x => x.Name == property);
                        if (defaultValue != null)
                            ToggleProperty(menuItemPropertyGroup, defaultValue);
                    }
                }
            }
        }

        public void UpdatePortion(string portionName, decimal price, string priceTag)
        {
            PortionName = portionName;
            Price = price;
            PriceTag = priceTag;
            CurrencyCode = CurrencyContext.DefaultCurrency;

            foreach (var ticketItemProperty in Properties)
            {
                ticketItemProperty.PortionName = portionName;
            }
        }

        public void ToggleProperty(MenuItemPropertyGroup group, MenuItemProperty property)
        {
            if (group.MultipleSelection && property.Price.Amount == 0)
            {
                var groupItems = Properties.Where(x => x.PropertyGroupId == group.Id).ToList();
                foreach (var tip in groupItems) Properties.Remove(tip);
                Quantity = 1;
                return;
            }

            var ti = FindProperty(property.Name) ?? new TicketItemProperty
                                                        {
                                                            Name = property.Name,
                                                            PropertyPrice = property.Price,
                                                            PropertyGroupId = group.Id,
                                                            MenuItemId = property.MenuItemId,
                                                            CalculateWithParentPrice = group.CalculateWithParentPrice,
                                                            PortionName = PortionName,
                                                            Quantity = group.MultipleSelection ? 0 : 1
                                                        };

            if (group.SingleSelection)
            {
                var tip = Properties.FirstOrDefault(x => x.PropertyGroupId == group.Id);
                if (tip != null)
                {
                    Properties.Insert(Properties.IndexOf(tip), ti);
                    Properties.Remove(tip);
                }
            }
            else if (group.MultipleSelection)
            {
                ti.Quantity++;
            }
            else if (!group.MultipleSelection && Properties.Contains(ti))
            {
                Properties.Remove(ti);
                return;
            }

            if (!Properties.Contains(ti)) Properties.Add(ti);
        }

        public TicketItemProperty GetCustomProperty()
        {
            return Properties.FirstOrDefault(x => x.PropertyGroupId == 0);
        }

        public TicketItemProperty GetOrCreateCustomProperty()
        {
            var tip = GetCustomProperty();
            if (tip == null)
            {
                tip = new TicketItemProperty
                          {
                              Name = "",
                              PropertyPrice = new Price(0, CurrencyContext.DefaultCurrency),
                              PropertyGroupId = 0,
                              MenuItemId = 0,
                              Quantity = 0
                          };
                Properties.Add(tip);
            }
            return tip;
        }

        public void UpdateCustomProperty(string text, decimal price, decimal quantity)
        {
            var tip = GetOrCreateCustomProperty();
            if (string.IsNullOrEmpty(text))
            {
                Properties.Remove(tip);
            }
            else
            {
                tip.Name = text;
                tip.PropertyPrice = new Price(price, CurrencyContext.DefaultCurrency);
                tip.Quantity = quantity;
            }
        }

        private TicketItemProperty FindProperty(string propertyName)
        {
            return Properties.FirstOrDefault(x => x.Name == propertyName);
        }

        public decimal GetTotal()
        {
            return GetTotal(CurrencyContext.DefaultCurrency, CurrencyContext.DefaultContext);
        }

        public decimal GetTotal(string currencyCode, CurrencyContext ccontext)
        {
            return Voided || Gifted ? 0 : GetItemValue(currencyCode, ccontext);
        }

        public decimal GetItemValue()
        {
            return GetItemValue(CurrencyContext.DefaultCurrency, CurrencyContext.DefaultContext);
        }

        public decimal GetItemValue(string currencyCode, CurrencyContext ccontext)
        {
            return Quantity * (GetItemPrice(currencyCode, ccontext));
        }

        public decimal GetSelectedValue()
        {
            return GetSelectedValue(CurrencyContext.DefaultCurrency, CurrencyContext.DefaultContext);
        }

        public decimal GetSelectedValue(string currencyCode, CurrencyContext ccontext)
        {
            return SelectedQuantity > 0 ?
                (SelectedQuantity * (GetItemPrice(currencyCode, ccontext))) :
                GetItemValue(currencyCode, ccontext);
        }

        public decimal GetItemPrice()
        {
            return GetItemPrice(CurrencyContext.DefaultCurrency, CurrencyContext.DefaultContext);
        }

        public decimal GetItemPrice(string currencyCode, CurrencyContext ccontext)
        {
            return (ccontext.ConvertTo(CurrencyCode, Price, currencyCode) +
                                GetTotalPropertyPrice(currencyCode, ccontext));
        }

        public decimal GetTotalPropertyPrice()
        {
            return GetTotalPropertyPrice(CurrencyContext.DefaultCurrency, CurrencyContext.DefaultContext);
        }

        private decimal GetTotalPropertyPrice(string currencyCode, CurrencyContext ccontext)
        {
            return Properties.Sum(property => ccontext.ConvertTo(CurrencyCode, property.PropertyPrice.Amount * property.Quantity, currencyCode));
        }

        public decimal GetPropertyPrice()
        {
            return GetPropertyPrice(CurrencyContext.DefaultCurrency, CurrencyContext.DefaultContext);
        }

        public decimal GetPropertyPrice(string currencyCode, CurrencyContext ccontext)
        {
            return Properties.Where(x => !x.CalculateWithParentPrice).Sum(property => ccontext.ConvertTo(CurrencyCode, property.PropertyPrice.Amount * property.Quantity, currencyCode));
        }

        public decimal GetMenuItemPropertyPrice()
        {
            return GetMenuItemPropertyPrice(CurrencyContext.DefaultCurrency, CurrencyContext.DefaultContext);
        }

        public decimal GetMenuItemPropertyPrice(string currencyCode, CurrencyContext ccontext)
        {
            return Properties.Where(x => x.CalculateWithParentPrice).Sum(property => ccontext.ConvertTo(CurrencyCode, property.PropertyPrice.Amount * property.Quantity, currencyCode));
        }

        public void IncSelectedQuantity()
        {
            _selectedQuantity++;
            if (_selectedQuantity > Quantity) _selectedQuantity = 1;
        }

        public void DecSelectedQuantity()
        {
            _selectedQuantity--;
            if (_selectedQuantity < 1) _selectedQuantity = 1;
        }

        public void ResetSelectedQuantity()
        {
            _selectedQuantity = Quantity;
        }

        public string GetPortionDesc()
        {
            if (PortionCount > 1
                && !string.IsNullOrEmpty(PortionName)
                && !string.IsNullOrEmpty(PortionName.Trim('\b', ' ', '\t'))
                && PortionName.ToLower() != "normal")
                return "." + PortionName;
            return "";
        }
    }
}
