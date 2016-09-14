using System;
using System.Text;
using ALinq.SqlClient;
using MySql.Data.MySqlClient;

namespace ALinq.MySQL
{
    internal class MySqlType : ProviderType
    {
        // Fields
        private int? applicationTypeIndex;
        protected int precision;
        private Type runtimeOnlyType;
        protected int scale;
        protected int? size;
        protected MySqlDbType sqlDbType;

        // Methods
        internal MySqlType(MySqlDbType type)
        {
            this.applicationTypeIndex = null;
            this.sqlDbType = type;
        }

        internal MySqlType(int applicationTypeIndex)
        {
            this.applicationTypeIndex = null;
            this.applicationTypeIndex = new int?(applicationTypeIndex);
        }

        internal MySqlType(Type type)
        {
            this.applicationTypeIndex = null;
            this.runtimeOnlyType = type;
        }

        internal MySqlType(MySqlDbType type, int? size)
        {
            this.applicationTypeIndex = null;
            this.sqlDbType = type;
            this.size = size;
        }

        internal MySqlType(MySqlDbType type, int precision, int scale)
        {
            this.applicationTypeIndex = null;
            this.sqlDbType = type;
            this.precision = precision;
            this.scale = scale;
        }

        public override bool AreValuesEqual(object o1, object o2)
        {
            string str;
            if ((o1 == null) || (o2 == null))
            {
                return false;
            }
            //var sqlDbType = this.sqlDbType;
            switch (sqlDbType)
            {
                case MySqlDbType.VarChar:
                case MySqlDbType.TinyText:
                case MySqlDbType.String:
                    str = o1 as string;
                    if (str != null)
                    {
                        var str2 = o2 as string;
                        if (str2 != null)
                            return str.TrimEnd(new[] { ' ' }).Equals(str2.TrimEnd(new[] { ' ' }),
                                                                     StringComparison.Ordinal);
                    }
                    break;
                case MySqlDbType.MediumText:
                case MySqlDbType.VarString:
                case MySqlDbType.LongText:
                case MySqlDbType.Text:
                    break;
            }
            return o1.Equals(o2);

            #region MyRegion
            //if (sqlDbType <= SqlDbType.NVarChar)
            //    if (sqlDbType <= MySqlDbType.)
            //    {
            //        switch (sqlDbType)
            //        {
            //            case SqlDbType.NChar:
            //            case SqlDbType.NVarChar:
            //            case SqlDbType.Char:
            //                goto Label_0039;

            //            case SqlDbType.NText:
            //                goto Label_007D;
            //        }
            //        goto Label_007D;
            //    }
            //    if ((sqlDbType != SqlDbType.Text) && (sqlDbType != SqlDbType.VarChar))
            //    {
            //        goto Label_007D;
            //    }
            //Label_0039:
            //    str = o1 as string;
            //    if (str != null)
            //    {
            //        string str2 = o2 as string;
            //        if (str2 != null)
            //        {
            //            return str.TrimEnd(new char[] { ' ' }).Equals(str2.TrimEnd(new char[] { ' ' }), StringComparison.Ordinal);
            //        }
            //    }
            //Label_007D:
            //return o1.Equals(o2); 
            #endregion
        }

        public override int ComparePrecedenceTo(IProviderType type)
        {
            var type2 = (MySqlType)type;
            int num = this.IsTypeKnownByProvider ? GetTypeCoercionPrecedence((MySqlDbType)this.SqlDbType) : -2147483648;
            int num2 = type2.IsTypeKnownByProvider ? GetTypeCoercionPrecedence((MySqlDbType)type2.SqlDbType) : -2147483648;
            return num.CompareTo(num2);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            var type = obj as MySqlType;
            return (((type != null) && (((this.runtimeOnlyType == type.runtimeOnlyType) && ((this.applicationTypeIndex == type.applicationTypeIndex) && (this.sqlDbType == type.sqlDbType))) && ((this.Size == type.Size) && (this.precision == type.precision)))) && (this.scale == type.scale));
        }

        public override Type GetClosestRuntimeType()
        {
            if (this.runtimeOnlyType != null)
            {
                return this.runtimeOnlyType;
            }
            switch (sqlDbType)
            {
                case MySqlDbType.Decimal:
                    return typeof(decimal);
                case MySqlDbType.Byte:
                    return typeof(sbyte);

                case MySqlDbType.Year:
                case MySqlDbType.Int16:
                    return typeof(Int16);
                case MySqlDbType.Int24:
                case MySqlDbType.Int32:
                    return typeof(Int32);
                case MySqlDbType.Int64:
                    return typeof(Int64);
                case MySqlDbType.Float:
                    return typeof(float);
                case MySqlDbType.Double:
                    return typeof(double);

                case MySqlDbType.Timestamp:
                case MySqlDbType.Date:
                    return typeof(DateTime);

                case MySqlDbType.Time:
                    return typeof(TimeSpan);

                case MySqlDbType.VarString:
                case MySqlDbType.VarChar:
                case MySqlDbType.String:
                case MySqlDbType.TinyBlob:
                case MySqlDbType.TinyText:
                case MySqlDbType.MediumText:
                case MySqlDbType.LongText:
                case MySqlDbType.Text:
                    return typeof(string);
                case MySqlDbType.Bit:
                    return typeof(bool);

                case MySqlDbType.NewDecimal:
                case MySqlDbType.Enum:
                case MySqlDbType.Set:
                    throw SqlClient.Error.UnexpectedTypeCode(MySqlDbType.NewDecimal);

                case MySqlDbType.MediumBlob:
                case MySqlDbType.LongBlob:
                case MySqlDbType.Blob:
                case MySqlDbType.Binary:
                case MySqlDbType.VarBinary:
                    return typeof(byte[]);

                case MySqlDbType.Geometry:
                    throw SqlClient.Error.UnexpectedTypeCode(MySqlDbType.Geometry);

                case MySqlDbType.UByte:
                    return typeof(byte);
                case MySqlDbType.UInt16:
                    return typeof(UInt16);
                case MySqlDbType.UInt24:
                case MySqlDbType.UInt32:
                    return typeof(UInt32);
                case MySqlDbType.UInt64:
                    return typeof(UInt64);



                #region MyRegion
                //case SqlDbType.BigInt:
                //    return typeof(long);

                //case SqlDbType.Binary:
                //case SqlDbType.Image:
                //case SqlDbType.Timestamp:
                //case SqlDbType.VarBinary:
                //    return typeof(byte[]);

                //case SqlDbType.Bit:
                //    return typeof(bool);

                //case SqlDbType.Char:
                //case SqlDbType.NChar:
                //case SqlDbType.NText:
                //case SqlDbType.NVarChar:
                //case SqlDbType.Text:
                //case SqlDbType.VarChar:
                //case SqlDbType.Xml:
                //    return typeof(string);

                //case SqlDbType.DateTime:
                //case SqlDbType.SmallDateTime:
                //    return typeof(DateTime);

                //case SqlDbType.Decimal:
                //case SqlDbType.Money:
                //case SqlDbType.SmallMoney:
                //    return typeof(decimal);

                //case SqlDbType.Float:
                //    return typeof(double);

                //case SqlDbType.Int:
                //    return typeof(int);

                //case SqlDbType.Real:
                //    return typeof(float);

                //case SqlDbType.UniqueIdentifier:
                //    return typeof(Guid);

                //case SqlDbType.SmallInt:
                //    return typeof(short);

                //case SqlDbType.TinyInt:
                //    return typeof(byte);

                //case SqlDbType.Udt:
                //    throw SqlClient.Error.UnexpectedTypeCode(SqlDbType.Udt); 
                #endregion
            }
            return typeof(object);
            //return SqlTypeSystem.GetClosestRuntimeType(this.sqlDbType);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            if (this.runtimeOnlyType != null)
            {
                hashCode = this.runtimeOnlyType.GetHashCode();
            }
            else if (this.applicationTypeIndex.HasValue)
            {
                hashCode = this.applicationTypeIndex.Value;
            }
            int? size = this.size;
            return ((((hashCode ^ this.sqlDbType.GetHashCode()) ^ (size.HasValue ? size.GetValueOrDefault() : 0)) ^ this.precision) ^ (this.scale << 8));
        }

        public override IProviderType GetNonUnicodeEquivalent()
        {
            if (this.IsUnicodeType)
            {
                switch (sqlDbType)
                {
                    //case MySqlDbType.C:
                    //    return new MySqlType(SqlDbType.Char, this.Size);

                    case MySqlDbType.Text:
                        return new MySqlType(MySqlDbType.Text);

                    case MySqlDbType.VarChar:
                        return new MySqlType(MySqlDbType.VarChar, this.Size);
                }
            }
            return this;
        }

        private static int GetTypeCoercionPrecedence(MySqlDbType type)
        {
            switch (type)
            {
                //case MySqlDbType.Byte:
                //    return 1;
                //case MySqlDbType.Int16:
                //    return 2;
                //case MySqlDbType.Int24:
                //    return 3;
                //case MySqlDbType.Int32:
                //    return 4;
                //case MySqlDbType.Int64:
                //    return 8;
                //case MySqlDbType.Bit:
                //    return 1;
                #region MyRegion
                //case SqlDbType.Binary:
                //    return 0;

                case MySqlDbType.Bit:
                    return 11;

                case MySqlDbType.String:
                    return 3;

                case MySqlDbType.DateTime:
                    return 0x16;

                case MySqlDbType.Decimal:
                    return 0x12;

                case MySqlDbType.Float:
                    return 20;

                case MySqlDbType.Binary:
                    return 8;

                case MySqlDbType.Int32:
                    return 14;

                //case SqlDbType.Money:
                //    return 0x11;

                //case SqlDbType.NChar:
                //    return 4;

                //case SqlDbType.NText:
                //    return 10;

                //case SqlDbType.NVarChar:
                //    return 5;

                case MySqlDbType.Double:
                    return 0x13;

                //case SqlDbType.UniqueIdentifier:
                //    return 6;

                //case SqlDbType.SmallDateTime:
                //    return 0x15;

                //case SqlDbType.SmallInt:
                //    return 13;

                //case SqlDbType.SmallMoney:
                //    return 0x10;

                //case SqlDbType.Text:
                //    return 9;

                //case SqlDbType.Timestamp:
                //    return 7;

                case MySqlDbType.UByte:
                    return 12;

                //case SqlDbType.VarBinary:
                //    return 1;

                case MySqlDbType.VarChar:
                    return 2;

                //case SqlDbType.Variant:
                //    return 0x18;

                //case SqlDbType.Xml:
                //    return 0x17;

                //case SqlDbType.Udt:
                //    return 0x19; 
                #endregion
            }
            throw SqlClient.Error.UnexpectedTypeCode(type);
        }

        public override bool IsApplicationTypeOf(int index)
        {
            if (!this.IsApplicationType)
            {
                return false;
            }
            int? applicationTypeIndex = this.applicationTypeIndex;
            int num = index;
            return ((applicationTypeIndex.GetValueOrDefault() == num) && applicationTypeIndex.HasValue);
        }

        public override bool IsSameTypeFamily(IProviderType type)
        {
            var type2 = (MySqlType)type;
            if (this.IsApplicationType)
            {
                return false;
            }
            if (type2.IsApplicationType)
            {
                return false;
            }
            return (this.Category == type2.Category);
        }

        protected static string KeyValue<T>(string key, T value)
        {
            if (value != null)
            {
                return (key + "=" + value.ToString() + " ");
            }
            return string.Empty;
        }

        protected static string SingleValue<T>(T value)
        {
            if (value != null)
            {
                return (value.ToString() + " ");
            }
            return string.Empty;
        }

        public override string ToQueryString()
        {
            return this.ToQueryString(QueryFormatOptions.None);
        }

        public override string ToQueryString(QueryFormatOptions formatFlags)
        {
            if (this.runtimeOnlyType != null)
            {
                return this.runtimeOnlyType.ToString();
            }
            var builder = new StringBuilder();
            switch (this.sqlDbType)
            {
                case MySqlDbType.Byte:
                case MySqlDbType.Int16:
                case MySqlDbType.Int24:
                case MySqlDbType.Int32:
                case MySqlDbType.Int64:
                case MySqlDbType.Timestamp:
                case MySqlDbType.Date:
                case MySqlDbType.Time:
                case MySqlDbType.DateTime:
                case MySqlDbType.Year:
                case MySqlDbType.Enum:
                case MySqlDbType.Set:

                case MySqlDbType.TinyBlob:
                case MySqlDbType.MediumBlob:
                case MySqlDbType.LongBlob:
                case MySqlDbType.Blob:

                case MySqlDbType.UByte:
                case MySqlDbType.UInt16:
                case MySqlDbType.UInt24:
                case MySqlDbType.UInt32:
                case MySqlDbType.UInt64:

                case MySqlDbType.TinyText:
                case MySqlDbType.MediumText:
                case MySqlDbType.LongText:
                    builder.Append(sqlDbType.ToString());
                    break;

                case MySqlDbType.Float:
                case MySqlDbType.Double:
                    builder.Append(sqlDbType);
                    if (this.precision != 0)
                    {
                        builder.Append("(");
                        builder.Append(this.precision);
                        if (this.scale != 0)
                        {
                            builder.Append(",");
                            builder.Append(this.scale);
                        }
                        builder.Append(")");
                    }
                    break;
                case MySqlDbType.VarString:
                case MySqlDbType.Binary:
                case MySqlDbType.VarBinary:
                case MySqlDbType.Text:
                    builder.Append(this.sqlDbType);
                    if (!this.size.HasValue || ((this.size == 0) ||
                                                ((formatFlags & QueryFormatOptions.SuppressSize) != QueryFormatOptions.None)))
                    {
                        break;
                    }
                    builder.Append("(");
                    if (this.size != -1)
                    {
                        builder.Append(this.size);
                        break;
                    }
                    builder.Append("MAX");
                    break;
            }
            return builder.ToString();
            #region MyRegion
            //    switch (this.sqlDbType)
            //    {
            //        case SqlDbType.BigInt:
            //        case SqlDbType.Bit:
            //        case SqlDbType.DateTime:
            //        case SqlDbType.Image:
            //        case SqlDbType.Int:
            //        case SqlDbType.Money:
            //        case SqlDbType.NText:
            //        case SqlDbType.UniqueIdentifier:
            //        case SqlDbType.SmallDateTime:
            //        case SqlDbType.SmallInt:
            //        case SqlDbType.SmallMoney:
            //        case SqlDbType.Text:
            //        case SqlDbType.Timestamp:
            //        case SqlDbType.TinyInt:
            //        case SqlDbType.Xml:
            //        case SqlDbType.Udt:
            //            builder.Append(this.sqlDbType.ToString());
            //            goto Label_021D;

            //        case SqlDbType.Binary:
            //        case SqlDbType.Char:
            //        case SqlDbType.NChar:
            //            builder.Append(this.sqlDbType);
            //            if ((formatFlags & QueryFormatOptions.SuppressSize) == QueryFormatOptions.None)
            //            {
            //                builder.Append("(");
            //                builder.Append(this.size);
            //                builder.Append(")");
            //            }
            //            goto Label_021D;

            //        case SqlDbType.Decimal:
            //        case SqlDbType.Float:
            //        case SqlDbType.Real:
            //            builder.Append(this.sqlDbType);
            //            if (this.precision != 0)
            //            {
            //                builder.Append("(");
            //                builder.Append(this.precision);
            //                if (this.scale != 0)
            //                {
            //                    builder.Append(",");
            //                    builder.Append(this.scale);
            //                }
            //                builder.Append(")");
            //            }
            //            goto Label_021D;

            //        case SqlDbType.NVarChar:
            //        case SqlDbType.VarBinary:
            //        case SqlDbType.VarChar:
            //            builder.Append(this.sqlDbType);
            //            if (!this.size.HasValue || ((this.size == 0) || ((formatFlags & QueryFormatOptions.SuppressSize) != QueryFormatOptions.None)))
            //            {
            //                goto Label_021D;
            //            }
            //            builder.Append("(");
            //            if (this.size != -1)
            //            {
            //                builder.Append(this.size);
            //                break;
            //            }
            //            builder.Append("MAX");
            //            break;

            //        case SqlDbType.Variant:
            //            builder.Append("sql_variant");
            //            goto Label_021D;

            //        default:
            //            goto Label_021D;
            //    }
            //    builder.Append(")");
            //Label_021D:
            //    return builder.ToString(); 
            #endregion
        }

        public override string ToString()
        {
            return (SingleValue<Type>(this.GetClosestRuntimeType()) + SingleValue<string>(this.ToQueryString()) + KeyValue<bool>("IsApplicationType", this.IsApplicationType) + KeyValue<bool>("IsUnicodeType", this.IsUnicodeType) + KeyValue<bool>("IsRuntimeOnlyType", this.IsRuntimeOnlyType) + KeyValue<bool>("SupportsComparison", this.SupportsComparison) + KeyValue<bool>("SupportsLength", this.SupportsLength) + KeyValue<bool>("IsLargeType", this.IsLargeType) + KeyValue<bool>("IsFixedSize", this.IsFixedSize) + KeyValue<bool>("IsOrderable", this.IsOrderable) + KeyValue<bool>("IsGroupable", this.IsGroupable) + KeyValue<bool>("IsNumeric", this.IsNumeric) + KeyValue<bool>("IsChar", this.IsChar) + KeyValue<bool>("IsString", this.IsString));
        }

        // Properties
        public override bool CanBeColumn
        {
            get
            {
                return (!this.IsApplicationType && !this.IsRuntimeOnlyType);
            }
        }

        public override bool CanBeParameter
        {
            get
            {
                return (!this.IsApplicationType && !this.IsRuntimeOnlyType);
            }
        }

        public override bool CanSuppressSizeForConversionToString
        {
            get
            {
                int num = 30;
                if (!this.IsLargeType)
                {
                    if (((!this.IsChar && !this.IsString) && this.IsFixedSize) && (this.Size > 0))
                    {
                        int? size = this.Size;
                        int num2 = num;
                        return ((size.GetValueOrDefault() < num2) && size.HasValue);
                    }
                    switch (sqlDbType)
                    {
                        case MySqlDbType.Double:
                        case MySqlDbType.Int16:
                        case MySqlDbType.Int24:
                        case MySqlDbType.Int32:
                        case MySqlDbType.Int64:
                        case MySqlDbType.Bit:
                        case MySqlDbType.Float:
                        case MySqlDbType.Decimal:
                            return true;
                    }
                }
                return false;
            }
        }

        internal TypeCategory Category
        {
            get
            {
                switch (sqlDbType)
                {
                    case MySqlDbType.Bit:
                    case MySqlDbType.Int16:
                    case MySqlDbType.Int24:
                    case MySqlDbType.Int32:
                    case MySqlDbType.Int64:
                    case MySqlDbType.Float:
                    case MySqlDbType.Double:
                    case MySqlDbType.Decimal:
                        return TypeCategory.Numeric;

                    case MySqlDbType.Binary:
                    case MySqlDbType.VarBinary:
                        return TypeCategory.Binary;

                    case MySqlDbType.VarChar:
                    case MySqlDbType.VarString:
                    case MySqlDbType.TinyText:
                        return TypeCategory.Char;
                }
                #region MyRegion
                //switch (this.SqlDbType)
                //{
                //    case SqlDbType.BigInt:
                //    case SqlDbType.Bit:
                //    case SqlDbType.Decimal:
                //    case SqlDbType.Float:
                //    case SqlDbType.Int:
                //    case SqlDbType.Money:
                //    case SqlDbType.Real:
                //    case SqlDbType.SmallInt:
                //    case SqlDbType.SmallMoney:
                //    case SqlDbType.TinyInt:
                //        return TypeCategory.Numeric;

                //    case SqlDbType.Binary:
                //    case SqlDbType.Timestamp:
                //    case SqlDbType.VarBinary:
                //        return TypeCategory.Binary;

                //    case SqlDbType.Char:
                //    case SqlDbType.NChar:
                //    case SqlDbType.NVarChar:
                //    case SqlDbType.VarChar:
                //        return TypeCategory.Char;

                //    case SqlDbType.DateTime:
                //    case SqlDbType.SmallDateTime:
                //        return TypeCategory.DateTime;

                //    case SqlDbType.Image:
                //        return TypeCategory.Image;

                //    case SqlDbType.NText:
                //    case SqlDbType.Text:
                //        return TypeCategory.Text;

                //    case SqlDbType.UniqueIdentifier:
                //        return TypeCategory.UniqueIdentifier;

                //    case SqlDbType.Variant:
                //        return TypeCategory.Variant;

                //    case SqlDbType.Xml:
                //        return TypeCategory.Xml;

                //    case SqlDbType.Udt:
                //        return TypeCategory.Udt; 

                //}
                #endregion
                throw SqlClient.Error.UnexpectedTypeCode(this);
            }
        }

        public override bool HasPrecisionAndScale
        {
            get
            {
                switch (sqlDbType)
                {
                    case MySqlDbType.Decimal:
                    case MySqlDbType.Float:
                    case MySqlDbType.Double:
                        return true;
                }
                return false;
            }
        }

        public override bool HasSizeOrIsLarge
        {
            get
            {
                if (!this.size.HasValue)
                {
                    return this.IsLargeType;
                }
                return true;
            }
        }

        public override bool IsApplicationType
        {
            get
            {
                return this.applicationTypeIndex.HasValue;
            }
        }

        public override bool IsChar
        {
            get
            {
                if (!this.IsApplicationType && !this.IsRuntimeOnlyType)
                {
                    switch (sqlDbType)
                    {
                        case MySqlDbType.VarChar:
                        case MySqlDbType.String:
                            return (this.Size == 1);
                    }
                }
                return false;
            }
        }

        public override bool IsFixedSize
        {
            get
            {
                switch (this.sqlDbType)
                {
                    case MySqlDbType.Binary:
                    case MySqlDbType.VarBinary:
                    case MySqlDbType.Blob:
                    case MySqlDbType.Text:

                        //case SqlDbType.VarBinary:
                        //case SqlDbType.VarChar:
                        //case SqlDbType.Xml:
                        //case SqlDbType.NText:
                        //case SqlDbType.NVarChar:
                        //case SqlDbType.Image:
                        return false;
                }
                return true;
            }
        }

        public override bool IsGroupable
        {
            get
            {
                if (this.IsRuntimeOnlyType)
                {
                    return false;
                }
                switch (sqlDbType)
                {
                    case MySqlDbType.Binary:
                    case MySqlDbType.VarBinary:
                    case MySqlDbType.Blob:
                    case MySqlDbType.Text:
                        return false;
                }
                return true;
                #region MyRegion
                //    var sqlDbType = this.sqlDbType;
                //    if (sqlDbType <= SqlDbType.NText)
                //    {
                //        switch (sqlDbType)
                //        {
                //            case SqlDbType.Image:
                //            case SqlDbType.NText:
                //                goto Label_002B;
                //        }
                //        goto Label_002D;
                //    }
                //    if ((sqlDbType != SqlDbType.Text) && (sqlDbType != SqlDbType.Xml))
                //    {
                //        goto Label_002D;
                //    }
                //Label_002B:
                //    return false;
                //Label_002D:
                //    return true; 
                #endregion
            }
        }

        public override bool IsLargeType
        {
            get
            {
                switch (this.sqlDbType)
                {
                    case MySqlDbType.Text:
                    case MySqlDbType.MediumBlob:
                    case MySqlDbType.MediumText:
                    case MySqlDbType.LongBlob:
                        return true;

                    case MySqlDbType.VarBinary:
                    case MySqlDbType.VarChar:
                    case MySqlDbType.Binary:
                        return (this.size == -1);
                }
                return false;
                //switch (this.sqlDbType)
                //{
                //    case SqlDbType.Text:
                //    case SqlDbType.Xml:
                //    case SqlDbType.NText:
                //    case SqlDbType.Image:
                //        return true;

                //    case SqlDbType.VarBinary:
                //    case SqlDbType.VarChar:
                //    case SqlDbType.NVarChar:
                //        return (this.size == -1);
                //}
                //return false;
            }
        }

        public override bool IsNumeric
        {
            get
            {
                if (!this.IsApplicationType && !this.IsRuntimeOnlyType)
                {
                    switch (sqlDbType)
                    {
                        case MySqlDbType.Bit:
                        case MySqlDbType.Decimal:
                        case MySqlDbType.Float:
                        case MySqlDbType.Double:
                        case MySqlDbType.Byte:
                        case MySqlDbType.Int16:
                        case MySqlDbType.Int24:
                        case MySqlDbType.Int32:
                        case MySqlDbType.Int64:
                        case MySqlDbType.UByte:
                        case MySqlDbType.UInt16:
                        case MySqlDbType.UInt24:
                        case MySqlDbType.UInt32:
                        case MySqlDbType.UInt64:
                            return true;
                    }
                }
                return false;
            }
        }

        public override bool IsOrderable
        {
            get
            {
                if (this.IsRuntimeOnlyType)
                {
                    return false;
                }
                return true;
                #region MyRegion
                //    var sqlDbType = this.sqlDbType;
                //    if (sqlDbType <= SqlDbType.NText)
                //    {
                //        switch (sqlDbType)
                //        {
                //            case SqlDbType.Image:
                //            case SqlDbType.NText:
                //                goto Label_002B;
                //        }
                //        goto Label_002D;
                //    }
                //    if ((sqlDbType != SqlDbType.Text) && (sqlDbType != SqlDbType.Xml))
                //    {
                //        goto Label_002D;
                //    }
                //Label_002B:
                //    return false;
                //Label_002D:
                //    return true;
                #endregion
            }
        }

        public override bool IsRuntimeOnlyType
        {
            get
            {
                return (this.runtimeOnlyType != null);
            }
        }

        public override bool IsString
        {
            get
            {
                if (this.IsApplicationType || this.IsRuntimeOnlyType)
                {
                    return false;
                }
                switch (sqlDbType)
                {
                    case MySqlDbType.String:
                    case MySqlDbType.Text:

                        return true;
                }
                int? nullable = this.Size;
                if (((nullable.GetValueOrDefault() != 0) || !nullable.HasValue) && (this.Size <= 1))
                {
                    return (this.Size == -1);
                }
                return false;
                #region MyRegion
                //int? nullable;
                //if (this.IsApplicationType || this.IsRuntimeOnlyType)
                //{
                //    return false;
                //}
                //var sqlDbType = this.SqlDbType;
                //if (sqlDbType <= SqlDbType.NVarChar)
                //{
                //    switch (sqlDbType)
                //    {
                //        case SqlDbType.NChar:
                //        case SqlDbType.NVarChar:
                //        case SqlDbType.Char:
                //            goto Label_0043;

                //        case SqlDbType.NText:
                //            goto Label_0099;
                //    }
                //    goto Label_009B;
                //}
                //if (sqlDbType == SqlDbType.Text)
                //{
                //    goto Label_0099;
                //}
                //if (sqlDbType != SqlDbType.VarChar)
                //{
                //    goto Label_009B;
                //}
                //Label_0043:
                //nullable = this.Size;
                //if (((nullable.GetValueOrDefault() != 0) || !nullable.HasValue) && (this.Size <= 1))
                //{
                //    return (this.Size == -1);
                //}
                //return true;
                //Label_0099:
                //return true;
                //Label_009B:
                //return false; 
                #endregion
            }
        }

        public override bool IsBinary
        {
            get { return Category == TypeCategory.Binary; }
        }

        public override bool IsDateTime
        {
            get { throw new System.NotImplementedException(); }
        }

        private bool IsTypeKnownByProvider
        {
            get
            {
                return (!this.IsApplicationType && !this.IsRuntimeOnlyType);
            }
        }

        public override bool IsUnicodeType
        {
            get
            {
                //switch (this.SqlDbType)
                //{
                //    case SqlDbType.NChar:
                //    case SqlDbType.NText:
                //    case SqlDbType.NVarChar:
                //        return true;
                //}
                return false;
            }
        }

        public override int Precision
        {
            get
            {
                return this.precision;
            }
        }

        public override int Scale
        {
            get
            {
                return this.scale;
            }
        }

        public override int? ApplicationTypeIndex
        {
            get { return applicationTypeIndex; }
            set { applicationTypeIndex = value; }
        }

        public override Type RuntimeOnlyType
        {
            get { return runtimeOnlyType; }
            set { runtimeOnlyType = value; }
        }

        public override int? Size
        {
            get
            {
                return this.size;
            }
        }

        public override bool SupportsComparison
        {
            get
            {
                switch (sqlDbType)
                {
                    case MySqlDbType.Binary:
                        return false;
                }
                return true;
                #region MyRegion
                //    SqlDbType sqlDbType = this.sqlDbType;
                //    if (sqlDbType <= SqlDbType.NText)
                //    {
                //        switch (sqlDbType)
                //        {
                //            case SqlDbType.Image:
                //            case SqlDbType.NText:
                //                goto Label_0021;
                //        }
                //        goto Label_0023;
                //    }
                //    if ((sqlDbType != SqlDbType.Text) && (sqlDbType != SqlDbType.Xml))
                //    {
                //        goto Label_0023;
                //    }
                //Label_0021:
                //    return false;
                //Label_0023:
                //    return true; 
                #endregion
            }
        }

        public override bool SupportsLength
        {
            get
            {
                switch (sqlDbType)
                {
                    case MySqlDbType.VarChar:
                    case MySqlDbType.VarBinary:
                    case MySqlDbType.VarString:
                        return true;
                }
                return false;
                #region MyRegion
                //    SqlDbType sqlDbType = this.sqlDbType;
                //    if (sqlDbType <= SqlDbType.NText)
                //    {
                //        switch (sqlDbType)
                //        {
                //            case SqlDbType.Image:
                //            case SqlDbType.NText:
                //                goto Label_0021;
                //        }
                //        goto Label_0023;
                //    }
                //    if ((sqlDbType != SqlDbType.Text) && (sqlDbType != SqlDbType.Xml))
                //    {
                //        goto Label_0023;
                //    }
                //Label_0021:
                //    return false;
                //Label_0023:
                //    return true; 
                #endregion
            }
        }

        // Nested Types
        internal enum TypeCategory
        {
            Numeric,
            Char,
            Text,
            Binary,
            Image,
            Xml,
            DateTime,
            UniqueIdentifier,
            Variant,
            Udt
        }
    }
}
