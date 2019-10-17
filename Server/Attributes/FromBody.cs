using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Attributes
{
    /// <summary>
    /// Атрибут, определяющий параметр метода контроллера, 
    /// принимающий данные из тела запроса
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromBody : System.Attribute
    {

    }
}
