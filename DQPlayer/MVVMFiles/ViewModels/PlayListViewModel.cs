using DQPlayer.Helpers;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.MVVMFiles.Models.PlayList;
using DQPlayer.ResourceFiles;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class PlayListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MovieFiles> MovieFiles { get; set; }

        private readonly VideoPlayerViewModel videoPlayerViewModel;

        private readonly Lazy<RelayCommand> _browseCommand;
        public RelayCommand BrowseCommand => _browseCommand.Value;

        private readonly Lazy<RelayCommand<DragEventArgs>> _windowFileDropCommand;
        public RelayCommand<DragEventArgs> WindowFileDropCommand => _windowFileDropCommand.Value;

        private readonly Lazy<RelayCommand<ListView>> _removeCommand;
        public RelayCommand<ListView> RemoveCommand => _removeCommand.Value;

        private readonly Lazy<RelayCommand> _clearAllCommand;
        public RelayCommand ClearAllCommand => _clearAllCommand.Value;

        public PlayListViewModel(){}

        public PlayListViewModel(VideoPlayerViewModel videoPlayerViewModel)
        {
            this.videoPlayerViewModel = videoPlayerViewModel;
            videoPlayerViewModel.MediaFileDropped += OnMediaFileDropped;
            videoPlayerViewModel.MediaFileBrowsed += OnMediaFileBrowsed;
            _browseCommand = CreateLazyRelayCommand(OnBrowseCommand);
            _windowFileDropCommand = CreateLazyRelayCommand<DragEventArgs>(OnWindowFileDropCommand);
            _removeCommand = CreateLazyRelayCommand<ListView>(OnRemoveCommand);
            _clearAllCommand = CreateLazyRelayCommand(OnClearAllCommand);
            MovieFiles = new ObservableCollection<MovieFiles>();
        }

        private static Lazy<RelayCommand> CreateLazyRelayCommand(Action execute, Func<bool> canExecute = null)
           => new Lazy<RelayCommand>(() => new RelayCommand(execute, canExecute));

        private static Lazy<RelayCommand<T>> CreateLazyRelayCommand<T>(Action<T> execute, Func<T, bool> canExecute = null)
            => new Lazy<RelayCommand<T>>(() => new RelayCommand<T>(execute, canExecute));

        private void OnBrowseCommand()
        {           
            var fileDialog = new OpenFileDialog
            {
                Filter = Settings.MediaPlayerExtensionPackageFilter.Filter
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                OnMediaFileBrowsed(new Uri(fileDialog.FileName));
            }
        }

        private void OnWindowFileDropCommand(DragEventArgs e)
        {
            if (FileDropHandler.TryExtractDroppedItemsUri(e, Settings.MediaPlayerExtensionPackage, out var uris))
            {
                OnMediaFileDropped(uris);
                return;
            }
            MessageBox.Show($"{Strings.InvalidFileType}", "Error");
        }

        private void OnRemoveCommand(ListView obj)
        {
            if (obj.SelectedIndex != -1)
            {
                MovieFiles.RemoveAt(obj.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Select a movie!");
            }         
        }

        private void OnClearAllCommand()
        {
            MovieFiles.Clear();
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void OnMediaFileDropped(IEnumerable<Uri> uris)
        {
            int movieFilesCountBefourChange = MovieFiles.Count;
            foreach (var uri in uris)
            {
                MovieFiles.Add(new MovieFiles(uri));
            }
            if (movieFilesCountBefourChange + 1 < MovieFiles.Count &&
                videoPlayerViewModel.WindowService.PlayListView.Visibility == Visibility.Hidden)
            {
                videoPlayerViewModel.WindowService.PlayListView.Show();
            }
        }

        private void OnMediaFileBrowsed(Uri uri)
        {   
            MovieFiles.Add(new MovieFiles(uri));
        }
    }
}
