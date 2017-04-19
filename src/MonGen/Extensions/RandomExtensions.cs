using System;
using System.Collections.Generic;

namespace MonGen
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

        /// <summary>
        /// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#The_.22inside-out.22_algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rng"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IReadOnlyList<T> Shuffle<T>(this Random rng, IReadOnlyList<T> source)
        {
            var result = new T[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                var j = rng.Next(0, i+1);
                if (i != j)
                    result[i] = result[j];
                result[j] = source[i];
            }
            return result;
        }        
    }
}