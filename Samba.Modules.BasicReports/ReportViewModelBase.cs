using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Threading;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.BasicReports
{
    public abstract class ReportViewModelBase : ObservableObject
    {
        public string Header { get { return GetHeader(); } }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged("Selected");
                RaisePropertyChanged("Background");
                RaisePropertyChanged("Foreground");
            }
        }

        public string Background { get { return Selected ? "Orange" : "White"; } }
        public string Foreground { get { return Selected ? "White" : "Black"; } }

        public ObservableCollection<FilterGroup> FilterGroups { get; set; }

        public string StartDateString { get { return ReportContext.StartDateString; } set { ReportContext.StartDateString = value; } }
        public string EndDateString { get { return ReportContext.EndDateString; } set { ReportContext.EndDateString = value; } }

        public ICaptionCommand PrintDocumentCommand { get; set; }
        public ICaptionCommand RefreshFiltersCommand { get; set; }

        public FlowDocument Document { get; set; }

        public bool CanUserChangeDates { get { return AppServices.IsUserPermittedFor(PermissionNames.ChangeReportDate); } }

        protected ReportViewModelBase()
        {
            PrintDocumentCommand = new CaptionCommand<string>(Resources.Print, OnPrintDocument);
            RefreshFiltersCommand = new CaptionCommand<string>(Resources.Refresh, OnRefreshFilters, CanRefreshFilters);
            FilterGroups = new ObservableCollection<FilterGroup>();
        }

        protected virtual void OnRefreshFilters(string obj)
        {
            var sw = FilterGroups[0].SelectedValue as WorkPeriod;
            if (sw == null) return;
            if (ReportContext.CurrentWorkPeriod != null && (ReportContext.StartDate != sw.StartDate || ReportContext.EndDate != sw.EndDate))
            {
                ReportContext.CurrentWorkPeriod =
                    ReportContext.CreateCustomWorkPeriod("", ReportContext.StartDate, ReportContext.EndDate);
            }
            else ReportContext.CurrentWorkPeriod =
                FilterGroups[0].SelectedValue as WorkPeriod;
            RefreshReport();
        }

        protected abstract void CreateFilterGroups();

        protected FilterGroup CreateWorkPeriodFilterGroup()
        {
            var wpList = ReportContext.GetWorkPeriods(ReportContext.StartDate, ReportContext.EndDate).ToList();
            wpList.Insert(0, ReportContext.ThisMonthWorkPeriod);
            wpList.Insert(0, ReportContext.LastMonthWorkPeriod);
            wpList.Insert(0, ReportContext.ThisWeekWorkPeriod);
            wpList.Insert(0, ReportContext.LastWeekWorkPeriod);
            wpList.Insert(0, ReportContext.YesterdayWorkPeriod);
            wpList.Insert(0, ReportContext.TodayWorkPeriod);

            if (!wpList.Contains(ReportContext.CurrentWorkPeriod))
            { wpList.Insert(0, ReportContext.CurrentWorkPeriod); }

            if (!wpList.Contains(AppServices.MainDataContext.CurrentWorkPeriod))
                wpList.Insert(0, AppServices.MainDataContext.CurrentWorkPeriod);

            return new FilterGroup { Values = wpList, SelectedValue = ReportContext.CurrentWorkPeriod };
        }

        private bool CanRefreshFilters(string arg)
        {
            return FilterGroups.Count > 0;
        }

        private void OnPrintDocument(string obj)
        {
            AppServices.PrintService.PrintSlipReport(Document);
        }

        public void RefreshReport()
        {
            Document = null;
            RaisePropertyChanged("Document");
            //Program ilk yüklendiğinde aktif gün başı işlemi yoktur.
            if (ReportContext.CurrentWorkPeriod == null) return;
            var memStream = new MemoryStream();
            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                {
                    var doc = GetReport();
                    XamlWriter.Save(doc, memStream);
                    memStream.Position = 0;
                };

                worker.RunWorkerCompleted +=
                    delegate
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(
                        delegate
                        {
                            Document = (FlowDocument)XamlReader.Load(memStream);
                            RaisePropertyChanged("Document");
                            RaisePropertyChanged("StartDateString");
                            RaisePropertyChanged("EndDateString");
                            CreateFilterGroups();
                            foreach (var filterGroup in FilterGroups)
                            {
                                var group = filterGroup;
                                filterGroup.ValueChanged = delegate
                                                               {
                                                                   var sw = group.SelectedValue as WorkPeriod;
                                                                   if (sw != null)
                                                                   {
                                                                       ReportContext.StartDate = sw.StartDate;
                                                                       ReportContext.EndDate = sw.EndDate;
                                                                       RefreshFiltersCommand.Execute("");
                                                                   }
                                                               };
                            }
                        }));
                    };

                worker.RunWorkerAsync();
            }
        }

        protected abstract FlowDocument GetReport();
        protected abstract string GetHeader();

        public void AddDefaultReportHeader(SimpleReport report, WorkPeriod workPeriod, string caption)
        {
            report.AddHeader("Samba POS");
            report.AddHeader(caption);
            if (workPeriod.EndDate > workPeriod.StartDate)
                report.AddHeader(workPeriod.StartDate.ToString("dd MMMM yyyy HH:mm") +
                    " - " + workPeriod.EndDate.ToString("dd MMMM yyyy HH:mm"));
            else
            {
                report.AddHeader(workPeriod.StartDate.ToString("dd MMMM yyyy HH:mm") +
                " - " + DateTime.Now.ToString("dd MMMM yyyy HH:mm"));
            }
        }
    }
}
