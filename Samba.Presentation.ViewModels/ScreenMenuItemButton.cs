using System.Windows.Input;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Settings;

namespace Samba.Presentation.ViewModels
{
    public class ScreenMenuItemButton
    {
        private readonly ICommand _command;
        private readonly ScreenMenuCategory _category;
        private readonly ScreenMenuItem _screenMenuItem;

        public ScreenMenuItemButton(ScreenMenuItem screenMenuItem, ICommand command, ScreenMenuCategory category)
        {
            _screenMenuItem = screenMenuItem;
            _command = command;
            _category = category;

            var color = screenMenuItem.ButtonColor;

            if (string.IsNullOrEmpty(color))
                color = category != null ? category.ButtonColor : "Green";

            ButtonColor = color;
        }

        public ScreenMenuItem ScreenMenuItem
        {
            get { return _screenMenuItem; }
        }

        public string Caption
        {
            get
            {
                if (Category.WrapText)
                {
                    if (!_screenMenuItem.Name.Contains("\r")) return _screenMenuItem.Name.Replace(' ', '\r');
                }
                return _screenMenuItem.Name.Replace("\\r", "\r");
            }
        }
        public ICommand Command { get { return _command; } }
        public ScreenMenuCategory Category { get { return _category; } }
        public double ButtonHeight { get { return Category.ButtonHeight > 0 ? Category.ButtonHeight : double.NaN; } }
        public string ButtonColor { get; private set; }
        public string ImagePath { get { return !string.IsNullOrEmpty(ScreenMenuItem.ImagePath) ? ScreenMenuItem.ImagePath : LocalSettings.AppPath + "\\images\\empty.png"; } }
    }
}
