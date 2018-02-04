using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.Extensions;
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
        public event Action<FileInformation> PlayListRemovedItem;
        public event Action<FileInformation> PlayListFileDoubleClicked;

        private  Lazy<RelayCommand<DragEventArgs>> _listViewFileDrop;
        public RelayCommand<DragEventArgs> ListViewFileDrop => _listViewFileDrop.Value;

        private  Lazy<RelayCommand<MouseButtonEventArgs>> _listViewDoubleClickCommand;
        public RelayCommand<MouseButtonEventArgs> ListViewDoubleClickCommand => _listViewDoubleClickCommand.Value;

        private  Lazy<RelayCommand> _clearAllCommand;
        public RelayCommand ClearAllCommand => _clearAllCommand.Value;

        private  Lazy<RelayCommand<ListView>> _removeCommand;
        public RelayCommand<ListView> RemoveCommand => _removeCommand.Value;

        private  Lazy<RelayCommand> _browseCommand;
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

        public PlayListViewModel()
        {
            StartUp();
        }

        public PlayListViewModel(VideoPlayerViewModel videoPlayerViewModel)
        {
            _videoPlayerViewModel = videoPlayerViewModel;
            _videoPlayerViewModel.MediaInputNewFiles += OnMediaInputNewFiles;
            _videoPlayerViewModel.MediaPlayedNewSource += OnMediaPlayedNewSource;
            StartUp();
        }

        private void StartUp()
        {
            _listViewFileDrop = CreateLazyRelayCommand<DragEventArgs>(OnListViewFileDropCommand);
            _listViewDoubleClickCommand = CreateLazyRelayCommand<MouseButtonEventArgs>(OnListViewDoubleClickCommand);
            _removeCommand = CreateLazyRelayCommand<ListView>(OnRemoveCommand);
            _clearAllCommand = CreateLazyRelayCommand(OnClearAllCommand);
            _browseCommand = CreateLazyRelayCommand(OnBrowseCommand);
            FilesCollection = new ObservableCircularList<FileInformation>();
            _filesDuration = new TimeSpan();
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
            if (FileDropHandler.TryExtractDroppedItemsUri(e, Settings.MediaPlayerExtensionPackage, out var uris))
            {
                FilesCollection.AddRange(uris
                    .Where(uri => uri.OriginalString.GetFileExtension() != Settings.SubtitleExtensionString)
                    .Select(uri => new FileInformation(uri)));
                UpdateMoviesDuration();
            }
        }

        private void OnListViewDoubleClickCommand(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                FileInformation item = ((FrameworkElement)e.OriginalSource).DataContext as FileInformation;
                if (item != null)
                {
                    OnPlayListFileDoubleClicked(item);
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
                FileInformation temp = (FileInformation) listView.SelectedItems[0];
                int indexOfSelectedFile = FilesCollection.IndexOf(temp);
                if (FilesCollection.IndexOf(_lastPlayedFile) > indexOfSelectedFile)
                {
                    FilesCollection.MovePrevious();
                }
                FilesCollection.RemoveAt(indexOfSelectedFile);
                OnPlayListRemovedItems(temp);
            }
            UpdateMoviesDuration();
        }

        private void OnBrowseCommand()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = Settings.MediaPlayerExtensionPackageFilter.Filter
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                if (fileDialog.FileName.GetFileExtension() != Settings.SubtitleExtensionString)
                {
                    FilesCollection.Add(new FileInformation(new Uri(fileDialog.FileName)));
                    UpdateMoviesDuration();
                }
            }
        }

        private void OnPlayListRemovedItems(FileInformation file)
        {
            PlayListRemovedItem?.Invoke(file);
        }

        private void OnMediaInputNewFiles(IEnumerable<Uri> files)
        {
            List<Uri> filteredFiles = new List<Uri>(files);
            filteredFiles.RemoveAll(f => f.OriginalString.GetFileExtension() == Settings.SubtitleExtensionString);

            FilesCollection.AddRange(filteredFiles.Select(f => new FileInformation(f)));
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
            duration = FilesCollection.Aggregate(duration, (current, item) => current + item.Time);
            FilesDuration = duration;
        }

        public FileInformation ChangeCurrent(Func<FileInformation> func)
        {
            func.Invoke();
            return FilesCollection.Current;
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

        private void OnPlayListFileDoubleClicked(FileInformation file)
        {
            PlayListFileDoubleClicked?.Invoke(file);
        }
    }
}
