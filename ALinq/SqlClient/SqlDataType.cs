using System;
using System.Collections.Generic;
using System.Diagnostics;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    abstract class SqlDataType<DBType> : IProviderType
    {
        internal const int NULL_VALUE = -2;
        internal const int DEFAULT_DECIMAL_SCALE = 4;
        internal const int DEFAULT_DECIMAL_PRECISION = 29;
        internal const int STRING_SIZE = 4000;

        private readonly bool isUnicodeType = false;
        // Fields
        //private int precision;
        //private Type runtimeOnlyType;
        //private int scale;
        //protected int? size;
        //private Enum sqlDbType;

        // Methods
        #region MyRegion
        //internal SqlDataType(Enum type)
        //{
        //    ApplicationTypeIndex = null;
        //    sqlDbType = type;
        //}

        //internal SqlDataType(int applicationTypeIndex)
        //{
        //    ApplicationTypeIndex = applicationTypeIndex;
        //}

        //internal SqlDataType(Type type)
        //{
        //    ApplicationTypeIndex = null;
        //    runtimeOnlyType = type;
        //}

        //internal SqlDataType(Enum type, int? size)
        //{
        //    ApplicationTypeIndex = null;
        //    sqlDbType = type;
        //    Size = size;
        //}

        //internal SqlDataType(Enum type, int precision, int scale)
        //{
        //    ApplicationTypeIndex = null;
        //    sqlDbType = type;
        //    this.precision = precision;
        //    this.scale = scale;
        //} 
        #endregion

        protected SqlDataType(Dictionary<TypeCode, DBType> typeMapping)
            : this(typeMapping, default(DBType), null, NULL_VALUE, NULL_VALUE, null)
        {
            //TypeMapping = typeMapping;
        }

        protected SqlDataType(Dictionary<TypeCode, DBType> typeMapping, DBType dbType)
            : this(typeMapping, dbType, null, NULL_VALUE, NULL_VALUE, null)
        {
            //TypeMapping = typeMapping;
            //SqlDbType = dbType;
        }

        protected SqlDataType(Dictionary<TypeCode, DBType> typeMapping, DBType dbType, int precision, int scale)
            : this(typeMapping, dbType, null, precision, scale, null)
        {
            //TypeMapping = typeMapping;
            //SqlDbType = dbType;
            //Precision = precision;
            //Scale = scale;
        }

        protected SqlDataType(Dictionary<TypeCode, DBType> typeMapping, DBType dbType, int size)
            : this(typeMapping, dbType, null, NULL_VALUE, NULL_VALUE, size)
        {
            //TypeMapping = typeMapping;
            //this.SqlDbType = dbType;
            //Size = size;
        }

        protected SqlDataType(Dictionary<TypeCode, DBType> typeMapping, DBType dbType, Type runtimeType, int precision, int scale, int? size)
        {
            TypeMapping = typeMapping;
            SqlDbType = dbType;
            RuntimeOnlyType = runtimeType;
            Precision = precision;
            Scale = scale;

            isUnicodeType = IsUnicode(dbType);

            if (IsString)
            {
                Size = size > 0 ? size : STRING_SIZE;
            }
            else
            {
                Size = size;
            }

        }

        protected virtual bool IsUnicode(DBType type)
        {
            return false;
        }

        private Dictionary<TypeCode, DBType> TypeMapping
        {
            get;
            set;
        }

        public bool AreValuesEqual(object o1, object o2)
        {
            if ((o1 == null) || (o2 == null))
            {
                return false;
            }
            if (Equals(SqlDbType, TypeMapping[TypeCode.Char]) || Equals(SqlDbType, TypeMapping[TypeCode.String]))
            {
                if (Size <= 1024)
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
            }

            return o1.Equals(o2);
        }

        public int ComparePrecedenceTo(IProviderType type)
        {
            var type2 = (SqlDataType<DBType>)type;
            int num = IsTypeKnownByProvider ? GetTypeCoercionPrecedence(SqlDbType) : Int32.MinValue;//-2147483648;
            int num2 = type2.IsTypeKnownByProvider ? GetTypeCoercionPrecedence(type2.SqlDbType) : Int32.MinValue;//-2147483648;
            return num.CompareTo(num2);
        }

        public override bool Equals(object obj)
        {
            //if (this == obj)
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var type = obj as SqlDataType<DBType>;
            return (((type != null) && (((RuntimeOnlyType == type.RuntimeOnlyType) &&
                                         ((ApplicationTypeIndex == type.ApplicationTypeIndex) && (Equals(SqlDbType, type.SqlDbType)))) &&
                                        ((Size == type.Size) && (Precision == type.Precision)))) && (Scale == type.Scale));
        }

        public abstract Type GetClosestRuntimeType();
        //public Type GetClosestRuntimeType()
        //{
        //    if (RuntimeOnlyType != null)
        //    {
        //        return RuntimeOnlyType;
        //    }
        //    return GetClosestRuntimeType(SqlDbType);
        //}

        //protected abstract Type GetClosestRuntimeType(DBType sqlDbType);

        public override int GetHashCode()
        {
            int hashCode = 0;
            if (RuntimeOnlyType != null)
            {
                hashCode = RuntimeOnlyType.GetHashCode();
            }
            else if (ApplicationTypeIndex.HasValue)
            {
                hashCode = ApplicationTypeIndex.Value;
            }
            return ((((hashCode ^ SqlDbType.GetHashCode()) ^ (Size.HasValue ? Size.GetValueOrDefault() : 0)) ^ Precision) ^ (Scale << 8));
        }

        public IProviderType GetNonUnicodeEquivalent()
        {
            return this;
        }

        protected abstract int GetTypeCoercionPrecedence(DBType type);

        public bool IsApplicationTypeOf(int index)
        {
            if (!IsApplicationType)
            {
                return false;
            }
            int? applicationTypeIndex = ApplicationTypeIndex;
            int num = index;
            return ((applicationTypeIndex.GetValueOrDefault() == num) && applicationTypeIndex.HasValue);
        }

        public bool IsSameTypeFamily(IProviderType type)
        {
            Debug.Assert(type is SqlDataType<DBType>);
            var type2 = (SqlDataType<DBType>)type;
            if (IsApplicationType)
            {
                return false;
            }
            if (Category == DBTypeCategory.Cursor && type.RuntimeOnlyType == typeof(object))
            {
                return true;
            }
            if (type2.IsApplicationType)
            {
                return false;
            }
            /*
            if (Category == DBTypeCategory.Numeric && type2.Category == DBTypeCategory.Text)
                return true;
            */
            return (Category == type2.Category);
        }

        protected static string KeyValue<T>(string key, T value)
        {
            if (value != null)
            {
                return (key + "=" + value + " ");
            }
            return string.Empty;
        }

        protected static string SingleValue<T>(T value)
        {
            if (value != null)
            {
                return (value + " ");
            }
            return string.Empty;
        }

        public string ToQueryString()
        {
            return ToQueryString(QueryFormatOptions.None);
        }

        public virtual string ToQueryString(QueryFormatOptions formatFlags)
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType.ToString();
            }
            return SqlDbType.ToString();
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

        // Properties
        public bool CanBeColumn
        {
            get
            {
                return (!IsApplicationType && !IsRuntimeOnlyType);
            }
        }

        public bool CanBeParameter
        {
            get
            {
                return (!IsApplicationType && !IsRuntimeOnlyType);
            }
        }

        public bool CanSuppressSizeForConversionToString
        {
            get
            {
                return false;
            }
        }

        internal abstract DBTypeCategory Category
        {
            get;
        }

        public abstract bool HasPrecisionAndScale { get; }

        public bool HasSizeOrIsLarge
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

        public bool IsApplicationType
        {
            get
            {
                return ApplicationTypeIndex.HasValue;
            }
        }

        public bool IsChar
        {
            get
            {
                if (!IsApplicationType && !IsRuntimeOnlyType)
                {
                    if (Size == 1)
                    {
                        var type1 = TypeMapping[TypeCode.Char];
                        var type2 = TypeMapping[TypeCode.String];
                        if (Equals(SqlDbType, type1) || Equals(SqlDbType, type2))
                            return true;
                    }
                }
                return false;
            }
        }

        public abstract bool IsFixedSize { get; }

        public virtual bool IsGroupable
        {
            get
            {
                if (IsRuntimeOnlyType)
                {
                    return false;
                }
                return true;
            }
        }

        public abstract bool IsLargeType { get; }

        public abstract bool IsNumeric { get; }

        public bool IsOrderable
        {
            get
            {
                return true;
            }
        }

        public bool IsRuntimeOnlyType
        {
            get
            {
                return (RuntimeOnlyType != null);
            }
        }

        public bool IsString
        {
            get
            {
                if (IsApplicationType || IsRuntimeOnlyType)
                {
                    return false;
                }
                if (Equals(SqlDbType, TypeMapping[TypeCode.Char]) ||
                    Equals(SqlDbType, TypeMapping[TypeCode.String]))
                    return true;
                return false;

            }
        }

        public bool IsBinary
        {
            get
            {
                return Category == DBTypeCategory.Binary;
            }
        }

        public bool IsDateTime
        {
            get { return Category == DBTypeCategory.DateTime; }
        }

        private bool IsTypeKnownByProvider
        {
            get
            {
                return (!IsApplicationType && !IsRuntimeOnlyType);
            }
        }

        public virtual bool IsUnicodeType
        {
            get { return isUnicodeType; }
        }

        public int Precision
        {
            get;
            set;
        }

        public int Scale
        {
            get;
            set;
        }


        public int? Size
        {
            get;
            set;
        }

        Enum IProviderType.SqlDbType
        {
            get
            {
                //return Convert.ToInt32(SqlDbType);
                return (Enum)(SqlDbType as object);
            }
        }

        public DBType SqlDbType
        {
            get;
            set;
        }

        public bool SupportsComparison
        {
            get
            {
                return true;
            }
        }

        public bool SupportsLength
        {
            get
            {
                return true;
            }
        }

        public int? ApplicationTypeIndex { get; set; }

        public Type RuntimeOnlyType
        {
            get;
            set;
        }


    }

    // Nested Types
    internal enum DBTypeCategory
    {
        Numeric,
        Text,
        Binary,
        DateTime,
        Cursor
    }
}