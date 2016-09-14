using System.Data.Common;
using System.Diagnostics;
using System.Data;
using ALinq.SqlClient;
using MySql.Data.MySqlClient;

namespace ALinq.MySQL
{
    [System.Obsolete]
    class MySqlTypeSystemProvider : SqlTypeSystem.Sql2000Provider
    {
        public override void InitializeParameter(IProviderType type, DbParameter parameter, object value)
        {
            var parameter2 = parameter as MySqlParameter;//System.Data.SqlClient.SqlParameter;
            if (parameter2 == null)
            {
                base.InitializeParameter(type, parameter, value);
                return;
            }
            var type2 = (SqlTypeSystem.SqlType)type;
            if (type2.IsRuntimeOnlyType)
            {
                throw SqlClient.Error.BadParameterType(type2.GetClosestRuntimeType());
            }
            Debug.Assert(parameter2 != null);

            switch ((SqlDbType)type2.SqlDbType)
            {
                case SqlDbType.Binary:
                    parameter.DbType = DbType.Binary;
                    break;
                case SqlDbType.Bit:
                    parameter.DbType = DbType.Boolean;
                    break;
                case SqlDbType.Char:
                    parameter.DbType = DbType.AnsiStringFixedLength;
                    break;
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                    parameter.DbType = DbType.DateTime;
                    break;
                case SqlDbType.DateTime2:
                    parameter.DbType = DbType.DateTime2;
                    break;
                case SqlDbType.DateTimeOffset:
                    parameter.DbType = DbType.DateTimeOffset;
                    break;
                case SqlDbType.Decimal:
                    parameter.DbType = DbType.Decimal;
                    break;
                case SqlDbType.Float:
                    parameter.DbType = DbType.Single;
                    break;
                case SqlDbType.Image:
                    parameter.DbType = DbType.Binary;
                    break;
                case SqlDbType.Int:
                    parameter2.DbType = DbType.Int32;
                    break;
                case SqlDbType.Money:
                    parameter.DbType = DbType.Currency;
                    break;
                case SqlDbType.NChar:
                case SqlDbType.NText:
                    parameter.DbType = DbType.StringFixedLength;
                    break;
                case SqlDbType.NVarChar:
                    parameter.DbType = DbType.String;
                    break;
                case SqlDbType.Real:
                    parameter.DbType = DbType.Double;
                    break;
                case SqlDbType.SmallDateTime:
                    parameter.DbType = DbType.DateTime;
                    break;
                case SqlDbType.SmallInt:
                    parameter.DbType = DbType.Int16;
                    break;
                case SqlDbType.SmallMoney:
                    parameter.DbType = DbType.Currency;
                    break;
                case SqlDbType.Structured:
                    break;
                case SqlDbType.Text:
                    parameter.DbType = DbType.AnsiString;
                    break;
                case SqlDbType.Time:
                    parameter.DbType = DbType.DateTime;
                    break;
                case SqlDbType.Timestamp:
                    break;
                case SqlDbType.TinyInt:
                    parameter.DbType = DbType.Byte;
                    break;
                case SqlDbType.Udt:
                    break;
                case SqlDbType.UniqueIdentifier:
                    parameter.DbType = DbType.Guid;
                    break;
                case SqlDbType.VarBinary:
                    parameter.DbType = DbType.Binary;
                    break;
                case SqlDbType.VarChar:
                    parameter.DbType = DbType.AnsiString;
                    break;
                case SqlDbType.Variant:
                    break;
                case SqlDbType.Xml:
                    parameter.DbType = DbType.Xml;
                    break;
            }
            if (type2.HasPrecisionAndScale)
            {
                parameter2.Precision = (byte)type2.Precision;
                parameter2.Scale = (byte)type2.Scale;
            }

            parameter.Value = GetParameterValue(type2, value);
            if (!((parameter.Direction == ParameterDirection.Input) && type2.IsFixedSize) &&
                (parameter.Direction == ParameterDirection.Input))
                return;
            
            if (type2.Size.HasValue)
            {
                if (parameter.Size < type2.Size)
                {
                    parameter.Size = type2.Size.Value;
                }
            }
            if (!type2.IsLargeType)
            {
                return;
            }
        }
    }
    #region MyRegion
    //class MySqlTypeSystemProvider : TypeSystemProvider
    //{
    //    private static readonly MySqlType theBigInt = new MySqlType(MySqlDbType.Int64);
    //    private static readonly MySqlType theBit = new MySqlType(MySqlDbType.Bit);
    //    private static readonly MySqlType theChar = new MySqlType(MySqlDbType.String);
    //    private static readonly MySqlType theDateTime = new MySqlType(MySqlDbType.DateTime);
    //    private static readonly MySqlType theDefaultDecimal = new MySqlType(MySqlDbType.Decimal, 0x1d, 4);
    //    private static readonly MySqlType theFloat = new MySqlType(MySqlDbType.Float);
    //    private static readonly MySqlType theImage = new MySqlType(MySqlDbType.VarBinary, -1);
    //    private static readonly MySqlType theInt = new MySqlType(MySqlDbType.Int32);
    //    //private static readonly MySqlType theMoney = new MySqlType(SqlDbType.Money, 0x13, 4);
    //    //internal static readonly MySqlType theNText = new MySqlType(SqlDbType.NText, -1);
    //    private static readonly MySqlType theReal = new MySqlType(MySqlDbType.Double);
    //    //private static readonly MySqlType theSmallDateTime = new MySqlType(SqlDbType.SmallDateTime);
    //    private static readonly MySqlType theSmallInt = new MySqlType(MySqlDbType.Int16);
    //    //private static readonly MySqlType theSmallMoney = new MySqlType(SqlDbType.SmallMoney, 10, 4);
    //    private static readonly MySqlType theText = new MySqlType(MySqlDbType.Text, -1);
    //    //private static readonly MySqlType theTimestamp = new MySqlType(SqlDbType.Timestamp);
    //    private static readonly MySqlType theTinyInt = new MySqlType(MySqlDbType.UByte);
    //    private static readonly MySqlType theUniqueIdentifier = new MySqlType(MySqlDbType.TinyBlob);
    //    internal static readonly MySqlType theXml = new MySqlType(MySqlDbType.Text, -1);

    //    protected Dictionary<int, SqlTypeSystem.SqlType> applicationTypes = new Dictionary<int, SqlTypeSystem.SqlType>();

    //    // Methods
    //    internal override ProviderType ChangeTypeFamilyTo(ProviderType type, ProviderType typeWithFamily)
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    internal override ProviderType From(object o)
    //    {
    //        Type type = (o != null) ? o.GetType() : typeof(object);
    //        if (type == typeof(string))
    //        {
    //            string str = (string)o;
    //            return this.From(type, new int?(str.Length));
    //        }
    //        if (type == typeof(bool))
    //        {
    //            return this.From(typeof(int));
    //        }
    //        if (type.IsArray)
    //        {
    //            Array array = (Array)o;
    //            return this.From(type, new int?(array.Length));
    //        }
    //        if (type == typeof(decimal))
    //        {
    //            decimal d = (decimal)o;
    //            int num2 = (decimal.GetBits(d)[3] & 0xff0000) >> 0x10;
    //            return this.From(type, new int?(num2));
    //        }
    //        return this.From(type);
    //    }

    //    internal override ProviderType From(Type runtimeType)
    //    {
    //        return this.From(runtimeType, null);
    //    }

    //    internal override ProviderType From(Type type, int? size)
    //    {
    //        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
    //        {
    //            type = type.GetGenericArguments()[0];
    //        }
    //        TypeCode typeCode = Type.GetTypeCode(type);
    //        switch (typeCode)
    //        {
    //            case TypeCode.Object:
    //                if (type != typeof(Guid))
    //                {
    //                    if ((type == typeof(byte[])) || (type == typeof(Binary)))
    //                    {
    //                        return GetBestType(MySqlDbType.VarBinary, size);
    //                    }
    //                    if (type == typeof(char[]))
    //                    {
    //                        return GetBestType(MySqlDbType.VarChar, size);
    //                    }
    //                    if (type == typeof(TimeSpan))
    //                    {
    //                        return SqlTypeSystem.Create(SqlDbType.BigInt);
    //                    }
    //                    if ((type != typeof(XDocument)) && (type != typeof(XElement)))
    //                    {
    //                        return new SqlTypeSystem.SqlType(type);
    //                    }
    //                    return SqlTypeSystem.theNText;
    //                }
    //                return Create(MySqlDbType.TinyBlob);

    //            case TypeCode.Boolean:
    //                return Create(MySqlDbType.Bit);

    //            case TypeCode.Char:
    //                return Create(MySqlDbType.String, 1);

    //            case TypeCode.SByte:
    //                return Create(MySqlDbType.Byte);
    //            case TypeCode.Int16:
    //                return Create(MySqlDbType.Int16);

    //            case TypeCode.Byte:
    //                return Create(MySqlDbType.UByte);

    //            case TypeCode.UInt16:
    //                return Create(MySqlDbType.UInt16);
    //            case TypeCode.Int32:
    //                return Create(MySqlDbType.Int32);

    //            case TypeCode.UInt32:
    //                return Create(MySqlDbType.UInt32);

    //            case TypeCode.Int64:
    //                return Create(MySqlDbType.Int64);

    //            case TypeCode.UInt64:
    //                return Create(MySqlDbType.UInt64);

    //            case TypeCode.Single:
    //                return Create(MySqlDbType.Float);

    //            case TypeCode.Double:
    //                return Create(MySqlDbType.Double);

    //            case TypeCode.Decimal:
    //                {
    //                    int? nullable = size;
    //                    return Create(MySqlDbType.Decimal, 0x1d, nullable.HasValue ? nullable.GetValueOrDefault() : 4);
    //                }
    //            case TypeCode.DateTime:
    //                return Create(MySqlDbType.DateTime);

    //            case TypeCode.String:
    //                return GetBestType(MySqlDbType.VarChar, size);
    //        }
    //        throw SqlClient.Error.UnexpectedTypeCode(typeCode);
    //    }

    //    internal override ProviderType GetApplicationType(int index)
    //    {
    //        if (index < 0)
    //        {
    //            throw Error.ArgumentOutOfRange("index");
    //        }
    //        SqlTypeSystem.SqlType type = null;
    //        if (!this.applicationTypes.TryGetValue(index, out type))
    //        {
    //            type = new SqlTypeSystem.SqlType(index);
    //            this.applicationTypes.Add(index, type);
    //        }
    //        return type;
    //    }

    //    internal override ProviderType GetBestLargeType(ProviderType type)
    //    {
    //        var type2 = (MySqlType)type;
    //        switch (type2.SqlDbType)
    //        {
    //            case MySqlDbType.Binary:
    //            case MySqlDbType.VarBinary:
    //                return Create(MySqlDbType.Binary);

    //            case MySqlDbType.Bit:
    //                return type;

    //            case MySqlDbType.String:
    //            case MySqlDbType.VarChar:
    //                return SqlTypeSystem.Create(SqlDbType.Text);

    //                //case SqlDbType.NChar:
    //                //case SqlDbType.NVarChar:
    //                //    return SqlTypeSystem.Create(SqlDbType.NText);

    //                //case SqlDbType.NText:
    //                return type;
    //        }
    //        return type;
    //    }

    //    protected ProviderType GetBestType(MySqlDbType targetType, int? size)
    //    {
    //        int num = 0;
    //        switch (targetType)
    //        {
    //            //case SqlDbType.Binary:
    //            case MySqlDbType.Binary:
    //            //case SqlDbType.Char:
    //            //case SqlDbType.VarBinary:
    //            case MySqlDbType.VarBinary:
    //            //case SqlDbType.VarChar:
    //            case MySqlDbType.VarChar:
    //                num = 0x1f40;
    //                break;
    //                //case SqlDbType.NChar:
    //                //case SqlDbType.NVarChar:
    //                num = 0xfa0;
    //                break;
    //        }
    //        if (!size.HasValue)
    //        {
    //            return Create(targetType, SupportsMaxSize ? -1 : num);
    //        }
    //        if (size.Value <= num)
    //        {
    //            return Create(targetType, size.Value);
    //        }
    //        return this.GetBestLargeType(Create(targetType));
    //    }

    //    internal static ProviderType Create(MySqlDbType type)
    //    {
    //        switch (type)
    //        {
    //            case MySqlDbType.Int64:
    //                return theBigInt;

    //            case MySqlDbType.Bit:
    //                return theBit;

    //            case MySqlDbType.String:
    //                return theChar;

    //            case MySqlDbType.DateTime:
    //                return theDateTime;

    //            case MySqlDbType.Decimal:
    //                return theDefaultDecimal;

    //            case MySqlDbType.Float:
    //                return theFloat;

    //            case MySqlDbType.VarBinary:
    //                return theImage;

    //            case MySqlDbType.Int32:
    //                return theInt;

    //            //case SqlDbType.Money:
    //            //    return theMoney;

    //            //case SqlDbType.NText:
    //            //    return theNText;

    //            case MySqlDbType.Double:
    //                return theReal;

    //            //case SqlDbType.UniqueIdentifier:
    //            //    return theUniqueIdentifier;

    //            //case SqlDbType.SmallDateTime:
    //            //    return theSmallDateTime;

    //            //case SqlDbType.SmallInt:
    //            //    return theSmallInt;

    //            //case SqlDbType.SmallMoney:
    //            //    return theSmallMoney;

    //            case MySqlDbType.Text:
    //                return theText;

    //            //case SqlDbType.Timestamp:
    //            //    return theTimestamp;

    //            case MySqlDbType.UByte:
    //                return theTinyInt;

    //            //case SqlDbType.Xml:
    //            //    return theXml;
    //        }
    //        return new MySqlType(type);
    //    }

    //    internal static ProviderType Create(MySqlDbType type, int size)
    //    {
    //        return new MySqlType(type, new int?(size));
    //    }

    //    internal static ProviderType Create(MySqlDbType type, int precision, int scale)
    //    {
    //        if (((type != MySqlDbType.Decimal) && (precision == 0)) && (scale == 0))
    //        {
    //            return Create(type);
    //        }
    //        if (((type == MySqlDbType.Decimal) && (precision == 0x1d)) && (scale == 4))
    //        {
    //            return Create(type);
    //        }
    //        return new MySqlType(type, precision, scale);
    //    }

    //    private static bool SupportsMaxSize
    //    {
    //        get { return false; }
    //    }

    //    internal override ProviderType GetBestType(ProviderType typeA, ProviderType typeB)
    //    {
    //        var type = (typeA.ComparePrecedenceTo(typeB) > 0) ? ((MySqlType)typeA) : ((MySqlType)typeB);
    //        if (typeA.IsApplicationType || typeA.IsRuntimeOnlyType)
    //        {
    //            return typeA;
    //        }
    //        if (typeB.IsApplicationType || typeB.IsRuntimeOnlyType)
    //        {
    //            return typeB;
    //        }
    //        var type2 = (MySqlType)typeA;
    //        var type3 = (MySqlType)typeB;
    //        if ((type2.HasPrecisionAndScale && type3.HasPrecisionAndScale) && (type.SqlDbType == MySqlDbType.Decimal))
    //        {
    //            int precision = type2.Precision;
    //            int scale = type2.Scale;
    //            int num3 = type3.Precision;
    //            int num4 = type3.Scale;
    //            if (((precision == 0) && (scale == 0)) && ((num3 == 0) && (num4 == 0)))
    //            {
    //                return Create(type.SqlDbType);
    //            }
    //            if ((precision == 0) && (scale == 0))
    //            {
    //                return Create(type.SqlDbType, num3, num4);
    //            }
    //            if ((num3 == 0) && (num4 == 0))
    //            {
    //                return Create(type.SqlDbType, precision, scale);
    //            }
    //            int num5 = Math.Max((int)(precision - scale), (int)(num3 - num4));
    //            int num6 = Math.Max(scale, num4);
    //            Math.Min(num5 + num6, 0x25);
    //            return Create(type.SqlDbType, num5 + num6, num6);
    //        }
    //        int? size = null;
    //        if (type2.Size.HasValue && type3.Size.HasValue)
    //        {
    //            int? nullable4 = type3.Size;
    //            int? nullable5 = type2.Size;
    //            size = ((nullable4.GetValueOrDefault() > nullable5.GetValueOrDefault()) && (nullable4.HasValue & nullable5.HasValue)) ? type3.Size : type2.Size;
    //        }
    //        if ((type3.Size.HasValue && (type3.Size.Value == -1)) || (type2.Size.HasValue && (type2.Size.Value == -1)))
    //        {
    //            size = -1;
    //        }
    //        return new MySqlType(type.SqlDbType, size);
    //    }

    //    internal override void InitializeParameter(ProviderType type, DbParameter parameter, object value)
    //    {
    //        var type2 = (MySqlType)type;
    //        if (type2.IsRuntimeOnlyType)
    //        {
    //            throw SqlClient.Error.BadParameterType(type2.GetClosestRuntimeType());
    //        }
    //        var parameter2 = parameter as MySqlParameter;
    //        if (parameter2 != null)
    //        {
    //            parameter2.MySqlDbType = type2.SqlDbType;
    //            if (type2.HasPrecisionAndScale)
    //            {
    //                parameter2.Precision = (byte)type2.Precision;
    //                parameter2.Scale = (byte)type2.Scale;
    //            }
    //        }
    //        else
    //        {
    //            PropertyInfo property = parameter.GetType().GetProperty("SqlDbType");
    //            if (property != null)
    //            {
    //                property.SetValue(parameter, type2.SqlDbType, null);
    //            }
    //            if (type2.HasPrecisionAndScale)
    //            {
    //                PropertyInfo info2 = parameter.GetType().GetProperty("Precision");
    //                if (info2 != null)
    //                {
    //                    info2.SetValue(parameter, Convert.ChangeType(type2.Precision, info2.PropertyType, CultureInfo.InvariantCulture), null);
    //                }
    //                PropertyInfo info3 = parameter.GetType().GetProperty("Scale");
    //                if (info3 != null)
    //                {
    //                    info3.SetValue(parameter, Convert.ChangeType(type2.Scale, info3.PropertyType, CultureInfo.InvariantCulture), null);
    //                }
    //            }
    //        }
    //        parameter.Value = GetParameterValue(type2, value);
    //        if (!((parameter.Direction == ParameterDirection.Input) && type2.IsFixedSize) && (parameter.Direction == ParameterDirection.Input))
    //        {
    //            return;
    //        }
    //        if (type2.Size.HasValue)
    //        {
    //            if (parameter.Size < type2.Size)
    //            {
    //                goto Label_016B;
    //            }
    //        }
    //        if (!type2.IsLargeType)
    //        {
    //            return;
    //        }
    //    Label_016B:
    //        parameter.Size = type2.Size.Value;
    //    }

    //    protected static object GetParameterValue(MySqlType type, object value)
    //    {
    //        if (value == null)
    //        {
    //            return DBNull.Value;
    //        }
    //        Type type2 = value.GetType();
    //        Type closestRuntimeType = type.GetClosestRuntimeType();
    //        if (closestRuntimeType == type2)
    //        {
    //            return value;
    //        }
    //        return DBConvert.ChangeType(value, closestRuntimeType);
    //    }

    //    internal override ProviderType MostPreciseTypeInFamily(ProviderType type)
    //    {
    //        var type2 = (MySqlType)type;
    //        switch (type2.SqlDbType)
    //        {
    //            case MySqlDbType.DateTime:
    //            case MySqlDbType.Date:
    //                return this.From(typeof(DateTime));

    //            case MySqlDbType.Decimal:
    //            case MySqlDbType.Binary:
    //            //case SqlDbType.NChar:
    //            //case SqlDbType.NText:
    //            //case SqlDbType.NVarChar:
    //            //case SqlDbType.UniqueIdentifier:
    //            case MySqlDbType.Text:
    //                //case SqlDbType.Timestamp:
    //                return type;

    //            case MySqlDbType.Float:
    //            case MySqlDbType.Double:
    //                return this.From(typeof(double));

    //            case MySqlDbType.Int32:
    //            case MySqlDbType.Int16:
    //            case MySqlDbType.Byte:
    //                return this.From(typeof(int));

    //            //case SqlDbType.Money:
    //            //case SqlDbType.SmallMoney:
    //            //    return SqlTypeSystem.Create(SqlDbType.Money);
    //        }
    //        return type;
    //    }

    //    internal override ProviderType Parse(string stype)
    //    {
    //        string strA = null;
    //        string s = null;
    //        string str3 = null;
    //        int index = stype.IndexOf('(');
    //        int num2 = stype.IndexOf(' ');
    //        int length = ((index != -1) && (num2 != -1)) ? Math.Min(num2, index) : ((index != -1) ? index : ((num2 != -1) ? num2 : -1));
    //        if (length == -1)
    //        {
    //            strA = stype;
    //            length = stype.Length;
    //        }
    //        else
    //        {
    //            strA = stype.Substring(0, length);
    //        }
    //        int startIndex = length;
    //        if ((startIndex < stype.Length) && (stype[startIndex] == '('))
    //        {
    //            startIndex++;
    //            length = stype.IndexOf(',', startIndex);
    //            if (length > 0)
    //            {
    //                s = stype.Substring(startIndex, length - startIndex);
    //                startIndex = length + 1;
    //                length = stype.IndexOf(')', startIndex);
    //                str3 = stype.Substring(startIndex, length - startIndex);
    //            }
    //            else
    //            {
    //                length = stype.IndexOf(')', startIndex);
    //                s = stype.Substring(startIndex, length - startIndex);
    //            }
    //            startIndex = length++;
    //        }
    //        if (string.Compare(strA, "rowversion", StringComparison.OrdinalIgnoreCase) == 0)
    //        {
    //            strA = "Timestamp";
    //        }
    //        if (string.Compare(strA, "numeric", StringComparison.OrdinalIgnoreCase) == 0)
    //        {
    //            strA = "Decimal";
    //        }
    //        if (string.Compare(strA, "sql_variant", StringComparison.OrdinalIgnoreCase) == 0)
    //        {
    //            strA = "Variant";
    //        }
    //        if (!Enum.GetNames(typeof(SqlDbType)).Select<string, string>(delegate(string n)
    //        {
    //            return n.ToUpperInvariant();
    //        }).Contains<string>(strA.ToUpperInvariant()))
    //        {
    //            throw SqlClient.Error.InvalidProviderType(strA);
    //        }
    //        int size = 0;
    //        int scale = 0;
    //        var type = (MySqlDbType)Enum.Parse(typeof(MySqlDbType), strA, true);
    //        if (s != null)
    //        {
    //            if (string.Compare(s.Trim(), "max", StringComparison.OrdinalIgnoreCase) == 0)
    //            {
    //                size = -1;
    //            }
    //            else
    //            {
    //                size = int.Parse(s, CultureInfo.InvariantCulture);
    //                if (size == 0x7fffffff)
    //                {
    //                    size = -1;
    //                }
    //            }
    //        }
    //        if (str3 != null)
    //        {
    //            if (string.Compare(str3.Trim(), "max", StringComparison.OrdinalIgnoreCase) == 0)
    //            {
    //                scale = -1;
    //            }
    //            else
    //            {
    //                scale = int.Parse(str3, CultureInfo.InvariantCulture);
    //                if (scale == 0x7fffffff)
    //                {
    //                    scale = -1;
    //                }
    //            }
    //        }
    //        switch (type)
    //        {
    //            case MySqlDbType.Binary:
    //            case MySqlDbType.Text:
    //            //case SqlDbType.NChar:
    //            //case SqlDbType.NVarChar:
    //            case MySqlDbType.VarBinary:
    //            case MySqlDbType.VarChar:
    //                return Create(type, size);

    //            case MySqlDbType.Decimal:
    //            case MySqlDbType.Float:
    //            case MySqlDbType.Double:
    //                return Create(type, size, scale);
    //        }
    //        return Create(type);
    //    }

    //    internal override ProviderType PredictTypeForBinary(SqlNodeType binaryOp, ProviderType leftType, ProviderType rightType)
    //    {
    //        MySqlType bestType;
    //        if (leftType.IsSameTypeFamily(this.From(typeof(string))) && rightType.IsSameTypeFamily(this.From(typeof(string))))
    //        {
    //            bestType = (MySqlType)this.GetBestType(leftType, rightType);
    //        }
    //        else
    //        {
    //            bestType = (leftType.ComparePrecedenceTo(rightType) > 0) ? ((MySqlType)leftType) : ((MySqlType)rightType);
    //        }
    //        switch (binaryOp)
    //        {
    //            case SqlNodeType.BitAnd:
    //            case SqlNodeType.BitOr:
    //            case SqlNodeType.BitXor:
    //                return bestType;

    //            case SqlNodeType.And:
    //            case SqlNodeType.EQ:
    //            case SqlNodeType.EQ2V:
    //            case SqlNodeType.LE:
    //            case SqlNodeType.LT:
    //            case SqlNodeType.GE:
    //            case SqlNodeType.GT:
    //            case SqlNodeType.NE:
    //            case SqlNodeType.NE2V:
    //            case SqlNodeType.Or:
    //                return theInt;

    //            case SqlNodeType.Add:
    //                return bestType;

    //            case SqlNodeType.Coalesce:
    //                return bestType;

    //            case SqlNodeType.Concat:
    //                {
    //                    if (!bestType.HasSizeOrIsLarge)
    //                    {
    //                        return bestType;
    //                    }
    //                    ProviderType type2 = this.GetBestType(bestType.SqlDbType, null);
    //                    if ((leftType.IsLargeType || !leftType.Size.HasValue) || (rightType.IsLargeType || !rightType.Size.HasValue))
    //                    {
    //                        return type2;
    //                    }
    //                    int num2 = leftType.Size.Value + rightType.Size.Value;
    //                    int num3 = num2;
    //                    if ((num3 >= type2.Size) && !type2.IsLargeType)
    //                    {
    //                        return type2;
    //                    }
    //                    return this.GetBestType(bestType.SqlDbType, new int?(num2));
    //                }
    //            case SqlNodeType.Div:
    //                return bestType;

    //            case SqlNodeType.Mod:
    //            case SqlNodeType.Mul:
    //                return bestType;

    //            case SqlNodeType.Sub:
    //                return bestType;
    //        }
    //        throw SqlClient.Error.UnexpectedNode(binaryOp);
    //    }

    //    internal override ProviderType PredictTypeForUnary(SqlNodeType unaryOp, ProviderType operandType)
    //    {
    //        switch (unaryOp)
    //        {
    //            case SqlNodeType.Avg:
    //            case SqlNodeType.Covar:
    //            case SqlNodeType.Stddev:
    //            case SqlNodeType.Sum:
    //                return this.MostPreciseTypeInFamily(operandType);

    //            case SqlNodeType.BitNot:
    //                return operandType;

    //            case SqlNodeType.ClrLength:
    //                if (operandType.IsLargeType)
    //                {
    //                    return this.From(typeof(long));
    //                }
    //                return this.From(typeof(int));

    //            case SqlNodeType.LongCount:
    //                return this.From(typeof(long));

    //            case SqlNodeType.Max:
    //                return operandType;

    //            case SqlNodeType.Count:
    //                return this.From(typeof(int));

    //            case SqlNodeType.IsNotNull:
    //            case SqlNodeType.IsNull:
    //            case SqlNodeType.Not:
    //            case SqlNodeType.Not2V:
    //                return theBit;

    //            case SqlNodeType.Negate:
    //                return operandType;

    //            case SqlNodeType.OuterJoinedValue:
    //                return operandType;

    //            case SqlNodeType.Min:
    //                return operandType;

    //            case SqlNodeType.Treat:
    //            case SqlNodeType.ValueOf:
    //                return operandType;
    //        }
    //        throw SqlClient.Error.UnexpectedNode(unaryOp);
    //    }

    //    internal override ProviderType ReturnTypeOfFunction(SqlFunctionCall functionCall)
    //    {
    //        ProviderType[] argumentTypes = GetArgumentTypes(functionCall);
    //        var type = (MySqlType)argumentTypes[0];
    //        var type2 = (argumentTypes.Length > 1) ? ((MySqlType)argumentTypes[1]) : null;
    //        switch (functionCall.Name)
    //        {
    //            case "LEN":
    //            case "DATALENGTH":
    //                switch (type.SqlDbType)
    //                {
    //                    case MySqlDbType.VarBinary:
    //                    case MySqlDbType.VarChar:
    //                        //case SqlDbType.NVarChar:
    //                        if (type.IsLargeType)
    //                        {
    //                            return Create(MySqlDbType.Int64);
    //                        }
    //                        return Create(MySqlDbType.Int32);
    //                }
    //                return Create(MySqlDbType.Int32);

    //            case "ABS":
    //            case "SIGN":
    //            case "ROUND":
    //            case "CEILING":
    //            case "FLOOR":
    //            case "POWER":
    //                {
    //                    var sqlDbType = type.SqlDbType;

    //                    switch (sqlDbType)
    //                    {
    //                        case MySqlDbType.Int16:
    //                        case MySqlDbType.Byte:
    //                            return type;
    //                        case MySqlDbType.Float:
    //                        case MySqlDbType.Double:
    //                            return Create(MySqlDbType.Float);

    //                        case MySqlDbType.Binary:
    //                            return type;
    //                    }
    //                    return type;
    //                    //var sqlDbType = type.SqlDbType;
    //                    //if (sqlDbType > SqlDbType.Real)
    //                    //{
    //                    //    if ((sqlDbType != SqlDbType.SmallInt) && (sqlDbType != SqlDbType.TinyInt))
    //                    //    {
    //                    //        return type;
    //                    //    }
    //                    //    break;
    //                    //}
    //                    //switch (sqlDbType)
    //                    //{
    //                    //    case SqlDbType.Float:
    //                    //    case SqlDbType.Real:
    //                    //        return SqlTypeSystem.Create(SqlDbType.Float);

    //                    //    case SqlDbType.Image:
    //                    //        return type;
    //                    //}
    //                    //return type;
    //                }
    //            case "PATINDEX":
    //            case "CHARINDEX":
    //                if (!type2.IsLargeType)
    //                {
    //                    return Create(MySqlDbType.Int32);
    //                }
    //                return Create(MySqlDbType.Int64);

    //            case "SUBSTRING":
    //                {
    //                    if (functionCall.Arguments[2].NodeType != SqlNodeType.Value)
    //                    {
    //                        goto Label_02DA;
    //                    }
    //                    SqlValue value2 = (SqlValue)functionCall.Arguments[2];
    //                    if (!(value2.Value is int))
    //                    {
    //                        goto Label_02DA;
    //                    }
    //                    switch (type.SqlDbType)
    //                    {
    //                        //case SqlDbType.NChar:
    //                        //case SqlDbType.NVarChar:
    //                        case MySqlDbType.VarChar:
    //                        case MySqlDbType.String:
    //                            return Create(type.SqlDbType, (int)value2.Value);

    //                        case MySqlDbType.Text:
    //                            goto Label_02D8;
    //                    }
    //                    goto Label_02D8;
    //                }
    //            case "STUFF":
    //                {
    //                    if (functionCall.Arguments.Count != 4)
    //                    {
    //                        goto Label_0375;
    //                    }
    //                    SqlValue value3 = functionCall.Arguments[2] as SqlValue;
    //                    if ((value3 == null) || (((int)value3.Value) != 0))
    //                    {
    //                        goto Label_0375;
    //                    }
    //                    return this.PredictTypeForBinary(SqlNodeType.Concat, functionCall.Arguments[0].SqlType, functionCall.Arguments[3].SqlType);
    //                }
    //            case "LOWER":
    //            case "UPPER":
    //            case "RTRIM":
    //            case "LTRIM":
    //            case "INSERT":
    //            case "REPLACE":
    //            case "LEFT":
    //            case "RIGHT":
    //            case "REVERSE":
    //                return type;

    //            default:
    //                return null;
    //        }
    //        return SqlTypeSystem.Create(SqlDbType.Int);
    //    Label_02D8:
    //        return null;
    //    Label_02DA:
    //        switch (type.SqlDbType)
    //        {
    //            //case SqlDbType.NChar:
    //            //case SqlDbType.NVarChar:
    //            //    return SqlTypeSystem.Create(SqlDbType.NVarChar);

    //            case MySqlDbType.VarChar:
    //            case MySqlDbType.String:
    //                return Create(MySqlDbType.VarChar);

    //            default:
    //                return null;
    //        }
    //    Label_0375:
    //        return null;
    //    }

    //    private ProviderType[] GetArgumentTypes(SqlFunctionCall fc)
    //    {
    //        ProviderType[] typeArray = new ProviderType[fc.Arguments.Count];
    //        int index = 0;
    //        int length = typeArray.Length;
    //        while (index < length)
    //        {
    //            typeArray[index] = fc.Arguments[index].SqlType;
    //            index++;
    //        }
    //        return typeArray;
    //    }

    //    // Properties
    //    //protected override bool SupportsMaxSize
    //    //{
    //    //    get
    //    //    {
    //    //        return false;
    //    //    }
    //    //}
    //} 
    #endregion

    
}

