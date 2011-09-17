using System;
using System.Windows.Input;

namespace Samba.Presentation.ViewModels
{
    public class ScreenSubCategoryButton
    {
        public string Name { get; set; }
        public ICommand Command { get; set; }
        public int Height { get; set; }

        public ScreenSubCategoryButton(string name, ICommand command, int height)
        {
            Name = name;
            Command = command;
            Height = height;
        }
    }
}