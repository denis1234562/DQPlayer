using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DQPlayer.CustomControls
{
    public class ThumbDragSlider : Slider
    {
        public event DragStartedEventHandler DragStarted;
        public event DragDeltaEventHandler DragDelta;
        public event DragCompletedEventHandler DragCompleted;

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
