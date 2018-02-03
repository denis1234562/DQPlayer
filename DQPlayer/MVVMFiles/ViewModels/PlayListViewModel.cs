using DQPlayer.Annotations;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class PlayListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<FileInformation> PlayListRemovedItem;
        public event Action<FileInformation> PlayListFileDoubleClicked;
        public event Action<object> Loaded;

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

        private  Lazy<RelayCommand> _loadedCommand;
        public RelayCommand LoadedCommand => _loadedCommand.Value;

        public ObservableCircularList<FileInformation> FilesCollection { get; set; }

        private TimeSpan _filesDuration;
        public TimeSpan FilesDuration
        {
            get { return _filesDuration; }
            set
            {
                _filesDuration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TestProperty));
            }
        }

        public bool TestProperty
        {
            get
            {
                return _testProperty;
            }
            set
            {
                _testProperty = value;
            }
        }

        private readonly VideoPlayerViewModel _videoPlayerViewModel;

        public PlayListViewModel()
        {
            StartUp();
            TestProperty = true;
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
            _loadedCommand = CreateLazyRelayCommand(OnLoadedCommand);
        }

        private void OnLoadedCommand()
        {
            OnLoaded(this);
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

        private void OnLoaded(object obj)
        {
            Loaded?.Invoke(obj);
        }

        private FileInformation _lastPlayedFile;
        private bool _testProperty = true;

        private void OnListViewFileDropCommand(DragEventArgs e)
        {
            if (FileDropHandler.TryExtractDroppedItemsUri(e, Settings.MediaPlayerExtensionPackage, out var uris))
            {
                foreach (var uri in uris)
                {
                    if (uri.AbsolutePath.GetFileExtension() != ".srt")
                    {
                        FilesCollection.Add(new FileInformation(uri));
                    }
                }
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
            if (listView.SelectedItems.Count != 0)
            {                
                while (listView.SelectedItems.Count != 0)
                {
                    FileInformation temp = (FileInformation)listView.SelectedItems[0];
                    int indexOfSelectedFile = FilesCollection.IndexOf(temp);
                    if (FilesCollection.IndexOf(_lastPlayedFile) > indexOfSelectedFile)
                    {
                        FilesCollection.MovePrevious();
                    }
                    FilesCollection.RemoveAt(indexOfSelectedFile);
                    OnPlayListRemovedItems(temp);
                }
                UpdateMoviesDuration();
                return;
            }           
            MessageBox.Show("Select a File!");
        }

        private void OnBrowseCommand()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = Settings.MediaPlayerExtensionPackageFilter.Filter
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                if (fileDialog.FileName.GetFileExtension() != ".srt")
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
            filteredFiles.RemoveAll(f => f.AbsolutePath.GetFileExtension() == ".srt");

            foreach (var file in filteredFiles)
            {
                FilesCollection.Add(new FileInformation(file));
            }
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
            foreach (var item in FilesCollection)
            {
                duration += item.Time;
            }
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
