using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGen.ParserCombinators;

namespace MonGen.ParserCombinators
{

    class ParserException : Exception
    {
        public ParserException(string message, ParserFailedException innerException) : base(message, innerException)
        {
            (Line, Column) = GetLineAndColumFromPosition(innerException.Str, innerException.Pos);
        }

        public int Line { get; }

        public int Column { get; }

        private static (int, int) GetLineAndColumFromPosition(string str, int pos)
        {
            var (line, column) = (1, 1);
            for (var i = 0; i < str.Length; i++)
            {
                column += 1;
                if (str[i] == '\n')
                {
                    line++; column = 1;
                }
            }
            return (line, column);
        }
    }
}
