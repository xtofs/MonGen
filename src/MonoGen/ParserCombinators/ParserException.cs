using System;

namespace MonoGen.ParserCombinators
{
    [Serializable]
    public class ParserException : Exception
    {
        protected int Pos { get; }
        protected string Str { get; }

        public ParserException(string str, int pos) : base($"Parser failed at {pos} in {str}")
        {
            this.Str = str;
            this.Pos = pos;
        }
    }
}