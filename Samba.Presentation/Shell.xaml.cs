using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Interaction;
using Samba.Services;

namespace Samba.Presentation
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>

    [Export]
    public partial class Shell : Window
    {
        [ImportingConstructor]
        public Shell()
        {
            InitializeComponent();
            LanguageProperty.OverrideMetadata(
                                  typeof(FrameworkElement),
                                  new FrameworkPropertyMetadata(
                                      XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            var selectedIndexChange = DependencyPropertyDescriptor.FromProperty(Selector.SelectedIndexProperty, typeof(TabControl));

            selectedIndexChange.AddValueChanged(MainTabControl, this.MainTabControl_SelectedIndexChanged);

            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.UserLoggedIn) UserLoggedIn(x.Value);
                if (x.Topic == EventTopicNames.UserLoggedOut) UserLoggedOut(x.Value);
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<UserControl>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.DashboardClosed)
                        AppServices.ResetCache();
                });

            UserRegion.Visibility = Visibility.Collapsed;
            RightUserRegion.Visibility = Visibility.Collapsed;
            Height = Properties.Settings.Default.ShellHeight;
            Width = Properties.Settings.Default.ShellWidth;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += timer_Tick;
            timer.Start();
            TimeLabel.Text = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString();

#if !DEBUG
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
#endif
        }

        void timer_Tick(object sender, EventArgs e)
        {
            var time = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString();
            TimeLabel.Text = TimeLabel.Text.Contains(":") ? time.Replace(":", " ") : time;
            if (AppServices.CurrentLoggedInUser != User.Nobody && AppServices.MainDataContext.SelectedTicket == null)
                InteractionService.UserIntraction.DisplayPopups();
        }

        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public void UserLoggedIn(User user)
        {
            UserRegion.Visibility = Visibility.Visible;
            RightUserRegion.Visibility = Visibility.Visible;
        }

        public void UserLoggedOut(User user)
        {
            AppServices.ActiveAppScreen = AppScreens.LoginScreen;
            MainTabControl.SelectedIndex = 0;
            UserRegion.Visibility = Visibility.Collapsed;
            RightUserRegion.Visibility = Visibility.Collapsed;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowStyle != WindowStyle.SingleBorderWindow)
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (AppServices.MainDataContext.SelectedTicket != null)
            {
                e.Cancel = true;
                return;
            }

            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.ShellHeight = Height;
                Properties.Settings.Default.ShellWidth = Width;
            }
            Properties.Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = Title + " [App: " + LocalSettings.AppVersion + "]";
            if (LocalSettings.CurrentDbVersion > 0)
                Title += " [DB: " + LocalSettings.DbVersion + "-" + LocalSettings.CurrentDbVersion + "]";
        }
    }
}
