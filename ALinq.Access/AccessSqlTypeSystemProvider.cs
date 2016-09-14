using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Xml.Linq;
using ALinq.SqlClient;

namespace ALinq.Access
{
    [System.Obsolete]
    class AccessTypeSystemProvider : SqlTypeSystem.ProviderBase
    {
        public override IProviderType From(Type type, int? size)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                type = type.GetGenericArguments()[0];
            }
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Object:
                    if (type != typeof(Guid))
                    {
                        if ((type == typeof(byte[])) || (type == typeof(Binary)))
                        {
                            return GetBestType(SqlDbType.VarBinary, size);
                        }
                        if (type == typeof(char[]))
                        {
                            return GetBestType(SqlDbType.NVarChar, size);
                        }
                        if (type == typeof(TimeSpan))
                        {
                            return SqlTypeSystem.Create(SqlDbType.BigInt);
                        }
                        if ((type != typeof(XDocument)) && (type != typeof(XElement)))
                        {
                            return new SqlTypeSystem.SqlType(type);
                        }
                        return SqlTypeSystem.theXml;
                    }
                    return SqlTypeSystem.Create(SqlDbType.UniqueIdentifier);

                case TypeCode.Boolean:
                    return SqlTypeSystem.Create(SqlDbType.Bit);

                case TypeCode.Char:
                    return SqlTypeSystem.Create(SqlDbType.NChar, 1);

                case TypeCode.SByte:
                case TypeCode.Int16:
                    return SqlTypeSystem.Create(SqlDbType.SmallInt);

                case TypeCode.Byte:
                    return SqlTypeSystem.Create(SqlDbType.TinyInt);

                case TypeCode.UInt16:
                case TypeCode.Int32:
                    return SqlTypeSystem.Create(SqlDbType.Int);

                case TypeCode.UInt32:
                case TypeCode.Int64:
                    return SqlTypeSystem.Create(SqlDbType.BigInt);

                case TypeCode.UInt64:
                    return SqlTypeSystem.Create(SqlDbType.Decimal, 20, 0);

                case TypeCode.Single:
                    return SqlTypeSystem.Create(SqlDbType.Real);

                case TypeCode.Double:
                    return SqlTypeSystem.Create(SqlDbType.Float);

                case TypeCode.Decimal:
                    {
                        int? nullable = size;
                        return SqlTypeSystem.Create(SqlDbType.Real, 0x1d, nullable.HasValue ? nullable.GetValueOrDefault() : 4);
                    }
                case TypeCode.DateTime:
                    return SqlTypeSystem.Create(SqlDbType.DateTime);

                case TypeCode.String:
                    return base.GetBestType(SqlDbType.NVarChar, size);
            }
            throw SqlClient.Error.UnexpectedTypeCode(typeCode);

        }

        public override IProviderType GetBestLargeType(IProviderType type)
        {
            var type2 = (SqlTypeSystem.SqlType)type;
            switch ((SqlDbType)type2.SqlDbType)
            {
                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.VarBinary:
                    return SqlTypeSystem.Create(SqlDbType.VarBinary, -1);

                case SqlDbType.Bit:
                    return type;

                case SqlDbType.Char:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                    return SqlTypeSystem.Create(SqlDbType.VarChar, -1);

                case SqlDbType.Int:
                case SqlDbType.Money:
                    return type;

                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                    return SqlTypeSystem.Create(SqlDbType.NVarChar, -1);

                case SqlDbType.Timestamp:
                case SqlDbType.TinyInt:
                    return type;
            }
            return type;
        }

        public override IProviderType GetBestType(IProviderType typeA, IProviderType typeB)
        {
            SqlTypeSystem.SqlType type = (typeA.ComparePrecedenceTo(typeB) > 0) ? ((SqlTypeSystem.SqlType)typeA) : ((ALinq.SqlClient.SqlTypeSystem.SqlType)typeB);
            if (typeA.IsApplicationType || typeA.IsRuntimeOnlyType)
            {
                return typeA;
            }
            if (typeB.IsApplicationType || typeB.IsRuntimeOnlyType)
            {
                return typeB;
            }
            var type2 = (SqlTypeSystem.SqlType)typeA;
            var type3 = (SqlTypeSystem.SqlType)typeB;
            if ((type2.HasPrecisionAndScale && type3.HasPrecisionAndScale) && ((SqlDbType)type.SqlDbType == SqlDbType.Decimal))
            {
                int precision = type2.Precision;
                int scale = type2.Scale;
                int num3 = type3.Precision;
                int num4 = type3.Scale;
                if (((precision == 0) && (scale == 0)) && ((num3 == 0) && (num4 == 0)))
                {
                    return SqlTypeSystem.Create((SqlDbType)type.SqlDbType);
                }
                if ((precision == 0) && (scale == 0))
                {
                    return SqlTypeSystem.Create((SqlDbType)type.SqlDbType, num3, num4);
                }
                if ((num3 == 0) && (num4 == 0))
                {
                    return SqlTypeSystem.Create((SqlDbType)type.SqlDbType, precision, scale);
                }
                int num5 = Math.Max(precision - scale, num3 - num4);
                int num6 = Math.Max(scale, num4);
                int num7 = Math.Min(num5 + num6, 0x26);
                return SqlTypeSystem.Create((SqlDbType)type.SqlDbType, num7, num6);
            }
            int? size = null;
            if (type2.Size.HasValue && type3.Size.HasValue)
            {
                int? nullable4 = type3.Size;
                int? nullable5 = type2.Size;
                size = ((nullable4.GetValueOrDefault() > nullable5.GetValueOrDefault()) && (nullable4.HasValue & nullable5.HasValue)) ? type3.Size : type2.Size;
            }
            if ((type3.Size.HasValue && (type3.Size.Value == -1)) || (type2.Size.HasValue && (type2.Size.Value == -1)))
            {
                size = -1;
            }
            return new SqlTypeSystem.SqlType((SqlDbType)type.SqlDbType, size);
        }

        protected override bool SupportsMaxSize
        {
            get
            {
                return true;
            }
        }


        public override void InitializeParameter(IProviderType type, DbParameter parameter, object value)
        {
            base.InitializeParameter(type, parameter, value);
            if (value is DateTime)
                parameter.DbType = DbType.String;
            if (value is bool)
            {
                parameter.DbType = DbType.Boolean;
                parameter.Value = value;
            }
        }

    }
}