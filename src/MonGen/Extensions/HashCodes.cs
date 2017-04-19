using System.Collections.Generic;
using System.Linq;

namespace MonGen
{
    internal static class HashCodes
    {
        internal static int CombineHashCodes(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }

        internal static int CombineHashCodes(int h1, int h2, int h3)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3), h4);
        }

        internal static int CombineHashCodes<T>(ICollection<T> items)
        {
            return items.Aggregate(0, (c,i) => CombineHashCodes(c, i.GetHashCode()));
        }
    }
}