using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace DQPlayer
{
    public partial class MainWindow
    {
        private readonly DispatcherTimer _currentMovieTimeTimer = new DispatcherTimer();
        private TimeSpan _movieElapsedTime;
        private TimeSpan _movieLeftTime;

        private IReadOnlyDictionary<MoviePlayerState, Action> _modifyPlayerState;

        private bool _isPlaying;
        private MoviePlayerSourceTracker _moviePlayerTracker;

        public MainWindow()
        {
            InitializeComponent();
            SetupControls();
            SetupCaches();
            SetupTimers();
        }

        private void CurrentMovieTimeTimer_Tick(object sender, EventArgs e)
        {
            _movieElapsedTime = _movieElapsedTime.Add(TimeSpan.FromSeconds(1));
            _movieLeftTime = _movieLeftTime.Subtract(TimeSpan.FromSeconds(1));
            UpdateTimerLabels();
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
            _moviePlayerTracker = new MoviePlayerSourceTracker(Player);
            _moviePlayerTracker.PropertyChanged += MoviePlayer_PropertyChanged;
        }

        private void SetupCaches()
        {
            _modifyPlayerState = new Dictionary<MoviePlayerState, Action>
            {
                //TODO maybe check if video is available?
                [MoviePlayerState.Pause] = () =>
                {
                    Player.Pause();
                    _isPlaying = false;
                    UpdateChangeStateContent();
                    _currentMovieTimeTimer.Stop();
                },
                [MoviePlayerState.Play] = () =>
                {
                    Player.Play();
                    _isPlaying = true;
                    UpdateChangeStateContent();
                    _currentMovieTimeTimer.Start();
                },
                [MoviePlayerState.Stop] = () =>
                {
                    Player.Stop();
                    _isPlaying = false;
                    UpdateChangeStateContent();
                    _currentMovieTimeTimer.Stop();
                    AlignTimersWithSource();
                }
            };
        }

        private void SetupTimers()
        {
            _currentMovieTimeTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _currentMovieTimeTimer.Tick += CurrentMovieTimeTimer_Tick;
        }
        #endregion

        #region Private Methods

        private void SetPlayerState(MoviePlayerState state) => _modifyPlayerState[state].Invoke();

        private void UpdateChangeStateContent()
        {
            bChangeStatePlayer.Content = _isPlaying
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
            _moviePlayerTracker.MoviePlayerSource = source;
        }

        private void PlayNewPlayerSource(Uri source)
        {
            SetNewPlayerSource(source);
            SetPlayerState(MoviePlayerState.Play);
        }

        private void AlignTimersWithSource(TimeSpan currentPosition)
        {
            _movieLeftTime = Player.NaturalDuration.TimeSpan - currentPosition;
            _movieElapsedTime = currentPosition;
            UpdateTimerLabels();
        }

        private void AlignTimersWithSource()
        {
            _movieLeftTime = Player.NaturalDuration.TimeSpan;
            _movieElapsedTime = new TimeSpan(0);
            UpdateTimerLabels();
        }

        private void UpdateTimerLabels()
        {
            lbMovieTimeLeft.Content = _movieLeftTime.ToString(@"hh\:mm\:ss");
            lbMovieElapsedTime.Content = _movieElapsedTime.ToString(@"hh\:mm\:ss");
        }
        #endregion

        #region Event Handlers

        private void MoviePlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
            => SourceChanged(Player.Source != null);

        private void bChangeStatePlayer_Click(object sender, RoutedEventArgs e)
            => SetPlayerState(_isPlaying ? MoviePlayerState.Pause : MoviePlayerState.Play);

        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = $"Media files ({string.Join(",", Settings.AllowedExtensions.Select(ae => "*" + ae))})|" +
                         $"{string.Join(";", Settings.AllowedExtensions.Select(ae => "*" + ae))}|" +
                         "Matrioshka files (*.mkv)|*.mkv|" +
                         "Music files (*.mp3)|*.mp3"
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
            SetPlayerState(MoviePlayerState.Stop);
        }

        private void SMovieSkipSlider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            SetPlayerState(MoviePlayerState.Pause);
        }

        private void SMovieSkipSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            SetPlayerState(MoviePlayerState.Play);
            Player.Position = TimeSpan.FromSeconds(sMovieSkipSlider.Value);
            AlignTimersWithSource(Player.Position);
        }
        #endregion
    }

    public enum MoviePlayerState
    {
        Play,
        Pause,
        Stop
    }
}
