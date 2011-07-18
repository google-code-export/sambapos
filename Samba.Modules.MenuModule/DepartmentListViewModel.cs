using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class DepartmentListViewModel : EntityCollectionViewModelBase<DepartmentViewModel, Department>
    {
        protected override DepartmentViewModel CreateNewViewModel(Department model)
        {
            return new DepartmentViewModel(model);
        }

        protected override Department CreateNewModel()
        {
            return new Department();
        }

        protected override string CanDeleteItem(Department model)
        {
            var count = Dao.Count<UserRole>(x => x.DepartmentId == model.Id);
            if (count > 0) return "Bu departman bir kullanıcı rolünde kullanıldığı için silinemez.";
            return base.CanDeleteItem(model);
        }
    }
}
