using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common;

namespace Samba.Modules.MenuModule
{
    public enum NumeratorType
    {
        Yok,
        Küçük,
        Büyük
    }

    public class ScreenMenuCategoryViewModel : ObservableObject
    {
        public ScreenMenuCategoryViewModel(ScreenMenuCategory model)
        {
            Model = model;
        }

        [Browsable(false)]
        public ScreenMenuCategory Model { get; private set; }

        [Browsable(false)]
        public string CategoryListDisplay { get { return ScreenMenuItems.Count > 0 ? string.Format("{0} ({1})", Name, ScreenMenuItems.Count) : Name; } }

        [Browsable(false)]
        public IList<ScreenMenuItem> ScreenMenuItems { get { return Model.ScreenMenuItems; } }

        [DisplayName("Kategori Adı"), Category("Ana Özellikler")]
        public string Name { get { return Model.Name; } set { Model.Name = value; RaisePropertyChanged("Name"); } }

        [DisplayName("Hızlı Menü"), Category("Ana Özellikler")]
        public bool MostUsedItemsCategory { get { return Model.MostUsedItemsCategory; } set { Model.MostUsedItemsCategory = value; } }

        [DisplayName("Düğme Yüksekliği"), Category("Ana Özellikler")]
        public int MainButtonHeight { get { return Model.MButtonHeight; } set { Model.MButtonHeight = value; } }

        [DisplayName("Düğme Rengi"), Category("Ana Özellikler")]
        public SolidColorBrush MainButtonColor
        {
            get
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Model.MButtonColor));
            }
            set
            {
                Model.MButtonColor = value != Brushes.Transparent ? value.ToString() : string.Empty;
            }
        }

        [DisplayName("Resim Yolu"), Category("Ana Özellikler")]
        public string ImagePath { get { return Model.ImagePath; } set { Model.ImagePath = value; } }
        
        [DisplayName("Sütun Sayısı"), Category("Alt Menü Özellikleri")]
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }

        [DisplayName("Düğme Yüksekliği"), Category("Alt Menü Özellikleri")]
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }

        [DisplayName("Sayfa Sayısı"), Category("Alt Menü Özellikleri")]
        public int PageCount { get { return Model.PageCount; } set { Model.PageCount= value; } }

        [DisplayName("Etiket Kaydır"), Category("Alt Menü Özellikleri")]
        public bool WrapText { get { return Model.WrapText; } set { Model.WrapText = value; } }

        [DisplayName("Düğme Rengi"), Category("Alt Menü Özellikleri")]
        public SolidColorBrush ButtonColor
        {
            get
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Model.ButtonColor));
            }
            set
            {
                Model.ButtonColor = value != Brushes.Transparent ? value.ToString() : string.Empty;
            }
        }

        [DisplayName("Numeratör Tipi"), Category("Numeratör Özellikleri")]
        public NumeratorType NumeratorType { get { return (NumeratorType)Model.NumeratorType; } set { Model.NumeratorType = (int)value; } }

        [DisplayName("Numeratör Değeri"), Category("Numeratör Özellikleri")]
        public string NumeratorValues { get { return Model.NumeratorValues; } set { Model.NumeratorValues = value; } }

        [DisplayName("Alfanümerik Düğme Değeri"), Category("Numeratör Özellikleri")]
        public string AlphaButtonValues { get { return Model.AlphaButtonValues; } set { Model.AlphaButtonValues = value; } }

        internal void UpdateDisplay()
        {
            RaisePropertyChanged("CategoryListDisplay");
        }

        public bool ContainsMenuItem(MenuItem item)
        {
            return ScreenMenuItems.Where(x => x.MenuItemId == item.Id).Count() > 0;
        }
    }
}
