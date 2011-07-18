using Samba.Domain.Models.Settings;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class PrinterTemplateCollectionViewModel : EntityCollectionViewModelBase<PrinterTemplateViewModel, PrinterTemplate>
    {
        protected override PrinterTemplateViewModel CreateNewViewModel(PrinterTemplate model)
        {
            return new PrinterTemplateViewModel(model);
        }

        protected override PrinterTemplate CreateNewModel()
        {
            return new PrinterTemplate();
        }

        protected override string CanDeleteItem(PrinterTemplate model)
        {
            var count = Dao.Count<PrinterMap>(x => x.PrinterTemplate.Id == model.Id);
            if (count > 0) return "Bu yazıcı şablonu bir yazdırma görevinde kullanıldığından silinemez.";
            return base.CanDeleteItem(model);
        }
    }
}
