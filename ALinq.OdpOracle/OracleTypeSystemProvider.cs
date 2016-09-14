using System;
using System.Data.Common;
using System.Diagnostics;
using System.Data;
using System.Xml.Linq;
using ALinq.SqlClient;
using Oracle.DataAccess.Client;

namespace ALinq.Oracle.Odp
{
    class OracleTypeSystemProvider : SqlTypeSystem.Sql2000Provider
    {
        public override IProviderType Parse(string stype)
        {
            if (stype == "Cursor")
                return new SqlTypeSystem.SqlType(SqlDbType.Udt);
            return base.Parse(stype);
        }

        public override IProviderType ReturnTypeOfFunction(SqlFunctionCall functionCall)
        {
            switch (functionCall.Name)
            {
                case "TO_BYTE":
                    return SqlTypeSystem.Create(SqlDbType.Decimal);
            }
            return base.ReturnTypeOfFunction(functionCall);
        }

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
                            return base.GetBestType(SqlDbType.VarBinary, size);
                        }
                        if (type == typeof(char[]))
                        {
                            return base.GetBestType(SqlDbType.NVarChar, size);
                        }
                        if (type == typeof(TimeSpan))
                        {
                            return SqlTypeSystem.Create(SqlDbType.BigInt);
                        }
                        if ((type != typeof(XDocument)) && (type != typeof(XElement)))
                        {
                            return new SqlTypeSystem.SqlType(type);
                        }
                        return SqlTypeSystem.theNText;
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
                        return SqlTypeSystem.Create(SqlDbType.Decimal, 0x1d, nullable.HasValue ? nullable.GetValueOrDefault() : 4);
                    }
                case TypeCode.DateTime:
                    return SqlTypeSystem.Create(SqlDbType.DateTime);

                case TypeCode.String:
                    return base.GetBestType(SqlDbType.NVarChar, size);
            }
            throw SqlClient.Error.UnexpectedTypeCode(typeCode);
        }

        public override void InitializeParameter(IProviderType type, DbParameter parameter, object value)
        {

            var parameter2 = parameter as OracleParameter;//System.Data.SqlClient.SqlParameter;
            if (parameter2 == null)
            {
                base.InitializeParameter(type, parameter, value);
                return;
            }
            var type2 = (SqlTypeSystem.SqlType)type;
            if (type2.IsRuntimeOnlyType)
            {
                throw SqlClient.Error.BadParameterType(type2.GetClosestRuntimeType());
            }
            Debug.Assert(parameter2 != null);


            parameter2.OracleDbType = ToOracleType((SqlDbType)type2.SqlDbType);
            if (type2.HasPrecisionAndScale)
            {
                parameter2.Precision = (byte)type2.Precision;
                parameter2.Scale = (byte)type2.Scale;
            }

            if (type2.HasPrecisionAndScale)
            {
                parameter2.Precision = (byte)type2.Precision;
                parameter2.Scale = (byte)type2.Scale;
            }

            parameter.Value = GetParameterValue(type2, value);
            if (!((parameter.Direction == ParameterDirection.Input) && type2.IsFixedSize) &&
                (parameter.Direction == ParameterDirection.Input))
                return;

            if (type2.Size.HasValue)
            {
                if (parameter.Size < type2.Size)
                {
                    parameter.Size = type2.Size.Value;
                }
            }
            if (!type2.IsLargeType)
            {
                return;
            }
        }

        OracleDbType ToOracleType(SqlDbType sqlDbType)
        {
            switch (sqlDbType)
            {
                case SqlDbType.BigInt:
                    return OracleDbType.Decimal;
                case SqlDbType.Binary:
                    return OracleDbType.Blob;
                case SqlDbType.Bit:
                    return OracleDbType.Byte;
                case SqlDbType.Char:
                    return OracleDbType.Char;
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                    return OracleDbType.Date;

                case SqlDbType.DateTimeOffset:
                    break;
                case SqlDbType.Decimal:
                    return OracleDbType.Decimal;
                case SqlDbType.Float:
                    return OracleDbType.Double;
                case SqlDbType.Image:
                    return OracleDbType.LongRaw;
                case SqlDbType.Int:
                    return OracleDbType.Int32;
                case SqlDbType.Money:
                    return OracleDbType.Decimal;
                case SqlDbType.NChar:
                    return OracleDbType.NChar;
                case SqlDbType.NText:
                    return OracleDbType.NClob;
                case SqlDbType.NVarChar:
                    return OracleDbType.NVarchar2;
                case SqlDbType.Real:
                    return OracleDbType.Single;
                case SqlDbType.SmallDateTime:
                    return OracleDbType.Date;
                case SqlDbType.SmallInt:
                    return OracleDbType.Int16;
                case SqlDbType.SmallMoney:
                    return OracleDbType.Decimal;
                case SqlDbType.Text:
                    return OracleDbType.Clob;
                case SqlDbType.Time:
                    return OracleDbType.Date;
                case SqlDbType.Timestamp:
                    return OracleDbType.TimeStamp;
                case SqlDbType.TinyInt:
                    return OracleDbType.Byte;
                case SqlDbType.Udt:
                    return OracleDbType.RefCursor;
                case SqlDbType.UniqueIdentifier:
                    return OracleDbType.Raw;
                case SqlDbType.VarBinary:
                    return OracleDbType.LongRaw;
                case SqlDbType.VarChar:
                    return OracleDbType.Varchar2;
                case SqlDbType.Variant:
                    return OracleDbType.LongRaw;
                case SqlDbType.Xml:
                    return OracleDbType.Varchar2;
            }
            return OracleDbType.Varchar2;
        }

        protected override object GetParameterValue(SqlTypeSystem.SqlType type, object value)
        {
            if (value != null && value.GetType() == typeof(Guid))
            {
                return ((Guid)value).ToByteArray();
            }
            return base.GetParameterValue(type, value);
        }
    }
}

