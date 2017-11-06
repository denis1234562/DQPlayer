using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DQPlayer
{
    public static class Extensions
    {
        public static StringBuilder Prepend(this StringBuilder sb, string content)
        {
            return sb.Insert(0, content);
        }

        public static T Min<T>(params T[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var comparer = Comparer<T>.Default;
            switch (values.Length)
            {
                case 0: throw new ArgumentException();
                case 1: return values[0];
                case 2:
                    return comparer.Compare(values[0], values[1]) < 0
                        ? values[0]
                        : values[1];
                default:
                    T best = values[0];
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (comparer.Compare(values[i], best) < 0)
                        {
                            best = values[i];
                        }
                    }
                    return best;
            }
        }

        public static T Max<T>(params T[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var comparer = Comparer<T>.Default;
            switch (values.Length)
            {
                case 0: throw new ArgumentException();
                case 1: return values[0];
                case 2:
                    return comparer.Compare(values[0], values[1]) > 0
                        ? values[0]
                        : values[1];
                default:
                    T best = values[0];
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (comparer.Compare(values[i], best) > 0)
                        {
                            best = values[i];
                        }
                    }
                    return best;
            }
        }

        public static T SerializedClone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException(@"The type must be serializable.", nameof(source));
            }

            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }

    public static class TimespanExtensions
    {
        public static string ToShortString(this TimeSpan source)
        {
            //@"hh\:mm\:ss
            StringBuilder formattedTimespan = new StringBuilder(@"mm\:ss");
            if ((int)source.TotalHours > 0)
            {
                formattedTimespan.Prepend(@"hh\:");
            }
            if ((int)source.TotalDays > 0)
            {
                formattedTimespan.Prepend(@"dd\:");
            }
            return source.ToString(formattedTimespan.ToString());
        }
    }

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
            SetLeftMargin(target, target.Margin.Right + valueToAdd);
        }

        public static void AddToTopMargin(this FrameworkElement target, double valueToAdd)
        {
            SetLeftMargin(target, target.Margin.Top + valueToAdd);
        }

        public static void AddToBottomMargin(this FrameworkElement target, double valueToAdd)
        {
            SetLeftMargin(target, target.Margin.Bottom + valueToAdd);
        }

        public static void RemoveFromLeftMargin(this FrameworkElement target, double valueToRemove)
        {
            SetLeftMargin(target, target.Margin.Left - valueToRemove);
        }

        public static void RemoveFromRightMargin(this FrameworkElement target, double valueToRemove)
        {
            SetLeftMargin(target, target.Margin.Right - valueToRemove);
        }

        public static void RemoveFromTopMargin(this FrameworkElement target, double valueToRemove)
        {
            SetLeftMargin(target, target.Margin.Top - valueToRemove);
        }

        public static void RemoveFromBottomMargin(this FrameworkElement target, double valueToRemove)
        {
            SetLeftMargin(target, target.Margin.Bottom - valueToRemove);
        }
    }
}
