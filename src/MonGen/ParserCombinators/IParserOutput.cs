namespace MonoGen.ParserCombinators
{
    public interface IParserOutput<out T>
    {
        T Value { get; }

        string Str { get; }

        int Pos { get; }
    }
}