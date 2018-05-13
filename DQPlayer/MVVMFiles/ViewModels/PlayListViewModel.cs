using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Collections;
using System.Windows.Input;
using DQPlayer.Annotations;
using System.ComponentModel;
using DQPlayer.MVVMFiles.Views;
using System.Collections.Generic;
using DQPlayer.Helpers.Extensions;
using DQPlayer.MVVMFiles.Commands;
using System.Collections.Specialized;
using DQPlayer.Helpers.DialogHelpers;
using System.Runtime.CompilerServices;
using DQPlayer.Helpers.FileManagement;
using DQPlayer.Helpers.InputManagement;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class PlaylistViewModel : INotifyPropertyChanged
    {
        public RelayCommand<DragEventArgs> ListViewFileDrop { get; }
        public RelayCommand<MouseButtonEventArgs> ListViewDoubleClickCommand { get; }
        public RelayCommand ClearAllCommand { get; }
        public RelayCommand<IList> RemoveCommand { get; }
        public RelayCommand BrowseCommand { get; }
        public RelayCommand<CancelEventArgs> WindowClosingCommand { get; }

        public ObservableCircularList<MediaFileInformation> FilesCollection { get; set; }

        private delegate void PlaylistActionDelegate(ObservableCircularList<MediaFileInformation> files);

        private readonly Dictionary<PlaylistAction, PlaylistActionDelegate> _playlistActions;

        public PlaylistViewModel()
        {
            FilesCollection = new ObservableCircularList<MediaFileInformation>();
            ListViewFileDrop = new RelayCommand<DragEventArgs>(OnListViewFileDropCommand);
            ListViewDoubleClickCommand = new RelayCommand<MouseButtonEventArgs>(OnListViewDoubleClickCommand);
            RemoveCommand = new RelayCommand<IList>(OnRemoveCommand);
            ClearAllCommand = new RelayCommand(OnClearAllCommand);
            BrowseCommand = new RelayCommand(OnBrowseCommand);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(OnWindowClosingCommand);

            _playlistActions = new Dictionary<PlaylistAction, PlaylistActionDelegate>
            {
                [PlaylistAction.PlayNext] = files => RequestNewFiles(files.Next, callback: FileManagerCallback),
                [PlaylistAction.PlayPrevious] = files => RequestNewFiles(files.Previous, callback: FileManagerCallback),
            };

            FilesCollection.CollectionChanged += FilesCollection_CollectionChanged;

            FileManager<MediaFileInformation>.Instance.Notify += FileManager_OnNewRequest;
            PlaylistManager.Instance.Notify += PlaylistManager_OnNewRequest;
        }

        private void PlaylistManager_OnNewRequest(object sender, PlaylistManagerEventArgs e) =>
            _playlistActions[e.PlaylistAction].Invoke(FilesCollection);

        private void FilesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(FilesCollection));

        private void OnWindowClosingCommand(CancelEventArgs e)
        {
            e.Cancel = true;
            WindowDialogHelper<PlaylistView>.Instance.Hide();
        }

        private void OnListViewFileDropCommand(DragEventArgs e)
        {
            if (FileDropHandler.ExtractDroppedFiles(e, Settings.MediaExtensionsPackage, out var filesInformation))
            {
                FilesCollection.AddRange(filesInformation.Cast<MediaFileInformation>());
            }
        }

        private void OnBrowseCommand()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = Settings.MediaExtensionPackageFilter.Filter,
                Multiselect = true
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                FilesCollection.AddRange(fileDialog.FileNames.Select(t => new MediaFileInformation(t)));
            }
        }

        private void OnListViewDoubleClickCommand(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left &&
                ((FrameworkElement)e.OriginalSource).DataContext is MediaFileInformation item)
            {
                RequestNewFiles(item, callback: FileManagerCallback);
            }
        }

        private void OnClearAllCommand()
        {
            FilesCollection.Clear();
            RequestNewFiles(new MediaFileInformation[] { null });
        }

        private void OnRemoveCommand(IList selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }
            var lastPlayedFile = FilesCollection.Current;
            while (selectedItems.Count > 0)
            {
                FilesCollection.Remove((MediaFileInformation)selectedItems[0]);
            }
            if (!lastPlayedFile.Equals(FilesCollection.Current))
            {
                RequestNewFiles(new MediaFileInformation[] { null });
            }
        }

        private void FileManager_OnNewRequest(object sender, FileManagerEventArgs<MediaFileInformation> e)
        {
            var firstMediaFile = e.SelectedFiles.First();
            if (firstMediaFile == null)
            {
                if (FilesCollection.Current != null)
                {
                    FilesCollection.Current.IsPlaying = false;
                }
                return;
            }
            if (!sender.Equals(this))
            {
                FilesCollection.AddRange(e.SelectedFiles);
            }
            ChangePlayingFile(firstMediaFile);
        }

        private void ChangePlayingFile(MediaFileInformation file, bool newFileState = true)
        {
            if (FilesCollection.Current != null)
            {
                FilesCollection.Current.IsPlaying = false;
            }
            FilesCollection.SetCurrent(FilesCollection.IndexOf(file));
            FilesCollection.Current.IsPlaying = newFileState;
        }

        private void RequestNewFiles(
            MediaFileInformation file,
            object originalSource = null,
            FileManager<MediaFileInformation>.FileManagerCallback callback = null)
        {
            RequestNewFiles(file.AsEnumerable(), originalSource, callback);
        }

        private void RequestNewFiles(
            IEnumerable<MediaFileInformation> files,
            object originalSource = null,
            FileManager<MediaFileInformation>.FileManagerCallback callback = null)
        {
            FileManager<MediaFileInformation>.Instance.Request(this,
                new FileManagerEventArgs<MediaFileInformation>(files, originalSource, callback));
        }

        private void FileManagerCallback(MediaFileInformation currentPlayingFile, bool repeatState)
        {
            if (!FilesCollection.Last().Equals(FilesCollection.Current) || repeatState)
            {
                RequestNewFiles(FilesCollection.Next, callback: FileManagerCallback);
            }
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
