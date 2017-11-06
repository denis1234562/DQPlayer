using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using DQPlayer.CustomControls;
using DQPlayer.MVVMFiles.Converters;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.MVVMFiles.ViewModels;
using DQPlayer.States;
using Point = System.Windows.Point;

namespace DQPlayer
{
    public partial class MainWindow
    {
        private MediaPlayerState _lastState = MediaPlayerStates.Pause;

        private Track _movieSkipSliderTrack;

        public VideoPlayerViewModel ViewModel => DataContext as VideoPlayerViewModel;

        public MediaPlayerModel MediaPlayer
        {
            get => (MediaPlayerModel)GetValue(MediaPlayerProperty);
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
                new Binding { Path = new PropertyPath("MediaPlayer"), Mode = BindingMode.OneWayToSource });
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

        private void SetPlayerPositionToCursor()
        {
            Point mousePosition = new Point(Mouse.GetPosition(sMovieSkipSlider).X, 0);
            double simulatedValue = _movieSkipSliderTrack.SimulateTrackPosition(mousePosition);
            ViewModel.MediaPlayer.MediaController.SetNewPlayerPosition(TimeSpan.FromSeconds(simulatedValue));
        }

        private void SetNewPlayerSource(Uri source)
        {
            Player.Source = source;
            sMovieSkipSlider.Value = new TimeSpan(0);
        }

        private void PlayNewPlayerSource(Uri source)
        {
            SetNewPlayerSource(source);
            ViewModel.MediaPlayer.SetMediaState(MediaPlayerStates.Play);
        }

        #endregion

        #region Event Handlers

        private void imgChangePlayerState_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.MediaPlayer.SetMediaState(Equals(ViewModel.MediaPlayer.CurrentState, MediaPlayerStates.Play)
                ? MediaPlayerStates.Pause
                : MediaPlayerStates.Play);
        }

        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = $"Media files ({string.Join(",", Settings.AllowedExtensions.Select(ae => "*" + ae))})|" +
                         $"{string.Join(";", Settings.AllowedExtensions.Select(ae => "*" + ae))}|" +
                         "Matrioshka files (*.mkv)|*.mkv|" +
                         "Music files (*.mp3)|*.mp3|" +
                         "Movie files (*.mp4)|*.mp4"
            };
            var shown = fileDialog.ShowDialog();
            if (shown.HasValue && shown.Value)
            {
                imgSplashScreen.Visibility = Visibility.Collapsed;
                PlayNewPlayerSource(new Uri(fileDialog.FileName));
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var filePath = ((DataObject) e.Data).GetFileDropList()[0];
            var fileExtension = filePath.Substring(filePath.LastIndexOf(".", StringComparison.Ordinal));
            if (!Settings.AllowedExtensions.Contains(fileExtension))
            {
                MessageBox.Show(this,
                    $"Invalid file extension! Allowed file types are: {string.Join(",", Settings.AllowedExtensions)}",
                    "Error");
                return;
            }
            PlayNewPlayerSource(new Uri(filePath));
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
                SetPlayerPositionToCursor();
                _movieSkipSliderTrack.Thumb.RaiseEvent(args);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            SetPlayerPositionToCursor();
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
