using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Attributes
{
    /// <summary>
    /// Атрибут, определяющий параметр метода контроллера, 
    /// принимающий данные из тела запроса
    /// </summary>
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
