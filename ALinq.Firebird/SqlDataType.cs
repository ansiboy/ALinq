using System;
using System.Collections.Generic;
using System.Diagnostics;
using ALinq.SqlClient;

namespace ALinq.Firebird
{
    #region MyRegion
    //abstract class SqlDataType : IProviderType
    //{
    //    // Fields
    //    protected int precision;
    //    private Type runtimeOnlyType;
    //    protected int scale;
    //    //protected int? size;
    //    protected Enum sqlDbType;

    //    // Methods
    //    internal SqlDataType(Enum type)
    //    {
    //        ApplicationTypeIndex = null;
    //        sqlDbType = type;
    //    }

    //    internal SqlDataType(int applicationTypeIndex)
    //    {
    //        ApplicationTypeIndex = applicationTypeIndex;
    //    }

    //    internal SqlDataType(Type type)
    //    {
    //        ApplicationTypeIndex = null;
    //        runtimeOnlyType = type;
    //    }

    //    internal SqlDataType(Enum type, int? size)
    //    {
    //        ApplicationTypeIndex = null;
    //        sqlDbType = type;
    //        Size = size;
    //    }

    //    internal SqlDataType(Enum type, int precision, int scale)
    //    {
    //        ApplicationTypeIndex = null;
    //        sqlDbType = type;
    //        this.precision = precision;
    //        this.scale = scale;
    //    }

    //    protected SqlDataType()
    //    {

    //    }

    //    //protected abstract Dictionary<TypeCode, Enum> TypeMapping
    //    //{
    //    //    get;
    //    //}

    //    public bool AreValuesEqual(object o1, object o2)
    //    {
    //        if ((o1 == null) || (o2 == null))
    //        {
    //            return false;
    //        }
    //        //var sqlDbType = this.sqlDbType;
    //        //if (sqlDbType == FbDbType.Char || sqlDbType == FbDbType.VarChar)
    //        if (SqlDbType == TypeMapping[TypeCode.Char] || SqlDbType == TypeMapping[TypeCode.String])
    //        {
    //            //case System.Data.SqlDbType.Char:
    //            //case System.Data.SqlDbType.Text:
    //            //case System.Data.SqlDbType.VarChar:
    //            var str = o1 as string;
    //            if (str != null)
    //            {
    //                var str2 = o2 as string;
    //                if (str2 != null)
    //                {
    //                    return str.TrimEnd(new[] { ' ' }).Equals(str2.TrimEnd(new[] { ' ' }), StringComparison.Ordinal);
    //                }
    //            }
    //        }

    //        return o1.Equals(o2);
    //    }

    //    public int ComparePrecedenceTo(IProviderType type)
    //    {
    //        var type2 = (SqlDataType)type;
    //        int num = IsTypeKnownByProvider ? GetTypeCoercionPrecedence(SqlDbType) : Int32.MinValue;//-2147483648;
    //        int num2 = type2.IsTypeKnownByProvider ? GetTypeCoercionPrecedence(type2.SqlDbType) : Int32.MinValue;//-2147483648;
    //        return num.CompareTo(num2);
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        //if (this == obj)
    //        if (ReferenceEquals(this, obj))
    //        {
    //            return true;
    //        }
    //        var type = obj as SqlDataType;
    //        return (((type != null) && (((runtimeOnlyType == type.runtimeOnlyType) &&
    //                                     ((ApplicationTypeIndex == type.ApplicationTypeIndex) && (sqlDbType == type.sqlDbType))) &&
    //                                    ((Size == type.Size) && (precision == type.precision)))) && (scale == type.scale));
    //    }

    //    public Type GetClosestRuntimeType()
    //    {
    //        if (runtimeOnlyType != null)
    //        {
    //            return runtimeOnlyType;
    //        }
    //        return GetClosestRuntimeType(sqlDbType);
    //    }

    //    protected abstract Type GetClosestRuntimeType(Enum sqlDbType);

    //    public override int GetHashCode()
    //    {
    //        int hashCode = 0;
    //        if (runtimeOnlyType != null)
    //        {
    //            hashCode = runtimeOnlyType.GetHashCode();
    //        }
    //        else if (ApplicationTypeIndex.HasValue)
    //        {
    //            hashCode = ApplicationTypeIndex.Value;
    //        }
    //        return ((((hashCode ^ sqlDbType.GetHashCode()) ^ (Size.HasValue ? Size.GetValueOrDefault() : 0)) ^ precision) ^ (scale << 8));
    //    }

    //    public IProviderType GetNonUnicodeEquivalent()
    //    {
    //        return this;
    //    }

    //    protected abstract int GetTypeCoercionPrecedence(Enum type);

    //    public bool IsApplicationTypeOf(int index)
    //    {
    //        if (!IsApplicationType)
    //        {
    //            return false;
    //        }
    //        int? applicationTypeIndex = ApplicationTypeIndex;
    //        int num = index;
    //        return ((applicationTypeIndex.GetValueOrDefault() == num) && applicationTypeIndex.HasValue);
    //    }

    //    public bool IsSameTypeFamily(IProviderType type)
    //    {
    //        Debug.Assert(type is SqlDataType);
    //        var type2 = (SqlDataType)type;
    //        if (IsApplicationType)
    //        {
    //            return false;
    //        }
    //        if (type2.IsApplicationType)
    //        {
    //            return false;
    //        }
    //        return (Category == type2.Category);
    //    }

    //    protected static string KeyValue<T>(string key, T value)
    //    {
    //        if (value != null)
    //        {
    //            return (key + "=" + value + " ");
    //        }
    //        return string.Empty;
    //    }

    //    protected static string SingleValue<T>(T value)
    //    {
    //        if (value != null)
    //        {
    //            return (value + " ");
    //        }
    //        return string.Empty;
    //    }

    //    public string ToQueryString()
    //    {
    //        return this.ToQueryString(QueryFormatOptions.None);
    //    }

    //    public string ToQueryString(QueryFormatOptions formatFlags)
    //    {
    //        if (this.runtimeOnlyType != null)
    //        {
    //            return this.runtimeOnlyType.ToString();
    //        }
    //        return sqlDbType.ToString();
    //    }

    //    public override string ToString()
    //    {
    //        return (SingleValue(GetClosestRuntimeType()) + SingleValue(ToQueryString()) +
    //                KeyValue("IsApplicationType", IsApplicationType) + KeyValue("IsUnicodeType", IsUnicodeType) +
    //                KeyValue("IsRuntimeOnlyType", IsRuntimeOnlyType) + KeyValue("SupportsComparison", SupportsComparison) +
    //                KeyValue("SupportsLength", SupportsLength) + KeyValue("IsLargeType", IsLargeType) +
    //                KeyValue("IsFixedSize", IsFixedSize) + KeyValue("IsOrderable", IsOrderable) +
    //                KeyValue("IsGroupable", IsGroupable) + KeyValue("IsNumeric", IsNumeric) +
    //                KeyValue("IsChar", IsChar) + KeyValue("IsString", IsString));
    //    }

    //    // Properties
    //    public bool CanBeColumn
    //    {
    //        get
    //        {
    //            return (!IsApplicationType && !IsRuntimeOnlyType);
    //        }
    //    }

    //    public bool CanBeParameter
    //    {
    //        get
    //        {
    //            return (!IsApplicationType && !IsRuntimeOnlyType);
    //        }
    //    }

    //    public bool CanSuppressSizeForConversionToString
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    internal abstract TypeCategory Category
    //    {
    //        get;
    //    }

    //    public abstract bool HasPrecisionAndScale { get; }

    //    public bool HasSizeOrIsLarge
    //    {
    //        get
    //        {
    //            if (!Size.HasValue)
    //            {
    //                return IsLargeType;
    //            }
    //            return true;
    //        }
    //    }

    //    public bool IsApplicationType
    //    {
    //        get
    //        {
    //            return this.ApplicationTypeIndex.HasValue;
    //        }
    //    }

    //    public bool IsChar
    //    {
    //        get
    //        {
    //            if (!IsApplicationType && !IsRuntimeOnlyType)
    //            {
    //                if (Size == 1)
    //                {
    //                    var type1 = TypeMapping[TypeCode.Char];
    //                    var type2 = TypeMapping[TypeCode.String];
    //                    if (SqlDbType == type1 || SqlDbType == type2)
    //                        return true;
    //                }
    //            }
    //            return false;
    //        }
    //    }

    //    public abstract bool IsFixedSize { get; }

    //    public virtual bool IsGroupable
    //    {
    //        get
    //        {
    //            if (IsRuntimeOnlyType)
    //            {
    //                return false;
    //            }
    //            return true;
    //        }
    //    }

    //    public abstract bool IsLargeType { get; }

    //    public abstract bool IsNumeric { get; }

    //    public bool IsOrderable
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }

    //    public bool IsRuntimeOnlyType
    //    {
    //        get
    //        {
    //            return (this.runtimeOnlyType != null);
    //        }
    //    }

    //    public bool IsString
    //    {
    //        get
    //        {
    //            if (IsApplicationType || IsRuntimeOnlyType)
    //            {
    //                return false;
    //            }
    //            if (SqlDbType == TypeMapping[TypeCode.Char] ||
    //                SqlDbType == TypeMapping[TypeCode.String])
    //                return true;
    //            return false;

    //        }
    //    }

    //    private bool IsTypeKnownByProvider
    //    {
    //        get
    //        {
    //            return (!IsApplicationType && !IsRuntimeOnlyType);
    //        }
    //    }

    //    public bool IsUnicodeType
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    public int Precision
    //    {
    //        get;
    //        set;
    //    }

    //    public int Scale
    //    {
    //        get;
    //        set;
    //    }


    //    public int? Size
    //    {
    //        get;
    //        set;
    //    }

    //    public Enum SqlDbType
    //    {
    //        get { return sqlDbType; }
    //        set { sqlDbType = value; }
    //    }

    //    public bool SupportsComparison
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }

    //    public bool SupportsLength
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }

    //    public int? ApplicationTypeIndex { get; set; }
    //    public Type RuntimeOnlyType
    //    {
    //        get { return runtimeOnlyType; }
    //        set { runtimeOnlyType = value; ; }
    //    }

    //    // Nested Types
    //    internal enum TypeCategory
    //    {
    //        Numeric,
    //        Text,
    //        Binary,
    //        DateTime,
    //    }
    //} 
    #endregion
}