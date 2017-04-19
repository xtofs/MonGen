using MonGen.ParserCombinators;
using MonGen.RegexParsers;
using Xunit;
using Xunit.Abstractions;

namespace MonGen.Tests
{
    public class ParserTests
    {
        [Fact]
        public void TestAlternative()
        {
            var p = RegexAstParsers.Alternatives;

            var result = p.Parse("MS[0-9]{6}|abcd");

            var expected = new RegexParsers.Regex(
                new Sequence(
                    new Atom(new Literal("MS")),
                    new Atom(new Charset(new Charset.Range('0', '9')), new Multiplicity(6))),
                new Sequence(new Atom(new Literal("abcd"))));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TestExactRepetition()
        {
            var p = RegexAstParsers.Atom;

            var result = p.Parse("[0-9]{6}");

            var expected = new Atom(
                new Charset(new Charset.Range('0', '9')),
                new Multiplicity(6));


            Assert.Equal(expected, result);
        }

        [Fact]
        public void TestNoRepetition()
        {
            var p = RegexAstParsers.Atom;

            var result = p.Parse("[A-Za-z]");

            var expected = new Atom(
                new Charset(new Charset.Range('A', 'Z'), new Charset.Range('a', 'z')),
                Multiplicities.One);


            Assert.Equal(expected, result);
        }

        [Fact]
        public void TestRangeRepetition()
        {
            var p = RegexAstParsers.Atom;

            var result = p.Parse("[A-Za-z]{3,7}");

            var expected = new Atom(
                new Charset(new Charset.Range('A', 'Z'), new Charset.Range('a', 'z')),
                new Multiplicity(3, 7));


            Assert.Equal(expected, result);
        }

        [Fact]
        public void TestSequence()
        {
            var p = RegexAstParsers.Sequence;

            var result = p.Parse("MS[0-9]{6}");

            var expected = new Sequence(
                new Atom(new Literal("MS"), Multiplicities.One),
                new Atom(
                    new Charset(new Charset.Range('0', '9')),
                    new Multiplicity(6)));


            Assert.Equal(expected, result);
        }

        [Fact]
        public void TestNegatedCharsetRegex()
        {
            var p = RegexAstParsers.Charset;

            var actual = p.Parse("[^A-Z]");

            var expected =
                new Charset(new Charset.Range('A', 'Z')) { Negated = true };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestSampleRegex()
        {
            var p = RegexAstParsers.Alternatives;

            var actual = p.Parse("MS[0-9]{6}|(MSFT|LNKD)[1-9][0-9]{3,5}X");

            var expected = new RegexParsers.Regex(
                new Sequence(
                    new Atom(new Literal("MS")),
                    new Atom(
                        new Charset(new Charset.Range('0', '9')),
                        new Multiplicity(6))),
                new Sequence(
                    new Atom(new Group(new RegexParsers.Regex(new Sequence(new Atom(new Literal("MSFT"))),
                        new Sequence(new Atom(new Literal("LNKD")))))),
                    new Atom(new Charset(new Charset.Range('1', '9'))),
                    new Atom(new Charset(new Charset.Range('0', '9')), new Multiplicity(3, 5)),
                    new Atom(new Literal("X"))));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestEmbeddedAlternative()
        {
            var p = RegexAstParsers.Alternatives;

            var actual = p.Parse("a(b|c)d");

            var expected = new RegexParsers.Regex(
                new Sequence(
                    new Atom(new Literal("a")),
                    new Atom(
                        new Group(new RegexParsers.Regex(
                            new Sequence(new Atom(new Literal("b"))),
                            new Sequence(new Atom(new Literal("c")))))),

                    new Atom(new Literal("d"))));

            var x = expected[0][1];
            output.WriteLine(x.ToString());
            Assert.Equal(expected[0][1], actual[0][1]);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestRangeWithMinus()
        {
            var p = RegexAstParsers.Alternatives;

            var actual = p.Parse("[-A-Z]{2}");

            var expected = new RegexParsers.Regex(
                new Sequence(
                    new Atom(new Charset(new Charset.Single('-'), new Charset.Range('A', 'Z')),
                    new Multiplicity(2))));

            Assert.Equal(expected, actual);
        }
        [Fact]
        public void TestError()
        {
            var p = RegexAstParsers.Alternatives;

            var ex = Assert.Throws<ParserException>(() => {
                var actual = p.Parse("[a-z");
            });
            Assert.Equal(ex.Line, 1);
            Assert.Equal(ex.Column, 5);
        }


        [Theory]
        [InlineData("[A-Z]")]

        [InlineData("[a]")]
        [InlineData("[^a]")]
        public void TestSampleRegexsParsable(string regex)
        {
            var p = RegexAstParsers.Alternatives;

            var e = Record.Exception(() =>
            {
                p.Parse(regex);
            });

            Assert.Null(e);
        }

        private readonly ITestOutputHelper output;

        public ParserTests(ITestOutputHelper output)
        {
            this.output = output;
        }
    }
}