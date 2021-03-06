using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using DQPlayer.Annotations;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.Helpers.FileManagement.FileInformation
{
    public class MediaFileInformation : DependencyObject, IFileInformation, INotifyPropertyChanged, IEquatable<MediaFileInformation>
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

        public MediaFileInformation([NotNull] string filePath)
            : this(new Uri(filePath))
        {
        }

        public MediaFileInformation([NotNull] Uri fileUri)
        {
            Uri = fileUri ?? throw new ArgumentNullException(nameof(fileUri));
            FileInfo = new FileInfo(fileUri.OriginalString);
            FileName = Path.GetFileNameWithoutExtension(FileInfo.Name);
            FileLength = FileInfo.GetFileDuration();
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Equality members

        public bool Equals(MediaFileInformation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FileLength.Equals(other.FileLength) && string.Equals(FileName, other.FileName) &&
                   Equals(FileInfo, other.FileInfo) && Equals(Uri, other.Uri);
        }

        #endregion
    }
}