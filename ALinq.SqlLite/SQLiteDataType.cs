using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.SQLite
{
    class SQLiteDataType : SqlDataType<DbType>, IProviderType
    {
        public SQLiteDataType(Dictionary<TypeCode, DbType> typeMapping)
            : base(typeMapping)
        {
        }


        public SQLiteDataType(Dictionary<TypeCode, DbType> typeMapping, DbType dbType)
            : base(typeMapping, dbType)
        {
            if(dbType == DbType.UInt64)
            {
                Precision = 29;
                Scale = 4;
            }
            else if(dbType == DbType.Decimal)
            {
                Precision = DEFAULT_DECIMAL_PRECISION;
                Scale = DEFAULT_DECIMAL_SCALE;
            }

        }

        public SQLiteDataType(Dictionary<TypeCode, DbType> typeMapping, DbType dbType, int precision, int scale)
            : base(typeMapping, dbType, precision, scale)
        {
        }

        public SQLiteDataType(Dictionary<TypeCode, DbType> typeMapping, DbType dbType, int size)
            : base(typeMapping, dbType, size)
        {
        }

        protected override int GetTypeCoercionPrecedence(DbType type)
        {
            switch ((DbType)type)
            {
                case DbType.Binary:
                    return 0;
                case DbType.AnsiString:
                    return 1;
                case DbType.AnsiStringFixedLength:
                    return 2;
                case DbType.String:
                    return 3;
                case DbType.StringFixedLength:
                    return 4;
                case DbType.Guid:
                    return 5;
                case DbType.Boolean:
                    return 6;
                case DbType.SByte:
                    return 7;
                case DbType.Byte:
                    return 8;
                case DbType.Int16:
                    return 9;
                case DbType.Int32:
                    return 10;
                case DbType.UInt16:
                    return 11;
                case DbType.Int64:
                    return 12;
                case DbType.UInt32:
                    return 13;
                case DbType.UInt64:
                    return 14;
                case DbType.Currency:
                    return 15;
                case DbType.Decimal:
                    return 16;
                case DbType.Single:
                    return 17;
                case DbType.Double:
                    return 18;
                case DbType.VarNumeric:
                    return 19;
                case DbType.Date:
                    return 20;
                case DbType.Time:
                    return 21;
                case DbType.DateTimeOffset:
                    return 22;
                case DbType.DateTime:
                    return 23;
                case DbType.DateTime2:
                    return 24;
                case DbType.Xml:
                    return 25;
                case DbType.Object:
                    return 26;
            }
            //FbDbType.Array
            throw SqlClient.Error.UnexpectedTypeCode(type);
        }

        private bool IsTypeKnownByProvider
        {
            get
            {
                return (!IsApplicationType && !IsRuntimeOnlyType);
            }
        }

        public override Type GetClosestRuntimeType()
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType;
            }

            if (this == SQLiteDataTypeProvider.Instance.GuidType)
                return typeof(Guid);

            switch ((DbType)SqlDbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return typeof(string);
                case DbType.Binary:
                    return typeof(Byte[]);
                case DbType.Boolean:
                    return typeof(bool);
                case DbType.Byte:
                    return typeof(byte);
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.VarNumeric:
                    return typeof(decimal);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Time:
                    return typeof(DateTime);
                case DbType.Double:
                    return typeof(double);
                case DbType.Guid:
                    return typeof(Guid);
                case DbType.Int16:
                    return typeof(short);
                case DbType.Int32:
                    return typeof(int);
                case DbType.Int64:
                    return typeof(long);
                case DbType.Object:
                    return typeof(object);
                case DbType.SByte:
                    return typeof(sbyte);
                case DbType.Single:
                    return typeof(Single);
                case DbType.UInt16:
                    return typeof(UInt16);
                case DbType.UInt32:
                    return typeof(UInt32);
                case DbType.UInt64:
                    return typeof(UInt64);
            }
            return typeof(object);
        }

        internal override DBTypeCategory Category
        {
            get
            {
                switch (SqlDbType)
                {
                    case DbType.Binary:
                        return DBTypeCategory.Binary;
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                    case DbType.String:
                    case DbType.StringFixedLength:
                        return DBTypeCategory.Text;
                    case DbType.Date:
                    case DbType.DateTime:
                    case DbType.DateTime2:
                    case DbType.DateTimeOffset:
                        return DBTypeCategory.DateTime;
                    //return TypeCategory.Image;
                    case DbType.Boolean:
                    case DbType.Byte:
                    case DbType.Currency:
                    case DbType.Decimal:
                    case DbType.Double:
                    case DbType.Int16:
                    case DbType.Int32:
                    case DbType.Int64:
                    case DbType.SByte:
                    case DbType.Single:
                    case DbType.UInt16:
                    case DbType.UInt32:
                    case DbType.UInt64:
                    case DbType.VarNumeric:
                        return DBTypeCategory.Numeric;
                    //return TypeCategory.Text;
                    //return TypeCategory.Udt;
                    case DbType.Guid:
                        return DBTypeCategory.Binary;
                    //return TypeCategory.Variant;
                    case DbType.Xml:
                        return DBTypeCategory.Text;
                }
                throw SqlClient.Error.UnexpectedTypeCode(this);
            }
        }

        public override bool HasPrecisionAndScale
        {
            get
            {
                switch (SqlDbType)
                {
                    case DbType.Currency:
                    case DbType.Decimal:
                    //case DbType.Double:
                        return true;
                }
                return false;
            }
        }

        public override bool IsFixedSize
        {
            get
            {
                switch ((DbType)SqlDbType)
                {
                    case DbType.AnsiString:
                    case DbType.Binary:
                    case DbType.String:
                    case DbType.VarNumeric:
                    case DbType.Xml:
                        return false;
                }
                return true;
            }
        }

        public override bool IsLargeType
        {
            get
            {
                switch ((DbType)SqlDbType)
                {
                    case DbType.Binary:
                    case DbType.Object:
                    case DbType.Xml:
                        return true;
                    case DbType.String:
                    case DbType.AnsiString:
                        if (Size <= 0)
                            return true;
                        break;
                }
                return false;
            }
        }

        public override bool IsNumeric
        {
            get
            {
                switch ((DbType)SqlDbType)
                {
                    case DbType.Boolean:
                    case DbType.Byte:
                    case DbType.Currency:
                    case DbType.Decimal:
                    case DbType.Double:
                    case DbType.Int16:
                    case DbType.Int32:
                    case DbType.Int64:
                    case DbType.SByte:
                    case DbType.Single:
                    case DbType.UInt16:
                    case DbType.UInt32:
                    case DbType.UInt64:
                        return true;
                }
                return false;
            }

        }

        public override string ToQueryString(QueryFormatOptions formatFlags)
        {
            switch (SqlDbType)
            {
                case DbType.Boolean:
                    return "Bit";
                case DbType.Binary:
                    return "Binary";
                case DbType.DateTime:
                    return "DateTime";
                case DbType.Double:
                    return "Float";
                case DbType.Guid:
                    return "UniqueIdentifier";
                case DbType.Int16:
                    return "SmallInt";
                case DbType.Int32:
                    return "Integer";
                case DbType.Int64:
                    return "BitInt";
                case DbType.SByte:
                    return "SmallInt";
                case DbType.Single:
                    return "Real";
                case DbType.String:
                    return string.Format("VarChar({0})", Size);
                case DbType.StringFixedLength:
                    return string.Format("Char({0})", Size);
                case DbType.UInt16:
                    return string.Format("Int");
                case DbType.UInt32:
                    return string.Format("BitInt");

                case DbType.Decimal:
                    return string.Format("Decimal({0},{1})", Precision, Scale);

            }
            return base.ToQueryString(formatFlags);
        }

    }
}
