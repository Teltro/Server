using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace Server
{
    public static class JsonDeserializator
    {
        public static object Deserialize(Type targetType, string jsonStr)
        {
            Console.WriteLine("deserialize");
            //Type jsonConverter = typeof(JsonConvert);
            //object instance = Activator.CreateInstance(jsonConverter);
            //Console.WriteLine();

            var deserializeMethod = typeof(JsonConvert)
                                    .GetMethods()
                                    .Where(m => m.Name == "DeserializeObject"
                                        && m.GetParameters().Count() == 1
                                        && m.IsGenericMethod
                                    ).FirstOrDefault();
            Console.WriteLine("hot method");
            deserializeMethod.MakeGenericMethod(targetType);
            Console.WriteLine("make generic");
            object[] deserializeMethodParams = new object[1]; // ??
            deserializeMethodParams[0] = jsonStr;
            return deserializeMethod.Invoke(null, deserializeMethodParams);
        }
    }


}
