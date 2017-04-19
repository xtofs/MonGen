using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MonGen.Tests
{
    public class RandomExtensionsTests
    {
        [Fact]
        public void AllElementsPresentAfterShuffle()
        {
            var input = Enumerable.Range(0, 16).ToList();
            var rng = new Random();

            var perm = rng.Shuffle(input);

            Assert.Equal(new HashSet<int>(input), new HashSet<int>(perm), new SetComparer<int>() );            

        }

        private class SetComparer<T> : IEqualityComparer<ISet<T>>
        {
            public bool Equals(ISet<T> x, ISet<T> y)
            {
                return x.SetEquals(y);
            }

            public int GetHashCode(ISet<T> obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}