using System;

namespace DQPlayer.Helpers.Extensions
{
    public static class LoopUtilities
    {
        public static void Repeat(int count, Action action)
        {
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }
    }
}
