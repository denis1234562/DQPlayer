using System;
using System.Text;

namespace DQPlayer.Helpers.Extensions
{
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
}