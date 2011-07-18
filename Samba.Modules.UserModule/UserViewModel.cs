using System;
using System.Collections.Generic;
using Samba.Domain.Models.Users;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;
using System.Linq;

namespace Samba.Modules.UserModule
{
    public class UserViewModel : EntityViewModelBase<User>
    {
        public UserViewModel(User user)
            : base(user)
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<UserRole>>().Subscribe(x => RaisePropertyChanged("Roles"));
        }

        public string PinCode
        {
            get { return Model.PinCode; }
            set
            {
                Model.PinCode = value;
                RaisePropertyChanged("PinCode");
            }
        }

        public UserRole Role { get { return Model.UserRole; } set { Model.UserRole = value; } }

        public IEnumerable<UserRole> Roles { get; private set; }

        public override Type GetViewType()
        {
            return typeof(UserView);
        }

        public override string GetModelTypeString()
        {
            return "Kullanıcı";
        }

        public override void Initialize(IWorkspace workspace)
        {
            Roles = workspace.All<UserRole>();
        }

        protected override string GetSaveErrorMessage()
        {
            var users = AppServices.Workspace.All<User>(x => x.PinCode == PinCode);
            return users.Count() > 1 || (users.Count() == 1 && users.ElementAt(0).Id != Model.Id)
                ? "Bu pin kodunu başka bir kullanıcı kullanıyor" : "";
        }
    }
}
