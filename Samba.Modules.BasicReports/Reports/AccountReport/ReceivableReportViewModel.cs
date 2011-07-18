using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    public class ReceivableReportViewModel : AccountReportViewModelBase
    {
        protected override FlowDocument GetReport()
        {
            return CreateReport("Borçlu Hesaplar", true, false);
        }

        protected override string GetHeader()
        {
            return "Borçlu Hesaplar";
        }
    }
}
