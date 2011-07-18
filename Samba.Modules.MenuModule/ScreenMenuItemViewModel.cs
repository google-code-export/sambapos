using System.ComponentModel;
using System.Windows.Media;
using Samba.Domain.Models.Menus;

namespace Samba.Modules.MenuModule
{
    public class ScreenMenuItemViewModel
    {
        public ScreenMenuItemViewModel(ScreenMenuItem model)
        {
            Model = model;
        }

        [Browsable(false)]
        public ScreenMenuItem Model { get; private set; }

        [DisplayName("Ürün")]
        public string MenuItemDisplayString
        {
            get { return Name; }
        }

        [DisplayName("Sıra")]
        public int Order
        {
            get { return Model.Order; }
            set { Model.Order = value; }
        }

        [DisplayName("Oto Seç")]
        public bool AutoSelect
        {
            get { return Model.AutoSelect; }
            set { Model.AutoSelect = value; }
        }

        [DisplayName("Renk")]
        public SolidColorBrush ButtonColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Model.ButtonColor))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Model.ButtonColor));
                return Brushes.Transparent;
            }
            set
            {
                Model.ButtonColor = value != Brushes.Transparent ? value.ToString() : string.Empty;
            }
        }

        [DisplayName("Resim Yolu")]
        public string ImagePath { get { return Model.ImagePath; } set { Model.ImagePath = value; } }

        [DisplayName("Başlık")]
        public string Name { get { return Model.Name; } set { Model.Name = value; } }

        [DisplayName("Miktar")]
        public int Quantity { get { return Model.Quantity; } set { Model.Quantity = value; } }

        [DisplayName("İkram")]
        public bool Gift { get { return Model.Gift; } set { Model.Gift = value; } }

        [DisplayName("Varsayılan Özellikler")]
        public string DefaultProperties { get { return Model.DefaultProperties; } set { Model.DefaultProperties = value; } }
    }
}
