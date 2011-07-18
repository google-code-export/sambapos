using System;
using System.Collections.Generic;
using Samba.Services;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class AbstractEntityCollectionViewModelBase : VisibleViewModelBase
    {
        public ICaptionCommand AddItemCommand { get; set; }
        public ICaptionCommand EditItemCommand { get; set; }
        public ICaptionCommand DeleteItemCommand { get; set; }
        public ICaptionCommand DuplicateItemCommand { get; set; }

        public IList<ICaptionCommand> CustomCommands { get; set; }

        public string ModelTitle { get; set; }


        private IEnumerable<ICaptionCommand> _allCommands;
        public IEnumerable<ICaptionCommand> AllCommands
        {
            get { return _allCommands ?? (_allCommands = GetCommands()); }
        }

        private IEnumerable<ICaptionCommand> GetCommands()
        {
            var result = new List<ICaptionCommand> {AddItemCommand, EditItemCommand, DeleteItemCommand};
            result.AddRange(CustomCommands);
            return result;
        }

        protected AbstractEntityCollectionViewModelBase()
        {
            ModelTitle = GetModelTitle();
            AddItemCommand = new CaptionCommand<object>(ModelTitle + " Ekle", OnAddItem, CanAddItem);
            EditItemCommand = new CaptionCommand<object>(ModelTitle + " Düzenle", OnEditItem, CanEditItem);
            DeleteItemCommand = new CaptionCommand<object>(ModelTitle + " Sil", OnDeleteItem, CanEditItem);
            DuplicateItemCommand = new CaptionCommand<object>(ModelTitle + " Kopyası Oluştur", OnDuplicateItem, CanDuplicateItem);
            CustomCommands = new List<ICaptionCommand>();
        }

        public abstract string GetModelTitle();

        protected abstract void OnDeleteItem(object obj);
        protected abstract void OnAddItem(object obj);
        protected abstract void OnEditItem(object obj);
        protected abstract bool CanEditItem(object obj);
        protected abstract bool CanDuplicateItem(object arg);
        protected abstract bool CanAddItem(object obj);
        protected abstract void OnDuplicateItem(object obj);

        protected override string GetHeaderInfo()
        {
            return ModelTitle + " Listesi";
        }

        public override Type GetViewType()
        {
            return typeof(EntityCollectionBaseView);
        }
    }
}
