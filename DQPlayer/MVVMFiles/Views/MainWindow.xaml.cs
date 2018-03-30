using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using DQPlayer.Helpers.DialogHelpers;
using DQPlayer.Helpers.Extensions;
using DQPlayer.MVVMFiles.ViewModels;
using DQPlayer.Helpers.InputManagement;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.MVVMFiles.Views
{
    public partial class MainWindow
    {
        public MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();

            ConfigureUserControls();
        }

        private void ConfigureUserControls()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            WindowDialogHelper<PlaylistView>.Instance.Show();
            WindowDialogHelper<PlaylistView>.Instance.Hide();

            var mediaControlsVM = ucMediaPlayerControls.DataContext as IMediaControlsViewModel;
            var mediaElementVM = ucMediaElement.DataContext as IMediaElementViewModel;

            mediaElementVM.CurentControls = mediaControlsVM;

            mediaControlsVM.CurrentMediaPlayer = ucMediaElement;

            ViewModel.WindowFullScreen += ViewModel_WindowFullScreen;
            ViewModel.WindowNormalize += ViewModel_WindowNormalize;
        }

        private void ViewModel_WindowFullScreen()
        {
            //TODO * Use 2 grids
            //TODO serialize/resume + use settings
            ucMediaPlayerControls.SetBottomMargin(20);
            Grid.SetColumn(ucMediaPlayerControls, 1);
            Grid.SetColumnSpan(ucMediaPlayerControls, 1);

            Grid.SetRowSpan(ucMediaElement, 2);

            Settings.AnimationManager.BeginAnimation("FadeOut", ucMediaPlayerControls);
        }

        private void ViewModel_WindowNormalize()
        {
            ucMediaPlayerControls.Visibility = Visibility.Visible;
            ucMediaPlayerControls.Opacity = 1;

            ucMediaPlayerControls.SetBottomMargin(0);
            Grid.SetColumn(ucMediaPlayerControls, 0);
            Grid.SetColumnSpan(ucMediaPlayerControls, 3);

            Grid.SetRowSpan(ucMediaElement, 1);

            Settings.AnimationManager.CancelAnimation("FadeOut", ucMediaPlayerControls);
        }

        private void MainWindow_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel.IsFullScreen)
            {
                Settings.AnimationManager.CancelAnimation("FadeOut", ucMediaPlayerControls);
                ucMediaPlayerControls.Visibility = Visibility.Visible;
                ucMediaPlayerControls.Opacity = 0.9;
                if (!ucMediaPlayerControls.IsMouseOver)
                {
                    Settings.AnimationManager.BeginAnimation("FadeOut", ucMediaPlayerControls);
                }
            }
        }

        private void Window_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Equals(e.Source, ucMediaElement))
            {
                ViewModel.HandleWindowClick(this, e);
            }
        }

        private void MediaPlayerControls_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel.IsFullScreen)
            {
                Settings.AnimationManager.BeginAnimation("FadeOut", ucMediaPlayerControls);
            }
        }
    }
}
