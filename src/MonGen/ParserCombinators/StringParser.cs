namespace MonGen.ParserCombinators
{
    /// <summary>
    ///     the type of a parser that returns a value of type T
    /// </summary>
    /// <typeparam name="T">type of value of sucessfully parsed</typeparam>
    /// <param name="str">the input string to parse</param>
    /// <param name="pos">the current position in the input string</param>
    /// <returns>
    ///     Either an IParserOutput which represents the sucessfull outcome and the next position in the input 
    ///     or throws and ParserException
    /// </returns>
    public delegate IParserOutput<T> StringParser<out T>(string str, int pos);
}