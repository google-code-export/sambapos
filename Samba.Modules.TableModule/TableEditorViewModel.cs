using System;
using System.Collections.Generic;
using Samba.Domain.Models.Tables;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TableModule
{
    public class TableEditorViewModel : EntityViewModelBase<Table>
    {
        public TableEditorViewModel(Table model)
            : base(model)
        {

        }

        private IEnumerable<string> _categories;
        public IEnumerable<string> Categories { get { return _categories; } }

        public string Category { get { return Model.Category; } set { Model.Category = value; } }
        public string GroupValue { get { return Model.Category; } }

        public override Type GetViewType()
        {
            return typeof(TableEditorView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Table;
        }

        public override void Initialize(IWorkspace workspace)
        {
            _categories = Dao.Distinct<Table>(x => x.Category);
        }

        protected override bool CanSave(string arg)
        {
            if (Model.TicketId > 0) return false;
            return base.CanSave(arg);
        }

        protected override string GetSaveErrorMessage()
        {
            if (Dao.Single<Table>(x => x.Name.ToLower() == Model.Name.ToLower() && x.Id != Model.Id) != null)
                return Resources.SaveErrorDuplicateTableName;
            return base.GetSaveErrorMessage();
        }

    }
}
