using System;

namespace ALinq.SqlClient
{
    internal interface IProviderType
    {
        bool AreValuesEqual(object o1, object o2);
        int ComparePrecedenceTo(IProviderType type);
        bool Equals(object obj);
        Type GetClosestRuntimeType();
        int GetHashCode();
        IProviderType GetNonUnicodeEquivalent();
        bool IsApplicationTypeOf(int index);
        bool IsSameTypeFamily(IProviderType type);
        string ToQueryString();
        string ToQueryString(QueryFormatOptions formatOptions);
        bool CanBeColumn { get; }
        bool CanBeParameter { get; }
        bool CanSuppressSizeForConversionToString { get; }
        bool HasPrecisionAndScale { get; }
        bool HasSizeOrIsLarge { get; }
        bool IsApplicationType { get; }
        bool IsChar { get; }
        bool IsFixedSize { get; }
        bool IsGroupable { get; }
        bool IsLargeType { get; }
        bool IsNumeric { get; }
        bool IsOrderable { get; }
        bool IsRuntimeOnlyType { get; }
        bool IsString { get; }
        bool IsBinary { get; }
        bool IsDateTime { get; }
        bool IsUnicodeType { get; }
        int? Size { get; }
        bool SupportsComparison { get; }
        bool SupportsLength { get; }
        int Precision { get; }//{ get; set; }
        int Scale { get; }//{ get; set; }}
        Enum SqlDbType { get; }
        int? ApplicationTypeIndex { get; set; }
        Type RuntimeOnlyType { get; set; }
    }

    internal abstract class ProviderType : IProviderType
    {
        // Methods
        protected ProviderType()
        {
        }

        public abstract bool AreValuesEqual(object o1, object o2);
        public abstract int ComparePrecedenceTo(IProviderType type);
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public abstract Type GetClosestRuntimeType();
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public abstract IProviderType GetNonUnicodeEquivalent();
        public abstract bool IsApplicationTypeOf(int index);
        public abstract bool IsSameTypeFamily(IProviderType type);

        //public static bool operator ==(ProviderType typeA, ProviderType typeB)
        //{
        //    //return ((typeA == typeB) || ((typeA != null) && typeA.Equals(typeB)));
        //    return ((Equals(typeA, typeB)) || (!Equals(typeA, null) && typeA.Equals(typeB)));
        //}

        //public static bool operator !=(ProviderType typeA, ProviderType typeB)
        //{
        //    if (Equals(typeA, typeB))
        //    {
        //        return false;
        //    }
        //    if (!Equals(typeA, null))
        //    {
        //        return !typeA.Equals(typeB);
        //    }
        //    return true;
        //}

        public abstract string ToQueryString();
        public abstract string ToQueryString(QueryFormatOptions formatOptions);

        // Properties
        public abstract bool CanBeColumn { get; }

        public abstract bool CanBeParameter { get; }

        public abstract bool CanSuppressSizeForConversionToString { get; }

        public abstract bool HasPrecisionAndScale { get; }

        public abstract bool HasSizeOrIsLarge { get; }

        public abstract bool IsApplicationType { get; }

        public abstract bool IsChar { get; }

        public abstract bool IsFixedSize { get; }

        public abstract bool IsGroupable { get; }

        public abstract bool IsLargeType { get; }

        public abstract bool IsNumeric { get; }

        public abstract bool IsOrderable { get; }

        public abstract bool IsRuntimeOnlyType { get; }

        public abstract bool IsString { get; }
        public abstract bool IsBinary { get; }
        public abstract bool IsDateTime { get; }

        public abstract bool IsUnicodeType { get; }

        public virtual int? Size { get; set; }

        public abstract bool SupportsComparison { get; }

        public abstract bool SupportsLength { get; }

        public virtual int Precision { get; set; }

        public virtual int Scale { get; set; }

        public Enum SqlDbType { get; set; }
        public abstract int? ApplicationTypeIndex { get; set; }
        public abstract Type RuntimeOnlyType { get; set; }


        //public abstract Enum SqlDbType { get; }


    }

    internal abstract class ProviderType<DBType> : IProviderType
    {
        // Methods

        public abstract bool AreValuesEqual(object o1, object o2);

        int IProviderType.ComparePrecedenceTo(IProviderType type)
        {
            return ComparePrecedenceTo((ProviderType<DBType>)type);
        }

        public abstract int ComparePrecedenceTo(ProviderType<DBType> type);

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public abstract Type GetClosestRuntimeType();

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        IProviderType IProviderType.GetNonUnicodeEquivalent()
        {
            return GetNonUnicodeEquivalent();
        }

        public abstract ProviderType<DBType> GetNonUnicodeEquivalent();

        public abstract bool IsApplicationTypeOf(int index);
        public abstract bool IsSameTypeFamily(IProviderType type);

        //public static bool operator ==(ProviderType typeA, ProviderType typeB)
        //{
        //    //return ((typeA == typeB) || ((typeA != null) && typeA.Equals(typeB)));
        //    return ((Equals(typeA, typeB)) || (!Equals(typeA, null) && typeA.Equals(typeB)));
        //}

        //public static bool operator !=(ProviderType typeA, ProviderType typeB)
        //{
        //    if (Equals(typeA, typeB))
        //    {
        //        return false;
        //    }
        //    if (!Equals(typeA, null))
        //    {
        //        return !typeA.Equals(typeB);
        //    }
        //    return true;
        //}

        public abstract string ToQueryString();
        public abstract string ToQueryString(QueryFormatOptions formatOptions);

        // Properties
        public abstract bool CanBeColumn { get; }

        public abstract bool CanBeParameter { get; }

        public abstract bool CanSuppressSizeForConversionToString { get; }

        public abstract bool HasPrecisionAndScale { get; }

        public abstract bool HasSizeOrIsLarge { get; }

        public abstract bool IsApplicationType { get; }

        public abstract bool IsChar { get; }

        public abstract bool IsFixedSize { get; }

        public abstract bool IsGroupable { get; }

        public abstract bool IsLargeType { get; }

        public abstract bool IsNumeric { get; }

        public abstract bool IsOrderable { get; }

        public abstract bool IsRuntimeOnlyType { get; }

        public abstract bool IsString { get; }
        public abstract bool IsBinary { get; }
        public abstract bool IsDateTime { get; }

        public abstract bool IsUnicodeType { get; }

        public virtual int? Size { get; set; }

        public abstract bool SupportsComparison { get; }

        public abstract bool SupportsLength { get; }

        public virtual int Precision { get; set; }

        public virtual int Scale { get; set; }

        Enum IProviderType.SqlDbType
        {
            get
            {
                return SqlDbType as Enum;
            }
        }

        public abstract int? ApplicationTypeIndex { get; set; }
        public abstract Type RuntimeOnlyType { get; set; }


        public abstract DBType SqlDbType { get; set; }


    }
}