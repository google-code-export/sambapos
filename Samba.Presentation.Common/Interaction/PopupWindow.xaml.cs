using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Samba.Presentation.Common.Interaction
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        public PopupWindow()
        {
            InitializeComponent();
            Height = Application.Current.MainWindow.WindowState == WindowState.Normal
                ? SystemParameters.WorkArea.Bottom
                : SystemParameters.PrimaryScreenHeight-25;
            Width = 250;
            Top = 0;
            Left = SystemParameters.PrimaryScreenWidth - Width;
            Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowStyle();
        }

        private void SetWindowStyle()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            const int gwlExstyle = (-20);
            const int wsExNoactivate = 0x08000000;
            const int wsExToolWindow = 0x00000080;
            NativeWin32.SetWindowLong(hwnd, gwlExstyle, (IntPtr)(wsExNoactivate | wsExToolWindow));
        }
    }
}
