using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using DQPlayer.ViewModels;

namespace DQPlayer
{
    public partial class MainWindow
    {
        private readonly DispatcherTimer _currentMovieTimeTimer = new DispatcherTimer();

        public VideoPlayerViewModel ViewModel => DataContext as VideoPlayerViewModel;

        private TimeSpan _movieLeftTime;

        private IReadOnlyDictionary<PlayerState, Action> _modifyPlayerState;

        private PlayerState _currentState;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new VideoPlayerViewModel();
            SetupControls();
            SetupCaches();
            SetupTimers();
        }

        #region Startup Methods
        private void SetupControls()
        {
            SetupControlTexts();
            SetupPlayer();
            SourceChanged(false);
        }

        private void SetupControlTexts()
        {
            UpdateChangeStateContent();
            bBrowse.Content = ResourceFiles.Strings.Browse;
        }

        private void SetupPlayer()
        {
            Player.ScrubbingEnabled = true;
            Player.LoadedBehavior = MediaState.Manual;
            Player.UnloadedBehavior = MediaState.Manual;
        }

        private void SetupCaches()
        {
            _modifyPlayerState = new Dictionary<PlayerState, Action>
            {
                [PlayerStates.Pause] = () =>
                {
                    SetPlayerStateImpl(PlayerStates.Pause, Player.Pause);
                },
                [PlayerStates.Play] = () =>
                {
                    SetPlayerStateImpl(PlayerStates.Play, Player.Play);
                },
                [PlayerStates.Stop] = () =>
                {
                    SetPlayerStateImpl(PlayerStates.Stop, Player.Stop);
                    AlignTimersWithSource();
                }
            };
        }

        private void SetupTimers()
        {
            _currentMovieTimeTimer.Interval = TimeSpan.FromMilliseconds(500);
            _currentMovieTimeTimer.Tick += CurrentMovieTimeTimer_Tick;
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

        private void SetPlayerState(PlayerState state) => _modifyPlayerState[state].Invoke();

        private void SetPlayerStateImpl(PlayerState state, Action action)
        {
            action.Invoke();
            _currentState = state;
            UpdateChangeStateContent();
            if (state.IsRunning)
            {
                _currentMovieTimeTimer.Start();
            }
            else
            {
                _currentMovieTimeTimer.Stop();
            }
        }

        private void UpdateChangeStateContent()
        {
            bChangeStatePlayer.Content = Equals(_currentState, PlayerStates.Play)
                ? ResourceFiles.Strings.Pause
                : ResourceFiles.Strings.Resume;
        }

        private void SourceChanged(bool state)
        {
            bChangeStatePlayer.IsEnabled = state;
            sMovieSkipSlider.IsEnabled = state;
        }

        private void SetNewPlayerSource(Uri source)
        {
            Player.Source = source;
            SourceChanged(Player.Source != null);
        }
        private void PlayNewPlayerSource(Uri source)
        {
            SetNewPlayerSource(source);
            SetPlayerState(PlayerStates.Play);
        }

        private void AlignTimersWithSource(TimeSpan currentPosition)
        {
            _movieLeftTime = Player.NaturalDuration.TimeSpan - currentPosition;
            ViewModel.MovieElapsedTime = currentPosition;
            UpdateTimerLabels();
        }
        private void AlignTimersWithSource()
        {
            _movieLeftTime = Player.NaturalDuration.TimeSpan;
            ViewModel.MovieElapsedTime = new TimeSpan(0);
            UpdateTimerLabels();
        }

        private void UpdateTimerLabels()
        {
            lbMovieTimeLeft.Content = _movieLeftTime.ToString(@"hh\:mm\:ss");
            lbMovieElapsedTime.Content = ViewModel.MovieElapsedTime.ToString(@"hh\:mm\:ss");
        }
        #endregion

        #region Event Handlers

        private void bChangeStatePlayer_Click(object sender, RoutedEventArgs e)
            => SetPlayerState(Equals(_currentState, PlayerStates.Play) ? PlayerStates.Pause : PlayerStates.Play);

        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = $"Media files ({string.Join(",", Settings.AllowedExtensions.Select(ae => "*" + ae))})|" +
                         $"{string.Join(";", Settings.AllowedExtensions.Select(ae => "*" + ae))}|" +
                         "Matrioshka files (*.mkv)|*.mkv|" +
                         "Music files (*.mp3)|*.mp3|"+
                         "Movie files (*.mp4)|*.mp4"
            };
            var shown = fileDialog.ShowDialog();
            if (shown.HasValue && shown.Value)
            {
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
            if (Player.NaturalDuration.HasTimeSpan)
            {
                sMovieSkipSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
                AlignTimersWithSource(new TimeSpan(0));
                _currentMovieTimeTimer.Start();
            }
        }
        private void Player_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            SetPlayerState(PlayerStates.Stop);
        }

        private PlayerState _lastState = PlayerStates.Pause;
        private void SMovieSkipSlider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            _lastState = _currentState;
            SetPlayerState(PlayerStates.Pause);
        }
        private void SMovieSkipSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            SetPlayerState(_lastState);
            Player.Position = TimeSpan.FromSeconds(sMovieSkipSlider.Value);
            AlignTimersWithSource(Player.Position);
        }

        private void SMovieSkipSlider_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //TODO
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
            ViewModel.MovieElapsedTime = ViewModel.MovieElapsedTime.Add(_currentMovieTimeTimer.Interval);
            _movieLeftTime = _movieLeftTime.Subtract(_currentMovieTimeTimer.Interval);
            UpdateTimerLabels();
        }
        #endregion
    }
}
