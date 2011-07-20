using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Samba.Infrastructure.Settings;
using Samba.Services;

namespace Samba.Login
{
    [Export]
    public class LoginViewModel
    {
        public string LogoPath
        {
            get
            {
                if (File.Exists(LocalSettings.LogoPath))
                    return LocalSettings.LogoPath;
                if (File.Exists(LocalSettings.DocumentPath + "\\Images\\logo.png"))
                    return LocalSettings.DocumentPath + "\\Images\\logo.png";
                if (File.Exists(LocalSettings.AppPath + "\\Images\\logo.png"))
                    return LocalSettings.AppPath + "\\Images\\logo.png";
                return LocalSettings.AppPath + "\\Images\\empty.png";
            }
            set { LocalSettings.LogoPath = value; }
        }

        public string AppLabel { get { return "SAMBA POS " + LocalSettings.AppVersion + " - " + GetDatabaseLabel(); } }
        public string AdminPasswordHint { get { return GetAdminPasswordHint(); } }
        public string SqlHint { get { return GetSqlHint(); } }

        private static string GetSqlHint()
        {
            return !string.IsNullOrEmpty(GetAdminPasswordHint()) 
                ? "Veritabanını SQL ile çalıştırmak ile ilgili bilgi için tıklayın." : "";
        }

        private static string GetDatabaseLabel()
        {
            if (LocalSettings.ConnectionString.ToLower().Contains(".sdf")) return "CE";
            if (LocalSettings.ConnectionString.ToLower().Contains("data source")) return "SQ";
            if (LocalSettings.ConnectionString.ToLower().StartsWith("mongodb://")) return "MG";
            return "TX";
        }

        public static string GetAdminPasswordHint()
        {
            if (GetDatabaseLabel() == "TX"
                && AppServices.MainDataContext.Users.Count() == 1
                && AppServices.MainDataContext.Users.ElementAt(0).PinCode == "1234")
            {
                return "Admin PIN: 1234\rGizlemek için pin kodunu değiştirin";
            }
                
            return "";
        }
    }
}
