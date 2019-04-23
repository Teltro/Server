using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Http : Attribute
    {
        public string Type;
        public Http(string type)
        {
            Type = type;
        }
    }
}
