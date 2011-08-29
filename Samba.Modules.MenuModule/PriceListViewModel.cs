using System;
using System.Collections.Generic;
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

        public IEnumerable<string> PriceTags { get { return Items.SelectMany(x => x.AdditionalPrices.Select(y => y.PriceTag)).Distinct(); } }

        private ObservableCollection<PriceViewModel> CreateItems()
        {
            var result = new ObservableCollection<PriceViewModel>(
                    _workspace.All<MenuItem>(x=>x.Portions.Select(y=>y.Prices))
                    .SelectMany(y => y.Portions, (mi, pt) => new PriceViewModel(pt, mi.Name)));

            var tags = Dao.Select<MenuItemPriceDefinition, string>(x => x.PriceTag, x => x.Id > 0);

            foreach (var tag in tags)
            {
                var portions = result.Where(x => !x.AdditionalPrices.Select(y => y.PriceTag).Contains(tag)).ToList();
                portions.ForEach(x => x.AdditionalPrices.Add(new MenuItemPrice() { PriceTag = tag }));
            }

            return result;
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
