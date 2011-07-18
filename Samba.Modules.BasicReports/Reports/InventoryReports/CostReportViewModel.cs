using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Samba.Modules.BasicReports.Reports.InventoryReports
{
    class CostReportViewModel : ReportViewModelBase
    {
        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected override FlowDocument GetReport()
        {
            var report = new SimpleReport("8cm");
            AddDefaultReportHeader(report, ReportContext.CurrentWorkPeriod, "Maliyet Raporu");
           
            var costItems = ReportContext.PeriodicConsumptions.SelectMany(x => x.CostItems)
                .GroupBy(x => new { ItemName = x.Name, PortionName = x.Portion.Name })
                .Select(x => new { x.Key.ItemName, x.Key.PortionName, TotalQuantity = x.Sum(y => y.Quantity), TotalCost = x.Sum(y => y.Cost * y.Quantity) });

            if (costItems.Count() > 0)
            {
                report.AddColumTextAlignment("Maliyet", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                report.AddColumnLength("Maliyet", "38*", "20*", "17*", "25*");
                report.AddTable("Maliyet", "Ürün", "Porsiyon", "Miktar", "Ortalama Maliyet");

                foreach (var costItem in costItems)
                {
                    report.AddRow("Maliyet",
                        costItem.ItemName,
                        costItem.PortionName,
                        costItem.TotalQuantity.ToString("#,#0.##"),
                        (costItem.TotalCost / costItem.TotalQuantity).ToString(ReportContext.CurrencyFormat));
                }

                report.AddRow("Maliyet","Toplam","","",costItems.Sum(x=>x.TotalCost).ToString(ReportContext.CurrencyFormat));
            }
            else report.AddHeader("Seçili dönemde maliyet hesaplanabilecek bir ürün bulunmuyor.");

            return report.Document;
        }

        protected override string GetHeader()
        {
            return "Maliyet Raporu";
        }
    }
}
