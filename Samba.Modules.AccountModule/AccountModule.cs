using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Interaction;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    [ModuleExport(typeof(AccountModule))]
    public class AccountModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly AccountSelectorView _accountSelectorView;
        private AccountListViewModel _accountListViewModel;
        public ICategoryCommand ListAccountsCommand { get; set; }

        [ImportingConstructor]
        public AccountModule(IRegionManager regionManager, AccountSelectorView accountSelectorView)
        {
            _regionManager = regionManager;
            _accountSelectorView = accountSelectorView;
            ListAccountsCommand = new CategoryCommand<string>(Resources.AccountList, Resources.Accounts, OnListAccountsExecute) { Order = 40 };
            PermissionRegistry.RegisterPermission(PermissionNames.MakeAccountTransaction, PermissionCategories.Cash, Resources.CanMakeAccountTransaction);
            PermissionRegistry.RegisterPermission(PermissionNames.CreditOrDeptAccount, PermissionCategories.Cash, Resources.CanMakeCreditOrDeptTransaction);
        }

        private void OnListAccountsExecute(string obj)
        {
            if (_accountListViewModel == null)
                _accountListViewModel = new AccountListViewModel();
            CommonEventPublisher.PublishViewAddedEvent(_accountListViewModel);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountSelectorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<VisibleViewModelBase>>().Subscribe(s =>
            {
                if (s.Topic == EventTopicNames.ViewClosed)
                {
                    if (s.Value == _accountListViewModel)
                        _accountListViewModel = null;
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectAccount)
                {
                    ActivateAccountView();
                    ((AccountSelectorViewModel)_accountSelectorView.DataContext).RefreshSelectedAccount();
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateAccountView) ActivateAccountView();
                ((AccountSelectorViewModel)_accountSelectorView.DataContext).RefreshSelectedAccount();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateAccount)
                {
                    ActivateAccountView();
                    ((AccountSelectorViewModel)_accountSelectorView.DataContext).DisplayAccount(x.Value);
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.PopupClicked && x.Value.EventMessage == EventTopicNames.SelectAccount)
                    {
                        ActivateAccountView();
                        ((AccountSelectorViewModel)_accountSelectorView.DataContext).SearchAccount(x.Value.DataObject as string);
                    }
                }
                );
        }

        private void ActivateAccountView()
        {
            AppServices.ActiveAppScreen = AppScreens.AccountList;
            _regionManager.Regions[RegionNames.MainRegion].Activate(_accountSelectorView);
        }

        protected override void OnPostInitialization()
        {
            CommonEventPublisher.PublishDashboardCommandEvent(ListAccountsCommand);
        }
    }
}
