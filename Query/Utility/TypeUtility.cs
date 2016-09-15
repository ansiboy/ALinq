using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Globalization;

namespace ALinq.Dynamic
{
    static class TypeUtility
    {
        public static IEnumerable<Type> SelfAndBaseTypes(Type type)
        {
            IEnumerable<Type> result;
            if (type.IsInterface)
            {
                List<Type> types = new List<Type>();
                AddInterface(types, type);
                result = types;
            }
            else
            {
                result = SelfAndBaseClasses(type);
            }
            return result;
        }

        private static IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
            yield break;
        }

        private static void AddInterface(List<Type> types, Type type)
        {
            if (!types.Contains(type))
            {
                types.Add(type);
                Type[] interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    Type t = interfaces[i];
                    AddInterface(types, t);
                }
            }
        }

        public static bool IsNullableType(Type type)
        {
            var flag = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            return flag;
        }

        public static Type GetNonNullableType(Type type)
        {
            var t = IsNullableType(type) ? type.GetGenericArguments()[0] : type;
            return t;
        }

        public static string GetTypeName(Type type)
        {
            Type baseType = GetNonNullableType(type);
            string s = baseType.Name;
            if (type != baseType)
            {
                s += '?';
            }
            return s;
        }

        public static bool IsSignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 2;
        }

        public static bool IsUnsignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 3;
        }

        public static int GetNumericTypeKind(Type type)
        {
            type = GetNonNullableType(type);
            int result;
            if (type.IsEnum)
            {
                result = 0;
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        {
                            result = 1;
                            break;
                        }
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        {
                            result = 2;
                            break;
                        }
                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        {
                            result = 3;
                            break;
                        }
                    default:
                        {
                            result = 0;
                            break;
                        }
                }
            }
            return result;
        }

        public static bool IsEnumType(Type type)
        {
            return GetNonNullableType(type).IsEnum;
        }

        public static bool IsCompatibleWith(Type source, Type target)
        {
            bool result;
            if (source == target)
            {
                result = true;
            }
            else
            {
                if (!target.IsValueType)
                {
                    result = target.IsAssignableFrom(source);
                }
                else
                {
                    Type st = TypeUtility.GetNonNullableType(source);
                    Type tt = TypeUtility.GetNonNullableType(target);
                    if (st != source && tt == target)
                    {
                        result = false;
                    }
                    else
                    {
                        TypeCode sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
                        TypeCode tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
                        switch (sc)
                        {
                            case TypeCode.SByte:
                                {
                                    switch (tc)
                                    {
                                        case TypeCode.SByte:
                                        case TypeCode.Int16:
                                        case TypeCode.Int32:
                                        case TypeCode.Int64:
                                        case TypeCode.Single:
                                        case TypeCode.Double:
                                        case TypeCode.Decimal:
                                            {
                                                result = true;
                                                return result;
                                            }
                                    }
                                    break;
                                }
                            case TypeCode.Byte:
                                {
                                    switch (tc)
                                    {
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
                                            {
                                                result = true;
                                                return result;
                                            }
                                    }
                                    break;
                                }
                            case TypeCode.Int16:
                                {
                                    switch (tc)
                                    {
                                        case TypeCode.Int16:
                                        case TypeCode.Int32:
                                        case TypeCode.Int64:
                                        case TypeCode.Single:
                                        case TypeCode.Double:
                                        case TypeCode.Decimal:
                                            {
                                                result = true;
                                                return result;
                                            }
                                    }
                                    break;
                                }
                            case TypeCode.UInt16:
                                {
                                    switch (tc)
                                    {
                                        case TypeCode.UInt16:
                                        case TypeCode.Int32:
                                        case TypeCode.UInt32:
                                        case TypeCode.Int64:
                                        case TypeCode.UInt64:
                                        case TypeCode.Single:
                                        case TypeCode.Double:
                                        case TypeCode.Decimal:
                                            {
                                                result = true;
                                                return result;
                                            }
                                    }
                                    break;
                                }
                            case TypeCode.Int32:
                                {
                                    switch (tc)
                                    {
                                        case TypeCode.Int32:
                                        case TypeCode.Int64:
                                        case TypeCode.Single:
                                        case TypeCode.Double:
                                        case TypeCode.Decimal:
                                            {
                                                result = true;
                                                return result;
                                            }
                                    }
                                    break;
                                }
                            case TypeCode.UInt32:
                                {
                                    switch (tc)
                                    {
                                        case TypeCode.UInt32:
                                        case TypeCode.Int64:
                                        case TypeCode.UInt64:
                                        case TypeCode.Single:
                                        case TypeCode.Double:
                                        case TypeCode.Decimal:
                                            {
                                                result = true;
                                                return result;
                                            }
                                    }
                                    break;
                                }
                            case TypeCode.Int64:
                                {
                                    switch (tc)
                                    {
                                        case TypeCode.Int64:
                                        case TypeCode.Single:
                                        case TypeCode.Double:
                                        case TypeCode.Decimal:
                                            {
                                                result = true;
                                                return result;
                                            }
                                    }
                                    break;
                                }
                            case TypeCode.UInt64:
                                {
                                    switch (tc)
                                    {
                                        case TypeCode.UInt64:
                                        case TypeCode.Single:
                                        case TypeCode.Double:
                                        case TypeCode.Decimal:
                                            {
                                                result = true;
                                                return result;
                                            }
                                    }
                                    break;
                                }
                            case TypeCode.Single:
                                {
                                    switch (tc)
                                    {
                                        case TypeCode.Single:
                                        case TypeCode.Double:
                                            {
                                                result = true;
                                                return result;
                                            }
                                    }
                                    break;
                                }
                            default:
                                {
                                    if (st == tt)
                                    {
                                        result = true;
                                        return result;
                                    }
                                    break;
                                }
                        }
                        result = false;
                    }
                }
            }
            return result;
        }

        public static int FindIndexer(Type type, Expression[] args, out MethodBase method)
        {
            foreach (Type t in SelfAndBaseTypes(type))
            {
                MemberInfo[] members = t.GetMembers();//t.GetDefaultMembers();
                if (members.Length != 0)
                {
                    IEnumerable<MethodBase> methods = members.
                        OfType<PropertyInfo>().
                        Select(p => (MethodBase)p.GetGetMethod()).
                        Where(m => m != null);
                    int count = ExpressionUtility.FindBestMethod(methods, args, out method);
                    if (count != 0) return count;
                }
            }
            method = null;
            return 0;
        }

        public static MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                                 (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type))
            {
                //MemberInfo[] members = t.FindMembers(MemberTypes.Property | MemberTypes.Field,
                //                                     flags, Type.FilterNameIgnoreCase, memberName);
                MemberInfo[] members = t.FindMembers(MemberTypes.Property | MemberTypes.Field,
                                                     flags, Type.FilterName, memberName);
                if (members.Length != 0) return members[0];
            }
            return null;
        }

        public static int CompareConversions(Type s, Type t1, Type t2)
        {
            int result;
            if (t1 == t2)
            {
                result = 0;
            }
            else
            {
                if (s == t1)
                {
                    result = 1;
                }
                else
                {
                    if (s == t2)
                    {
                        result = -1;
                    }
                    else
                    {
                        bool t1t2 = TypeUtility.IsCompatibleWith(t1, t2);
                        bool t2t = TypeUtility.IsCompatibleWith(t2, t1);
                        if (t1t2 && !t2t)
                        {
                            result = 1;
                        }
                        else
                        {
                            if (t2t && !t1t2)
                            {
                                result = -1;
                            }
                            else
                            {
                                if (TypeUtility.IsSignedIntegralType(t1) && TypeUtility.IsUnsignedIntegralType(t2))
                                {
                                    result = 1;
                                }
                                else
                                {
                                    if (TypeUtility.IsSignedIntegralType(t2) && TypeUtility.IsUnsignedIntegralType(t1))
                                    {
                                        result = -1;
                                    }
                                    else
                                    {
                                        result = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static bool IsNumericType(Type type)
        {
            return GetNumericTypeKind(type) != 0;
        }

        internal static Type GetElementType(Type collectionType)
        {
            Type elementType = collectionType.GetElementType();
            if (elementType != null)//&& elementType != typeof(char) && elementType != typeof(byte))
            {
                return elementType;
            }

            if (collectionType.IsGenericType)
            {
                var t = collectionType.GetGenericArguments()[0];
                var e = typeof(IEnumerable<>).MakeGenericType(t);
                if (e.IsAssignableFrom(collectionType))
                    return t;

            }

            return collectionType;

        }

        public static MethodInfo GetInstanceMethod<T>(Expression<Func<T, object>> expr)
        {
            Debug.Assert(expr.Body.NodeType == ExpressionType.Call);
            return ((MethodCallExpression)expr.Body).Method;
        }

        public static Type GetMemberReturnType(MemberInfo member)
        {
            Type memberType;
            if (member.MemberType == MemberTypes.Field)
                memberType = ((FieldInfo)member).FieldType;
            else if (member.MemberType == MemberTypes.Property)
                memberType = ((PropertyInfo)member).PropertyType;
            else if (member.MemberType == MemberTypes.Method)
                memberType = ((MethodInfo)member).ReturnType;
            else
                throw new NotImplementedException();

            return memberType;
        }

#if NET35
        static Type enumerableQueryType = typeof(System.Linq.Enumerable).Assembly.GetType("System.Linq.EnumerableQuery");
#else
        private static Type enumerableQueryType = typeof(System.Linq.EnumerableQuery);
#endif
        public static Type EnumerableQueryType
        {
            get
            {
                return enumerableQueryType;
            }
        }

        public static Type CreateGenericEnumerableQueryType(Type elementType)
        {
#if NET35
            var typeName = "System.Linq.EnumerableQuery`1";
            var type = typeof(System.Linq.Enumerable).Assembly.GetType(typeName);
            var result = type.MakeGenericType(elementType);
            return result;
#else
            var type = typeof(System.Linq.EnumerableQuery<>).MakeGenericType(elementType);
            return type;
#endif
        }

        public static IQueryable<TElement> CreateGenericEnumerableQueryInstance<TElement>(IEnumerable<TElement> items)
        {
            return (IQueryable<TElement>)CreateGenericEnumerableQueryInstance(items, typeof(TElement));
        }

        public static IQueryable CreateGenericEnumerableQueryInstance(IEnumerable items, Type elementType)
        {
#if NET35
            var type = CreateGenericEnumerableQueryType(elementType);

            //var c = type.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(elementType) });
            //var instance = c.Invoke(new object[] { items });

            var bf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance;
            var instance = Activator.CreateInstance(type, bf, null, new object[] { items }, CultureInfo.CurrentCulture);
            return (IQueryable)instance;
#else
            //return new System.Linq.EnumerableQuery(items);
            var type = CreateGenericEnumerableQueryType(elementType);
            var instance = Activator.CreateInstance(type, items);
            return (IQueryable)instance;
#endif
        }
    }
}
