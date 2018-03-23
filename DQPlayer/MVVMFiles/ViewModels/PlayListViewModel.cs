using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using DQPlayer.Annotations;
using System.ComponentModel;
using System.Windows.Controls;
using DQPlayer.MVVMFiles.Views;
using DQPlayer.Helpers.Extensions;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.Helpers.DialogHelpers;
using System.Runtime.CompilerServices;
using DQPlayer.Helpers.FileManagement;
using DQPlayer.Helpers.InputManagement;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.FileManagement.FileInformation;
using System.Collections.Specialized;

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

        private MediaFileInformation _lastPlayedFile;

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

            Manager<MediaFileInformation>.NewRequest += OnManagerRequest;
        }

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
            if (e.ChangedButton == MouseButton.Left)
            {
                if (((FrameworkElement) e.OriginalSource).DataContext is MediaFileInformation item)
                {
                    Manager<MediaFileInformation>.Request(this,item.AsEnumerable());
                }
            }
        }

        private void OnClearAllCommand()
        {
            FilesCollection.Clear();
            Manager<MediaFileInformation>.Request(this,new MediaFileInformation[] {null});
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
                MediaFileInformation selectedFile = (MediaFileInformation)listView.SelectedItems[0];
                FilesCollection.Remove(selectedFile);
                if (selectedFile.Equals(FilesCollection.Current))
                {
                    Manager<MediaFileInformation>.Request(this, FilesCollection.Current.AsEnumerable());
                }
            }
        }

        private void OnManagerRequest(object sender, ManagerEventArgs<MediaFileInformation> e)
        {
            OnMediaPlayedNewSource(FilesCollection.Current);
            var f = FilesCollection.Current;
            //null source was requested by system
            if (sender.Equals(this) || e.SelectedFiles.First() == null)
            {
                return;
            }
            int previousSize = FilesCollection.Count;
            FilesCollection.AddRange(e.SelectedFiles);
            FilesCollection.SetCurrent(previousSize);
            if (FilesCollection.Count > 1)
            {
                WindowDialogHelper<PlaylistView>.Instance.Show();
            }       
            
        }
      
        private void OnMediaPlayedNewSource(MediaFileInformation file)
        {
            if (_lastPlayedFile != null)
            {
                _lastPlayedFile.IsPlaying = false;
            }
            _lastPlayedFile = file;
            file.IsPlaying = true;
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
