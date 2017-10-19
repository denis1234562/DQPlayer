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
            bChangeStatePlayer.Content = _isPlaying
                ? ResourceFiles.Strings.Pause
                : ResourceFiles.Strings.Resume;
        }

        private void SourceChanged(bool state)
        {
            bChangeStatePlayer.IsEnabled = state;
        }

        private void SetNewPlayerSource(Uri source)
        {
            Player.Source = source;
            _moviePlayerTracker.MoviePlayerSource = source;
        }

        private void PlayNewPlayerSource(Uri source)
        {
            SetNewPlayerSource(source);
            Player.Play();
            _isPlaying = true;
            UpdateChangeStateContent();
        }
        #endregion

        #region Event Handlers

        private void MoviePlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
            => SourceChanged(Player.Source != null);

        private void bChangeStatePlayer_Click(object sender, RoutedEventArgs e) 
            => SwitchPlayerState();

        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog {Filter = "Matrioshka files (*.mkv)|*.mkv|Music files (*.mp3)|*.mp3"};
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
        #endregion

        private void SMovieSkipSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //TODO
        }
    }
}
