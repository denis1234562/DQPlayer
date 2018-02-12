using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DQPlayer.Helpers.LocalizationManagement;
using WPF.Themes;

namespace DQPlayer.MVVMFiles.UserControls.SettingsTabs
{
    public partial class GeneralTab : UserControl
    {
        public GeneralTab()
        {
            InitializeComponent();
            themes.ItemsSource = ThemeManager.GetThemes();
        }

        private void Themes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string theme = e.AddedItems[0].ToString();
                Application.Current.ApplyTheme(theme);

            }
        }

        private void CbLanguageSelection_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TranslationSource.Instance.CurrentCulture = (CultureInfo)cbLanguageSelection.SelectedItem;
        }
    }
}
