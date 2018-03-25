using DQPlayer.Helpers.FileManagement;
using DQPlayer.Helpers.InputManagement;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.ResourceFiles;
using System;
using System.Windows;
using System.Windows.Input;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class MainWindowViewModel
    {
        public event Action WindowFullScreen;
        public event Action WindowNormalize;

        public bool IsFullScreen { get; private set; }

        public RelayCommand<DragEventArgs> FileDropCommand { get; }

        public MainWindowViewModel()
        {
            FileDropCommand = new RelayCommand<DragEventArgs>(OnFileDrop);
        }

        public void HandleWindowClick(Window window, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (window.WindowState == WindowState.Normal)
                {
                    SetFullScreen(window);
                }
                else if (window.WindowState == WindowState.Maximized)
                {
                    SetNormalized(window);
                }
            }
        }

        private void SetFullScreen(Window window)
        {
            //TODO maybe not?
            window.Visibility = Visibility.Collapsed;

            window.WindowState = WindowState.Maximized;
            window.WindowStyle = WindowStyle.None;
            window.Topmost = true;
            window.Visibility = Visibility.Visible;

            IsFullScreen = true;
            WindowFullScreen?.Invoke();
        }

        private void SetNormalized(Window window)
        {
            window.WindowState = WindowState.Normal;
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.Topmost = false;

            IsFullScreen = false;
            WindowNormalize?.Invoke();
        }

        private void OnFileDrop(DragEventArgs e)
        {
            if (FileDropHandler.ExtractDroppedItemsUri(e, Settings.MediaPlayerExtensionsPackage, out var files))
            {
                FileManagerHelper.Request(this, files);
                return;
            }
            MessageBox.Show($"{Strings.InvalidFileType}", "Error");
        }
    }
}