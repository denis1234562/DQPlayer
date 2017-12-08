using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using DQPlayer.Helpers.Extensions;
using DQPlayer.Helpers.SubtitlesManagement;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.MVVMFiles.ViewModels;
using DQPlayer.MVVMFiles.View;

namespace DQPlayer
{
    public partial class MainWindow
    {
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

        #region Startup Methods

        private void ViewModel_Loaded(IMediaService obj)
        {
            Player = Settings.MediaPlayerTemplate.CloneAndOverride(Player);
            PlayList temp = new PlayList();
            SetupTimers();
            ViewModel.Show += ViewModel_Show;
            ViewModel.Hide += ViewModel_Hide;
        }

        private Dictionary<SubtitleSegment, Label[]> _currentlyShownSubtitles = new Dictionary<SubtitleSegment, Label[]>();
        private void ViewModel_Show(SubtitleHandler handler, SubtitleSegment segment)
        {
            if (_currentlyShownSubtitles.ContainsKey(segment))
            {
                return;
            }
            var lines = gridSubs.Children.Cast<Viewbox>().Select(vb => (Label) vb.Child).ToArray();
            var splitedLines = segment.Content.Split(new []{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty((string) lines[i].Content))
                {
                    if (splitedLines.Length == 1)
                    {
                        lines[i].Content = segment.Content;
                        _currentlyShownSubtitles.Add(segment, new[] {lines[i]});
                        break;
                    }
                    int count = 1;
                    while (count < splitedLines.Length && string.IsNullOrEmpty((string) lines[++i].Content))
                    {
                        count++;
                    }
                    var elements = lines.Skip(i - count - 1).Take(count).ToArray();
                    _currentlyShownSubtitles.Add(segment, elements);
                    for (int j = 0; j < splitedLines.Length; j++)
                    {
                        elements[j].Content = splitedLines[splitedLines.Length -1 - j];
                    }
                    break;
                }
            }
        }

        private void ViewModel_Hide(SubtitleHandler handler, SubtitleSegment segment)
        {
            if (_currentlyShownSubtitles.TryGetValue(segment, out var value))
            {
                foreach (var line in value)
                {
                    Dispatcher.Invoke(() => line.Content = string.Empty);
                }
                _currentlyShownSubtitles.Remove(segment);
            }
        }

        private void SetupBindings()
        {
            SetBinding(MediaPlayerProperty,
                new Binding {Path = new PropertyPath("MediaPlayer"), Mode = BindingMode.OneWayToSource});
            MediaPlayer = new MediaPlayerModel(sMovieSkipSlider);
            MediaPlayer.MediaController = new RegulatableMediaPlayerService(Player, MediaPlayer);
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

        private void Player_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            sMovieSkipSlider.SetBinding(RangeBase.MaximumProperty,
                new Binding("TotalSeconds") {Source = Player.NaturalDuration.TimeSpan});
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

        private void SMovieSkipSlider_OnMouseEnter(object sender, MouseEventArgs e)
        {
            lbTimeTooltip.Visibility = Visibility.Visible;
            lbTimeTooltip.SetLeftMargin(Mouse.GetPosition(sMovieSkipSlider).X);
        }

        private void SMovieSkipSlider_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            double simulatedPosition = sMovieSkipSlider.GetElementFromTemplate<Track>("PART_Track").SimulateTrackPosition(e.GetPosition(sMovieSkipSlider));
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
