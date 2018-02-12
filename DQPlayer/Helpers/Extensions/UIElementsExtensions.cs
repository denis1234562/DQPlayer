using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DQPlayer.Helpers.Extensions
{
    public static class UIElementsExtensions
    {
        //TODO some refactoring maybe with null checks

        public static double CalculateTrackDensity(this Track track)
        {
            double effectivePoints = Math.Max(0, track.Maximum - track.Minimum);
            double effectiveLength = track.Orientation == Orientation.Horizontal
                ? track.ActualWidth - track.Thumb.DesiredSize.Width
                : track.ActualHeight - track.Thumb.DesiredSize.Height;
            return effectivePoints / effectiveLength;
        }

        public static double SimulateTrackPosition(this Track track, Point point)
        {
            var simulatedPosition = (point.X - track.Thumb.DesiredSize.Width / 2) * CalculateTrackDensity(track);
            return Math.Min(Math.Max(simulatedPosition, 0), track.Maximum);
        }

        public static T GetElementFromTemplate<T>(this System.Windows.Controls.Control source, string name)
            where T : UIElement
        {
            return source.Template.FindName(name, source) as T;
        }

        public static void SetLeftMargin(this FrameworkElement target, double value)
        {
            target.Margin = new Thickness(value, target.Margin.Top, target.Margin.Right, target.Margin.Bottom);
        }

        public static void SetRightMargin(this FrameworkElement target, double value)
        {
            target.Margin = new Thickness(target.Margin.Left, target.Margin.Top, value, target.Margin.Bottom);
        }

        public static void SetTopMargin(this FrameworkElement target, double value)
        {
            target.Margin = new Thickness(target.Margin.Left, value, target.Margin.Right, target.Margin.Bottom);
        }

        public static void SetBottomMargin(this FrameworkElement target, double value)
        {
            target.Margin = new Thickness(target.Margin.Left, target.Margin.Top, target.Margin.Right, value);
        }

        public static void AddToLeftMargin(this FrameworkElement target, double valueToAdd)
        {
            SetLeftMargin(target, target.Margin.Left + valueToAdd);
        }

        public static void AddToRightMargin(this FrameworkElement target, double valueToAdd)
        {
            SetRightMargin(target, target.Margin.Right + valueToAdd);
        }

        public static void AddToTopMargin(this FrameworkElement target, double valueToAdd)
        {
            SetTopMargin(target, target.Margin.Top + valueToAdd);
        }

        public static void AddToBottomMargin(this FrameworkElement target, double valueToAdd)
        {
            SetBottomMargin(target, target.Margin.Bottom + valueToAdd);
        }

        public static void RemoveFromLeftMargin(this FrameworkElement target, double valueToRemove)
        {
            SetLeftMargin(target, target.Margin.Left - valueToRemove);
        }

        public static void RemoveFromRightMargin(this FrameworkElement target, double valueToRemove)
        {
            SetRightMargin(target, target.Margin.Right - valueToRemove);
        }

        public static void RemoveFromTopMargin(this FrameworkElement target, double valueToRemove)
        {
            SetTopMargin(target, target.Margin.Top - valueToRemove);
        }

        public static void RemoveFromBottomMargin(this FrameworkElement target, double valueToRemove)
        {
            SetBottomMargin(target, target.Margin.Bottom - valueToRemove);
        }
    }
}