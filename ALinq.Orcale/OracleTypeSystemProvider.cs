using System.Data.Common;
using System.Data.OracleClient;
using System.Diagnostics;
using System.Data;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    [System.Obsolete]
    class OracleTypeSystemProvider : SqlTypeSystem.Sql2000Provider
    {
        public override IProviderType Parse(string stype)
        {
            if (stype == "Cursor")
                return new SqlTypeSystem.SqlType(SqlDbType.Udt);
            return base.Parse(stype);
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

            parameter2.OracleType = ToOracleType((SqlDbType)type2.SqlDbType);
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

        OracleType ToOracleType(SqlDbType sqlDbType)
        {
            switch (sqlDbType)
            {
                case SqlDbType.BigInt:
                    return OracleType.Number;
                case SqlDbType.Binary:
                    return OracleType.Blob;
                case SqlDbType.Bit:
                    return OracleType.Number;
                case SqlDbType.Char:
                    return OracleType.Char;
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                    return OracleType.DateTime;
                case SqlDbType.DateTimeOffset:
                    return OracleType.Timestamp;
                case SqlDbType.Decimal:
                    return OracleType.Number;
                case SqlDbType.Float:
                    return OracleType.Float;
                case SqlDbType.Image:
                    return OracleType.Raw;
                case SqlDbType.Int:
                    return OracleType.Number;
                case SqlDbType.Money:
                    return OracleType.Number;
                case SqlDbType.NChar:
                    return OracleType.NChar;
                case SqlDbType.NText:
                    return OracleType.NClob;
                case SqlDbType.NVarChar:
                    return OracleType.NVarChar;
                case SqlDbType.Real:
                    return OracleType.Number;
                case SqlDbType.SmallDateTime:
                    return OracleType.DateTime;
                case SqlDbType.SmallInt:
                    return OracleType.Int16;
                case SqlDbType.SmallMoney:
                    return OracleType.Number;
                case SqlDbType.Text:
                    return OracleType.Clob;
                case SqlDbType.Time:
                    return OracleType.DateTime;
                case SqlDbType.Timestamp:
                    break;
                case SqlDbType.TinyInt:
                    return OracleType.Byte;
                case SqlDbType.Udt:
                    return OracleType.Cursor;
                case SqlDbType.UniqueIdentifier:
                    return OracleType.Raw;
                case SqlDbType.VarBinary:
                    return OracleType.Raw;
                case SqlDbType.VarChar:
                    return OracleType.VarChar;
                case SqlDbType.Variant:
                    return OracleType.Raw;
                case SqlDbType.Xml:
                    return OracleType.VarChar;
                default:
                    return OracleType.VarChar;
            }
            return OracleType.VarChar;
        }
    }
}

