﻿using DQPlayer.Annotations;
using DQPlayer.Helpers.Extensions;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace DQPlayer.MVVMFiles.Models.PlayList
{
    public class FileInformation : DependencyObject , INotifyPropertyChanged
    {
        public string Title { get; }
        public TimeSpan Time { get; }
        public Uri FilePath { get; }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(FileInformation),
                new PropertyMetadata(null));

        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set
            {
                SetValue(IsPlayingProperty, value);
                OnPropertyChanged();
            }
        } 

        public FileInformation(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentException(nameof(uri));
            }
            Time = uri.AbsolutePath.GetFileDuration();
            FilePath = uri;
            Title = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}