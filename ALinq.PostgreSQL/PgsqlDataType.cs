using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.SqlClient;
using NpgsqlTypes;

namespace ALinq.PostgreSQL
{
    class PgsqlDataType : SqlDataType<NpgsqlDbType>
    {
        public PgsqlDataType(Dictionary<TypeCode, NpgsqlDbType> typeMapping)
            : base(typeMapping)
        {

        }

        public PgsqlDataType(Dictionary<TypeCode, NpgsqlDbType> typeMapping, NpgsqlDbType dbType)
            : base(typeMapping, dbType)
        {
            if (dbType == NpgsqlDbType.Numeric)
            {
                Precision = 29;
                Scale = 4;
            }
        }

        public PgsqlDataType(Dictionary<TypeCode, NpgsqlDbType> typeMapping, NpgsqlDbType dbType, int precision, int scale)
            : base(typeMapping, dbType, precision, scale)
        {
        }

        public PgsqlDataType(Dictionary<TypeCode, NpgsqlDbType> typeMapping, NpgsqlDbType dbType, int size)
            : base(typeMapping, dbType, size)
        {
        }

        public override Type GetClosestRuntimeType()
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType;
            }
            if (this == PgsqlDataTypeProvider.Instance.GuidType)
                return typeof(Guid);

            switch (SqlDbType)
            {
                case NpgsqlDbType.Bigint:
                    return typeof(Int64);
                case NpgsqlDbType.Bit:
                case NpgsqlDbType.Boolean:
                    return typeof(bool);
                case NpgsqlDbType.Numeric:
                    return typeof(decimal);
                case NpgsqlDbType.Integer:
                    return typeof(int);
                case NpgsqlDbType.Real:
                case NpgsqlDbType.Double:
                    return typeof(double);
                case NpgsqlDbType.Smallint:
                    return typeof(Int16);

                case NpgsqlDbType.Char:
                case NpgsqlDbType.Varchar:
                    if (Size == 1)
                        return typeof(char);
                    return typeof(string);

                case NpgsqlDbType.Text:
                    return typeof(string);

                case NpgsqlDbType.Date:
                    return typeof(DateTime);
                case NpgsqlDbType.Interval:
                    return typeof(TimeSpan);

                case NpgsqlDbType.Uuid:
                    return typeof(Guid);
                case NpgsqlDbType.Array:
                    return typeof(object);
                case NpgsqlDbType.Box:
                    return typeof(object);
                case NpgsqlDbType.Bytea:
                    return typeof(byte[]);
                case NpgsqlDbType.Circle:
                    return typeof(object);
                case NpgsqlDbType.Inet:
                    return typeof(System.Net.IPAddress);
            }
            return typeof(object);
        }


        protected override int GetTypeCoercionPrecedence(NpgsqlDbType type)
        {
            switch (type)
            {
                case NpgsqlDbType.Bytea:
                    return 0;

                case NpgsqlDbType.Boolean:
                    return 4;

                case NpgsqlDbType.Smallint:
                    return 5;

                case NpgsqlDbType.Integer:
                    return 6;

                case NpgsqlDbType.Bigint:
                    return 7;

                case NpgsqlDbType.Real:
                    return 8;

                case NpgsqlDbType.Double:
                    return 9;

                case NpgsqlDbType.Numeric:
                    return 10;

                case NpgsqlDbType.Money:
                    return 11;

                case NpgsqlDbType.Time:
                    return 12;
                case NpgsqlDbType.Timestamp:
                    return 13;
                case NpgsqlDbType.TimestampTZ:
                    return 14;
                case NpgsqlDbType.TimeTZ:
                    return 15;

                case NpgsqlDbType.Array:
                    return 16;
                case NpgsqlDbType.Bit:
                    return 17;
                case NpgsqlDbType.Box:
                    return 18;
                case NpgsqlDbType.Circle:
                    return 19;
                case NpgsqlDbType.Date:
                    return 20;
                case NpgsqlDbType.Inet:
                    return 21;
                case NpgsqlDbType.Interval:
                    return 22;
                case NpgsqlDbType.Line:
                    return 23;
                case NpgsqlDbType.LSeg:
                    return 24;
                case NpgsqlDbType.Oidvector:
                    return 25;
                case NpgsqlDbType.Path:
                    return 26;
                case NpgsqlDbType.Point:
                    return 27;
                case NpgsqlDbType.Polygon:
                    return 28;
                case NpgsqlDbType.Refcursor:
                    return 29;
                case NpgsqlDbType.Uuid:
                    return 30;
                case NpgsqlDbType.Xml:
                    return 21;

                case NpgsqlDbType.Char:
                    return 100;
                case NpgsqlDbType.Varchar:
                    return 101;
                case NpgsqlDbType.Text:
                    return 102;
            }
            throw SqlClient.Error.UnexpectedTypeCode(type);
        }

        internal override DBTypeCategory Category
        {
            get
            {
                switch (SqlDbType)
                {
                    case NpgsqlDbType.Bytea:
                        return DBTypeCategory.Binary;

                    case NpgsqlDbType.Char:
                    case NpgsqlDbType.Varchar:
                    case NpgsqlDbType.Text:
                        return DBTypeCategory.Text;

                    case NpgsqlDbType.Boolean:
                    case NpgsqlDbType.Smallint:
                    case NpgsqlDbType.Integer:
                    case NpgsqlDbType.Bigint:
                    case NpgsqlDbType.Real:
                    case NpgsqlDbType.Double:
                    case NpgsqlDbType.Money:
                        return DBTypeCategory.Numeric;

                    case NpgsqlDbType.Time:
                    case NpgsqlDbType.Timestamp:
                    case NpgsqlDbType.TimestampTZ:
                    case NpgsqlDbType.TimeTZ:
                        return DBTypeCategory.DateTime;
                }
                return DBTypeCategory.Binary;
            }
        }

        public override bool HasPrecisionAndScale
        {
            get
            {
                switch (SqlDbType)
                {
                    case NpgsqlDbType.Numeric:
                        return true;
                }
                return false;
            }
        }

        public override bool IsFixedSize
        {
            get { return false; }
        }

        public override bool IsLargeType
        {
            get { return false; }
        }

        public override bool IsNumeric
        {
            get
            {
                switch (SqlDbType)
                {
                    case NpgsqlDbType.Bigint:
                    case NpgsqlDbType.Bit:
                    case NpgsqlDbType.Double:
                    case NpgsqlDbType.Integer:
                    case NpgsqlDbType.Money:
                    case NpgsqlDbType.Numeric:
                    case NpgsqlDbType.Real:
                    case NpgsqlDbType.Smallint:
                        return true;
                }
                return false;
            }
        }

        public override string ToQueryString(QueryFormatOptions formatFlags)
        {
            switch (SqlDbType)
            {
                case NpgsqlDbType.Char:
                    if (Size == STRING_SIZE)
                        return "Char(" + STRING_SIZE + ")";
                    return string.Format("Char({0})", Size);
                case NpgsqlDbType.Varchar:
                    if (Size == STRING_SIZE)
                        return "VarChar(" + STRING_SIZE + ")";
                    return string.Format("VarChar({0})", Size);
                case NpgsqlDbType.Numeric:
                    return string.Format("Decimal({0},{1})", Precision, Scale);
                //case NpgsqlDbType.Bytea:
                //    return "BYTEA";
                //case NpgsqlDbType.Bigint:
                //    return "INT8";
                //case NpgsqlDbType.Integer:
                //    return "INT4";
                //case NpgsqlDbType.Text:
                //    return "Text";
            }
            return base.ToQueryString(formatFlags);
        }
    }
}
