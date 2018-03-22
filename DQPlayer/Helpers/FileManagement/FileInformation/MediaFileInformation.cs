using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using DQPlayer.Annotations;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.Helpers.FileManagement.FileInformation
{
    public class MediaFileInformation : DependencyObject, IFileInformation, INotifyPropertyChanged
    {
        public TimeSpan FileLength { get; }
        public string FileName { get; }
        public FileInfo FileInfo { get; }
        public Uri Uri { get; }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(MediaFileInformation),
                new PropertyMetadata(null));

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set
            {
                SetValue(IsPlayingProperty, value);
                OnPropertyChanged();
            }
        }

        public MediaFileInformation(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            FileInfo = new FileInfo(filePath);
            FileName = Path.GetFileNameWithoutExtension(FileInfo.Name);
            FileLength = FileInfo.GetFileDuration();
            Uri = new Uri(FileInfo.FullName);
        }

        public MediaFileInformation(Uri fileUri)
            : this(fileUri.OriginalString)
        {
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}