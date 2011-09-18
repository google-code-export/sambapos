using System.ComponentModel;
using System.Windows.Media;
using Samba.Domain.Models.Menus;
using Samba.Localization;

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

        [LocalizedDisplayName(ResourceStrings.Product)]
        public string MenuItemDisplayString
        {
            get { return Name; }
        }

        [LocalizedDisplayName(ResourceStrings.SortOrder)]
        public int Order
        {
            get { return Model.Order; }
            set { Model.Order = value; }
        }

        [LocalizedDisplayName(ResourceStrings.AutoSelect)]
        public bool AutoSelect
        {
            get { return Model.AutoSelect; }
            set { Model.AutoSelect = value; }
        }

        [LocalizedDisplayName(ResourceStrings.Color)]
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

        [LocalizedDisplayName(ResourceStrings.ImagePath)]
        public string ImagePath { get { return Model.ImagePath; } set { Model.ImagePath = value; } }

        [LocalizedDisplayName(ResourceStrings.Header)]
        public string Name { get { return Model.Name; } set { Model.Name = value; } }

        [LocalizedDisplayName(ResourceStrings.Quantity)]
        public int Quantity { get { return Model.Quantity; } set { Model.Quantity = value; } }

        [LocalizedDisplayName(ResourceStrings.Gift)]
        public bool Gift { get { return Model.Gift; } set { Model.Gift = value; } }

        [LocalizedDisplayName(ResourceStrings.DefaultProperties)]
        public string DefaultProperties { get { return Model.DefaultProperties; } set { Model.DefaultProperties = value; } }

        [LocalizedDisplayName(ResourceStrings.Tag)]
        public string Tag { get { return Model.Tag; } set { Model.Tag = value; } }

        [LocalizedDisplayName(ResourceStrings.Portion)]
        public string Portion { get { return Model.ItemPortion; } set { Model.ItemPortion = value; } }
    }
}
