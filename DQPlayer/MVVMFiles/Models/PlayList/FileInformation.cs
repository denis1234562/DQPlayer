using DQPlayer.Helpers.Extensions;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using DQPlayer.Properties;

namespace DQPlayer.MVVMFiles.Models.PlayList
{
    public class FileInformation : DependencyObject , INotifyPropertyChanged
    {
        public TimeSpan FileLength { get; }
        public string FileName { get; }
        public FileInfo FileInfo { get; }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(FileInformation),
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

        public FileInformation(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            FileInfo = new FileInfo(filePath);
            FileName = Path.GetFileNameWithoutExtension(FileInfo.Name);
            FileLength = FileInfo.GetFileDuration();
        }

        public FileInformation(Uri fileUri)
            : this(fileUri.OriginalString)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
