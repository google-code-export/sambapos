using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class TerminalViewModel : EntityViewModelBase<Terminal>
    {
        public TerminalViewModel(Terminal model)
            : base(model)
        {
            SelectPrintJobsCommand = new CaptionCommand<string>("Görev Seç", OnAddPrintJob);
        }

        private IWorkspace _workspace;

        public bool IsDefault { get { return Model.IsDefault; } set { Model.IsDefault = value; } }
        public bool AutoLogout { get { return Model.AutoLogout; } set { Model.AutoLogout = value; } }
        public Printer SlipReportPrinter { get { return Model.SlipReportPrinter; } set { Model.SlipReportPrinter = value; } }
        public Printer ReportPrinter { get { return Model.ReportPrinter; } set { Model.ReportPrinter = value; } }
        public ObservableCollection<PrintJob> PrintJobs { get; set; }
        public IEnumerable<Printer> Printers { get; private set; }
        public IEnumerable<PrinterTemplate> PrinterTemplates { get; private set; }

        public ICaptionCommand SelectPrintJobsCommand { get; set; }

        public override Type GetViewType()
        {
            return typeof(TerminalView);
        }

        public override string GetModelTypeString()
        {
            return "Terminal";
        }

        public override void Initialize(IWorkspace workspace)
        {
            _workspace = workspace;
            Printers = workspace.All<Printer>();
            PrinterTemplates = workspace.All<PrinterTemplate>();
            PrintJobs = new ObservableCollection<PrintJob>(Model.PrintJobs);
        }

        private void OnAddPrintJob(string obj)
        {
            IList<IOrderable> values = new List<IOrderable>(_workspace.All<PrintJob>()
                .Where(x => PrintJobs.SingleOrDefault(y => y.Id == x.Id) == null));

            IList<IOrderable> selectedValues = new List<IOrderable>(PrintJobs.Select(x => x));

            var choosenValues =
                InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, "Yazdırma Görevi Listesi",
                Model.Name + " terminaline eklemek istediğiniz yazdırma görevlerini seçiniz", "Yazdırma Görevi", "Yazdırma Görevleri");

            PrintJobs.Clear();
            Model.PrintJobs.Clear();

            foreach (PrintJob choosenValue in choosenValues)
            {
                Model.PrintJobs.Add(choosenValue);
                PrintJobs.Add(choosenValue);
            }
        }
    }
}
