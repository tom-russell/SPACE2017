using System;

namespace Assets.Scripts.Extensions
{
    public static class CommonExtensions
    {
        public static string ToOutputString(this TimeSpan time)
        {
            return string.Format("{0:HH:mm:ss}", new DateTime(time.Ticks));
        }
    }
}
