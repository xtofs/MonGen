using System;
using System.Collections.Generic;
using System.Linq;
using MonGen.DataGeneration;
using MonGen.ParserCombinators;
using MonGen.RegexParsers;
using Xunit;
using Xunit.Abstractions;

namespace MonGen.Tests
{
    public class GeneratorsCombinatorsTests
    {
        private readonly ITestOutputHelper _output;

        public GeneratorsCombinatorsTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void EndToEnd()
        {
            var rng = new Random();

            var persons = Gen.Sequence(100).Gen(rng);
            Assert.Equal(100, persons.Count);
        }

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
            select new Person {Id = i, Name = n, Birthday = d};




        [Fact]
        public void PasswordGenerator()
        {
            var charsets = new[] {"a-z", "A-Z", "0-9", "!@#$"};

            for (int i = 8; i < 25; i++)
            {
                var gen = Generators.Password(i, charsets);
                var rng = new Random(0);

                // act
                var passwords = gen.Sequence(100000).Gen(rng);

                // assert

                //var stats = passwords.SelectMany(s => s)
                //    .GroupBy(c => c)
                //    .Select(grp => new KeyValuePair<char, int>(grp.Key, grp.Count()))
                //    .OrderBy(p => p.Value).ThenBy(p => p.Key);
                //_output.WriteLine(string.Join("\n", stats));

                var css = charsets.Select(cs => Tuple.Create(cs, RegexAstParsers.RawCharterSets.Parse(cs))).ToArray();

                var stats2 = passwords.SelectMany(s => s)
                    .GroupBy(c => css.First(cs => cs.Item2.Contains(c)))
                    .Select(grp => new KeyValuePair<string, int>(grp.Key.Item1, grp.Count()))
                    .ToPercentages()
                    .OrderBy(p => p.Value);

                _output.WriteLine(i.ToString());
                _output.WriteLine(string.Join("\n", stats2));
                _output.WriteLine(string.Empty);
            }
        }
    }


    internal static class EnumX
    {
        public static IEnumerable<KeyValuePair<T, double>> ToPercentages<T>(this IEnumerable<KeyValuePair<T, int>> items)
        {
            double sum = items.Sum(item => item.Value);
            return items.Select(item => new KeyValuePair<T, double>(item.Key, item.Value / sum));
        }
    }
}