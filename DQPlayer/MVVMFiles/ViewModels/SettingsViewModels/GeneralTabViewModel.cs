using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using DQPlayer.Annotations;
using DQPlayer.MVVMFiles.Converters;

namespace DQPlayer.MVVMFiles.ViewModels.SettingsViewModels
{
    public class GeneralTabViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CultureInfo> AvailableLanguages { get; set; }

        public GeneralTabViewModel()
        {
            AvailableLanguages = new ObservableCollection<CultureInfo>
            {
                new CultureInfo("en-GB"),
                new CultureInfo("bg-BG"),   
            };
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SettingsEventArgs : EventArgs
    {

    }

    public class BooleanSetting
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class Test : BaseConverter, IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (BooleanSettings.GetSetting("TestSetting").IsEnabled)
            {
                
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class BooleanSettings
    {
        public static BooleanSetting GetSetting(string name)
        {
            return new BooleanSetting { Name = "TestSetting", IsEnabled = true};
        }
    }
}
