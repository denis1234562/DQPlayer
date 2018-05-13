using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.Helpers.CustomControls
{
    public class ThumbDragSlider : Slider
    {
        public event DragStartedEventHandler DragStarted;
        public event DragDeltaEventHandler DragDelta;
        public event DragCompletedEventHandler DragCompleted;
        public event EventHandler<MouseEventArgs> ThumbMouseEnter;

        public ThumbDragSlider()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
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

        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            DragDelta?.Invoke(this, e);
        }

        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);
            DragCompleted?.Invoke(this, e);
        }
    }
}
