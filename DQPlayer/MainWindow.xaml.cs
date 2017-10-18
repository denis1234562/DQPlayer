using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DQPlayer.Annotations;
using Microsoft.Win32;

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
            _moviePlayerTracker.PropertyChanged += Asd_PropertyChanged;
        }

        private void Asd_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SourceChanged(((MediaElement)sender).Source != null);
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
        }

        private void SetupPlayer()
        {
            Player.LoadedBehavior = MediaState.Manual;
            Player.UnloadedBehavior = MediaState.Manual;
            _moviePlayerTracker = new MoviePlayerSourceTracker(Player);
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
        private void bChangeStatePlayer_Click(object sender, RoutedEventArgs e)
        {
            SwitchPlayerState();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
        #endregion
    }

    public class MoviePlayerSourceTracker : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly MediaElement _moviePlayer;

        private Uri _moviePlayerSource;
        public Uri MoviePlayerSource
        {
            get => _moviePlayerSource;
            set
            {
                if (value != _moviePlayerSource)
                {
                    _moviePlayerSource = value;
                    OnPropertyChanged(nameof(MoviePlayerSource));
                }
            }
        }

        public MoviePlayerSourceTracker(MediaElement moviePlayer)
        {
            _moviePlayer = moviePlayer;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(_moviePlayer, new PropertyChangedEventArgs(propertyName));
        }
    }
}
