using System;

namespace MonoGen.ParserCombinators
{
    public class ParserException : Exception
    {
        protected readonly int Pos;
        protected readonly string Str;

        public ParserException(string str, int pos)
        {
            this.Str = str;
            this.Pos = pos;
        }
    }
}