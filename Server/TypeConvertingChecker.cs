using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class TypeConvertingChecker
    {
        public static bool MatchResultTypeAndExceptedType(Type sourceType, Type targetType)
        {
            if (sourceType.IsValueType)
                return Check(sourceType, targetType);
            else
                return targetType.IsAssignableFrom(sourceType);
        }
        public static bool Check(Type from, Type to)
        {
            Type converterType = typeof(ValueTypeConvertingChecker<,>).MakeGenericType(from, to);
            object instance = Activator.CreateInstance(converterType);
            //TypeConvertingChecker<from, to> typeConvertingChecker;
            return (bool)converterType.GetProperty("CanConvert").GetGetMethod().Invoke(instance, null);
        }
        public class ValueTypeConvertingChecker<TFrom, TTo>
        {
            public bool CanConvert { get; private set; }
            public ValueTypeConvertingChecker()
            {
                TFrom from = default(TFrom);
                //if (from == null)
                //    if (typeof(TFrom).Equals(typeof(String)))
                //        from = (TFrom)(dynamic)"";
                //    else from = (TFrom)Activator.CreateInstance(typeof(TFrom));
                try
                {
                    TTo to = (dynamic)from;
                    CanConvert = true;
                }
                catch
                {
                    CanConvert = false;
                }
            }
        }
    }
}
