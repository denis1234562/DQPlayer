using DQPlayer.MVVMFiles.Models.PlayList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace DQPlayer.MVVMFiles.Converters
{
    [ValueConversion(typeof(ListViewItem), typeof(FileInformation))]
    public class TestConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
           System.Globalization.CultureInfo culture)
        {
            return (FileInformation)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return (ListViewItem)value;
        }
    }
}
