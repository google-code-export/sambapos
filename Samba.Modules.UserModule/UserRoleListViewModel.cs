using Samba.Domain.Models.Users;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.UserModule
{
    public class UserRoleListViewModel : EntityCollectionViewModelBase<UserRoleViewModel, UserRole>
    {
        protected override UserRoleViewModel CreateNewViewModel(UserRole model)
        {
            return new UserRoleViewModel(model);
        }
        
        protected override UserRole CreateNewModel()
        {
            return new UserRole();
        }

        protected override string CanDeleteItem(UserRole model)
        {
            var count = Dao.Count<User>(x => x.UserRole.Id == model.Id);
            if (count > 0) return "Bu rol bir kullanıcı hesabında kullanılmakta olduğu için silinemez.";
            return base.CanDeleteItem(model);
        }
    }
}
