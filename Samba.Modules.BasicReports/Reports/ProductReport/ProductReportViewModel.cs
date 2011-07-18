using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Tickets;

namespace Samba.Modules.BasicReports.Reports.ProductReport
{
    public class ProductReportViewModel : ReportViewModelBase
    {
        protected override FlowDocument GetReport()
        {
            var report = new SimpleReport("8cm");

            AddDefaultReportHeader(report, ReportContext.CurrentWorkPeriod, "Ürün Satış Raporu");

            var menuGroups = MenuGroupBuilder.CalculateMenuGroups(ReportContext.Tickets, ReportContext.MenuItems);

            report.AddColumTextAlignment("ÜrünGrubu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("ÜrünGrubu", "40*", "Auto", "35*");
            report.AddTable("ÜrünGrubu", "Ürün Grubu Bazında Satışlar", "", "");

            foreach (var menuItemInfo in menuGroups)
            {
                report.AddRow("ÜrünGrubu", menuItemInfo.GroupName,
                    string.Format("%{0:0.00}", menuItemInfo.Rate),
                    menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            report.AddRow("ÜrünGrubu", "Toplam", "", menuGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));


            //----------------------

            report.AddColumTextAlignment("ÜrünGrubuMiktar", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("ÜrünGrubuMiktar", "40*", "Auto", "35*");
            report.AddTable("ÜrünGrubuMiktar", "Ürün Grubu Bazında Miktarlar", "", "");

            foreach (var menuItemInfo in menuGroups)
            {
                report.AddRow("ÜrünGrubuMiktar", menuItemInfo.GroupName,
                    string.Format("%{0:0.00}", menuItemInfo.QuantityRate),
                    menuItemInfo.Quantity.ToString("#"));
            }

            report.AddRow("ÜrünGrubuMiktar", "Toplam", "", menuGroups.Sum(x => x.Quantity).ToString("#"));


            //----------------------

            var menuItems = MenuGroupBuilder.CalculateMenuItems(ReportContext.Tickets, ReportContext.MenuItems)
                .OrderByDescending(x => x.Quantity);

            report.AddColumTextAlignment("Ürün", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("Ürün", "50*", "Auto", "25*");
            report.AddTable("Ürün", "Ürün", "Adet", "Tutar");

            foreach (var menuItemInfo in menuItems)
            {
                report.AddRow("Ürün",
                    menuItemInfo.Name,
                    string.Format("{0:0.##}", menuItemInfo.Quantity),
                    menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            report.AddRow("Ürün", "Toplam", "", menuItems.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));


            //----------------------


            PrepareModificationTable(report, x => x.Voided, "İadeler");
            PrepareModificationTable(report, x => x.Gifted, "İkramlar");

            var discounts = ReportContext.Tickets
                .SelectMany(x => x.Discounts.Select(y => new { x.TicketNumber, y.UserId, Amount = y.DiscountAmount }))
                .GroupBy(x => new { x.TicketNumber, x.UserId }).Select(x => new { x.Key.TicketNumber, x.Key.UserId, Amount = x.Sum(y => y.Amount) });

            if (discounts.Count() > 0)
            {
                report.AddColumTextAlignment("İskontolar", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("İskontolar", "20*", "Auto", "35*");
                report.AddTable("İskontolar", "İskontolar", "", "");

                foreach (var discount in discounts.OrderByDescending(x => x.Amount))
                {
                    report.AddRow("İskontolar", discount.TicketNumber, ReportContext.GetUserName(discount.UserId), discount.Amount.ToString(ReportContext.CurrencyFormat));
                }

                if (discounts.Count() > 1)
                    report.AddRow("İskontolar", "Toplam", "", discounts.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            }

            //----------------------

            var ticketGroups = ReportContext.Tickets
                .GroupBy(x => new { x.DepartmentId })
                .Select(x => new { x.Key.DepartmentId, TicketCount = x.Count(), Amount = x.Sum(y => y.GetSum()) });

            if (ticketGroups.Count() > 0)
            {

                report.AddColumTextAlignment("Adisyonlar", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                report.AddColumnLength("Adisyonlar", "40*", "20*", "40*");
                report.AddTable("Adisyonlar", "Adisyonlar", "", "");

                foreach (var ticketGroup in ticketGroups)
                {
                    report.AddRow("Adisyonlar", ReportContext.GetDepartmentName(ticketGroup.DepartmentId), ticketGroup.TicketCount.ToString("#.##"), ticketGroup.Amount.ToString(ReportContext.CurrencyFormat));
                }

                if (ticketGroups.Count() > 1)
                    report.AddRow("Adisyonlar", "Toplam", ticketGroups.Sum(x => x.TicketCount).ToString("#.##"), ticketGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            }

            //----------------------

            var properties = ReportContext.Tickets
                .SelectMany(x => x.TicketItems.Where(y => y.Properties.Count > 0))
                .SelectMany(x => x.Properties.Where(y => y.MenuItemId == 0).Select(y => new { y.Name, x.Quantity }))
                .GroupBy(x => new { x.Name })
                .Select(x => new { x.Key.Name, Quantity = x.Sum(y => y.Quantity) });

            if (properties.Count() > 0)
            {

                report.AddColumTextAlignment("Özellikler", TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("Özellikler", "60*", "40*");
                report.AddTable("Özellikler", "Özellikler", "");

                foreach (var property in properties.OrderByDescending(x => x.Quantity))
                {
                    report.AddRow("Özellikler", property.Name, property.Quantity.ToString("#.##"));
                }
            }
            return report.Document;
        }

        private static void PrepareModificationTable(SimpleReport report, Func<TicketItem, bool> predicate, string title)
        {
            var modifiedItems = ReportContext.Tickets
                .SelectMany(x => x.TicketItems.Where(predicate).Select(y => new { Ticket = x, UserId = y.ModifiedUserId, MenuItem = y.MenuItemName, y.Quantity, y.ReasonId, y.ModifiedDateTime, Amount = y.GetItemValue() }));

            if (modifiedItems.Count() == 0) return;

            report.AddColumTextAlignment(title, TextAlignment.Left, TextAlignment.Left, TextAlignment.Left, TextAlignment.Left);
            report.AddColumnLength(title, "14*", "45*", "28*", "13*");
            report.AddTable(title, title, "", "", "");

            foreach (var voidItem in modifiedItems)
            {
                report.AddRow(title, voidItem.Ticket.TicketNumber, voidItem.Quantity.ToString("#.##") + " " + voidItem.MenuItem, ReportContext.GetUserName(voidItem.UserId), voidItem.ModifiedDateTime.ToShortTimeString());
                if (voidItem.ReasonId > 0)
                    report.AddRow(title, ReportContext.GetReasonName(voidItem.ReasonId), "", "", "");
            }

            var voidGroups =
                from c in modifiedItems
                group c by c.UserId into grp
                select new { UserId = grp.Key, Amount = grp.Sum(x => x.Amount) };

            report.AddColumTextAlignment("Personel" + title, TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Personel" + title, "60*", "40*");
            report.AddTable("Personel" + title, "Personel Bazlı " + title, "");

            foreach (var voidItem in voidGroups.OrderByDescending(x => x.Amount))
            {
                report.AddRow("Personel" + title, ReportContext.GetUserName(voidItem.UserId), voidItem.Amount.ToString(ReportContext.CurrencyFormat));
            }

            if (voidGroups.Count() > 1)
                report.AddRow("Personel" + title, "Toplam", voidGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
        }

        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected override string GetHeader()
        {
            return "Ürün Satış Raporu";
        }
    }
}
