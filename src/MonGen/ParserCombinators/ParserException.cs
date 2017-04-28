using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonGen.ParserCombinators;

namespace MonGen.ParserCombinators
{

    public class ParserException : Exception
    {
        public ParserException(string message, ParserFailedException innerException) : base(message, innerException)
        {
            var lc = GetLineAndColumFromPosition(innerException.Str, innerException.Pos);
            Line = lc.Item1;
            Column = lc.Item2;
        }

        public int Line { get; }

        public int Column { get; }

        private static Tuple<int, int> GetLineAndColumFromPosition(string str, int pos)
        {
            var line = 1;
            var column = 1;
            foreach (char t in str)
            {
                column += 1;
                if (t == '\n')
                {
                    line++; column = 1;
                }
            }
            return Tuple.Create(line, column);
        }
    }
}
