﻿using System;
using System.Windows;
using System.Windows.Data;
using DQPlayer.CustomControls;

namespace DQPlayer.MVVMFiles.Converters
{
    [ValueConversion(typeof(double), typeof(TimeSpan))]
    public class SecondsToTimeSpanConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
}