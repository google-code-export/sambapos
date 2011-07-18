using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class ScreenMenuListViewModel : EntityCollectionViewModelBase<ScreenMenuViewModel, ScreenMenu>
    {
        protected override ScreenMenuViewModel CreateNewViewModel(ScreenMenu model)
        {
            return new ScreenMenuViewModel(model);
        }

        protected override ScreenMenu CreateNewModel()
        {
            return new ScreenMenu();
        }

        protected override string CanDeleteItem(ScreenMenu model)
        {
            var count = Dao.Count<Department>(x=>x.ScreenMenuId == model.Id);
            if (count > 0) return "Bu menü görünümü bir departmanda kullanıldığından silinemez.";
            return base.CanDeleteItem(model);
        }
    }
}
