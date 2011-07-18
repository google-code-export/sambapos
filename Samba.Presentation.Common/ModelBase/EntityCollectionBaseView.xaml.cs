using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace Samba.Presentation.Common.ModelBase
{
    /// <summary>
    /// Interaction logic for EntityCollectionBaseView.xaml
    /// </summary>
    public partial class EntityCollectionBaseView : UserControl
    {
        public EntityCollectionBaseView()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var bm = (DataContext as AbstractEntityCollectionViewModelBase);
            if (bm != null && bm.EditItemCommand.CanExecute(null))
                bm.EditItemCommand.Execute(null);
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var baseModelView = DataContext as AbstractEntityCollectionViewModelBase;
            if (baseModelView != null && baseModelView.CustomCommands.Count > 0)
            {
                MainListBox.ContextMenu.Items.Add(new Separator());
                foreach (var item in (DataContext as AbstractEntityCollectionViewModelBase).CustomCommands)
                {
                    MainListBox.ContextMenu.Items.Add(
                        new MenuItem { Command = item, Header = item.Caption });
                }
            }
        }

        private void MainListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((DataContext as AbstractEntityCollectionViewModelBase).EditItemCommand.CanExecute(null))
                    (DataContext as AbstractEntityCollectionViewModelBase).EditItemCommand.Execute(null);
            }
        }
    }
}
