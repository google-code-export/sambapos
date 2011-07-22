using System;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class PriceListViewModel : VisibleViewModelBase
    {
        private readonly IWorkspace _workspace = WorkspaceFactory.Create();

        public ICaptionCommand SaveCommand { get; set; }

        private ObservableCollection<PriceViewModel> _items;
        public ObservableCollection<PriceViewModel> Items
        {
            get { return _items ?? (_items = CreateItems()); }
        }

        public PriceListViewModel()
        {
            SaveCommand = new CaptionCommand<string>(Resources.Save, OnSave);
        }

        private void OnSave(object obj)
        {
            _workspace.CommitChanges();
            foreach (var priceViewModel in Items)
            {
                priceViewModel.IsChanged = false;
            }
        }

        private ObservableCollection<PriceViewModel> CreateItems()
        {
            return
                new ObservableCollection<PriceViewModel>(
                    _workspace.All<MenuItem>()
                    .SelectMany(y => y.Portions, (mi, pt) => new PriceViewModel(pt, mi.Name))
                );
        }

        protected override string GetHeaderInfo()
        {
            return Resources.BatchPriceList;
        }

        public override Type GetViewType()
        {
            return typeof(PriceListView);
        }
    }
}
