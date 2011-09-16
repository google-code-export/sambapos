using System;
using System.Collections.Generic;
using System.Windows.Input;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Settings;

namespace Samba.Presentation.ViewModels
{
    public class ScreenMenuItemButton
    {
        private readonly ICommand _command;
        private readonly ScreenMenuCategory _category;
        private readonly ScreenMenuItem _screenMenuItem;

        public ScreenMenuItemButton(ScreenMenuItem screenMenuItem, ICommand command, ScreenMenuCategory category)
        {
            _screenMenuItem = screenMenuItem;
            _command = command;
            _category = category;

            var color = screenMenuItem.ButtonColor;

            if (string.IsNullOrEmpty(color))
                color = category != null ? category.ButtonColor : "Green";

            ButtonColor = color;
        }

        public ScreenMenuItem ScreenMenuItem
        {
            get { return _screenMenuItem; }
        }

        public string Caption
        {
            get
            {
                if (Category.WrapText)
                {
                    if (!_screenMenuItem.Name.Contains("\r")) return _screenMenuItem.Name.Replace(' ', '\r');
                }
                return _screenMenuItem.Name.Replace("\\r", "\r");
            }
        }
        public ICommand Command { get { return _command; } }
        public ScreenMenuCategory Category { get { return _category; } }
        public double ButtonHeight { get { return Category.ButtonHeight > 0 ? Category.ButtonHeight : double.NaN; } }
        public string ButtonColor { get; private set; }
        public string ImagePath { get { return !string.IsNullOrEmpty(ScreenMenuItem.ImagePath) ? ScreenMenuItem.ImagePath : LocalSettings.AppPath + "\\images\\empty.png"; } }

        public int FindOrder(string t)
        {
            if (Caption.ToLower().StartsWith(t.ToLower())) return -99 + Caption.Length;
            return t.Length == 1 ? Caption.Length : Distance(Caption, t);
        }

        public static Int32 Distance2(String source, String target)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target))
                {
                    return 0;
                }
                else
                {
                    return target.Length;
                }
            }
            else if (String.IsNullOrEmpty(target))
            {
                return source.Length;
            }

            Int32 m = source.Length;
            Int32 n = target.Length;
            Int32[,] H = new Int32[m + 2, n + 2];

            Int32 INF = m + n;
            H[0, 0] = INF;
            for (Int32 i = 0; i <= m; i++) { H[i + 1, 1] = i; H[i + 1, 0] = INF; }
            for (Int32 j = 0; j <= n; j++) { H[1, j + 1] = j; H[0, j + 1] = INF; }

            SortedDictionary<Char, Int32> sd = new SortedDictionary<Char, Int32>();
            foreach (Char Letter in (source + target))
            {
                if (!sd.ContainsKey(Letter))
                    sd.Add(Letter, 0);
            }

            for (Int32 i = 1; i <= m; i++)
            {
                Int32 DB = 0;
                for (Int32 j = 1; j <= n; j++)
                {
                    Int32 i1 = sd[target[j - 1]];
                    Int32 j1 = DB;

                    if (source[i - 1] == target[j - 1])
                    {
                        H[i + 1, j + 1] = H[i, j];
                        DB = j;
                    }
                    else
                    {
                        H[i + 1, j + 1] = Math.Min(H[i, j], Math.Min(H[i + 1, j], H[i, j + 1])) + 1;
                    }

                    H[i + 1, j + 1] = Math.Min(H[i + 1, j + 1], H[i1, j1] + (i - i1 - 1) + 1 + (j - j1 - 1));
                }

                sd[source[i - 1]] = i;
            }

            return H[m + 1, n + 1];
        }

        public static int Distance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

    }
}
