using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
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
            SaveSettingsCommand = new CaptionCommand<string>(Resources.Save, OnSaveSettings);
            StartMessagingServerCommand = new CaptionCommand<string>(Resources.StartClientNow, OnStartMessagingServer, CanStartMessagingServer);
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

        public string Language
        {
            get { return LocalSettings.CurrentLanguage; }
            set { LocalSettings.CurrentLanguage = value; }
        }

        public bool OverrideLanguage
        {
            get { return LocalSettings.OverrideLanguage; }
            set
            {
                LocalSettings.OverrideLanguage = value;
                if (!value) OverrideWindowsRegionalSettings = false;
                RaisePropertyChanged("OverrideLanguage");
            }
        }

        public bool OverrideWindowsRegionalSettings
        {
            get { return LocalSettings.OverrideWindowsRegionalSettings; }
            set
            {
                LocalSettings.OverrideWindowsRegionalSettings = value;
                RaisePropertyChanged("OverrideWindowsRegionalSettings");
            }
        }

        private IEnumerable<string> _terminalNames;
        public IEnumerable<string> TerminalNames
        {
            get { return _terminalNames ?? (_terminalNames = Dao.Distinct<Terminal>(x => x.Name)); }
        }

        private IEnumerable<CultureInfo> _supportedLanguages;
        public IEnumerable<CultureInfo> SupportedLanguages
        {
            get { return _supportedLanguages ?? (_supportedLanguages = GetSupportedLanguages()); }
        }

        private static IEnumerable<CultureInfo> GetSupportedLanguages()
        {
            var result = new List<CultureInfo>();
            foreach (var localSetting in LocalSettings.SupportedLanguages)
            {
                var ci = CultureInfo.GetCultureInfo(localSetting);

                result.Add(ci);
            }
            return result;
        }

        protected override string GetHeaderInfo()
        {
            return Resources.ProgramSettings;
        }

        public override Type GetViewType()
        {
            return typeof(SettingsView);
        }
    }
}
