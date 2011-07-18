using System.Linq;
using Samba.Domain.Models.Tables;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.TableModule
{
    public class TableListViewModel : EntityCollectionViewModelBase<TableEditorViewModel, Table>
    {
        public ICaptionCommand BatchCreateTables { get; set; }

        public TableListViewModel()
        {
            BatchCreateTables = new CaptionCommand<string>("Toplu Masa Ekle", OnBatchCreateTablesExecute);
            CustomCommands.Add(BatchCreateTables);
        }

        private void OnBatchCreateTablesExecute(string obj)
        {
            var values = InteractionService.UserIntraction.GetStringFromUser(
                "Toplu Masa Ekle",
                "Eklemek istediğiniz masa adlarını ekleyiniz. Kategorileri # karakteri ile başlatınız.");

            var createdItems = new DataCreationService().BatchCreateTables(values, Workspace);
            Workspace.CommitChanges();

            foreach (var table in createdItems)
                Items.Add(CreateNewViewModel(table));
        }

        protected override TableEditorViewModel CreateNewViewModel(Table model)
        {
            return new TableEditorViewModel(model);
        }

        protected override Table CreateNewModel()
        {
            return new Table();
        }

        protected override string CanDeleteItem(Table model)
        {
            if (model.TicketId > 0) return "Masaya bağlı adisyon varken masa silinemez.";
            var count = Dao.Count<TableScreen>(x => x.Tables.Any(y => y.Id == model.Id));
            if (count > 0) return "Bu masa bir masa görünümünde kullanıldığı için silinemez";
            return base.CanDeleteItem(model);
        }
    }
}
