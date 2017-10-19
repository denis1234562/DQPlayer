using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Linq;

namespace DQPlayer
{
    public partial class MainWindow
    {
        private bool _isPlaying;
        private MoviePlayerSourceTracker _moviePlayerTracker;

        public MainWindow()
        {
            InitializeComponent();
            SetupControls();         
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
            Player.LoadedBehavior = MediaState.Manual;
            Player.UnloadedBehavior = MediaState.Manual;
            _moviePlayerTracker = new MoviePlayerSourceTracker(Player);
            _moviePlayerTracker.PropertyChanged += MoviePlayer_PropertyChanged;
            sVolumeSlider.ValueChanged += SVolumeSlider_ValueChanged;
            sVolumeSlider.Value = 1;
            Player.Volume = 1;
        }
        #endregion

        #region Private Methods
        private void SwitchPlayerState()
        {
            if (_isPlaying)
            {
                Player.Pause();
            }
            else
            {
                Player.Play();
            }
            _isPlaying = !_isPlaying;
            UpdateChangeStateContent();
        }

        private void UpdateChangeStateContent()
        {
            bChangeStatePlayer.Content = _isPlaying ? ResourceFiles.Strings.Pause : ResourceFiles.Strings.Resume;
        }

        private void SourceChanged(bool state)
        {
            bChangeStatePlayer.IsEnabled = state;
        }
        #endregion

        #region Event Handlers
        private void MoviePlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SourceChanged(((MediaElement)sender).Source != null);
        }

        private void bChangeStatePlayer_Click(object sender, RoutedEventArgs e)
        {
            SwitchPlayerState();
        }

        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog =
                new OpenFileDialog {Filter = "Matrioshka files (*.mkv)|*.mkv|Music files (*.mp3)|*.mp3"};
            bool? shown = fileDialog.ShowDialog();
            if (shown.HasValue && shown.Value)
            {
                Player.Source = new Uri(fileDialog.FileName);
                Player.Play();
                _isPlaying = true;
                UpdateChangeStateContent();
                _moviePlayerTracker.MoviePlayerSource = Player.Source;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string filePath = ((DataObject) e.Data).GetFileDropList()[0];
            string fileExtension = filePath.Substring(filePath.LastIndexOf(".", StringComparison.Ordinal));
            if (!Settings.AllowedExtensions.Contains(fileExtension))
            {
                MessageBox.Show(this,
                    $"Invalid file extension! Allowed file types are: {string.Join(",", Settings.AllowedExtensions)}",
                    "Error");
                return;
            }
            Player.Source = new Uri(filePath);
            Player.Play();
            _isPlaying = true;
            UpdateChangeStateContent();
            _moviePlayerTracker.MoviePlayerSource = Player.Source;
        }

        private void SVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Volume = ((Slider)sender).Value;
        }
        #endregion
    }
}
