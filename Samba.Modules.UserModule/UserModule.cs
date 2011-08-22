using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.RuleActions;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.UserModule
{
    [ModuleExport(typeof(UserModule))]
    public class UserModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        private UserListViewModel _userListViewModel;
        private UserRoleListViewModel _userRolesListViewModel;

        public ICategoryCommand ListUsersCommand { get; set; }
        public ICategoryCommand ListUserRolesCommand { get; set; }
        public ICategoryCommand NavigateLogoutCommand { get; set; }

        [ImportingConstructor]
        public UserModule(IRegionManager regionManager)
        {
            ListUserRolesCommand = new CategoryCommand<string>(Resources.UserRoleList, Resources.Users, OnListRoles) { Order = 50 };
            ListUsersCommand = new CategoryCommand<string>(Resources.UserList, Resources.Users, OnListUsers);
            NavigateLogoutCommand = new CategoryCommand<string>("Logout", Resources.Common, "images/bmp.png", OnNavigateUserLogout) { Order = 99 };
            _regionManager = regionManager;
        }

        private static void OnNavigateUserLogout(string obj)
        {
            AppServices.CurrentLoggedInUser.PublishEvent(EventTopicNames.UserLoggedOut);
            AppServices.LogoutUser();
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.RightUserRegion, typeof(LoggedInUserView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<string>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.PinSubmitted)
                    PinEntered(x.Value);
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<VisibleViewModelBase>>().Subscribe(s =>
            {
                if (s.Topic == EventTopicNames.ViewClosed)
                {
                    if (s.Value == _userListViewModel)
                        _userListViewModel = null;

                    if (s.Value == _userRolesListViewModel)
                        _userRolesListViewModel = null;
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.UserLoggedIn)
                {
                    RuleExecutor.NotifyEvent(RuleEventNames.UserLoggedIn, new { User = x.Value, UserName = x.Value.Name, RoleName = x.Value.UserRole.Name });
                }

                if (x.Topic == EventTopicNames.UserLoggedOut)
                {
                    RuleExecutor.NotifyEvent(RuleEventNames.UserLoggedOut, new { User = x.Value, UserName = x.Value.Name, RoleName = x.Value.UserRole.Name });
                }
            }
        );
        }

        protected override void OnPostInitialization()
        {
            CommonEventPublisher.PublishDashboardCommandEvent(ListUserRolesCommand);
            CommonEventPublisher.PublishDashboardCommandEvent(ListUsersCommand);
            CommonEventPublisher.PublishNavigationCommandEvent(NavigateLogoutCommand);

            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.UserLoggedIn, "User LoggedIn", new { UserName = "", RoleName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.UserLoggedOut, "User LoggedOut", new { UserName = "", RoleName = "" });
        }

        public void PinEntered(string pin)
        {
            var u = AppServices.LoginUser(pin);
            if (u != User.Nobody)
                u.PublishEvent(EventTopicNames.UserLoggedIn);
        }

        public void OnListUsers(string value)
        {
            if (_userListViewModel == null)
                _userListViewModel = new UserListViewModel();
            CommonEventPublisher.PublishViewAddedEvent(_userListViewModel);
        }

        public void OnListRoles(string value)
        {
            if (_userRolesListViewModel == null)
                _userRolesListViewModel = new UserRoleListViewModel();
            CommonEventPublisher.PublishViewAddedEvent(_userRolesListViewModel);
        }
    }
}
