using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    public class GiftReasonListViewModel : EntityCollectionViewModelBase<GiftReasonViewModel, Reason>
    {
        public ICaptionCommand CreateBatchGiftReasons { get; set; }

        public GiftReasonListViewModel()
        {
            CreateBatchGiftReasons = new CaptionCommand<string>("Toplu İkram Nedeni Ekle", OnCreateBatchGiftReasons);
            CustomCommands.Add(CreateBatchGiftReasons);
        }

        private void OnCreateBatchGiftReasons(string obj)
        {
            var values = InteractionService.UserIntraction.GetStringFromUser(
                "Toplu İkram Nedeni Ekle",
                "Eklemek istediğiniz İkram Nedenlerini her satıra bir madde gelecek şekilde ekleyiniz.");

            var createdItems = new DataCreationService().BatchCreateReasons(values, 1, Workspace);
            Workspace.CommitChanges();

            foreach (var mv in createdItems.Select(CreateNewViewModel))
            {
                mv.Initialize(Workspace);
                Items.Add(mv);
            }
        }

        protected override GiftReasonViewModel CreateNewViewModel(Reason model)
        {
            return new GiftReasonViewModel(model);
        }

        protected override Reason CreateNewModel()
        {
            return new Reason() { ReasonType = 1 };
        }

        protected override System.Collections.ObjectModel.ObservableCollection<GiftReasonViewModel> GetItemsList()
        {
            return BuildViewModelList(Workspace.All<Reason>(x => x.ReasonType == 1));
        }

        protected override string CanDeleteItem(Reason model)
        {
            var gifts = Dao.Count<TicketItem>(x => x.Gifted && x.ReasonId == model.Id);
            return gifts > 0 ? "Bu ikram nedeni kullanıldığı için silinemez." : "";
        }
    }
}
