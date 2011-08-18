using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
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

        protected override string CanDeleteItem(InventoryItem model)
        {
            var item = Dao.Single<PeriodicConsumptionItem>(x => x.InventoryItem.Id == model.Id);
            if (item != null)
                return Resources.DeleteErrorInventoryItemUsedInEndOfDayRecord;
            return base.CanDeleteItem(model);
        }
    }
}
