using System;
using System.Windows;
using System.Windows.Controls;

namespace WPF.Themes
{   
    public static class ThemeManager
    {
        private static string _currentTheme = "Default";

        public static string[] Themes { get; }

        static ThemeManager()
        {
            Themes = new[]
            {
                "Default",
                "ExpressionLight",
                "BureauBlack",
                "BureauBlue",
            };
        }

        public static ResourceDictionary GetThemeResourceDictionary(string theme)
        {
            if (theme != null)
            {
                string packUri = $@" /WPF.Themes;component/{theme}/Theme.xaml";
                return Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
            }
            return null;
        }

        public static void ApplyTheme(this Application app, string theme)
        {
            if (_currentTheme == theme)
            {
                return;
            }
            _currentTheme = theme;

            ResourceDictionary dictionary = GetThemeResourceDictionary(theme);
            if (dictionary != null)
            {
                app.Resources.MergedDictionaries.Clear();
                app.Resources.MergedDictionaries.Add(dictionary);
            }
            
        }

        public static void ApplyTheme(this ContentControl control, string theme)
        {
            ResourceDictionary dictionary = GetThemeResourceDictionary(theme);

            if (dictionary != null)
            {
                control.Resources.MergedDictionaries.Clear();
                control.Resources.MergedDictionaries.Add(dictionary);
            }
        }
    }
}
