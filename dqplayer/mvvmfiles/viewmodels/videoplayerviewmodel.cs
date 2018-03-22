using System;
using System.Windows;
using System.Windows.Input;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class VideoPlayerViewModel
    {
        public event Action WindowFullScreen;
        public event Action WindowNormalize;

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

            WindowFullScreen?.Invoke();
        }

        private void SetNormalized(Window window)
        {
            window.WindowState = WindowState.Normal;
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.Topmost = false;
            WindowNormalize?.Invoke();
        }
    }
}