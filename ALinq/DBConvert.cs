using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ALinq;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using ALinq.SqlClient;

namespace ALinq
{
    /// <summary>
    /// Used internally to convert one type to another.
    /// </summary>
    public static class DBConvert
    {
        // Fields
        private static readonly Type[] StringArg = new[] { typeof(string) };

        /// <summary>
        /// Changes the specified value to the current type.
        /// </summary>
        /// <typeparam name="T">The type to change to.</typeparam>
        /// <param name="value">The object to be converted.</param>
        /// <returns>An object of the specified type that contains the converted value.</returns>
        public static T ChangeType<T>(object value)
        {
            return (T)ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Changes the specified value to the specified type.
        /// </summary>
        /// <param name="value">The object to be converted.</param>
        /// <param name="type">The type to convert the object to.</param>
        /// <returns>An object that contains the converted value of the specified type.</returns>
        public static object ChangeType(object value, Type type)
        {
            object obj3;
            if (value == null)
            {
                return null;
            }
            type = TypeSystem.GetNonNullableType(type);
            Type enumType = value.GetType();
            if (type.IsAssignableFrom(value.GetType()))
            {
                return value;
            }
            if (type == typeof(Binary))
            {
                byte[] buffer;
                if (enumType == typeof(byte[]))
                {
                    return new Binary((byte[])value);
                }
                if (enumType == typeof(Guid))
                {
                    var guid = (Guid)value;
                    return new Binary(guid.ToByteArray());
                }
                var formatter = new BinaryFormatter();
                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, value);
                    buffer = stream.ToArray();
                }
                return new Binary(buffer);
            }
            if (type == typeof(byte[]))
            {


                if (enumType == typeof(Binary))
                {
                    return ((Binary)value).ToArray();
                }
                if (enumType == typeof(Guid))
                {
                    var guid2 = (Guid)value;
                    return guid2.ToByteArray();
                }
                var formatter2 = new BinaryFormatter();
                using (var stream2 = new MemoryStream())
                {
                    formatter2.Serialize(stream2, value);
                    return stream2.ToArray();
                }
            }
            if (enumType == typeof(byte[]))
            {
                if (type == typeof(Guid))
                {
                    return new Guid((byte[])value);
                }
                var formatter3 = new BinaryFormatter();
                using (var stream3 = new MemoryStream((byte[])value))
                {
                    return ChangeType(formatter3.Deserialize(stream3), type);
                }
            }
            if (enumType == typeof(Binary))
            {
                if (type == typeof(Guid))
                {
                    return new Guid(((Binary)value).ToArray());
                }
                var formatter4 = new BinaryFormatter();
                using (var stream4 = new MemoryStream(((Binary)value).ToArray(), false))
                {
                    return ChangeType(formatter4.Deserialize(stream4), type);
                }
            }
            if (type.IsEnum)
            {
                if (enumType == typeof(string))
                {
                    string str = ((string)value).Trim();
                    return Enum.Parse(type, str);
                }
                return Enum.ToObject(type, Convert.ChangeType(value, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture));
            }
            if (enumType.IsEnum)
            {
                if (type == typeof(string))
                {
                    //return Enum.GetName(enumType, value);
                    return value.ToString();
                }
                return Convert.ChangeType(Convert.ChangeType(value, Enum.GetUnderlyingType(enumType), CultureInfo.InvariantCulture), type, CultureInfo.InvariantCulture);
            }
            if (type == typeof(TimeSpan))
            {
                if (enumType == typeof(string))
                {
                    return TimeSpan.Parse((string)value);
                }
                return new TimeSpan((long)Convert.ChangeType(value, typeof(long), CultureInfo.InvariantCulture));
            }
            if (enumType == typeof(TimeSpan))
            {
                if (type == typeof(string))
                {
                    return value.ToString();
                }
                var span = (TimeSpan)value;
                return Convert.ChangeType(span.Ticks, type, CultureInfo.InvariantCulture);
            }
            if ((type == typeof(string)) && !typeof(IConvertible).IsAssignableFrom(enumType))
            {
                if (enumType == typeof(char[]))
                {
                    return new string((char[])value);
                }
                return value.ToString();
            }
            if (enumType == typeof(string))
            {
                MethodInfo info;
                if (type == typeof(Guid))
                {
                    return new Guid((string)value);
                }
                if (type == typeof(char[]))
                {
                    return ((string)value).ToCharArray();
                }
                if ((type == typeof(XDocument)) && (((string)value) == string.Empty))
                {
                    return new XDocument();
                }
                if (!typeof(IConvertible).IsAssignableFrom(type) && ((info = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, StringArg, null)) != null))
                {
                    try
                    {
                        return info.Invoke(null, new[] { value });
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.GetBaseException();
                    }
                }
                return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
            if ((type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IQueryable<>))) && typeof(IEnumerable<>).MakeGenericType(new Type[] { type.GetGenericArguments()[0] }).IsAssignableFrom(enumType))
            {
                return ((IEnumerable)value).AsQueryable();
            }
            try
            {
                obj3 = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException)
            {
                throw Error.CouldNotConvert(value.GetType(), type);
            }
            return obj3;
        }
    }



}
