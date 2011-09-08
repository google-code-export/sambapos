using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Samba.Infrastructure.Settings
{
    public class SettingsObject
    {
        public int MessagingServerPort { get; set; }
        public string MessagingServerName { get; set; }
        public string TerminalName { get; set; }
        public string ConnectionString { get; set; }
        public bool StartMessagingClient { get; set; }
        public string LogoPath { get; set; }
        public string DefaultHtmlReportHeader { get; set; }
        public string CurrentLanguage { get; set; }
        public bool OverrideLanguage { get; set; }
        public bool OverrideWindowsRegionalSettings { get; set; }

        public SettingsObject()
        {
            MessagingServerPort = 8080;
            ConnectionString = "";
            DefaultHtmlReportHeader =
                @"
<style type='text/css'> 
html
{
  font-family: 'Courier New', monospace;
} 
</style>";
        }
    }

    public static class LocalSettings
    {
        private static SettingsObject _settingsObject;

        public static int Decimals { get { return 2; } }

        public static int MessagingServerPort
        {
            get { return _settingsObject.MessagingServerPort; }
            set { _settingsObject.MessagingServerPort = value; }
        }

        public static string MessagingServerName
        {
            get { return _settingsObject.MessagingServerName; }
            set { _settingsObject.MessagingServerName = value; }
        }

        public static string TerminalName
        {
            get { return _settingsObject.TerminalName; }
            set { _settingsObject.TerminalName = value; }
        }

        public static string ConnectionString
        {
            get { return _settingsObject.ConnectionString; }
            set { _settingsObject.ConnectionString = value; }
        }

        public static bool StartMessagingClient
        {
            get { return _settingsObject.StartMessagingClient; }
            set { _settingsObject.StartMessagingClient = value; }
        }

        public static string LogoPath
        {
            get { return _settingsObject.LogoPath; }
            set { _settingsObject.LogoPath = value; }
        }

        public static string DefaultHtmlReportHeader
        {
            get { return _settingsObject.DefaultHtmlReportHeader; }
            set { _settingsObject.DefaultHtmlReportHeader = value; }
        }

        private static CultureInfo _cultureInfo;
        public static string CurrentLanguage
        {
            get { return _settingsObject.CurrentLanguage; }
            set
            {
                _settingsObject.CurrentLanguage = value;
                if (OverrideLanguage)
                {
                    _cultureInfo = CultureInfo.GetCultureInfo(value);
                    UpdateThreadLanguage();
                }
                SaveSettings();
            }
        }

        public static bool OverrideLanguage
        {
            get { return _settingsObject.OverrideLanguage; }
            set { _settingsObject.OverrideLanguage = value; }
        }

        public static bool OverrideWindowsRegionalSettings
        {
            get { return _settingsObject.OverrideWindowsRegionalSettings; }
            set { _settingsObject.OverrideWindowsRegionalSettings = value; }
        }

        public static string AppPath { get; set; }
        public static string DocumentPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SambaPOS2"; } }

        public static string DataPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Ozgu Tech\\SambaPOS2"; } }
        public static string UserPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ozgu Tech\\SambaPOS2"; } }

        public static string CommonSettingsFileName { get { return DataPath + "\\SambaSettings.txt"; } }
        public static string UserSettingsFileName { get { return UserPath + "\\SambaSettings.txt"; } }

        public static string SettingsFileName { get { return File.Exists(UserSettingsFileName) ? UserSettingsFileName : CommonSettingsFileName; } }

        public static string DefaultCurrencyFormat { get; set; }

        public static int DbVersion { get { return 7; } }
        public static string AppVersion { get { return "2.12"; } }
        public static IList<string> SupportedLanguages { get { return new[] { "en", "tr" }; } }

        public static long CurrentDbVersion { get; set; }

        public static void SaveSettings()
        {
            var serializer = new XmlSerializer(_settingsObject.GetType());
            var writer = new XmlTextWriter(SettingsFileName, null);
            try
            {
                serializer.Serialize(writer, _settingsObject);
            }
            finally
            {
                writer.Close();
            }
        }

        public static void LoadSettings()
        {
            _settingsObject = new SettingsObject();
            string fileName = SettingsFileName;
            if (File.Exists(fileName))
            {
                var serializer = new XmlSerializer(_settingsObject.GetType());
                var reader = new XmlTextReader(fileName);
                try
                {
                    _settingsObject = serializer.Deserialize(reader) as SettingsObject;
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        static LocalSettings()
        {
            if (!Directory.Exists(DocumentPath))
                Directory.CreateDirectory(DocumentPath);
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);
            if (!Directory.Exists(UserPath))
                Directory.CreateDirectory(UserPath);
            LoadSettings();
        }

        public static void UpdateThreadLanguage()
        {
            if (OverrideLanguage && _cultureInfo != null)
            {
                if (OverrideWindowsRegionalSettings)
                    Thread.CurrentThread.CurrentCulture = _cultureInfo;
                Thread.CurrentThread.CurrentUICulture = _cultureInfo;
            }
        }
    }
}
