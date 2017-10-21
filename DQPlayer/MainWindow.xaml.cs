﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Linq;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DQPlayer.CustomControls;
using DQPlayer.ViewModels;

namespace DQPlayer
{
    public partial class MainWindow
    {
        private readonly DispatcherTimer _currentMovieTimeTimer = new DispatcherTimer();

        public VideoPlayerViewModel ViewModel => DataContext as VideoPlayerViewModel;

        private TimeSpan _movieLeftTime;

        private IReadOnlyDictionary<PlayerState, Action> _modifyPlayerState;

        private PlayerState _lastState = PlayerStates.Pause;
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
            SetupControlImages();
            SetupPlayer();
            SourceChanged(false);          
        }

        private void SetupControlTexts()
        {            
            bBrowse.Content = ResourceFiles.Strings.Browse;
        }

        private void SetupPlayer()
        {
            Player.ScrubbingEnabled = true;
            Player.LoadedBehavior = MediaState.Manual;
            Player.UnloadedBehavior = MediaState.Manual;
        }

        private void SetupControlImages()
        {
            UpdateChangeStateImage();
            imgSkipBack.Source = Settings.SkipBack;
            imgSkipForward.Source = Settings.SkipForward;
            imgStop.Source = Settings.Stop;
            imgSplashScreen.Source = Settings.SpashScreenImage;
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
            _currentMovieTimeTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _currentMovieTimeTimer.Tick += CurrentMovieTimeTimer_Tick;
        }
        #endregion

        #region Private Methods
        private Thumb GetThumb(Slider slider)
        {
            var track = slider.Template.FindName("PART_Track", slider) as Track;
            return track?.Thumb;
        }

        private void SetFullScreen()
        {
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void SetNewPlayerPosition(TimeSpan newPosition)
        {
            Player.Position = newPosition;
            AlignTimersWithSource(Player.Position);
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
            UpdateChangeStateImage();
            if (state.IsRunning)
            {
                _currentMovieTimeTimer.Start();
            }
            else
            {
                _currentMovieTimeTimer.Stop();
            }
        }

        private void UpdateChangeStateImage()
        {
            imgChangePlayerState.Source = Equals(_currentState, PlayerStates.Play)
                 ? Settings.PauseImage
                 : Settings.PlayImage;
        }

        private void SourceChanged(bool state)
        {
            imgChangePlayerState.IsEnabled = state;
            sMovieSkipSlider.IsEnabled = state;
            imgSkipForward.IsEnabled = state;
            imgSkipBack.IsEnabled = state;
            imgStop.IsEnabled = state;
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

        private void imgChangePlayerState_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SetPlayerState(Equals(_currentState, PlayerStates.Play) ? PlayerStates.Pause : PlayerStates.Play);
            }
        }

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
                imgSplashScreen.Visibility = Visibility.Collapsed;
                SetNewPlayerSource(new Uri(fileDialog.FileName));
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

        private void SMovieSkipSlider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            _lastState = _currentState;
            SetPlayerState(PlayerStates.Pause);
        }
        private void SMovieSkipSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            SetPlayerState(_lastState);
            SetNewPlayerPosition(TimeSpan.FromSeconds(sMovieSkipSlider.Value));
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

        private void SMovieSkipSlider_OnLoaded(object sender, RoutedEventArgs e)
        {
            var thumb = GetThumb(sMovieSkipSlider);
            thumb.DragDelta += Thumb_DragDelta;
            thumb.PreviewMouseDown += Thumb_MouseDown;
        }

        private void Thumb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                lbTimeTooltip.Content = TimeSpan.FromSeconds(sMovieSkipSlider.Value).ToString(@"hh\:mm\:ss");
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            lbTimeTooltip.Content = TimeSpan.FromSeconds(sMovieSkipSlider.Value).ToString(@"hh\:mm\:ss");
        }

        private void SMovieSkipSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((int)Math.Abs(e.NewValue - e.OldValue) != 1)
            {
                SetNewPlayerPosition(TimeSpan.FromSeconds(sMovieSkipSlider.Value));
            }
        }
        private void imgSkipBack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (ViewModel.MovieElapsedTime.Subtract(Settings.SkipSeconds).TotalSeconds >= 0)
                {
                    SetNewPlayerPosition(ViewModel.MovieElapsedTime.Subtract(Settings.SkipSeconds));
                }
                else
                {
                    SetNewPlayerPosition(new TimeSpan(0));
                }
            }
        }
        private void imgSkipForward_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (ViewModel.MovieElapsedTime.Add(Settings.SkipSeconds).TotalSeconds <= Player.NaturalDuration.TimeSpan.TotalSeconds)
                {
                    SetNewPlayerPosition(ViewModel.MovieElapsedTime.Add(Settings.SkipSeconds));
                }
                else
                {
                    SetNewPlayerPosition(Player.NaturalDuration.TimeSpan);
                }

            }
        }
        private void imgStop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SetPlayerState(PlayerStates.Stop);
            }
        }
        #endregion
    }
}
