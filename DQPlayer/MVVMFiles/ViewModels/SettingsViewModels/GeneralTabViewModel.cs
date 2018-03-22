using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DQPlayer.Annotations;
using DQPlayer.Helpers.LocalizationManagement;
using DQPlayer.MVVMFiles.Commands;
using WPF.Themes;

namespace DQPlayer.MVVMFiles.ViewModels.SettingsViewModels
{
    public class GeneralTabViewModel : INotifyPropertyChanged
    {
        public RelayCommand<SelectionChangedEventArgs> LanguageSelectionCommand { get; }
        public RelayCommand<SelectionChangedEventArgs> ThemeSelectionCommand { get; }

        public ObservableCollection<CultureInfo> AvailableLanguages { get; set; }

        public GeneralTabViewModel()
        {
            AvailableLanguages = new ObservableCollection<CultureInfo>
            {
                new CultureInfo("en-GB"),
                new CultureInfo("bg-BG"),   
            };

            LanguageSelectionCommand = new RelayCommand<SelectionChangedEventArgs>(OnLanugageSelectionChanged);
            ThemeSelectionCommand = new RelayCommand<SelectionChangedEventArgs>(OnThemeSelectionChanged);
        }

        private void OnLanugageSelectionChanged(SelectionChangedEventArgs e)
        {
            TranslationSource.Instance.CurrentCulture = (CultureInfo) e.AddedItems[0];
        }

        private void OnThemeSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string theme = e.AddedItems[0].ToString();
                Application.Current.ApplyTheme(theme);
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
