using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.FileManagement;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.MVVMFiles.Models.PlayList;
using DQPlayer.States;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DQPlayer.Properties;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class PlayListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Lazy<RelayCommand<DragEventArgs>> _listViewFileDrop;
        public RelayCommand<DragEventArgs> ListViewFileDrop => _listViewFileDrop.Value;

        private readonly Lazy<RelayCommand<MouseButtonEventArgs>> _listViewDoubleClickCommand;
        public RelayCommand<MouseButtonEventArgs> ListViewDoubleClickCommand => _listViewDoubleClickCommand.Value;

        private readonly Lazy<RelayCommand> _clearAllCommand;
        public RelayCommand ClearAllCommand => _clearAllCommand.Value;

        private readonly Lazy<RelayCommand<ListView>> _removeCommand;
        public RelayCommand<ListView> RemoveCommand => _removeCommand.Value;

        private readonly Lazy<RelayCommand> _browseCommand;
        public RelayCommand BrowseCommand => _browseCommand.Value;

        public ObservableCircularList<FileInformation> FilesCollection { get; set; }

        private TimeSpan _filesDuration;
        public TimeSpan FilesDuration
        {
            get => _filesDuration;
            set
            {
                _filesDuration = value;
                OnPropertyChanged();
            }
        }

        private readonly VideoPlayerViewModel _videoPlayerViewModel;

        private FileInformation _lastPlayedFile;

        public PlayListViewModel(VideoPlayerViewModel videoPlayerViewModel)
        {
            _videoPlayerViewModel = videoPlayerViewModel;
            _videoPlayerViewModel.MediaInputNewFiles += OnMediaInputNewFiles;
            _videoPlayerViewModel.MediaPlayedNewSource += OnMediaPlayedNewSource;
            FilesCollection = new ObservableCircularList<FileInformation>();
            _listViewFileDrop = CreateLazyRelayCommand<DragEventArgs>(OnListViewFileDropCommand);
            _listViewDoubleClickCommand = CreateLazyRelayCommand<MouseButtonEventArgs>(OnListViewDoubleClickCommand);
            _removeCommand = CreateLazyRelayCommand<ListView>(OnRemoveCommand);
            _clearAllCommand = CreateLazyRelayCommand(OnClearAllCommand);
            _browseCommand = CreateLazyRelayCommand(OnBrowseCommand);
        }

        private static Lazy<RelayCommand> CreateLazyRelayCommand(Action execute, Func<bool> canExecute = null)
          => new Lazy<RelayCommand>(() => new RelayCommand(execute, canExecute));

        private static Lazy<RelayCommand<T>> CreateLazyRelayCommand<T>(Action<T> execute, Func<T, bool> canExecute = null)
            => new Lazy<RelayCommand<T>>(() => new RelayCommand<T>(execute, canExecute));

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void OnListViewFileDropCommand(DragEventArgs e)
        {
            if (FileDropHandler.ExtractDroppedItemsUri(e, Settings.MediaPlayerExtensionPackage, out var filesInformation))
            {
                AddMediaFilesToPlaylist(filesInformation);
            }
        }

        private void OnBrowseCommand()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = Settings.MediaPlayerExtensionPackageFilter.Filter,
                Multiselect = true
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                AddMediaFilesToPlaylist(fileDialog.FileNames.Select(f => new FileInformation(f)));
            }
        }

        private void AddMediaFilesToPlaylist(IEnumerable<FileInformation> files)
        {
            FilesCollection.AddRange(files.Where(file => file.FileInfo.Extension != Settings.SubtitleExtensionString));
            UpdateMoviesDuration();
        }

        private void OnListViewDoubleClickCommand(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (((FrameworkElement)e.OriginalSource).DataContext is FileInformation item)
                {
                    _videoPlayerViewModel.ChangeMediaPlayerSource(item);
                }
            }
        }

        private void OnClearAllCommand()
        {
            FilesCollection.Clear();
            FilesDuration = new TimeSpan();
            if (_videoPlayerViewModel.PlayerSourceState)
            {
                _videoPlayerViewModel.MediaPlayer.SetMediaState(MediaPlayerStates.Stop);
            }
        }

        private void OnRemoveCommand(ListView listView)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a file to remove.");
                return;
            }
            while (listView.SelectedItems.Count != 0)
            {
                int indexOfSelectedFile = FilesCollection.IndexOf((FileInformation)listView.SelectedItems[0]);
                int currentIndex = FilesCollection.IndexOf(_lastPlayedFile);
                FilesCollection.RemoveAt(indexOfSelectedFile);
                if (indexOfSelectedFile == currentIndex)
                {
                    _videoPlayerViewModel.ChangeMediaPlayerSource(FilesCollection.Current);
                }
            }
            UpdateMoviesDuration();
        }

        private void OnMediaInputNewFiles(IEnumerable<FileInformation> files)
        {
            var filteredFiles =
                new List<FileInformation>(files.Where(f => f.FileInfo.Extension != Settings.SubtitleExtensionString));

            FilesCollection.AddRange(filteredFiles);
            if (filteredFiles.Count != 0)
            {
                FilesCollection.SetCurrent(FilesCollection.Count - filteredFiles.Count);
                if (FilesCollection.Count > 1)
                {
                    _videoPlayerViewModel.PlayListView.Show();
                }               
                UpdateMoviesDuration();
            }          
        }

        private void UpdateMoviesDuration()
        {
            TimeSpan duration = new TimeSpan();
            duration = FilesCollection.Aggregate(duration, (current, item) => current + item.FileLength);
            FilesDuration = duration;
        }
      
        private void OnMediaPlayedNewSource(FileInformation file)
        {
            if (_lastPlayedFile != null)
            {
                _lastPlayedFile.IsPlaying = false;
            }
            _lastPlayedFile = file;
            file.IsPlaying = true;
            FilesCollection.SetCurrent(FilesCollection.IndexOf(file));
        }
    }
}
