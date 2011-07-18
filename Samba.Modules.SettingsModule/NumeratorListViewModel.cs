using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class NumeratorListViewModel:EntityCollectionViewModelBase<NumeratorViewModel,Numerator>
    {
        protected override NumeratorViewModel CreateNewViewModel(Numerator model)
        {
            return new NumeratorViewModel(model);
        }

        protected override Numerator CreateNewModel()
        {
            return new Numerator();
        }

        protected override string CanDeleteItem(Numerator model)
        {
            var count = Dao.Count<Department>(x => x.OrderNumerator.Id == model.Id);
            if (count > 0) return "Bu numaratör bir departmanda sipariş numaratörü olarak kullanıldığı için silinemez.";
            count = Dao.Count<Department>(x => x.TicketNumerator.Id == model.Id);
            if (count > 0) return "Bu numaratör bir departmanda adisyon numaratörü olarak kullanıldığı için silinemez.";
            count = Dao.Count<TicketTagGroup>(x => x.Numerator.Id == model.Id);
            if (count > 0) return "Bu numaratör bir adisyon etiketinde kullanıldığı için silinemez.";
            return base.CanDeleteItem(model);
        }
    }
}
