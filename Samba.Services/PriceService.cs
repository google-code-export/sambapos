using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public class MenuItemPriceData
    {
        public int PortionId { get; set; }
        public decimal Price { get; set; }
        public string PriceTag { get; set; }
    }

    public static class PriceService
    {
        public static IDictionary<int, MenuItemPriceData> Prices { get; set; }

        private static void RebuildPrices()
        {
            var menuItems = Dao.Query<MenuItem>(x => x.Portions.Select(y => y.Prices));
        }

        public static decimal GetCurrentPrice(int id)
        {
            return 0;
        }
    }
}
