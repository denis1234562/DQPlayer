using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DQPlayer
{
    public static class Extensions
    {
        public static StringBuilder Prepend(this StringBuilder sb, string content)
        {
            return sb.Insert(0, content);
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
