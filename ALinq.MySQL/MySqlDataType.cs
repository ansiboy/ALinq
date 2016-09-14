using System;
using System.Collections.Generic;
using System.Text;
using ALinq.SqlClient;
using MySql.Data.MySqlClient;

namespace ALinq.MySQL
{
    class MySqlDataType : SqlDataType<MySqlDbType>, IProviderType
    {
        private bool isUnicode;
        private Dictionary<TypeCode, MySqlDbType> TypeMapping;
        private MySqlDbType mySqlDbType;
        private Type type;

        public MySqlDataType(Dictionary<TypeCode, MySqlDbType> typeMapping)
            : base(typeMapping)
        {
        }

        public MySqlDataType(Dictionary<TypeCode, MySqlDbType> typeMapping, MySqlDbType dbType)
            : base(typeMapping, dbType)
        {
        }

        public MySqlDataType(Dictionary<TypeCode, MySqlDbType> typeMapping, MySqlDbType dbType, int precision, int scale)
            : base(typeMapping, dbType, precision, scale)
        {
        }

        public MySqlDataType(Dictionary<TypeCode, MySqlDbType> typeMapping, MySqlDbType dbType, int size)
            : base(typeMapping, dbType, size)
        {
        }

        public MySqlDataType(Dictionary<TypeCode, MySqlDbType> typeMapping, MySqlDbType dbType, Type runtimeType)
            :this(typeMapping,dbType)
        {
            //this.RuntimeOnlyType = runtimeType;
        }

        bool IProviderType.AreValuesEqual(object o1, object o2)
        {
            if ((o1 == null) || (o2 == null))
            {
                return false;
            }
            
            switch (SqlDbType)
            {
                case MySqlDbType.String:
                case MySqlDbType.Text:
                case MySqlDbType.VarChar:
                case MySqlDbType.VarString:
                case MySqlDbType.TinyBlob:
                    if (SqlDbType == MySqlDbType.String || Size <= 255)
                    {
                        var str = o1 as string;
                        if (str != null)
                        {
                            var str2 = o2 as string;
                            if (str2 != null)
                            {
                                return str.TrimEnd(new[] { ' ' }).Equals(str2.TrimEnd(new[] { ' ' }), StringComparison.Ordinal);
                            }
                        }
                    }
                    break;
            }
            return o1.Equals(o2);
        }

        int IProviderType.ComparePrecedenceTo(IProviderType type)
        {
            var type2 = (MySqlDataType)type;
            int num = IsTypeKnownByProvider ? GetTypeCoercionPrecedence(SqlDbType) : Int32.MinValue;//-2147483648;
            int num2 = type2.IsTypeKnownByProvider ? GetTypeCoercionPrecedence(type2.SqlDbType) : Int32.MinValue;//-2147483648;
            return num.CompareTo(num2);
        }

        protected override int GetTypeCoercionPrecedence(MySqlDbType type)
        {
            switch (type)
            {
                case MySqlDbType.Binary:
                    return 0;
                case MySqlDbType.VarBinary:
                    return 1;
                case MySqlDbType.Blob:
                    return 2;
                case MySqlDbType.MediumBlob:
                    return 3;
                case MySqlDbType.LongBlob:
                    return 4;
                case MySqlDbType.Text:
                    return 5;
                case MySqlDbType.MediumText:
                    return 6;
                case MySqlDbType.LongText:
                    return 7;
                case MySqlDbType.String:
                    return 8;
                case MySqlDbType.TinyText:
                    return 9;
                case MySqlDbType.TinyBlob:
                    return 10;
                case MySqlDbType.VarChar:
                    return 11;
                case MySqlDbType.VarString:
                    return 12;
                case MySqlDbType.Date:
                    return 13;
                case MySqlDbType.DateTime:
                    return 14;
                case MySqlDbType.Time:
                    return 15;
                case MySqlDbType.Timestamp:
                    return 16;
                case MySqlDbType.Year:
                    return 17;
                case MySqlDbType.Bit:
                    return 18;
                case MySqlDbType.Byte:
                    return 19;
                case MySqlDbType.UByte:
                    return 20;
                case MySqlDbType.Int16:
                    return 21;
                case MySqlDbType.UInt16:
                    return 22;
                case MySqlDbType.Int24:
                    return 23;
                case MySqlDbType.UInt24:
                    return 24;
                case MySqlDbType.Int32:
                    return 25;
                case MySqlDbType.UInt32:
                    return 26;
                case MySqlDbType.Int64:
                    return 27;
                case MySqlDbType.UInt64:
                    return 28;
                case MySqlDbType.Decimal:
                    return 29;
                case MySqlDbType.Double:
                    return 30;
                case MySqlDbType.Enum:
                    return 31;
                case MySqlDbType.Float:
                    return 32;
            }
            throw SqlClient.Error.UnexpectedTypeCode(type);
        }

        private bool IsTypeKnownByProvider
        {
            get
            {
                return (!IsApplicationType && !IsRuntimeOnlyType);
            }
        }


        #region MyRegion
        //Type IProviderType.GetClosestRuntimeType()
        //{
        //    switch (SqlDbType)
        //    {
        //        case MySqlDbType.Binary:
        //            return typeof(byte[]);
        //        case MySqlDbType.Bit:
        //            return typeof(bool);
        //        case MySqlDbType.Blob:
        //            return typeof(byte[]);
        //        case MySqlDbType.Byte:
        //            return typeof(sbyte);
        //        case MySqlDbType.Date:
        //        case MySqlDbType.DateTime:
        //            return typeof(DateTime);
        //        case MySqlDbType.Decimal:
        //            return typeof(decimal);
        //        case MySqlDbType.Double:
        //            return typeof(double);
        //        case MySqlDbType.Enum:
        //            return typeof(string);
        //        case MySqlDbType.Float:
        //            return typeof(float);
        //        case MySqlDbType.Geometry:
        //            break;
        //        case MySqlDbType.Int16:
        //            return typeof(Int16);
        //        case MySqlDbType.Int24:
        //            return typeof(Int32);
        //        case MySqlDbType.Int32:
        //            return typeof(Int32);
        //        case MySqlDbType.Int64:
        //            return typeof(Int64);
        //        case MySqlDbType.LongBlob:
        //            return typeof(byte[]);
        //        case MySqlDbType.LongText:
        //            return typeof(string);
        //        case MySqlDbType.MediumBlob:
        //            return typeof(byte[]);
        //        case MySqlDbType.MediumText:
        //            return typeof(string);
        //        case MySqlDbType.Newdate:
        //            break;
        //        case MySqlDbType.NewDecimal:
        //            break;
        //        case MySqlDbType.Set:
        //            break;

        //        case MySqlDbType.String:
        //        case MySqlDbType.VarChar:
        //            if (Size == 1)
        //                return typeof(char);
        //            return typeof(string);

        //        case MySqlDbType.Text:
        //        case MySqlDbType.TinyBlob:
        //        case MySqlDbType.TinyText:
        //        case MySqlDbType.VarString:
        //            return typeof(string);

        //        case MySqlDbType.Time:
        //            return typeof(DateTime);
        //        case MySqlDbType.Timestamp:
        //            return typeof(DateTime);
        //        case MySqlDbType.UByte:
        //            return typeof(Byte);
        //        case MySqlDbType.UInt16:
        //            return typeof(UInt16);
        //        case MySqlDbType.UInt24:
        //            return typeof(UInt32);
        //        case MySqlDbType.UInt32:
        //            return typeof(UInt32);
        //        case MySqlDbType.UInt64:
        //            return typeof(UInt64);
        //        case MySqlDbType.VarBinary:
        //            return typeof(byte[]);

        //        case MySqlDbType.Year:
        //            return typeof(short);
        //    }
        //    return typeof(object);
        //} 
        #endregion

        public override Type GetClosestRuntimeType()
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType;
            }

            //if (RuntimeType != null)
            //    return RuntimeType;

            switch (SqlDbType)
            {
                case MySqlDbType.Binary:
                    return typeof(byte[]);
                case MySqlDbType.Bit:
                    return typeof(bool);
                case MySqlDbType.Blob:
                    return typeof(byte[]);
                case MySqlDbType.Byte:
                    return typeof(SByte);
                case MySqlDbType.Date:
                    return typeof(DateTime);
                case MySqlDbType.DateTime:
                    return typeof(DateTime);
                case MySqlDbType.Decimal:
                    return typeof(decimal);
                case MySqlDbType.Double:
                    return typeof(double);
                case MySqlDbType.Float:
                    return typeof(float);
                case MySqlDbType.Int16:
                    return typeof(Int16);
                case MySqlDbType.Int24:
                    return typeof(Int32);
                case MySqlDbType.Int32:
                    return typeof(Int32);
                case MySqlDbType.Int64:
                    return typeof(Int64);
                case MySqlDbType.LongBlob:
                    return typeof(byte[]);
                case MySqlDbType.LongText:
                    return typeof(string);
                case MySqlDbType.MediumBlob:
                    return typeof(byte[]);
                case MySqlDbType.MediumText:
                    return typeof(string);
                case MySqlDbType.String:
                    if (Size == 1)
                        return typeof(char);
                    return typeof(string);
                case MySqlDbType.Text:
                    return typeof(string);
                case MySqlDbType.Time:
                    return typeof(DateTime);
                case MySqlDbType.Timestamp:
                    return typeof(DateTime);
                case MySqlDbType.TinyBlob:
                    return typeof(byte[]);
                case MySqlDbType.TinyText:
                    return typeof(string);
                case MySqlDbType.UByte:
                    return typeof(byte);
                case MySqlDbType.UInt16:
                    return typeof(UInt16);
                case MySqlDbType.UInt24:
                    return typeof(UInt32);
                case MySqlDbType.UInt32:
                    return typeof(UInt32);
                case MySqlDbType.UInt64:
                    return typeof(UInt64);
                case MySqlDbType.VarBinary:
                    return typeof(byte[]);
                case MySqlDbType.VarChar:
                    if (Size == 1)
                        return typeof(char);
                    return typeof(string);
                case MySqlDbType.VarString:
                    return typeof(string);
                case MySqlDbType.Year:
                    return typeof(short);
            }
            return typeof(object);
        }

        //public IProviderType GetNonUnicodeEquivalent()
        //{
        //    if (IsUnicodeType)
        //    {
        //        return new MySqlDataType(sqlDbType, Size);
        //    }
        //    return this;
        //}

        //protected override int GetTypeCoercionPrecedence(Enum type)
        //{
        //    throw new System.NotImplementedException();
        //}

        bool IProviderType.IsApplicationTypeOf(int index)
        {
            if (!IsApplicationType)
            {
                return false;
            }
            return (ApplicationTypeIndex.HasValue && (ApplicationTypeIndex.GetValueOrDefault() == index));
        }

        bool IProviderType.IsSameTypeFamily(IProviderType type)
        {
            var type2 = (MySqlDataType)type;
            if (IsApplicationType)
            {
                return false;
            }
            if (type2.IsApplicationType)
            {
                return false;
            }
            return (Category == type2.Category);
        }

        internal override DBTypeCategory Category
        {
            get
            {
                switch ((MySqlDbType)SqlDbType)
                {
                    case MySqlDbType.Binary:
                    case MySqlDbType.Blob:
                    case MySqlDbType.LongBlob:
                    case MySqlDbType.MediumBlob:
                    case MySqlDbType.VarBinary:
                        return DBTypeCategory.Binary;

                    case MySqlDbType.Bit:
                    case MySqlDbType.Byte:
                    case MySqlDbType.Decimal:
                    case MySqlDbType.Double:
                    case MySqlDbType.Float:
                    case MySqlDbType.Int16:
                    case MySqlDbType.Int24:
                    case MySqlDbType.Int32:
                    case MySqlDbType.Int64:
                    case MySqlDbType.UByte:
                    case MySqlDbType.UInt16:
                    case MySqlDbType.UInt24:
                    case MySqlDbType.UInt32:
                    case MySqlDbType.UInt64:
                    case MySqlDbType.Year:
                        return DBTypeCategory.Numeric;

                    case MySqlDbType.Date:
                    case MySqlDbType.DateTime:
                    case MySqlDbType.Time:
                    case MySqlDbType.Timestamp:
                        return DBTypeCategory.DateTime;

                    case MySqlDbType.String:
                    case MySqlDbType.VarChar:
                    case MySqlDbType.VarString:
                    case MySqlDbType.TinyBlob:
                    case MySqlDbType.TinyText:
                    case MySqlDbType.Guid:

                        return DBTypeCategory.Text;

                    case MySqlDbType.LongText:
                    case MySqlDbType.MediumText:
                    case MySqlDbType.Text:
                        return DBTypeCategory.Text;
                }
                throw SqlClient.Error.UnexpectedTypeCode(this);
            }
        }

        string IProviderType.ToQueryString()
        {
            return ToQueryString(QueryFormatOptions.None);
        }

        string IProviderType.ToQueryString(QueryFormatOptions formatOptions)
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType.ToString();
            }
            var builder = new StringBuilder();
            switch (SqlDbType)
            {
                case MySqlDbType.Int32:
                    builder.Append("SIGNED");
                    break;
                default:
                    builder.Append(SqlDbType.ToString());
                    break;
            }
            return builder.ToString();
        }

        bool IProviderType.CanBeColumn
        {
            get
            {
                return (!this.IsApplicationType && !this.IsRuntimeOnlyType);
            }
        }

        bool IProviderType.CanBeParameter
        {
            get
            {
                return (!this.IsApplicationType && !this.IsRuntimeOnlyType);
            }
        }

        bool IProviderType.CanSuppressSizeForConversionToString
        {
            get
            {
                int num = 30;
                if (!IsLargeType)
                {
                    if (((!IsChar && !IsString) && IsFixedSize) && (Size > 0))
                    {
                        return ((Size.GetValueOrDefault() < num) && Size.HasValue);
                    }
                    switch ((MySqlDbType)this.SqlDbType)
                    {
                        //case System.Data.SqlDbType.Real:
                        //case System.Data.SqlDbType.SmallInt:
                        //case System.Data.SqlDbType.SmallMoney:
                        //case System.Data.SqlDbType.TinyInt:
                        //case System.Data.SqlDbType.BigInt:
                        //case System.Data.SqlDbType.Bit:
                        //case System.Data.SqlDbType.Float:
                        //case System.Data.SqlDbType.Int:
                        //case System.Data.SqlDbType.Money:
                        case MySqlDbType.Bit:
                        case MySqlDbType.Byte:
                        case MySqlDbType.Decimal:
                        case MySqlDbType.Double:
                        case MySqlDbType.Float:
                        case MySqlDbType.Int16:
                        case MySqlDbType.Int24:
                        case MySqlDbType.Int32:
                        case MySqlDbType.Int64:
                        case MySqlDbType.UByte:
                        case MySqlDbType.UInt16:
                        case MySqlDbType.UInt24:
                        case MySqlDbType.UInt32:
                        case MySqlDbType.UInt64:
                        case MySqlDbType.Year:
                            return true;
                    }
                }
                return false;
            }
        }

        public override bool HasPrecisionAndScale
        {
            get
            {
                switch (SqlDbType)
                {
                    case MySqlDbType.Decimal:
                    case MySqlDbType.Double:
                    case MySqlDbType.Float:
                        if (Precision > 0 && Scale >= 0)
                            return true;
                        break;
                }
                return false;
            }
        }

        bool IProviderType.HasSizeOrIsLarge
        {
            get
            {
                if (!Size.HasValue)
                {
                    return IsLargeType;
                }
                return true;
            }
        }

        bool IProviderType.IsApplicationType
        {
            get
            {
                return ApplicationTypeIndex.HasValue;
            }
        }

        bool IProviderType.IsChar
        {
            get
            {
                if (!IsApplicationType && !IsRuntimeOnlyType)
                {
                    switch ((MySqlDbType)this.SqlDbType)
                    {
                        case MySqlDbType.String:
                        case MySqlDbType.VarChar:
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
                switch ((MySqlDbType)SqlDbType)
                {
                    case MySqlDbType.VarBinary:
                    case MySqlDbType.VarChar:
                    case MySqlDbType.VarString:
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
                switch ((MySqlDbType)SqlDbType)
                {
                    case MySqlDbType.LongBlob:
                    case MySqlDbType.LongText:
                    case MySqlDbType.MediumBlob:
                    case MySqlDbType.MediumText:
                        return false;
                }
                return true;
            }
        }

        public override bool IsLargeType
        {
            get
            {
                switch ((MySqlDbType)SqlDbType)
                {
                    case MySqlDbType.LongBlob:
                    case MySqlDbType.LongText:
                    case MySqlDbType.MediumBlob:
                    case MySqlDbType.MediumText:
                        return true;
                }
                return false;
            }
        }

        public override bool IsUnicodeType
        {
            get
            {
                switch (SqlDbType)
                {
                    case MySqlDbType.String:
                    case MySqlDbType.VarChar:
                    case MySqlDbType.VarString:
                        return this.isUnicode;
                }
                return false;
            }
        }

        //public override MySqlDbType SqlDbType
        //{
        //    get { return (MySqlDbType) sqlDbType; }
        //    set { sqlDbType = (int) value; }
        //}

        public override bool IsNumeric
        {
            get
            {
                switch (SqlDbType)
                {
                    case MySqlDbType.Bit:
                    case MySqlDbType.Byte:
                    case MySqlDbType.Decimal:
                    case MySqlDbType.Double:
                    case MySqlDbType.Int16:
                    case MySqlDbType.Int24:
                    case MySqlDbType.Int32:
                    case MySqlDbType.Int64:
                    case MySqlDbType.UByte:
                    case MySqlDbType.UInt16:
                    case MySqlDbType.UInt24:
                    case MySqlDbType.UInt32:
                    case MySqlDbType.UInt64:
                    case MySqlDbType.Year:
                        return true;
                }
                return false;
            }
        }

        bool IProviderType.IsOrderable
        {
            get
            {
                switch (SqlDbType)
                {
                    case MySqlDbType.LongBlob:
                    case MySqlDbType.LongText:
                    case MySqlDbType.MediumBlob:
                    case MySqlDbType.MediumText:
                        return false;
                }
                return true;
            }
        }

        bool IProviderType.IsRuntimeOnlyType
        {
            get
            {
                return (RuntimeOnlyType != null);
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
                    case MySqlDbType.String:
                    case MySqlDbType.VarChar:
                        return Size != 1;
                    case MySqlDbType.Text:
                    case MySqlDbType.TinyBlob:
                    case MySqlDbType.TinyText:
                    case MySqlDbType.MediumText:
                    case MySqlDbType.LongText:
                        return true;
                }
                return false;

            }
        }

        public bool Unsigned
        {
            get
            {
                switch (SqlDbType)
                {
                    case MySqlDbType.UByte:
                    case MySqlDbType.UInt16:
                    case MySqlDbType.UInt24:
                    case MySqlDbType.UInt32:
                    case MySqlDbType.UInt64:
                        return true;
                }
                return false;
            }
        }

        public bool ZeroFill { get; set; }

        public override string ToQueryString(QueryFormatOptions formatFlags)
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType.ToString();
            }
            var builder = new StringBuilder();

            string typeName;
            var size = Size;
            var unicode = false;

            switch (SqlDbType)
            {
                case MySqlDbType.Date:
                case MySqlDbType.DateTime:
                case MySqlDbType.Time:
                    typeName = SqlDbType.ToString();
                    size = 0;
                    break;
                case MySqlDbType.Bit:
                    typeName = "BIT";
                    break;
                case MySqlDbType.Byte:
                    typeName = "TINYINT";
                    break;
                case MySqlDbType.Double:
                    typeName = "DOUBLE";
                    break;
                case MySqlDbType.UByte:
                    typeName = "TINYINT";
                    break;
                case MySqlDbType.Int16:
                    typeName = "SMALLINT";
                    break;
                case MySqlDbType.Int24:
                    typeName = "MEDIUMINT";
                    break;
                case MySqlDbType.UInt24:
                    typeName = "MEDIUMINT";
                    break;
                case MySqlDbType.Int32:
                    typeName = "INT";
                    break;
                case MySqlDbType.UInt32:
                    typeName = "INT";
                    break;
                case MySqlDbType.Int64:
                    typeName = "BIGINT";
                    break;
                case MySqlDbType.UInt64:
                    typeName = "BIGINT";
                    break;
                case MySqlDbType.Float:
                    typeName = "FLOAT";
                    break;
                case MySqlDbType.VarChar:
                    typeName = "VARCHAR";
                    break;
                default:
                    typeName = SqlDbType.ToString();
                    break;
                    //throw SqlClient.Error.InvalidProviderType(SqlDbType);
            }
            builder.Append(typeName);
            if (size > 0 && formatFlags != QueryFormatOptions.SuppressSize)
            {
                builder.Append("(");
                builder.Append(Size);
                builder.Append(")");
            }
            if (Unsigned)
                builder.Append("UNSIGNED");
            return builder.ToString();
        }

        public void SetUnicodeType(bool unicode)
        {
            isUnicode = unicode;
        }

        //internal Type RuntimeType { get; private set; }
    }
}
