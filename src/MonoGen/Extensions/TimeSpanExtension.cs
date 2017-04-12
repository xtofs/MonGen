using System;

namespace MonoGen
{
    internal static class TimeSpanExtension
    {
        public static TimeSpan Multiply(this TimeSpan duration, int n)
        {
            return TimeSpan.FromTicks(duration.Ticks * n);
        }

        public static long Divide(this TimeSpan duration, TimeSpan step)
        {
            return duration.Ticks / step.Ticks;
        }
    }
}