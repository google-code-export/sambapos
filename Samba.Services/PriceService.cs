using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static DateTime LastRebuild { get; private set; }
        public static string CurrentPriceTag { get; private set; }

        public static IDictionary<int, MenuItemPriceData> Prices { get; private set; }

        static PriceService()
        {
            CurrentPriceTag = AppServices.SettingService.CurrentPriceTag;
            BuildPrices();
        }

        private static void RebuildPrices(string priceTag)
        {
            CurrentPriceTag = priceTag;
            MethodQueue.Queue("BuildPrices", BuildPrices);
        }

        public static void ApplyPriceList(string priceTag)
        {
            CurrentPriceTag = priceTag;
            MethodQueue.Queue("BuildPrices", ApplyPrices);
        }

        private static void ApplyPrices()
        {
            BuildPrices();
            AppServices.SettingService.LastPriceListRebuild = DateTime.Now;
            AppServices.SettingService.CurrentPriceTag = CurrentPriceTag;
            AppServices.SettingService.SaveChanges();
        }

        private static void BuildPrices()
        {
            var menuItems = Dao.Query<MenuItem>(x => x.Portions.Select(y => y.Prices));
            Prices = menuItems.SelectMany(x => x.Portions).ToDictionary(x => x.Id, y => new MenuItemPriceData() { Price = y.Price.Amount, PortionId = y.Id });
            if (!string.IsNullOrEmpty(CurrentPriceTag))
            {
                var subprices = menuItems.SelectMany(x => x.Portions).SelectMany(x => x.Prices).Where(x => x.Price > 0 && x.PriceTag == CurrentPriceTag).ToList();
                subprices.ForEach(x =>
                {
                    var p = Prices[x.MenuItemPortionId];
                    p.Price = x.Price;
                    p.PriceTag = x.PriceTag;
                });
            }
        }

        public static MenuItemPriceData GetCurrentPrice(int portionId)
        {
            Debug.Assert(Prices != null);
            if (Prices.ContainsKey(portionId))
                return Prices[portionId];
            return new MenuItemPriceData { Price = 0 };
        }

        public static void RebuildPricesIfNeeded()
        {
            var lastTime = AppServices.SettingService.LastPriceListRebuild;
            if (lastTime > LastRebuild)
            {
                LastRebuild = lastTime;
                CurrentPriceTag = AppServices.SettingService.CurrentPriceTag;
                RebuildPrices(CurrentPriceTag);
            }
        }

    }
}
