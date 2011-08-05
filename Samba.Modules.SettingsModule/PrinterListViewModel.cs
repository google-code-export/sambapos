using Samba.Domain.Models.Settings;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class PrinterListViewModel : EntityCollectionViewModelBase<PrinterViewModel, Printer>
    {
        protected override PrinterViewModel CreateNewViewModel(Printer model)
        {
            return new PrinterViewModel(model);
        }

        protected override Printer CreateNewModel()
        {
            return new Printer();
        }
    }
}
