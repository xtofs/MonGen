﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGen.DataGeneration;
using Xunit;
using Xunit.Abstractions;

namespace MonoGen.Tests
{
    public class RegexGeneratorsTests
    {
        private readonly ITestOutputHelper output;

        public RegexGeneratorsTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Theory]
        [InlineData("[A-Z]")]
        [InlineData("[A-Z-]")]
        [InlineData("[-A-Z-]{2}")]
        [InlineData("[A-Z-a-z]")]
        [InlineData("[A-Z]{2,3}")]
        [InlineData("abc|def")]
        [InlineData("abc[A-Z]")]
        [InlineData("a(b|c)d")]
        public void RegexPatternGeneratorsTests(string regex)
        {
            // arrange
            var sut = Generators.Regex(regex);

            // act
            var rng = new Random(0);
            var actual = sut.Sequence(100).Gen(rng);

            // assert
            output.WriteLine(string.Join("  ", actual));
            var re = new System.Text.RegularExpressions.Regex($"\\G{regex}$");
            Assert.True(actual.All(s => re.IsMatch(s)));
        }

    }
}