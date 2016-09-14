using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    class OracleDataType : SqlDataType<OracleType>
    {
        public OracleDataType(Dictionary<TypeCode, OracleType> typeMapping)
            : base(typeMapping)
        {
        }

        public OracleDataType(Dictionary<TypeCode, OracleType> typeMapping, OracleType dbType)
            : base(typeMapping, dbType)
        {
            if (dbType == OracleType.Number)
            {
                Precision = 29;
                Scale = 4;
            }
        }

        public OracleDataType(Dictionary<TypeCode, OracleType> typeMapping, OracleType dbType, int precision, int scale)
            : base(typeMapping, dbType, precision, scale)
        {
        }

        public OracleDataType(Dictionary<TypeCode, OracleType> typeMapping, OracleType dbType, int size)
            : base(typeMapping, dbType, size)
        {
        }

        public override Type GetClosestRuntimeType()
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType;
            }
            if (this == OracleDataTypeProvider.Instance.GuidType)
                return typeof(Guid);

            switch (SqlDbType)
            {
                case OracleType.BFile:
                case OracleType.Blob:
                    return typeof(byte[]);
                case OracleType.Byte:
                    return typeof(byte);
                case OracleType.Char:
                case OracleType.NChar:
                case OracleType.VarChar:
                case OracleType.NVarChar:
                    if (Size == 1)
                        return typeof(char);
                    return typeof(string);

                case OracleType.NClob:
                case OracleType.Clob:
                    return typeof(string);

                case OracleType.Cursor:
                    return typeof(object);

                case OracleType.DateTime:
                    return typeof(DateTime);
                case OracleType.Double:
                    return typeof(double);
                case OracleType.Float:
                    return typeof(float);

                case OracleType.Int16:
                    return typeof(Int16);
                case OracleType.Int32:
                    return typeof(Int32);

                case OracleType.IntervalDayToSecond:
                    return typeof(TimeSpan);

                case OracleType.IntervalYearToMonth:
                    return typeof(Int32);

                case OracleType.LongRaw:
                    return typeof(Byte[]);

                case OracleType.LongVarChar:
                    return typeof(string);

                case OracleType.Number:
                    return typeof(decimal);

                case OracleType.Raw:
                    return typeof(byte[]);

                case OracleType.RowId:
                    return typeof(string);

                case OracleType.SByte:
                    return typeof(SByte);
                case OracleType.Timestamp:
                    return typeof(DateTime);
                case OracleType.TimestampLocal:
                    return typeof(DateTime);
                case OracleType.TimestampWithTZ:
                    return typeof(DateTime);
                case OracleType.UInt16:
                    return typeof(UInt16);
                case OracleType.UInt32:
                    return typeof(UInt32);
            }
            return typeof(object);
        }

        protected override int GetTypeCoercionPrecedence(OracleType type)
        {
            switch (SqlDbType)
            {
                case OracleType.Raw:
                    return 0;
                case OracleType.Blob:
                    return 1;
                case OracleType.LongRaw:
                    return 2;
                case OracleType.BFile:
                    return 3;

                case OracleType.Char:
                    return 4;
                case OracleType.VarChar:
                    return 5;
                case OracleType.LongVarChar:
                    return 6;
                case OracleType.NChar:
                    return 7;
                case OracleType.NVarChar:
                    return 8;
                case OracleType.RowId:
                    return 9;
                case OracleType.NClob:
                    return 10;
                case OracleType.Clob:
                    return 11;

                case OracleType.SByte:
                    return 12;
                case OracleType.Byte:
                    return 13;
                case OracleType.Int16:
                    return 14;
                case OracleType.Int32:
                    return 15;
                case OracleType.UInt16:
                    return 16;
                case OracleType.UInt32:
                    return 17;
                case OracleType.Float:
                    return 18;
                case OracleType.Double:
                    return 19;
                case OracleType.Number:
                    return 20;
                case OracleType.IntervalYearToMonth:
                    return 21;
                case OracleType.DateTime:
                    return 22;
                case OracleType.Timestamp:
                    return 23;
                case OracleType.TimestampLocal:
                    return 24;
                case OracleType.TimestampWithTZ:
                    return 25;
                case OracleType.IntervalDayToSecond:
                    return 26;

                case OracleType.Cursor:
                    return 27;
            }
            throw SqlClient.Error.UnexpectedTypeCode(type);
        }

        internal override DBTypeCategory Category
        {
            get
            {
                switch (SqlDbType)
                {
                    case OracleType.Raw:
                    case OracleType.Blob:
                    case OracleType.LongRaw:
                    case OracleType.BFile:
                        return DBTypeCategory.Binary;

                    case OracleType.Char:
                    case OracleType.VarChar:
                    case OracleType.LongVarChar:
                    case OracleType.NChar:
                    case OracleType.NVarChar:
                    case OracleType.RowId:
                    case OracleType.NClob:
                    case OracleType.Clob:
                        return DBTypeCategory.Text;

                    case OracleType.SByte:
                    case OracleType.Byte:
                    case OracleType.Int16:
                    case OracleType.Int32:
                    case OracleType.UInt16:
                    case OracleType.UInt32:
                    case OracleType.Float:
                    case OracleType.Double:
                    case OracleType.Number:
                    case OracleType.IntervalYearToMonth:
                        return DBTypeCategory.Numeric;

                    case OracleType.DateTime:
                    case OracleType.Timestamp:
                    case OracleType.TimestampLocal:
                    case OracleType.TimestampWithTZ:
                    case OracleType.IntervalDayToSecond:
                        return DBTypeCategory.DateTime;

                    case OracleType.Cursor:
                        return DBTypeCategory.Cursor;
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
                    case OracleType.Number:
                        if (Precision >= 0 && Scale >= 0)
                            return true;
                        break;
                }
                return false;
            }
        }

        public override bool IsFixedSize
        {
            get
            {
                switch (SqlDbType)
                {
                    case OracleType.BFile:
                    case OracleType.Blob:
                    case OracleType.Clob:
                    case OracleType.LongRaw:
                    case OracleType.LongVarChar:
                    case OracleType.NVarChar:
                    case OracleType.Raw:
                    case OracleType.VarChar:
                        return false;
                }
                return true;
            }
        }

        public override bool IsLargeType
        {
            get
            {
                switch (SqlDbType)
                {
                    case OracleType.BFile:
                    case OracleType.Blob:
                    case OracleType.Clob:
                    case OracleType.NClob:
                        return true;

                    case OracleType.Char:
                    case OracleType.NChar:
                    case OracleType.NVarChar:
                    case OracleType.VarChar:
                        if (Size >= 2000)
                            return true;
                        return false;
                }
                return false;
            }
        }

        public override bool IsNumeric
        {
            get { return Category == DBTypeCategory.Numeric; }
        }

        public override bool IsUnicodeType
        {
            get
            {
                switch (SqlDbType)
                {
                    case OracleType.NChar:
                    case OracleType.NVarChar:
                    case OracleType.NClob:
                        return true;
                }
                return base.IsUnicodeType;
            }
        }
    }
}
