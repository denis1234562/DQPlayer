using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace DQPlayer
{
    public partial class MainWindow
    {
        private bool _isPlaying;

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
        }

        private void SetupControlTexts()
        {
            UpdateChangeStateContent();
        }

        private void SetupPlayer()
        {
            Player.LoadedBehavior = MediaState.Manual;
            Player.UnloadedBehavior = MediaState.Manual;
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

        #endregion

        #region Event Handlers
        private void bChangeStatePlayer_Click(object sender, RoutedEventArgs e)
        {
            SwitchPlayerState();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            bool? shown = fileDialog.ShowDialog();
            if (shown.HasValue && shown.Value)
            {
                Player.Source = new Uri(fileDialog.FileName);
                Player.Play();
                _isPlaying = true;
                UpdateChangeStateContent();
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string file = ((DataObject) e.Data).GetFileDropList()[0];
            Player.Source = new Uri(file);
            Player.Play();
            _isPlaying = true;
            UpdateChangeStateContent();
        }
        #endregion
    }
}
