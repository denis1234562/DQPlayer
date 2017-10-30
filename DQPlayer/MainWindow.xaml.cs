using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using DQPlayer.MVVMFiles.ViewModels;
using EnvDTE80;
using Point = System.Windows.Point;

namespace DQPlayer
{
    public partial class MainWindow
    {
        /// <summary>
        /// Updates <see cref="VideoPlayerViewModel.MovieElapsedTime"/> and <see cref="VideoPlayerViewModel.MovieLeftTime"/>
        /// and their respective labels every <see cref="Settings.TimerTickUpdate"/>.
        /// </summary>
        private readonly DispatcherTimer _currentMovieTimeTimer = new DispatcherTimer();

        /// <summary>
        /// Invokes predetermined <see cref="Action"/> depending on what <see cref="PlayerState"/> is passed as a key.
        /// </summary>
        private IReadOnlyDictionary<PlayerState, Action> _modifyPlayerState;

        /// <summary>
        /// Used when you need to save the <see cref="_currentState"/> to reuse it afterwards.
        /// </summary>
        private PlayerState _lastState = PlayerStates.Pause;

        /// <summary>
        /// Keeps tracks of the current state of the player.
        /// </summary>
        private PlayerState _currentState;

        /// <summary>
        /// Cache for the <see cref="Player"/> slider's track.
        /// </summary>
        private Track _movieSkipSliderTrack;

        /// <summary>
        /// Reference to the ViewModel.
        /// </summary>
        public VideoPlayerViewModel ViewModel => DataContext as VideoPlayerViewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new VideoPlayerViewModel();
            SetupControls();
            SetupCaches();
            SetupTimers();
        }

        #region Startup Methods

        /// <summary>
        /// Invokes all startup control configuration methods.
        /// </summary>
        private void SetupControls()
        {
            SetupControlTexts();
            SetupControlImages();
            SetupPlayer();
            SetupControlVisibility();
            SourceChanged(false);
        }

        private void SetupControlTexts()
        {            
            bBrowse.Content = ResourceFiles.Strings.Browse;
        }

        private void SetupControlVisibility()
        {
            lbTimeTooltip.Visibility = Visibility.Hidden;
        }

        private void SetupPlayer()
        {
            Player.ScrubbingEnabled = true;
            Player.LoadedBehavior = MediaState.Manual;
            Player.UnloadedBehavior = MediaState.Manual;
            Player.Volume = 0;
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
                    AlignTimersWithSource(new TimeSpan(0));
                }
            };
        }

        private void SetupTimers()
        {
            _currentMovieTimeTimer.Interval = Settings.TimerTickUpdate;
            _currentMovieTimeTimer.Tick += CurrentMovieTimeTimer_Tick;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the <see cref="Track"/> element from a specified <see cref="Slider"/> control.
        /// </summary>
        /// <param name="slider">The <see cref="Slider"/> from which the <see cref="Track"/> to be obtained from.</param>
        /// <returns>Returns the <see cref="Track"/> element from a specified <see cref="Slider"/> control.</returns>
        private static Track GetTrack(Slider slider)
        {
            return slider.Template.FindName("PART_Track", slider) as Track;
        }

        /// <summary>
        /// Returns the point density of the <see cref="Track"/>
        /// </summary>
        /// <param name="track"><see cref="Track"/> which's density should be calculated.</param>
        /// <returns>Returns the point density of the <see cref="Track"/></returns>
        private double CalculateTrackDensity(Track track)
        {
            double effectivePoints = Math.Max(0, track.Maximum - track.Minimum);
            double effectiveLength = track.Orientation == Orientation.Horizontal
                ? track.ActualWidth - track.Thumb.DesiredSize.Width
                : track.ActualHeight - track.Thumb.DesiredSize.Height;
            return effectivePoints / effectiveLength;
        }

        /// <summary>
        /// Simulates a projection of a <see cref="Point"/> over a <see cref="Track"/>.
        /// It returns the value at which the projection falls.
        /// </summary>
        /// <param name="point">Point to simulate projection of.</param>
        /// <param name="track">Track to project the simulation at.</param>
        /// <returns>It returns the value at which the projection falls.</returns>
        private double SimulateTrackPosition(Point point, Track track)
        {
            return (point.X - track.Thumb.DesiredSize.Width / 2) * CalculateTrackDensity(track);
        }

        /// <summary>
        /// Sets <see cref="MainWindow"/> to FullScreen mode.
        /// </summary>
        private void SetFullScreen()
        {
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        /// <summary>
        /// Sets <see cref="MainWindow"/> to Normalized mode.
        /// </summary>
        private void SetNormalized()
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
        }

        /// <summary>
        /// Sets a new <see cref="Player"/> position and invokes <see cref="AlignTimersWithSource()"/>
        /// </summary>
        /// <param name="newPosition"></param>
        private void SetNewPlayerPosition(TimeSpan newPosition)
        {
            Player.Position = newPosition;
            AlignTimersWithSource(Player.Position);
        }

        /// <summary>
        /// Invokes <see cref="_modifyPlayerState"/> with the specified <see cref="PlayerState"/>
        /// </summary>
        /// <param name="state">State to be set.</param>
        private void SetPlayerState(PlayerState state) => _modifyPlayerState[state].Invoke();

        /// <summary>
        /// Used internally by <see cref="_modifyPlayerState"/>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
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

        /// <summary>
        /// Updates the <see cref="Player"/> Play/Pause image button.
        /// </summary>
        private void UpdateChangeStateImage()
        {
            imgChangePlayerState.Source = Equals(_currentState, PlayerStates.Play)
                 ? Settings.PauseImage
                 : Settings.PlayImage;
        }

        /// <summary>
        /// Sets the <see cref="Control.IsEnabled"/> property of all controls interested in the change of <see cref="o:Player.Source"/>.
        /// </summary>
        /// <param name="state"></param>
        private void SourceChanged(bool state)
        {
            imgChangePlayerState.IsEnabled = state;
            sMovieSkipSlider.IsEnabled = state;
            imgSkipForward.IsEnabled = state;
            imgSkipBack.IsEnabled = state;
            imgStop.IsEnabled = state;
        }

        /// <summary>
        /// Sets new specified <see cref="source"/> to <see cref="o:Player.Source"/> and invokes <see cref="SourceChanged(bool)"/>
        /// </summary>
        /// <param name="source">New source to set.</param>
        private void SetNewPlayerSource(Uri source)
        {
            Player.Source = source;
            SourceChanged(Player.Source != null);
        }
        /// <summary>
        /// Set and plays new specified <see cref="source"/> and invokes <see cref="SetNewPlayerSource(Uri)"/>
        /// </summary>
        /// <param name="source">New source to set.</param>
        private void PlayNewPlayerSource(Uri source)
        {
            SetNewPlayerSource(source);
            SetPlayerState(PlayerStates.Play);
        }

        /// <summary>
        /// Updates <see cref="VideoPlayerViewModel.MovieElapsedTime"/>, <see cref="VideoPlayerViewModel.MovieLeftTime"/> 
        /// and their associated labels, in respect to the new value of <see cref="currentPosition"/>.
        /// </summary>
        /// <param name="currentPosition"></param>
        private void AlignTimersWithSource(TimeSpan currentPosition)
        {
            ViewModel.MovieLeftTime = Player.NaturalDuration.TimeSpan - currentPosition;
            ViewModel.MovieElapsedTime = currentPosition;
            UpdateTimerLabels();
        }

        /// <summary>
        /// Updates movie time labels.
        /// </summary>
        private void UpdateTimerLabels()
        {
            lbMovieTimeLeft.Content = ViewModel.MovieLeftTime.ToShortString();
            lbMovieElapsedTime.Content = ViewModel.MovieElapsedTime.ToShortString();
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
            ViewModel.MovieLeftTime = ViewModel.MovieLeftTime.Subtract(_currentMovieTimeTimer.Interval);
            UpdateTimerLabels();
        }

        private void SMovieSkipSlider_OnLoaded(object sender, RoutedEventArgs e)
        {
            _movieSkipSliderTrack = GetTrack(sMovieSkipSlider);
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
                _movieSkipSliderTrack.Thumb.RaiseEvent(args);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var mousePosition = new Point(Mouse.GetPosition(sMovieSkipSlider).X, 0);
            var simulatedValue = SimulateTrackPosition(mousePosition, _movieSkipSliderTrack);
            SetNewPlayerPosition(TimeSpan.FromSeconds(simulatedValue));
        }

        private void SMovieSkipSlider_OnMouseEnter(object sender, MouseEventArgs e)
        {
            lbTimeTooltip.Visibility = Visibility.Visible;
            lbTimeTooltip.SetLeftMargin(Mouse.GetPosition(sMovieSkipSlider).X);
        }
        private void SMovieSkipSlider_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            double simulatedPosition = SimulateTrackPosition(e.GetPosition(sMovieSkipSlider), _movieSkipSliderTrack);
            simulatedPosition = Math.Min(Math.Max(simulatedPosition, 0), sMovieSkipSlider.Maximum);
            lbTimeTooltip.AddToLeftMargin(Mouse.GetPosition(sMovieSkipSlider).X - lbTimeTooltip.Margin.Left + 35);
            lbTimeTooltip.Content = TimeSpan.FromSeconds(simulatedPosition).ToShortString();
        }
        private void SMovieSkipSlider_OnMouseLeave(object sender, MouseEventArgs e)
        {
            lbTimeTooltip.Visibility = Visibility.Hidden;
        }

        private void imgSkipBack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SetNewPlayerPosition(ViewModel.MovieElapsedTime.Subtract(Settings.SkipSeconds).TotalSeconds >= 0
                    ? ViewModel.MovieElapsedTime.Subtract(Settings.SkipSeconds)
                    : new TimeSpan(0));
            }
        }
        private void imgSkipForward_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SetNewPlayerPosition(
                    ViewModel.MovieElapsedTime.Add(Settings.SkipSeconds).TotalSeconds <=
                    Player.NaturalDuration.TimeSpan.TotalSeconds
                        ? ViewModel.MovieElapsedTime.Add(Settings.SkipSeconds)
                        : Player.NaturalDuration.TimeSpan);
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

        //pomaga pri testvane
        protected void ClearOutput()
        {
            DTE2 ide = (DTE2)Marshal.GetActiveObject("VisualStudio.DTE.15.0");
            ide.ToolWindows.OutputWindow.OutputWindowPanes.Item("Debug").Clear();
            Marshal.ReleaseComObject(ide);
        }
    }
}
