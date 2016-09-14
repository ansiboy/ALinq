using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal static class TypeSystem
    {
        // Fields
        private static ILookup<string, MethodInfo> _queryMethods;
        private static ILookup<string, MethodInfo> _sequenceMethods;

        // Methods
        private static bool ArgsMatchExact(MethodInfo m, Type[] argTypes, Type[] typeArgs)
        {
            ParameterInfo[] parameters = m.GetParameters();
            if (parameters.Length != argTypes.Length)
            {
                return false;
            }
            if ((!m.IsGenericMethodDefinition && m.IsGenericMethod) && m.ContainsGenericParameters)
            {
                m = m.GetGenericMethodDefinition();
            }
            if (m.IsGenericMethodDefinition)
            {
                if ((typeArgs == null) || (typeArgs.Length == 0))
                {
                    return false;
                }
                if (m.GetGenericArguments().Length != typeArgs.Length)
                {
                    return false;
                }
                m = m.MakeGenericMethod(typeArgs);
                parameters = m.GetParameters();
            }
            else if ((typeArgs != null) && (typeArgs.Length > 0))
            {
                return false;
            }
            int index = 0;
            int length = argTypes.Length;
            while (index < length)
            {
                Type parameterType = parameters[index].ParameterType;
                if (parameterType == null)
                {
                    return false;
                }
                Type c = argTypes[index];
                if (!parameterType.IsAssignableFrom(c))
                {
                    return false;
                }
                index++;
            }
            return true;
        }

        private static Type FindIEnumerable(Type seqType)
        {
            if ((seqType != null) && (seqType != typeof(string)))
            {
                if (seqType.IsArray)
                {
                    return typeof(IEnumerable<>).MakeGenericType(new[] { seqType.GetElementType() });
                }
                if (seqType.IsGenericType)
                {
                    foreach (Type type in seqType.GetGenericArguments())
                    {
                        Type type2 = typeof(IEnumerable<>).MakeGenericType(new[] { type });
                        if (type2.IsAssignableFrom(seqType))
                        {
                            return type2;
                        }
                    }
                }
                Type[] interfaces = seqType.GetInterfaces();
                if ((interfaces != null) && (interfaces.Length > 0))
                {
                    foreach (Type type3 in interfaces)
                    {
                        Type type4 = FindIEnumerable(type3);
                        if (type4 != null)
                        {
                            return type4;
                        }
                    }
                }
                if ((seqType.BaseType != null) && (seqType.BaseType != typeof(object)))
                {
                    return FindIEnumerable(seqType.BaseType);
                }
            }
            return null;
        }

        internal static MethodInfo FindQueryableMethod(string name, Type[] args, params Type[] typeArgs)
        {
            if (_queryMethods == null)
            {
                _queryMethods =
                    typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).ToLookup(m => m.Name);
            }
            MethodInfo info =
                _queryMethods[name].FirstOrDefault(m => ArgsMatchExact(m, args, typeArgs));
            if (info == null)
            {
                throw Error.NoMethodInTypeMatchingArguments(typeof(Queryable));
            }
            if (typeArgs != null)
            {
                return info.MakeGenericMethod(typeArgs);
            }
            return info;
        }

        internal static MethodInfo FindSequenceMethod(string name, IEnumerable sequence)
        {
            return FindSequenceMethod(name, new[] { sequence.GetType() },
                                      new[] { GetElementType(sequence.GetType()) });
        }

        internal static MethodInfo FindSequenceMethod(string name, Type[] args, params Type[] typeArgs)
        {
            if (_sequenceMethods == null)
            {
                _sequenceMethods =
                    typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static).ToLookup(m => m.Name);
            }
            var info = _sequenceMethods[name].FirstOrDefault(m => ArgsMatchExact(m, args, typeArgs));
            if (info == null)
            {
                return null;
            }
            if (typeArgs != null)
            {
                return info.MakeGenericMethod(typeArgs);
            }
            return info;
        }

        internal static MethodInfo FindStaticMethod(Type type, string name, Type[] args, params Type[] typeArgs)
        {
            var bf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            var info = type.GetMethods(bf).FirstOrDefault(m => (m.Name == name) && ArgsMatchExact(m, args, typeArgs));
            if (info == null)
            {
                throw Error.NoMethodInTypeMatchingArguments(type);
            }
            if (typeArgs != null)
            {
                return info.MakeGenericMethod(typeArgs);
            }
            return info;
        }

        internal static IEnumerable<FieldInfo> GetAllFields(Type type, BindingFlags flags)
        {
            var dictionary = new Dictionary<MetaPosition, FieldInfo>();
            Type baseType = type;
            do
            {
                foreach (FieldInfo info in baseType.GetFields(flags))
                {
                    if (info.IsPrivate || (type == baseType))
                    {
                        var position = new MetaPosition(info);
                        dictionary[position] = info;
                    }
                }
                baseType = baseType.BaseType;
            } while (baseType != null);
            return dictionary.Values;
        }

        internal static IEnumerable<PropertyInfo> GetAllProperties(Type type, BindingFlags flags)
        {
            var dictionary = new Dictionary<MetaPosition, PropertyInfo>();
            Type baseType = type;
            do
            {
                foreach (var info in baseType.GetProperties(flags))
                {
                    if ((type == baseType) || IsPrivate(info))
                    {
                        var position = new MetaPosition(info);
                        dictionary[position] = info;
                    }
                }
                baseType = baseType.BaseType;
            } while (baseType != null);
            return dictionary.Values;
        }

        public static Type GetElementType(Type seqType)
        {
            Type type = FindIEnumerable(seqType);
            if (type == null)
            {
                return seqType;
            }
            return type.GetGenericArguments()[0];
        }

        internal static Type GetFlatSequenceType(Type elementType)
        {
            Type type = FindIEnumerable(elementType);
            if (type != null)
            {
                return type;
            }
            return typeof(IEnumerable<>).MakeGenericType(new[] { elementType });
        }

        internal static Type GetMemberType(MemberInfo mi)
        {
            var info = mi as FieldInfo;
            if (info != null)
            {
                return info.FieldType;
            }
            var info2 = mi as PropertyInfo;
            if (info2 != null)
            {
                return info2.PropertyType;
            }
            var info3 = mi as EventInfo;
            if (info3 != null)
            {
                return info3.EventHandlerType;
            }
            var info4 = mi as MethodInfo;
            if (info4 != null)
            {
                return info4.ReturnType;
            }
            return null;
        }

        internal static Type GetNonNullableType(Type type)
        {
            if (IsNullableType(type))
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        internal static object GetDefaultValue(Type type)
        {
            var t = GetNonNullableType(type);
            if (t == typeof(DateTime))
                return DateTime.MinValue;//new DateTime(1000, 1, 1);

            if (t == typeof(string))
                return "";

            return 0;
        }

        public static Type GetSequenceType(Type elementType)
        {
            return typeof(IEnumerable<>).MakeGenericType(new[] { elementType });
        }

        internal static bool HasIEnumerable(Type seqType)
        {
            return (FindIEnumerable(seqType) != null);
        }

        internal static bool IsNullableType(Type type)
        {
            return (((type != null) && type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        internal static bool IsNullAssignable(Type type)
        {
            if (type.IsValueType)
            {
                return IsNullableType(type);
            }
            return true;
        }

        private static bool IsPrivate(PropertyInfo pi)
        {
            MethodInfo info = pi.GetGetMethod() ?? pi.GetSetMethod();
            if (info != null)
            {
                return info.IsPrivate;
            }
            return true;
        }

        internal static bool IsSequenceType(Type seqType)
        {
            return ((((seqType != typeof(string)) && (seqType != typeof(byte[]))) && (seqType != typeof(char[]))) &&
                    (FindIEnumerable(seqType) != null));
        }

        public static bool IsSimpleType(Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                type = type.GetGenericArguments()[0];
            }
            if (type.IsEnum)
            {
                return true;
            }
            if (type == typeof(Guid))
            {
                return true;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return true;
            }
            return false;
        }
    }
}