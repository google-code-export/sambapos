using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    class InternalAccountsViewModel : AccountReportViewModelBase
    {
        protected override FlowDocument GetReport()
        {
            return CreateReport("İç Kullanım Hesaplar", null, true);
        }

        protected override string GetHeader()
        {
            return "İç Kullanım Hesapları";
        }
    }
}
