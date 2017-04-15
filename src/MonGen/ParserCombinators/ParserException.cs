using System;
using System.Runtime.Serialization;

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Str", Str);
            info.AddValue("Pos", Pos);
            base.GetObjectData(info, context);
        }
    }
}