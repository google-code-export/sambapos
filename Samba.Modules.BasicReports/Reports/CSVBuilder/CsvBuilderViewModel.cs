using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Localization.Properties;

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
            var report = new SimpleReport("8cm");
            report.AddHeader("Samba POS");
            report.AddHeader(Resources.CsvBuilder);
            report.AddHeader(string.Format(Resources.As_f, DateTime.Now));
            report.AddLink("Export Sales");
            AddLink("Export Sales");
            report.Document.IsEnabled = true;
            return report.Document;
        }

        protected override void HandleClick(string text)
        {
            if (text == "Export Sales")
            {
                MessageBox.Show("Exporrrtttt");
            }
        }

        protected override string GetHeader()
        {
            return Resources.CsvBuilder;
        }
    }
}
