using System;
using System.Runtime.Serialization;

namespace MonGen.ParserCombinators
{
    public class ParserFailedException : Exception
    {
        public int Pos { get; }

        public string Str { get; }

        public ParserFailedException(string str, int pos)
        {
            this.Str = str;
            this.Pos = pos;
        }
    }
}