using System;

namespace MonoGen
{
    public static class RandomExtensions
    {
        public static long GetNextInt64(this Random rng, long min, long max)
        {
            var buf = new byte[8];
            rng.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand % (max - min)) + min;
        }
    }
}