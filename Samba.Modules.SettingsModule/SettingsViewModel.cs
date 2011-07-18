using System;
using System.Collections.Generic;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    public class SettingsViewModel : VisibleViewModelBase
    {
        public SettingsViewModel()
        {
            SaveSettingsCommand = new CaptionCommand<string>("Kaydet", OnSaveSettings);
            StartMessagingServerCommand = new CaptionCommand<string>("İstemciyi Şimdi Başlat", OnStartMessagingServer, CanStartMessagingServer);
        }

        private static bool CanStartMessagingServer(string arg)
        {
            return AppServices.MessagingService.CanStartMessagingClient();
        }

        private static void OnStartMessagingServer(string obj)
        {
            AppServices.MessagingService.StartMessagingClient();
        }

        private void OnSaveSettings(string obj)
        {
            LocalSettings.SaveSettings();
            ((VisibleViewModelBase)this).PublishEvent(EventTopicNames.ViewClosed);
        }

        public ICaptionCommand SaveSettingsCommand { get; set; }
        public ICaptionCommand StartMessagingServerCommand { get; set; }

        public string TerminalName
        {
            get { return LocalSettings.TerminalName; }
            set { LocalSettings.TerminalName = value; }
        }

        public string ConnectionString
        {
            get { return LocalSettings.ConnectionString; }
            set { LocalSettings.ConnectionString = value; }
        }

        public string MessagingServerName
        {
            get { return LocalSettings.MessagingServerName; }
            set { LocalSettings.MessagingServerName = value; }
        }

        public int MessagingServerPort
        {
            get { return LocalSettings.MessagingServerPort; }
            set { LocalSettings.MessagingServerPort = value; }
        }

        public bool StartMessagingClient
        {
            get { return LocalSettings.StartMessagingClient; }
            set { LocalSettings.StartMessagingClient = value; }
        }

        private IEnumerable<string> _terminalNames;
        public IEnumerable<string> TerminalNames
        {
            get { return _terminalNames ?? (_terminalNames = Dao.Distinct<Terminal>(x => x.Name)); }
        }

        protected override string GetHeaderInfo()
        {
            return "Program Ayarları";
        }

        public override Type GetViewType()
        {
            return typeof(SettingsView);
        }
    }
}
