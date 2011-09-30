using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.CashModule
{
    [ModuleExport(typeof(CashModule))]
    public class CashModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly CashView _cashView;
        public ICategoryCommand NavigateCashViewCommand { get; set; }

        [ImportingConstructor]
        public CashModule(IRegionManager regionManager, CashView cashView)
        {
            _regionManager = regionManager;
            _cashView = cashView;
            NavigateCashViewCommand = new CategoryCommand<string>(Resources.Drawer, Resources.Common, "images/Xls.png", OnNavigateCashView, CanNavigateCashView) { Order = 70 };
            PermissionRegistry.RegisterPermission(PermissionNames.NavigateCashView, PermissionCategories.Navigation, Resources.CanNavigateCash);
            PermissionRegistry.RegisterPermission(PermissionNames.MakeCashTransaction, PermissionCategories.Cash, Resources.CanMakeCashTransaction);
        }

        private static bool CanNavigateCashView(string arg)
        {
            return AppServices.IsUserPermittedFor(PermissionNames.NavigateCashView) && AppServices.MainDataContext.CurrentWorkPeriod != null;
        }

        private void OnNavigateCashView(string obj)
        {
            ActivateTransactionList();
        }

        private void ActivateTransactionList()
        {
            ActivateModuleScreen();
            ((CashViewModel)_cashView.DataContext).SelectedAccount = null;
            ((CashViewModel)_cashView.DataContext).ActivateTransactionList();
        }

        private void ActivateModuleScreen()
        {
            AppServices.ActiveAppScreen = AppScreens.CashView;
            _regionManager.Regions[RegionNames.MainRegion].Activate(_cashView);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(CashView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.MakePaymentToAccount)
                    {
                        ActivateModuleScreen();
                        ((CashViewModel)_cashView.DataContext).MakePaymentToAccount(x.Value);
                    }

                    if (x.Topic == EventTopicNames.GetPaymentFromAccount)
                    {
                        ActivateModuleScreen();
                        ((CashViewModel)_cashView.DataContext).GetPaymentFromAccount(x.Value);
                    }

                    if (x.Topic == EventTopicNames.AddLiabilityAmount)
                    {
                        ActivateModuleScreen();
                        ((CashViewModel)_cashView.DataContext).AddLiabilityAmount(x.Value);
                    }

                    if (x.Topic == EventTopicNames.AddReceivableAmount)
                    {
                        ActivateModuleScreen();
                        ((CashViewModel)_cashView.DataContext).AddReceivableAmount(x.Value);
                    }
                });
        }

        protected override void OnPostInitialization()
        {
            CommonEventPublisher.PublishNavigationCommandEvent(NavigateCashViewCommand);
        }
    }
}
