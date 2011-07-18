using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.BasicReports
{
    [ModuleExport(typeof(BasicReportModule))]
    public class BasicReportModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ICategoryCommand _navigateReportsCommand;
        private readonly BasicReportView _basicReportView;

        [ImportingConstructor]
        public BasicReportModule(IRegionManager regionManager, BasicReportView basicReportView)
        {
            _regionManager = regionManager;
            _basicReportView = basicReportView;
            _navigateReportsCommand = new CategoryCommand<string>("Raporlar", "Genel", "Images/Ppt.png", OnNavigateReportModule, CanNavigateReportModule) { Order = 80 };

            PermissionRegistry.RegisterPermission(PermissionNames.OpenReports, PermissionCategories.Navigation, "Raporları Açabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeReportDate, PermissionCategories.Report, "Rapor Tarihi Değiştirebilir");
        }

        private static bool CanNavigateReportModule(string arg)
        {
            return (AppServices.IsUserPermittedFor(PermissionNames.OpenReports) && AppServices.MainDataContext.CurrentWorkPeriod != null);
        }

        private void OnNavigateReportModule(string obj)
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_basicReportView);
            ReportContext.ResetCache();
            ReportContext.CurrentWorkPeriod = AppServices.MainDataContext.CurrentWorkPeriod;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(BasicReportView));
        }

        protected override void OnPostInitialization()
        {
            CommonEventPublisher.PublishNavigationCommandEvent(_navigateReportsCommand);
        }
    }
}
