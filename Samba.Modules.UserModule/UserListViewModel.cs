using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.UserModule
{
    public class UserListViewModel : EntityCollectionViewModelBase<UserViewModel, User>
    {
        protected override UserViewModel CreateNewViewModel(User model)
        {
            return new UserViewModel(model);
        }

        protected override User CreateNewModel()
        {
            return new User();
        }

        protected override string CanDeleteItem(User model)
        {
            if (model.UserRole.IsAdmin) return "Admin kullanıcı silinemez.";
            if (Workspace.Count<User>() == 1) return "Son kullanıcı hesabı silinemez.";
            var ti = Workspace.Single<TicketItem>(x => x.CreatingUserId == model.Id || x.ModifiedUserId == model.Id);
            if (ti != null) return "Adisyon işlemi yapmış kullanıcı silinemez.";
            return base.CanDeleteItem(model);
        }
    }
}
