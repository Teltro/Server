using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Exceptions
{
    //Not used
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }
        public NotFoundException(string str) : base(str) { }
        public NotFoundException(string str, Exception inner) : base(str, inner) { }
        protected NotFoundException(
            System.Runtime.Serialization.SerializationInfo si,
            System.Runtime.Serialization.StreamingContext st
            ) : base(si, st) { }

        public override string ToString()
        {
            return Message;
        }
    }
}
