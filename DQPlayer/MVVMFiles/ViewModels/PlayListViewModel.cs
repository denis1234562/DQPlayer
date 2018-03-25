using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using DQPlayer.Annotations;
using System.ComponentModel;
using System.Windows.Controls;
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
        public RelayCommand<ListView> RemoveCommand { get; }
        public RelayCommand BrowseCommand { get; }
        public RelayCommand<CancelEventArgs> WindowClosingCommand { get; }

        public ObservableCircularList<MediaFileInformation> FilesCollection { get; set; }

        private delegate MediaFileInformation
            PlaylistActionDelegate(ObservableCircularList<MediaFileInformation> files);

        private static readonly Dictionary<PlaylistAction, PlaylistActionDelegate> _playlistActions =
            new Dictionary<PlaylistAction, PlaylistActionDelegate>
            {
                [PlaylistAction.PlayNext] = files => files.Next,
                [PlaylistAction.PlayPrevious] = files => files.Previous,
            };

        public PlaylistViewModel()
        {
            FilesCollection = new ObservableCircularList<MediaFileInformation>();
            ListViewFileDrop = new RelayCommand<DragEventArgs>(OnListViewFileDropCommand);
            ListViewDoubleClickCommand = new RelayCommand<MouseButtonEventArgs>(OnListViewDoubleClickCommand);
            RemoveCommand = new RelayCommand<ListView>(OnRemoveCommand);
            ClearAllCommand = new RelayCommand(OnClearAllCommand);
            BrowseCommand = new RelayCommand(OnBrowseCommand);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(OnWindowClosingCommand);

            FilesCollection.CollectionChanged += FilesCollection_CollectionChanged;

            FileManager<MediaFileInformation>.Instance.NewRequest += FileManager_OnNewRequest;
            PlaylistManager.Instance.NewRequest += PlaylistManager_OnNewRequest;
        }

        private void PlaylistManager_OnNewRequest(object sender, PlaylistManagerEventArgs e) =>
            FileManager<MediaFileInformation>.Instance.Request(this,
                _playlistActions[e.PlaylistAction].Invoke(FilesCollection).AsEnumerable());

        private void FilesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(FilesCollection));

        private void OnWindowClosingCommand(CancelEventArgs e)
        {
            e.Cancel = true;
            WindowDialogHelper<PlaylistView>.Instance.Hide();
        }

        private void OnListViewFileDropCommand(DragEventArgs e)
        {
            if (FileDropHandler.ExtractDroppedItemsUri(e, Settings.MediaExtensionsPackage, out var filesInformation))
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
                ((FrameworkElement) e.OriginalSource).DataContext is MediaFileInformation item)
            {
                FileManager<MediaFileInformation>.Instance.Request(this, item.AsEnumerable());
            }
        }

        private void OnClearAllCommand()
        {
            FilesCollection.Clear();
            FileManager<MediaFileInformation>.Instance.Request(this, new MediaFileInformation[] { null });
        }

        private void OnRemoveCommand(ListView listView)
        {
            var lastPlayedFile = FilesCollection.Current;
            while(listView.SelectedItems.Count > 0)
            {
                FilesCollection.Remove((MediaFileInformation) listView.SelectedItems[0]);
            }
            if (!lastPlayedFile.Equals(FilesCollection.Current))
            {
                FileManager<MediaFileInformation>.Instance.Request(this, new MediaFileInformation[] { null });
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
