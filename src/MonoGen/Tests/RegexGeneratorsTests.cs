using System;
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
        [InlineData("[A-Z]+")]
        [InlineData("[A-Z]*")]
        [InlineData("[A-Z]{2}")]
        [InlineData("[A-Z]{2,}")]
        [InlineData("[A-Z]{2,4}")]
        [InlineData("[A-Z-]")]
        [InlineData("[-A-Z-]{2}")]
        [InlineData("[A-Z-a-z]")]
        [InlineData("abc|def")]
        [InlineData("abc[A-Z]")]
        [InlineData("a(b|c)d")]
        public void RegexPatternGeneratorsTests(string regex)
        {
            // arrange: create a generator from regex
            var sut = Generators.Regex(regex);

            // act: generate a sequence of 100 items from generator
            var rng = new Random(0);
            var actual = sut.Sequence(100).Gen(rng);

            // assert: the generated string must match the regex, anchored at start and end
            output.WriteLine(string.Join(Environment.NewLine, actual));
            var re = new System.Text.RegularExpressions.Regex($"\\G{regex}$", System.Text.RegularExpressions.RegexOptions.Singleline);
            Assert.True(actual.All(s => re.IsMatch(s)));
        }

    }


    public class GeneratorsCombinatorsTests
    {
        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Birthday { get; set; }
        }

        public static IGenerator<Person> Gen =
            from i in Generators.Range(0, 100)
            from n in Generators.Regex("[A-Z][a-z]{2,16}")
            from d in Generators.Range(DateTime.Parse("1930-01-01T00:00:00Z"), DateTime.Parse("2017-01-01T00:00:00Z"))
            select new Person { Id = i, Name = n, Birthday = d };

    }
}
