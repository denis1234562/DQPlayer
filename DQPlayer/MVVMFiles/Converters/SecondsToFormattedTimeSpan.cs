﻿using System;
using System.Windows.Data;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.MVVMFiles.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class SecondsToFormattedTimeSpan : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return TimeSpan.FromSeconds((double) value).ToShortString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}