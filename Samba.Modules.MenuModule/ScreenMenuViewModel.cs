using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class ScreenMenuViewModel : EntityViewModelBase<ScreenMenu>
    {
        private IWorkspace _workspace;

        public ScreenMenuViewModel(ScreenMenu model)
            : base(model)
        {
            AddCategoryCommand = new CaptionCommand<string>("Kategori Ekle", OnAddCategory);
            EditCategoryCommand = new CaptionCommand<string>("Kategori Düzenle", OnEditCategory, CanEditCategory);
            DeleteCategoryCommand = new CaptionCommand<string>("Kategori Sil", OnDeleteCategory, CanEditCategory);
            EditCategoryItemsCommand = new CaptionCommand<string>("Kategori Ürünlerini Düzenle", OnEditCategoryItems, CanEditCategory);
            SortCategoryItemsCommand = new CaptionCommand<string>("Kategori Ürünlerini Sırala", OnSortCategoryItems, CanEditCategory);
            SortCategoriesCommand = new CaptionCommand<string>("Kategorileri Sırala", OnSortCategories, CanSortCategories);
            EditCategoryItemPropertiesCommand = new CaptionCommand<string>("Ürün özelliklerini düzenle", OnEditCategoryItemProperties, CanEditCategory);
            EditAllCategoriesCommand = new CaptionCommand<string>("Tüm Kategorileri Düzenle", OnEditAllCategories);
        }

        public ICaptionCommand AddCategoryCommand { get; set; }
        public ICaptionCommand EditCategoryCommand { get; set; }
        public ICaptionCommand EditAllCategoriesCommand { get; set; }
        public ICaptionCommand EditCategoryItemsCommand { get; set; }
        public ICaptionCommand DeleteCategoryCommand { get; set; }
        public ICaptionCommand SortCategoryItemsCommand { get; set; }
        public ICaptionCommand SortCategoriesCommand { get; set; }
        public ICaptionCommand EditCategoryItemPropertiesCommand { get; set; }

        public ObservableCollection<ScreenMenuCategoryViewModel> Categories { get; set; }
        public ScreenMenuCategoryViewModel SelectedCategory { get; set; }

        public override string GetModelTypeString()
        {
            return "Menü";
        }

        public override void Initialize(IWorkspace workspace)
        {
            _workspace = workspace;
            Categories = new ObservableCollection<ScreenMenuCategoryViewModel>(GetCategories(Model));
        }

        public override Type GetViewType()
        {
            return typeof(ScreenMenuView);
        }

        private static IEnumerable<ScreenMenuCategoryViewModel> GetCategories(ScreenMenu baseModel)
        {
            return baseModel.Categories.Select(item => new ScreenMenuCategoryViewModel(item)).OrderBy(x => x.Model.Order).ToList();
        }

        private void OnAddCategory(string value)
        {
            string[] values = InteractionService.UserIntraction.GetStringFromUser("Kategoriler", "Eklemek istediğiniz kategorileri giriniz.");
            foreach (string val in values)
            {
                Categories.Add(new ScreenMenuCategoryViewModel(Model.AddCategory(val)));
            }
            if (values.Count() > 0)
            {
                bool answer = InteractionService.UserIntraction.AskQuestion(
                        "Yeni açtığınız kategorilere uygun ürünler otomatik seçilsin mi?");
                if (answer)
                {
                    foreach (var val in values)
                    {
                        //TODO EF ile çalışırken tolist yapmazsak count sql sorgusu üretiyor mu kontrol et.
                        var menuItems = GetMenuItemsByGroupCode(val).ToList();
                        if (menuItems.Count > 0)
                        {
                            Model.AddItemsToCategory(val, menuItems);
                        }
                    }
                }
            }
        }

        private IEnumerable<MenuItem> GetMenuItemsByGroupCode(string groupCode)
        { return _workspace.All<MenuItem>(x => x.GroupCode == groupCode); }

        private bool CanEditCategory(string value)
        {
            return SelectedCategory != null;
        }

        private void OnEditAllCategories(string obj)
        {
            InteractionService.UserIntraction.EditProperties(Categories);
        }

        private void OnEditCategory(string obj)
        {
            InteractionService.UserIntraction.EditProperties(SelectedCategory);
        }

        private void OnEditCategoryItemProperties(string obj)
        {
            InteractionService.UserIntraction.EditProperties(SelectedCategory.ScreenMenuItems.Select(x => new ScreenMenuItemViewModel(x)).ToList());
        }

        private void OnEditCategoryItems(string value)
        {
            if (SelectedCategory != null)
            {
                IList<IOrderable> values = new List<IOrderable>(_workspace.All<MenuItem>().OrderBy(x => x.GroupCode + x.Name)
                    .Where(x => !SelectedCategory.ContainsMenuItem(x))
                    .Select(x => new ScreenMenuItem { MenuItemId = x.Id, Name = x.Name, MenuItem = x }));

                IList<IOrderable> selectedValues = new List<IOrderable>(SelectedCategory.ScreenMenuItems);

                var choosenValues = InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, "Ürün Listesi",
                    SelectedCategory.Name + " kategorisine eklemek istediğiniz ürünleri seçiniz", "Ürün", "Ürünler");

                foreach (var screenMenuItem in SelectedCategory.ScreenMenuItems.ToList())
                {
                    if (!choosenValues.Contains(screenMenuItem))
                        _workspace.Delete(screenMenuItem);
                }

                SelectedCategory.ScreenMenuItems.Clear();

                foreach (ScreenMenuItem item in choosenValues)
                {
                    SelectedCategory.ScreenMenuItems.Add(item);
                }

                SelectedCategory.UpdateDisplay();
            }
        }

        private void OnDeleteCategory(string value)
        {
            if (MessageBox.Show("Seçili kategori silinsin mi?", "Onay", MessageBoxButton.YesNo) == MessageBoxResult.Yes
                && SelectedCategory != null)
            {
                _workspace.Delete(SelectedCategory.Model);
                Model.Categories.Remove(SelectedCategory.Model);
                Categories.Remove(SelectedCategory);
            }
        }

        private void OnSortCategoryItems(string obj)
        {
            InteractionService.UserIntraction.SortItems(SelectedCategory.ScreenMenuItems.OrderBy(x => x.Order), "Kategori Ürünlerini Sırala",
                SelectedCategory.Name + " kategorisinde bulunan ürünleri sıralamak için mouse ile ürünleri sürükleyip bırakınız.");
        }

        private void OnSortCategories(string obj)
        {
            InteractionService.UserIntraction.SortItems(Model.Categories, "Kategorileri Sırala",
                Model.Name + " menüsünde bulunan kategorileri sıralamak için mouse ile kategorileri sürükleyip bırakınız.");
            Categories = new ObservableCollection<ScreenMenuCategoryViewModel>(Categories.OrderBy(x => x.Model.Order));
            RaisePropertyChanged("Categories");
        }

        private bool CanSortCategories(string arg)
        {
            return Categories.Count > 1;
        }
    }
}
