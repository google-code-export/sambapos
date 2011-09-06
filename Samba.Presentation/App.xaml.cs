using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Samba.Infrastructure;
using Samba.Presentation.Common.Services;

namespace Samba.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if (DEBUG)
            RunInDebugMode();
#else
            RunInReleaseMode();
#endif
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (MessagingClient.IsConnected)
                MessagingClient.Disconnect();
            TriggerService.CloseTriggers();
        }

        private static void RunInDebugMode()
        {
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        private static void RunInReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            try
            {
                var bootstrapper = new Bootstrapper();
                bootstrapper.Run();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void HandleException(Exception ex)
        {
            if (ex == null)
                return;

            ExceptionPolicy.HandleException(ex, "Policy");
            MessageBox.Show(Presentation.Properties.Resources.UnhandledException, "Uyarı ");
            Environment.Exit(1);
        }
    }
}
