using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Win32;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Modules.BasicReports.Reports.CSVBuilder
{
    class CsvBuilderViewModel : ReportViewModelBase
    {
        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());

        }

        protected override FlowDocument GetReport()
        {
            var currentPeriod = ReportContext.CurrentWorkPeriod;
            var report = new SimpleReport("8cm");
            AddDefaultReportHeader(report, currentPeriod, Resources.CsvBuilder);
            report.Header.TextAlignment = TextAlignment.Left;
            report.AddHeader("");
            report.AddHeader(Resources.ClickLinksToExportData);
            report.AddHeader("");
            report.AddLink(Resources.ExportSalesData);
            HandleLink(Resources.ExportSalesData);

            return report.Document;
        }

        protected override void HandleClick(string text)
        {
            if (text == Resources.ExportSalesData)
            {
                ExportSalesData();
            }
        }

        private static void ExportSalesData()
        {
            var saveFileDialog = new SaveFileDialog
                                     {
                                         FileName = Resources.ExportSalesData + "_" + DateTime.Now.ToString().Replace(":", "").Replace(" ", "_"),
                                         DefaultExt = ".csv"
                                     };

            var result = saveFileDialog.ShowDialog();
            if (!result.GetValueOrDefault(false))
            {
                return;
            }

            var lines = ReportContext.Tickets.SelectMany(x => x.TicketItems, (t, ti) => new { Ticket = t, TicketItem = ti });
            var data = lines.Select(x =>
                new
                    {
                        DateTime = x.TicketItem.CreatedDateTime,
                        Date = x.TicketItem.CreatedDateTime.ToShortDateString(),
                        Time = x.TicketItem.CreatedDateTime.ToShortTimeString(),
                        TicketNumber = x.Ticket.TicketNumber,
                        Account = x.Ticket.CustomerName,
                        Location = x.Ticket.LocationName,
                        x.TicketItem.OrderNumber,
                        x.TicketItem.Voided,
                        x.TicketItem.Gifted,
                        Name = x.TicketItem.MenuItemName,
                        Portion = x.TicketItem.PortionName,
                        x.TicketItem.Quantity,
                        Price = x.TicketItem.GetItemPrice(),
                        Value = x.TicketItem.GetItemValue(),
                        Discount = x.Ticket.GetTotalDiscounts() / x.Ticket.GetPlainSum(),
                        Total = MenuGroupBuilder.CalculateTicketItemTotal(x.Ticket, x.TicketItem),
                    }
                );
            var csv = data.AsCsv();
            File.WriteAllText(saveFileDialog.FileName, csv);
        }

        protected override string GetHeader()
        {
            return Resources.CsvBuilder;
        }
    }
}
