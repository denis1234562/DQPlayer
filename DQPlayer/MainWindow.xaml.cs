using System;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using DQPlayer.Extensions;
using DQPlayer.Helpers;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.MVVMFiles.ViewModels;
using DQPlayer.ResourceFiles;
using DQPlayer.States;

namespace DQPlayer
{
    public partial class MainWindow
    {
        private readonly FileDropHandler _fileDropHandler = new FileDropHandler();

        private MediaPlayerState _lastState = MediaPlayerStates.None;

        private Track _movieSkipSliderTrack;

        public VideoPlayerViewModel ViewModel => DataContext as VideoPlayerViewModel;

        public MediaPlayerModel MediaPlayer
        {
            get => (MediaPlayerModel) GetValue(MediaPlayerProperty);
            set => SetValue(MediaPlayerProperty, value);
        }

        public static readonly DependencyProperty MediaPlayerProperty =
            DependencyProperty.Register(nameof(MediaPlayer), typeof(MediaPlayerModel), typeof(MainWindow),
                new PropertyMetadata(null));

        public MainWindow()
        {
            InitializeComponent();
            SetupBindings();
            ViewModel.Loaded += ViewModel_Loaded;
        }

        private void ViewModel_Loaded(IMediaService obj)
        {
            Player = Settings.MediaPlayerTemplate.CloneAndOverride(Player);
            SetupTimers();
        }

        #region Startup Methods

        private void SetupBindings()
        {
            SetBinding(MediaPlayerProperty,
                new Binding {Path = new PropertyPath("MediaPlayer"), Mode = BindingMode.OneWayToSource});
            MediaPlayer = new MediaPlayerModel(sMovieSkipSlider);
            MediaPlayer.MediaController = new RegulatableMediaService(Player, MediaPlayer);
        }

        private void SetupTimers()
        {
            ViewModel.MediaPlayer.MediaPlayerTimer.Interval = Settings.TimerTickUpdate;
            ViewModel.MediaPlayer.MediaPlayerTimer.Tick += CurrentMovieTimeTimer_Tick;
        }

        #endregion

        #region Private Methods

        private void SetFullScreen()
        {
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void SetNormalized()
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
        }

        #endregion

        #region Event Handlers

        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = Settings.MediaPlayerExtensionPackageFilter.Filter
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                ViewModel.MediaPlayer.PlayNewPlayerSource(new Uri(fileDialog.FileName));
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (_fileDropHandler.TryExtractDroppedItemUri(e, Settings.MediaPlayerExtensionPackage, out var uri))
            {
                ViewModel.MediaPlayer.PlayNewPlayerSource(uri);
                return;
            }
            MessageBox.Show($"{Strings.InvalidFileType}", "Error");
        }

        private void Player_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            sMovieSkipSlider.SetBinding(RangeBase.MaximumProperty,
                new Binding("TotalSeconds") {Source = Player.NaturalDuration.TimeSpan});
        }

        private void Player_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            ViewModel.MediaPlayer.SetMediaState(MediaPlayerStates.Stop);
        }

        private void SMovieSkipSlider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            _lastState = ViewModel.MediaPlayer.CurrentState.SerializedClone();
            ViewModel.MediaPlayer.SetMediaState(MediaPlayerStates.Pause);
        }

        private void SMovieSkipSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            ViewModel.MediaPlayer.SetMediaState(_lastState);
            ViewModel.MediaPlayer.MediaController.SetNewPlayerPosition(sMovieSkipSlider.Value);
        }

        private void Player_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                {
                    SetFullScreen();
                }
                else if (WindowState == WindowState.Maximized)
                {
                    SetNormalized();
                }
            }
        }

        private void CurrentMovieTimeTimer_Tick(object sender, EventArgs e)
        {
            sMovieSkipSlider.Value = sMovieSkipSlider.Value.Add(ViewModel.MediaPlayer.MediaPlayerTimer.Interval);
        }

        private void SMovieSkipSlider_OnLoaded(object sender, RoutedEventArgs e)
        {
            _movieSkipSliderTrack = sMovieSkipSlider.GetElementFromTemplate<Track>("PART_Track");
            _movieSkipSliderTrack.Thumb.DragDelta += Thumb_DragDelta;
            _movieSkipSliderTrack.Thumb.MouseEnter += Thumb_MouseEnter;
        }

        private void Thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
            {
                var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = MouseLeftButtonDownEvent
                };
                ViewModel.MediaPlayer.SetPlayerPositionToCursor();
                _movieSkipSliderTrack.Thumb.RaiseEvent(args);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ViewModel.MediaPlayer.SetPlayerPositionToCursor();
        }

        private void SMovieSkipSlider_OnMouseEnter(object sender, MouseEventArgs e)
        {
            lbTimeTooltip.Visibility = Visibility.Visible;
            lbTimeTooltip.SetLeftMargin(Mouse.GetPosition(sMovieSkipSlider).X);
        }

        private void SMovieSkipSlider_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            double simulatedPosition = _movieSkipSliderTrack.SimulateTrackPosition(e.GetPosition(sMovieSkipSlider));
            lbTimeTooltip.AddToLeftMargin(Mouse.GetPosition(sMovieSkipSlider).X - lbTimeTooltip.Margin.Left + 35);
            lbTimeTooltip.Content = TimeSpan.FromSeconds(simulatedPosition).ToShortString();
        }

        private void SMovieSkipSlider_OnMouseLeave(object sender, MouseEventArgs e)
        {
            lbTimeTooltip.Visibility = Visibility.Hidden;
        }

        #endregion
    }
}
