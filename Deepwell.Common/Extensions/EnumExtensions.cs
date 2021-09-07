using System;
using System.ComponentModel;
using System.Reflection;

namespace Deepwell.Common.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value, T defaultValue) where T: struct, IComparable, IFormattable, IConvertible
        {
            if (typeof(T).IsEnum == false)
                throw new ArgumentException("T must be an enumerated type");

            T response;

            if (System.Enum.TryParse(value, true, out response))
            {
                if (System.Enum.IsDefined(typeof(T), response))
                {
                    return response;
                }
            }

            return defaultValue;
        }

        public static string ToDescription<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes.IsNotNull() && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }
    } 
}
