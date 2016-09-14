using System;
using System.Data;
using ALinq;

namespace LinqToSqlShared.Mapping
{
    internal static class MappingSystem
    {
        // Methods
        internal static bool IsSupportedDiscriminatorType(SqlDbType type)
        {
            switch (type)
            {
                case SqlDbType.TinyInt:
                case SqlDbType.VarChar:
                case SqlDbType.SmallInt:
                case SqlDbType.BigInt:
                case SqlDbType.Bit:
                case SqlDbType.Char:
                case SqlDbType.Int:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                    return true;
            }
            return false;
        }

        internal static bool IsSupportedDiscriminatorType(Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                type = type.GetGenericArguments()[0];
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
                case TypeCode.String:
                    return true;
            }
            return false;
        }

        internal static bool IsSupportedIdentityType(SqlDbType type)
        {
            switch (type)
            {
                case SqlDbType.Variant:
                case SqlDbType.Xml:
                case SqlDbType.Udt:
                case SqlDbType.Text:
                case SqlDbType.Image:
                case SqlDbType.NText:
                    return false;
            }
            return true;
        }

        internal static bool IsSupportedIdentityType(Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                type = type.GetGenericArguments()[0];
            }
            if (((type == typeof(Guid)) || (type == typeof(DateTime))) || (((type == typeof(DateTimeOffset)) || (type == typeof(TimeSpan))) || (type == typeof(Binary))))
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
                case TypeCode.String:
                    return true;
            }
            return false;
        }
    }
}
