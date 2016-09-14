using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.SqlClient;
using System.Data.OleDb;

namespace ALinq.Access
{
    class AccessDataType : SqlClient.SqlDataType<OleDbType>, IProviderType
    {
        public AccessDataType(Dictionary<TypeCode, OleDbType> typeMapping)
            : base(typeMapping)
        {
        }

        public AccessDataType(Dictionary<TypeCode, OleDbType> typeMapping, OleDbType dbType)
            : base(typeMapping, dbType)
        {
        }

        public AccessDataType(Dictionary<TypeCode, OleDbType> typeMapping, OleDbType dbType, int precision, int scale)
            : base(typeMapping, dbType, precision == NULL_VALUE ? 20 : precision, scale == NULL_VALUE ? 20 : scale)
        {
        }

        public AccessDataType(Dictionary<TypeCode, OleDbType> typeMapping, OleDbType dbType, int size)
            : base(typeMapping, dbType, size == NULL_VALUE ? 20 : size)
        {
        }

        public override Type GetClosestRuntimeType()
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType;
            }
            switch (SqlDbType)
            {
                case OleDbType.BigInt:
                    return typeof(long);
                case OleDbType.Binary:
                    return typeof(byte[]);
                case OleDbType.Boolean:
                    return typeof(bool);
                case OleDbType.BSTR:
                    return typeof(string);
                case OleDbType.Char:
                    return typeof(string);
                case OleDbType.Currency:
                    return typeof(decimal);
                case OleDbType.Date:
                    return typeof(DateTime);
                case OleDbType.DBDate:
                    return typeof(DateTime);
                case OleDbType.DBTime:
                    return typeof(TimeSpan);
                case OleDbType.DBTimeStamp:
                    return typeof(DateTime);
                case OleDbType.Decimal:
                    return typeof(decimal);
                case OleDbType.Double:
                    return typeof(double);
                case OleDbType.Error:
                    return typeof(Exception);
                case OleDbType.Filetime:
                    return typeof(DateTime);
                case OleDbType.Guid:
                    return typeof(Guid);
                case OleDbType.IDispatch:
                    return typeof(object);
                case OleDbType.Integer:
                    return typeof(int);
                case OleDbType.IUnknown:
                    return typeof(object);
                case OleDbType.LongVarBinary:
                    return typeof(byte[]);
                case OleDbType.LongVarChar:
                    return typeof(string);
                case OleDbType.LongVarWChar:
                    return typeof(string);
                case OleDbType.Numeric:
                    return typeof(decimal);
                case OleDbType.PropVariant:
                    return typeof(object);
                case OleDbType.Single:
                    return typeof(System.Single);
                case OleDbType.SmallInt:
                    return typeof(Int16);
                case OleDbType.TinyInt:
                    return typeof(SByte);
                case OleDbType.UnsignedBigInt:
                    return typeof(UInt64);
                case OleDbType.UnsignedInt:
                    return typeof(UInt32);
                case OleDbType.UnsignedSmallInt:
                    return typeof(UInt16);
                case OleDbType.UnsignedTinyInt:
                    return typeof(Byte);
                case OleDbType.VarBinary:
                    return typeof(byte[]);
                case OleDbType.VarChar:
                    return typeof(string);
                case OleDbType.Variant:
                    return typeof(object);
                case OleDbType.VarNumeric:
                    return typeof(decimal);
                case OleDbType.VarWChar:
                    return typeof(string);
                case OleDbType.WChar:
                    return typeof(string);
            }
            return typeof(object);
        }

        protected override int GetTypeCoercionPrecedence(OleDbType type)
        {
            switch (type)
            {
                case OleDbType.BigInt:
                    return 0;
                case OleDbType.Binary:
                    return 1;
                case OleDbType.Boolean:
                    return 2;
                case OleDbType.BSTR:
                    return 3;
                case OleDbType.Char:
                    return 4;
                case OleDbType.Currency:
                    return 5;
                case OleDbType.Date:
                    return 6;
                case OleDbType.DBDate:
                    return 7;
                case OleDbType.DBTime:
                    return 8;
                case OleDbType.DBTimeStamp:
                    return 9;
                case OleDbType.Decimal:
                    return 10;
                case OleDbType.Double:
                    return 11;
                case OleDbType.Error:
                    return 12;
                case OleDbType.Filetime:
                    return 13;
                case OleDbType.Guid:
                    return 14;
                case OleDbType.IDispatch:
                    return 15;
                case OleDbType.Integer:
                    return 16;
                case OleDbType.IUnknown:
                    return 17;
                case OleDbType.LongVarBinary:
                    return 18;
                case OleDbType.LongVarChar:
                    return 19;
                case OleDbType.LongVarWChar:
                    return 20;
                case OleDbType.Numeric:
                    return 21;
                case OleDbType.PropVariant:
                    return 22;
                case OleDbType.Single:
                    return 23;
                case OleDbType.SmallInt:
                    return 24;
                case OleDbType.TinyInt:
                    return 25;
                case OleDbType.UnsignedBigInt:
                    return 26;
                case OleDbType.UnsignedInt:
                    return 27;
                case OleDbType.UnsignedSmallInt:
                    return 28;
                case OleDbType.UnsignedTinyInt:
                    return 29;
                case OleDbType.VarBinary:
                    return 30;
                case OleDbType.VarChar:
                    return 31;
                case OleDbType.Variant:
                    return 32;
                case OleDbType.VarNumeric:
                    return 33;
                case OleDbType.VarWChar:
                    return 34;
                case OleDbType.WChar:
                    return 35;
            }
            throw SqlClient.Error.UnexpectedTypeCode(type);
        }

        internal override DBTypeCategory Category
        {
            get
            {
                switch (SqlDbType)
                {
                    case OleDbType.BigInt:
                    case OleDbType.Boolean:
                    case OleDbType.Decimal:
                    case OleDbType.Double:
                    case OleDbType.Numeric:
                    case OleDbType.Single:
                    case OleDbType.SmallInt:
                    case OleDbType.TinyInt:
                    case OleDbType.UnsignedBigInt:
                    case OleDbType.UnsignedInt:
                    case OleDbType.UnsignedSmallInt:
                    case OleDbType.UnsignedTinyInt:
                    case OleDbType.Integer:
                    case OleDbType.VarNumeric:
                    case OleDbType.Currency:
                        return DBTypeCategory.Numeric;

                    case OleDbType.Empty:
                    case OleDbType.Binary:
                    case OleDbType.Error:
                    case OleDbType.Guid:
                    case OleDbType.IDispatch:
                    case OleDbType.IUnknown:
                    case OleDbType.LongVarBinary:
                    case OleDbType.PropVariant:
                    case OleDbType.VarBinary:
                    case OleDbType.Variant:
                        return DBTypeCategory.Binary;

                    case OleDbType.BSTR:
                    case OleDbType.LongVarChar:
                    case OleDbType.LongVarWChar:
                    case OleDbType.VarChar:
                    case OleDbType.Char:
                    case OleDbType.VarWChar:
                    case OleDbType.WChar:
                        return DBTypeCategory.Text;

                    case OleDbType.Date:
                    case OleDbType.DBDate:
                    case OleDbType.DBTime:
                    case OleDbType.DBTimeStamp:
                    case OleDbType.Filetime:
                        return DBTypeCategory.DateTime;
                }
                throw SqlClient.Error.UnexpectedTypeCode(this);
            }
        }

        public override bool HasPrecisionAndScale
        {
            get
            {
                //switch (SqlDbType)
                //{
                //    case OleDbType.Currency:
                //    //case OleDbType.Double:
                //    case OleDbType.Numeric:
                //    //case OleDbType.Single:
                //    case OleDbType.VarNumeric:
                //        return true;
                //}
                return false;
            }
        }

        public override bool IsFixedSize
        {
            get
            {
                switch (SqlDbType)
                {
                    case OleDbType.Binary:
                    case OleDbType.BSTR:
                    case OleDbType.LongVarBinary:
                    case OleDbType.LongVarChar:
                    case OleDbType.LongVarWChar:
                    case OleDbType.Numeric:
                    case OleDbType.VarBinary:
                    case OleDbType.VarChar:
                    case OleDbType.Variant:
                    case OleDbType.VarNumeric:
                    case OleDbType.VarWChar:
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
                    case OleDbType.Binary:
                    case OleDbType.LongVarChar:
                    case OleDbType.LongVarWChar:
                    case OleDbType.LongVarBinary:
                        return true;
                }
                return false;
            }
        }

        public override bool IsNumeric
        {
            get
            {
                switch (SqlDbType)
                {
                    case OleDbType.BigInt:
                    case OleDbType.Boolean:
                    case OleDbType.Decimal:
                    case OleDbType.Double:
                    case OleDbType.Numeric:
                    case OleDbType.Single:
                    case OleDbType.SmallInt:
                    case OleDbType.TinyInt:
                    case OleDbType.UnsignedBigInt:
                    case OleDbType.UnsignedInt:
                    case OleDbType.UnsignedSmallInt:
                    case OleDbType.UnsignedTinyInt:
                    case OleDbType.Integer:
                    case OleDbType.VarNumeric:
                    case OleDbType.Currency:
                        return true;
                }
                return false;
            }
        }

        public override string ToQueryString(QueryFormatOptions formatFlags)
        {
            switch(SqlDbType)
            {
                case OleDbType.VarChar:
                case OleDbType.LongVarChar:
                    if (Size == STRING_SIZE)
                        return "Text";
                    return string.Format("VarChar({0})", Size);
                case OleDbType.Decimal:
                case OleDbType.Numeric:
                    return string.Format("Decimal({0},{1})", Precision, Scale);
                case OleDbType.BigInt:
                    return "Long";
            }
            return base.ToQueryString(formatFlags);
        }

    }
}

