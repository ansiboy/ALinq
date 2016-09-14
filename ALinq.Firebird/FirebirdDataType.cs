using System;
using System.Collections.Generic;
using System.Text;
using ALinq.SqlClient;
using FirebirdSql.Data.FirebirdClient;

namespace ALinq.Firebird
{
    internal class FirebirdDataType : SqlDataType<FbDbType>, IProviderType
    {
        public FirebirdDataType(Dictionary<TypeCode, FbDbType> typeMapping)
            : base(typeMapping)
        {
        }

        public FirebirdDataType(Dictionary<TypeCode, FbDbType> typeMapping, FbDbType dbType)
            : base(typeMapping, dbType)
        {
        }

        public FirebirdDataType(Dictionary<TypeCode, FbDbType> typeMapping, FbDbType dbType, int precision, int scale)
            : base(typeMapping, dbType, precision, scale)
        {
        }

        public FirebirdDataType(Dictionary<TypeCode, FbDbType> typeMapping, FbDbType dbType, int size)
            : base(typeMapping, dbType, CheckSize(dbType, size))
        {

        }

        static int CheckSize(FbDbType dbType, int size)
        {
            if (size == 0)
            {
                switch (dbType)
                {
                    case FbDbType.Binary:
                    case FbDbType.VarChar:
                        size = 4000;
                        break;
                }
            }
            return size;
        }

        public override Type GetClosestRuntimeType()
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType;
            }
            switch (SqlDbType)
            {
                case FbDbType.BigInt:
                    return typeof(long);
                case FbDbType.Binary:
                    return typeof(byte[]);
                case FbDbType.Boolean:
                    return typeof(bool);
                case FbDbType.Char:
                    return typeof(string);
                case FbDbType.Date:
                    return typeof(DateTime);
                case FbDbType.Decimal:
                    return typeof(decimal);
                case FbDbType.Double:
                    return typeof(double);
                case FbDbType.Float:
                    return typeof(float);
                case FbDbType.Guid:
                    return typeof(Guid);
                case FbDbType.Integer:
                    return typeof(int);
                case FbDbType.Numeric:
                    return typeof(decimal);
                case FbDbType.SmallInt:
                    return typeof(short);
                case FbDbType.Text:
                    return typeof(string);
                case FbDbType.Time:
                    return typeof(DateTime);
                case FbDbType.TimeStamp:
                    return typeof(DateTime);
                case FbDbType.VarChar:
                    return typeof(string);
            }
            return typeof(object);
        }

        protected override int GetTypeCoercionPrecedence(FbDbType type)
        {
            switch (type)
            {
                case FbDbType.Binary:
                    return 0;
                case FbDbType.VarChar:
                    return 1;
                case FbDbType.Char:
                    return 2;
                case FbDbType.Guid:
                    return 3;
                case FbDbType.Text:
                    return 4;
                case FbDbType.Boolean:
                    return 5;
                case FbDbType.SmallInt:
                    return 6;
                case FbDbType.Integer:
                    return 7;
                case FbDbType.BigInt:
                    return 8;
                case FbDbType.Numeric:
                    return 9;
                case FbDbType.Decimal:
                    return 10;
                case FbDbType.Double:
                    return 11;
                case FbDbType.Float:
                    return 12;
                case FbDbType.Date:
                    return 13;
                case FbDbType.Time:
                    return 14;
                case FbDbType.TimeStamp:
                    return 15;
            }
            //FbDbType.Array
            throw SqlClient.Error.UnexpectedTypeCode(type);
        }

        public override string ToQueryString(QueryFormatOptions formatFlags)
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType.ToString();
            }
            var builder = new StringBuilder();
            switch (SqlDbType)
            {
                case FbDbType.VarChar://System.Data.SqlDbType.NVarChar:
                    builder.Append("VarChar");
                    builder.Append("(");
                    builder.Append(Size);
                    builder.Append(")");
                    return builder.ToString();

                case FbDbType.Guid:
                    builder.Append("CHAR(38)");
                    return builder.ToString();

                case FbDbType.Binary:
                    builder.Append("BLOB");
                    builder.Append("(");
                    builder.Append(Size);
                    builder.Append(")");
                    return builder.ToString();

                case FbDbType.TimeStamp:
                    builder.Append(FbDbType.TimeStamp);
                    return builder.ToString();

                //case System.Data.SqlDbType.DateTimeOffset:
                //    builder.Append(FbDbType.Time);
                //    return builder.ToString();

                case FbDbType.Double://System.Data.SqlDbType.Real:
                    builder.Append("Double Precision");
                    return builder.ToString();

                case FbDbType.Decimal://System.Data.SqlDbType.Decimal:
                    builder.Append(FbDbType.Decimal);
                    //if (Precision != 0 && Scale != 0)
                    //{
                    //const int MaxPrecision = 18;
                    builder.Append("(");
                    //builder.Append(Precision > MaxPrecision ? 18 : Precision);
                    builder.Append(Precision);
                    //if (Scale != 0)
                    //{
                    builder.Append(",");
                    builder.Append(Scale);
                    //}
                    builder.Append(")");
                    //}
                    return builder.ToString();

                case FbDbType.Char://System.Data.SqlDbType.NChar:
                    builder.Append("Char");
                    builder.Append("(");
                    builder.Append(Size);
                    builder.Append(")");
                    //builder.Append(" CHARACTER SET UNICODE_FSS");
                    return builder.ToString();
                case FbDbType.Text://System.Data.SqlDbType.NText:
                    builder.Append("BLOB SUB_TYPE TEXT SEGMENT SIZE 4000");
                    //builder.Append("VarChar(4000)");
                    return builder.ToString();
                #region MyRegion
                //case FbDbType.SmallInt://System.Data.SqlDbType.TinyInt:
                //    builder.Append(FbDbType.SmallInt);
                //    return builder.ToString();
                //case System.Data.SqlDbType.BigInt:
                //case System.Data.SqlDbType.Bit:
                //case System.Data.SqlDbType.Image:
                //case System.Data.SqlDbType.Int:
                //case System.Data.SqlDbType.Money:
                //case System.Data.SqlDbType.SmallDateTime:
                //case System.Data.SqlDbType.SmallInt:
                //case System.Data.SqlDbType.SmallMoney:
                //case System.Data.SqlDbType.Text:
                //case System.Data.SqlDbType.Timestamp:
                //case System.Data.SqlDbType.Xml:
                //case System.Data.SqlDbType.Udt: 
                #endregion
                default:
                    builder.Append(SqlDbType.ToString());
                    return builder.ToString();

                #region MyRegion
                //case FbDbType.Binary://System.Data.SqlDbType.Binary:
                //case System.Data.SqlDbType.Char:
                //    //case System.Data.SqlDbType.NChar:
                //    builder.Append(this.sqlDbType);
                //    if ((formatFlags & QueryFormatOptions.SuppressSize) == QueryFormatOptions.None)
                //    {
                //        builder.Append("(");
                //        builder.Append(this.size);
                //        builder.Append(")");
                //    }
                //    return builder.ToString();


                //case System.Data.SqlDbType.Float:
                //    builder.Append(this.sqlDbType);
                //    if (this.precision != 0)
                //    {
                //        builder.Append("(");
                //        builder.Append(this.precision);
                //        if (this.scale != 0)
                //        {
                //            builder.Append(",");
                //            builder.Append(this.scale);
                //        }
                //        builder.Append(")");
                //    }
                //    return builder.ToString();

                //case System.Data.SqlDbType.VarChar:
                //    builder.Append(this.sqlDbType);
                //    if (!size.HasValue || ((size == 0) || ((formatFlags & QueryFormatOptions.SuppressSize) != QueryFormatOptions.None)))
                //    {
                //        return builder.ToString();
                //    }
                //    builder.Append("(");
                //    if (this.size != -1)
                //        builder.Append(this.size);
                //    else
                //        builder.Append("MAX");

                //    builder.Append(")");
                //    return builder.ToString();

                //case System.Data.SqlDbType.Variant:
                //    builder.Append("sql_variant");
                //    return builder.ToString();

                //default:
                //    return builder.ToString(); 
                #endregion
            }


        }

        public override string ToString()
        {
            return (SingleValue(GetClosestRuntimeType()) + SingleValue(ToQueryString()) +
                    KeyValue("IsApplicationType", IsApplicationType) + KeyValue("IsUnicodeType", IsUnicodeType) +
                    KeyValue("IsRuntimeOnlyType", IsRuntimeOnlyType) + KeyValue("SupportsComparison", SupportsComparison) +
                    KeyValue("SupportsLength", SupportsLength) + KeyValue("IsLargeType", IsLargeType) +
                    KeyValue("IsFixedSize", IsFixedSize) + KeyValue("IsOrderable", IsOrderable) +
                    KeyValue("IsGroupable", IsGroupable) + KeyValue("IsNumeric", IsNumeric) +
                    KeyValue("IsChar", IsChar) + KeyValue("IsString", IsString));
        }


        #region MyRegion
        //bool IProviderType.CanSuppressSizeForConversionToString
        //{
        //    get
        //    {
        //        int num = 30;
        //        if (!IsLargeType)
        //        {
        //            if (((!IsChar && !IsString) && IsFixedSize) && (Size > 0))
        //            {
        //                //int? size = this.Size;
        //                //int num2 = num;
        //                return ((Size.GetValueOrDefault() < num) && Size.HasValue);
        //            }
        //            switch ((FbDbType)SqlDbType)
        //            {
        //                //case System.Data.SqlDbType.Real:
        //                //case System.Data.SqlDbType.SmallInt:
        //                //case System.Data.SqlDbType.SmallMoney:
        //                //case System.Data.SqlDbType.TinyInt:
        //                //case System.Data.SqlDbType.BigInt:
        //                //case System.Data.SqlDbType.Bit:
        //                //case System.Data.SqlDbType.Float:
        //                //case System.Data.SqlDbType.Int:
        //                //case System.Data.SqlDbType.Money:
        //                case FbDbType.BigInt:
        //                case FbDbType.Boolean:
        //                case FbDbType.Decimal:
        //                case FbDbType.Double:
        //                case FbDbType.Float:
        //                case FbDbType.Integer:
        //                case FbDbType.Numeric:
        //                case FbDbType.SmallInt:
        //                    return true;
        //            }
        //        }
        //        return false;
        //    }
        //} 
        #endregion

        internal override DBTypeCategory Category
        {
            get
            {
                switch ((FbDbType)SqlDbType)
                {
                    case FbDbType.BigInt:
                    case FbDbType.Decimal:
                    case FbDbType.Double:
                    case FbDbType.Float:
                    case FbDbType.Integer:
                    case FbDbType.Numeric:
                    case FbDbType.SmallInt:
                    case FbDbType.Boolean:
                        return DBTypeCategory.Numeric;

                    case FbDbType.Binary:
                        return DBTypeCategory.Binary;

                    case FbDbType.Char:
                    case FbDbType.VarChar:
                        return DBTypeCategory.Text;

                    case FbDbType.Date://System.Data.SqlDbType.DateTime:
                    case FbDbType.TimeStamp://System.Data.SqlDbType.SmallDateTime:
                    case FbDbType.Time:
                        return DBTypeCategory.DateTime;

                    case FbDbType.Text:
                        return DBTypeCategory.Text;

                    case FbDbType.Guid:
                        return DBTypeCategory.Text;
                }
                throw SqlClient.Error.UnexpectedTypeCode(this);
            }
        }

        public override bool HasPrecisionAndScale
        {
            get
            {
                switch ((FbDbType)SqlDbType)
                {
                    //case System.Data.SqlDbType.Decimal:
                    //case System.Data.SqlDbType.Float:
                    //case System.Data.SqlDbType.Money:
                    //case System.Data.SqlDbType.Real:
                    //case System.Data.SqlDbType.SmallMoney:
                    case FbDbType.Decimal:
                        //case FbDbType.Double:
                        //case FbDbType.Float:
                        //case FbDbType.Numeric:
                        return true;
                }
                return false;
            }
        }

        bool IProviderType.IsChar
        {
            get
            {
                if (!IsApplicationType && !IsRuntimeOnlyType)
                {
                    switch ((FbDbType)SqlDbType)
                    {
                        case FbDbType.Char:
                        case FbDbType.VarChar:
                            return (Size == 1);
                    }
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
                    #region Delete
                    //case System.Data.SqlDbType.Text:
                    //case System.Data.SqlDbType.VarBinary:
                    //case System.Data.SqlDbType.VarChar:
                    //case System.Data.SqlDbType.Xml:
                    //case System.Data.SqlDbType.NText:
                    //case System.Data.SqlDbType.NVarChar:
                    //case System.Data.SqlDbType.Image: 
                    #endregion
                    case FbDbType.Binary:
                    case FbDbType.Numeric:
                    case FbDbType.Text:
                    case FbDbType.VarChar:
                        return false;
                }
                return true;
            }
        }

        bool IProviderType.IsGroupable
        {
            get
            {
                if (IsRuntimeOnlyType)
                {
                    return false;
                }
                //var sqlDbType = this.sqlDbType;
                switch ((FbDbType)SqlDbType)
                {
                    //case System.Data.SqlDbType.Image:
                    //case System.Data.SqlDbType.NText:
                    //case System.Data.SqlDbType.Text:
                    //case System.Data.SqlDbType.Xml:
                    case FbDbType.Array:
                    case FbDbType.Binary:
                    case FbDbType.Text:
                        return false;
                }
                return true;
            }
        }

        public override bool IsLargeType
        {
            get
            {
                switch ((FbDbType)SqlDbType)
                {
                    case FbDbType.Binary:
                    case FbDbType.Text:
                        return true;

                    case FbDbType.VarChar:
                        return (Size == -1);
                }
                return false;
            }
        }

        public override bool IsNumeric
        {
            get
            {
                if (!IsApplicationType && !IsRuntimeOnlyType)
                {
                    switch (SqlDbType)
                    {
                        case FbDbType.BigInt:
                        case FbDbType.Boolean:
                        case FbDbType.Decimal:
                        case FbDbType.Double:
                        case FbDbType.Float:
                        case FbDbType.Integer:
                        case FbDbType.Numeric:
                        case FbDbType.SmallInt:
                            return true;
                    }
                }
                return false;
            }
        }

        bool IProviderType.IsOrderable
        {
            get
            {
                if (IsRuntimeOnlyType)
                {
                    return false;
                }
                switch (SqlDbType)
                {
                    case FbDbType.Array:
                    case FbDbType.Binary:
                    case FbDbType.Text:
                        return false;
                }
                return true;
            }
        }



        bool IProviderType.IsString
        {
            get
            {
                if (IsApplicationType || IsRuntimeOnlyType)
                {
                    return false;
                }
                switch (SqlDbType)
                {
                    case FbDbType.Char:
                    case FbDbType.VarChar:
                        return Size != 1;
                    case FbDbType.Text:
                        return true;
                }
                return false;

            }
        }

        bool IProviderType.SupportsComparison
        {
            get
            {
                //var sqlDbType = this.sqlDbType;

                switch (SqlDbType)
                {
                    case FbDbType.Array:
                    case FbDbType.Binary:
                    case FbDbType.Text:
                        return false;
                }
                return true;
            }
        }

        bool IProviderType.SupportsLength
        {
            get
            {
                switch (SqlDbType)
                {
                    case FbDbType.Binary:
                    case FbDbType.Numeric:
                    case FbDbType.Text:
                        return false;
                }
                return true;
            }
        }
    }
}
