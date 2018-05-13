using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using DQPlayer.Helpers.DialogHelpers;
using DQPlayer.Helpers.Extensions;
using DQPlayer.Helpers.FileManagement.FileInformation;
using DQPlayer.Helpers.InputManagement;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.MVVMFiles.ViewModels;
using DQPlayer.States;

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
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 30 });
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            WindowDialogHelper<PlaylistView>.Instance.Show();
            WindowDialogHelper<PlaylistView>.Instance.Hide();

            var mediaElementVM = ucMediaElement.DataContext as IMediaElementViewModel;
            var controlsVM = ucMediaPlayerControls.DataContext as IMediaControlsViewModel;
            var subtitlesVM = ucSubtitles.DataContext as ISubtitlesViewModel;

            mediaElementVM.MediaPlayerModel = new MediaPlayerModel(MediaPlayerStates.None);
            mediaElementVM.MediaPlayerModel.MediaController =
                new RegulatableMediaPlayerService(ucMediaElement.MediaPlayer, mediaElementVM.MediaPlayerModel);

            mediaElementVM.CurrentControls = controlsVM;
            controlsVM.CurrentMediaPlayer = mediaElementVM;

            subtitlesVM.CurrentMediaElement = mediaElementVM;

            ViewModel.WindowFullScreen += ViewModel_WindowFullScreen;
            ViewModel.WindowNormalize += ViewModel_WindowNormalize;

            FileManagerHelper.Request(this, new MediaFileInformation(new Uri(
                @"F:\Movies\World Of Warcraft\07\game.of.thrones.s07e07.720p.web.h264-strife.mkv")).AsEnumerable());
        }

        private void ViewModel_WindowFullScreen()
        {
            ucMediaPlayerControls.SetBottomMargin(20);
            Grid.SetColumn(ucMediaPlayerControls, 1);
            Grid.SetColumnSpan(ucMediaPlayerControls, 1);

            Grid.SetRowSpan(ucMediaElement, 2);
            Grid.SetRowSpan(ucSubtitles, 2);

            Settings.AnimationManager.BeginAnimation("FadeOut", ucMediaPlayerControls);
        }

        private void ViewModel_WindowNormalize()
        {
            Settings.AnimationManager.CancelAnimation("FadeOut", ucMediaPlayerControls);
            ucMediaPlayerControls.Visibility = Visibility.Visible;
            ucMediaPlayerControls.Opacity = 1;

            ucMediaPlayerControls.SetBottomMargin(0);
            Grid.SetColumn(ucMediaPlayerControls, 0);
            Grid.SetColumnSpan(ucMediaPlayerControls, 3);

            Grid.SetRowSpan(ucMediaElement, 1);
            Grid.SetRowSpan(ucSubtitles, 1);
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
    }
}
