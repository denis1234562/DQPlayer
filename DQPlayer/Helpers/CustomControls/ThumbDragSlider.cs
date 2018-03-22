using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.Helpers.CustomControls
{
    public class ThumbDragSlider : Slider
    {
        public event DragStartedEventHandler DragStarted;
        public event DragCompletedEventHandler DragCompleted;
        public event EventHandler<MouseEventArgs> ThumbMouseEnter;

        public new TimeSpan Value
        {
            get => TimeSpan.FromSeconds(base.Value);
            set => base.Value = value.TotalSeconds;
        }

        public ThumbDragSlider()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            var track = this.GetElementFromTemplate<Track>("PART_Track");
            track.Thumb.MouseEnter += (o, args) => ThumbMouseEnter?.Invoke(o, args);
        }

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            DragStarted?.Invoke(this, e);
        }

        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);
            DragCompleted?.Invoke(this, e);
        }
    }
}
