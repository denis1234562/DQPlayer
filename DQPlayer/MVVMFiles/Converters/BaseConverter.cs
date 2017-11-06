﻿using System;
using System.Windows.Markup;

namespace DQPlayer.MVVMFiles.Converters
{
    public abstract class BaseConverter : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
