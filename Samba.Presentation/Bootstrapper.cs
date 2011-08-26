using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.ServiceLocation;
using Samba.Infrastructure.Settings;
using Samba.Localization.Engine;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation
{
    public class Bootstrapper : MefBootstrapper
    {
        private readonly EntLibLoggerAdapter _logger = new EntLibLoggerAdapter();

        protected override DependencyObject CreateShell()
        {
            return Container.GetExportedValue<Shell>();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();
            var path = System.IO.Path.GetDirectoryName(Application.ResourceAssembly.Location);
            if (path != null)
            {
                AggregateCatalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Login*"));
                AggregateCatalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Modules*"));
                AggregateCatalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Presentation*"));
                AggregateCatalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Services.dll"));
            }
            LocalSettings.AppPath = path;
        }

        protected override ILoggerFacade CreateLogger()
        {
            return _logger;
        }

        protected override void InitializeModules()
        {
            var moduleManager = Container.GetExportedValue<IModuleManager>();
            moduleManager.LoadModule("DashboardModule");
            moduleManager.LoadModule("NavigationModule");

            base.InitializeModules();

            var moduleInitializationService = ServiceLocator.Current.GetInstance<IModuleInitializationService>();
            moduleInitializationService.Initialize();
        }

        protected override void InitializeShell()
        {
            LocalizeDictionary.ChangeLanguage(LocalSettings.CurrentLanguage);

            InteractionService.UserIntraction = ServiceLocator.Current.GetInstance<IUserInteraction>();
            InteractionService.UserIntraction.ToggleSplashScreen();

            AppServices.MainDispatcher = Application.Current.Dispatcher;
            var creationService = new DataCreationService();

            AppServices.MessagingService.RegisterMessageListener(new MessageListener());

            if (LocalSettings.StartMessagingClient)
                AppServices.MessagingService.StartMessagingClient();

            PresentationServices.Initialize();

            base.InitializeShell();

            TriggerService.UpdateCronObjects();

            try
            {
                creationService.CreateData();
                Application.Current.MainWindow = (Shell)Shell;
                Application.Current.MainWindow.Show();
                InteractionService.UserIntraction.ToggleSplashScreen();
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(LocalSettings.ConnectionString))
                {
                    var connectionString =
                        InteractionService.UserIntraction.GetStringFromUser(
                        "Connection String",
                        "Şu anki bağlantı ayarları ile veri tabanına bağlanılamıyor. Lütfen aşağıdaki bağlantı bilgisini kontrol ederek tekrar deneyiniz.\r\r" +
                        "Hata Mesajı:\r" + e.Message,
                        LocalSettings.ConnectionString);

                    var cs = String.Join(" ", connectionString);

                    if (!string.IsNullOrEmpty(cs))
                        LocalSettings.ConnectionString = cs.Trim();

                    AppServices.LogError(e, "Programı yeniden başlatınız. Mevcut problem log dosyasına kaydedildi.");
                }
                else
                {
                    AppServices.LogError(e);
                    LocalSettings.ConnectionString = "";
                }
                LocalSettings.SaveSettings();
                Environment.Exit(1);
            }
        }
    }
}
