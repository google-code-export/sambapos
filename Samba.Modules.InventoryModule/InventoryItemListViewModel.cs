using Samba.Domain.Models.Inventory;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    class InventoryItemListViewModel : EntityCollectionViewModelBase<InventoryItemViewModel, InventoryItem>
    {
        protected override InventoryItemViewModel CreateNewViewModel(InventoryItem model)
        {
            return new InventoryItemViewModel(model);
        }

        protected override InventoryItem CreateNewModel()
        {
            return new InventoryItem();
        }
    }
}
