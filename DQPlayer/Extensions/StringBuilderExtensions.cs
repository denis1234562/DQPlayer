using System.Text;

namespace DQPlayer.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder Prepend(this StringBuilder sb, string content)
        {
            return sb.Insert(0, content);
        }
    }
}