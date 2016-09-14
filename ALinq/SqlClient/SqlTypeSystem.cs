using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using ALinq;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace ALinq.SqlClient
{
    internal static partial class SqlTypeSystem
    {
        // Fields
        private const int defaultDecimalPrecision = 0x1d;
        private const int defaultDecimalScale = 4;
        internal const short LargeTypeSizeIndicator = -1;
        private static readonly SqlType theBigInt = new SqlType(System.Data.SqlDbType.BigInt);
        private static readonly SqlType theBit = new SqlType(System.Data.SqlDbType.Bit);
        private static readonly SqlType theChar = new SqlType(System.Data.SqlDbType.Char);
        private static readonly SqlType theDateTime = new SqlType(System.Data.SqlDbType.DateTime);
        private static readonly SqlType theDefaultDecimal = new SqlType(System.Data.SqlDbType.Decimal, 0x1d, 4);
        private static readonly SqlType theFloat = new SqlType(System.Data.SqlDbType.Float);
        private static readonly SqlType theImage = new SqlType(System.Data.SqlDbType.Image, -1);
        internal static readonly SqlType theInt = new SqlType(System.Data.SqlDbType.Int);
        private static readonly SqlType theMoney = new SqlType(System.Data.SqlDbType.Money, 0x13, 4);
        internal static readonly SqlType theNText = new SqlType(System.Data.SqlDbType.NText, -1);
        private static readonly SqlType theReal = new SqlType(System.Data.SqlDbType.Real);
        private static readonly SqlType theSmallDateTime = new SqlType(System.Data.SqlDbType.SmallDateTime);
        private static readonly SqlType theSmallInt = new SqlType(System.Data.SqlDbType.SmallInt);
        private static readonly SqlType theSmallMoney = new SqlType(System.Data.SqlDbType.SmallMoney, 10, 4);
        private static readonly SqlType theText = new SqlType(System.Data.SqlDbType.Text, -1);
        private static readonly SqlType theTimestamp = new SqlType(System.Data.SqlDbType.Timestamp);
        private static readonly SqlType theTinyInt = new SqlType(System.Data.SqlDbType.TinyInt);
        private static readonly SqlType theUniqueIdentifier = new SqlType(System.Data.SqlDbType.UniqueIdentifier);
        internal static readonly SqlType theXml = new SqlType(System.Data.SqlDbType.Xml, -1);

        // Methods
        internal static IProviderType Create(SqlDbType type)
        {
            switch (type)
            {
                case System.Data.SqlDbType.BigInt:
                    return theBigInt;

                case System.Data.SqlDbType.Bit:
                    return theBit;

                case System.Data.SqlDbType.Char:
                    return theChar;

                case System.Data.SqlDbType.DateTime:
                    return theDateTime;

                case System.Data.SqlDbType.Decimal:
                    return theDefaultDecimal;

                case System.Data.SqlDbType.Float:
                    return theFloat;

                case System.Data.SqlDbType.Image:
                    return theImage;

                case System.Data.SqlDbType.Int:
                    return theInt;

                case System.Data.SqlDbType.Money:
                    return theMoney;

                case System.Data.SqlDbType.NText:
                    return theNText;

                case System.Data.SqlDbType.Real:
                    return theReal;

                case System.Data.SqlDbType.UniqueIdentifier:
                    return theUniqueIdentifier;

                case System.Data.SqlDbType.SmallDateTime:
                    return theSmallDateTime;

                case System.Data.SqlDbType.SmallInt:
                    return theSmallInt;

                case System.Data.SqlDbType.SmallMoney:
                    return theSmallMoney;

                case System.Data.SqlDbType.Text:
                    return theText;

                case System.Data.SqlDbType.Timestamp:
                    return theTimestamp;

                case System.Data.SqlDbType.TinyInt:
                    return theTinyInt;

                case System.Data.SqlDbType.Xml:
                    return theXml;
            }
            return new SqlType(type);
        }

        internal static IProviderType Create(SqlDbType type, int size)
        {
            return new SqlType(type, size);
        }

        internal static IProviderType Create(SqlDbType type, int precision, int scale)
        {
            if (((type != System.Data.SqlDbType.Decimal) && (precision == 0)) && (scale == 0))
            {
                return Create(type);
            }
            if (((type == System.Data.SqlDbType.Decimal) && (precision == 0x1d)) && (scale == 4))
            {
                return Create(type);
            }
            return new SqlType(type, precision, scale);
        }

        internal static Type GetClosestRuntimeType(SqlDbType sqlDbType)
        {
            switch (sqlDbType)
            {
                case System.Data.SqlDbType.BigInt:
                    return typeof(long);

                case System.Data.SqlDbType.Binary:
                case System.Data.SqlDbType.Image:
                case System.Data.SqlDbType.Timestamp:
                case System.Data.SqlDbType.VarBinary:
                    return typeof(byte[]);

                case System.Data.SqlDbType.Bit:
                    return typeof(bool);

                case System.Data.SqlDbType.Char:
                case System.Data.SqlDbType.NChar:
                case System.Data.SqlDbType.NText:
                case System.Data.SqlDbType.NVarChar:
                case System.Data.SqlDbType.Text:
                case System.Data.SqlDbType.VarChar:
                case System.Data.SqlDbType.Xml:
                    return typeof(string);

                case System.Data.SqlDbType.DateTime:
                case System.Data.SqlDbType.SmallDateTime:
                    return typeof(DateTime);

                case System.Data.SqlDbType.Decimal:
                case System.Data.SqlDbType.Money:
                case System.Data.SqlDbType.SmallMoney:
                    return typeof(decimal);

                case System.Data.SqlDbType.Float:
                    return typeof(double);

                case System.Data.SqlDbType.Int:
                    return typeof(int);

                case System.Data.SqlDbType.Real:
                    return typeof(float);

                case System.Data.SqlDbType.UniqueIdentifier:
                    return typeof(Guid);

                case System.Data.SqlDbType.SmallInt:
                    return typeof(short);

                case System.Data.SqlDbType.TinyInt:
                    return typeof(byte);

                case System.Data.SqlDbType.Udt:
                    throw Error.UnexpectedTypeCode(System.Data.SqlDbType.Udt);
            }
            return typeof(object);
        }

        #region ProviderBase
        // Nested Types
        internal abstract class ProviderBase : TypeSystemProvider
        {
            // Fields
            protected Dictionary<int, SqlType> applicationTypes = new Dictionary<int, SqlType>();

            // Methods

            public override IProviderType ChangeTypeFamilyTo(IProviderType type, IProviderType toType)
            {
                if (type.IsSameTypeFamily(toType))
                {
                    return type;
                }
                if (type.IsApplicationType || toType.IsApplicationType)
                {
                    return toType;
                }
                var type2 = (SqlType)toType;
                var type3 = (SqlType)type;
                if ((type2.Category != SqlType.TypeCategory.Numeric) || (type3.Category != SqlType.TypeCategory.Char))
                {
                    return toType;
                }
                var sqlDbType = (SqlDbType)type3.SqlDbType;
                if (sqlDbType != System.Data.SqlDbType.Char)
                {
                    if (sqlDbType == System.Data.SqlDbType.NChar)
                    {
                        return Create(System.Data.SqlDbType.Int);
                    }
                    return toType;
                }
                return Create(System.Data.SqlDbType.SmallInt);
            }

            public override IProviderType From(object o)
            {
                Type type = (o != null) ? o.GetType() : typeof(object);
                if (type == typeof(string))
                {
                    var str = (string)o;
                    return this.From(type, new int?(str.Length));
                }
                if (type == typeof(bool))
                {
                    return this.From(typeof(int));
                }
                if (type.IsArray)
                {
                    var array = (Array)o;
                    return From(type, new int?(array.Length));
                }
                if (type == typeof(decimal))
                {
                    var d = (decimal)o;
                    int num2 = (decimal.GetBits(d)[3] & 0xff0000) >> 0x10;
                    return From(type, num2);
                }
                return this.From(type);
            }

            public override IProviderType From(Type type)
            {
                return this.From(type, null);
            }

            public override IProviderType From(Type type, int? size)
            {
                return this.From(type, size);
            }

            public override IProviderType GetApplicationType(int index)
            {
                if (index < 0)
                {
                    throw Error.ArgumentOutOfRange("index");
                }
                SqlTypeSystem.SqlType type = null;
                if (!this.applicationTypes.TryGetValue(index, out type))
                {
                    type = new SqlTypeSystem.SqlType(index);
                    this.applicationTypes.Add(index, type);
                }
                return type;
            }

            private IProviderType[] GetArgumentTypes(SqlFunctionCall fc)
            {
                var typeArray = new IProviderType[fc.Arguments.Count];
                int index = 0;
                int length = typeArray.Length;
                while (index < length)
                {
                    typeArray[index] = fc.Arguments[index].SqlType;
                    index++;
                }
                return typeArray;
            }

            public override IProviderType GetBestType(IProviderType typeA, IProviderType typeB)
            {
                SqlTypeSystem.SqlType type = (typeA.ComparePrecedenceTo(typeB) > 0) ? ((SqlTypeSystem.SqlType)typeA) : ((SqlTypeSystem.SqlType)typeB);
                if (typeA.IsApplicationType || typeA.IsRuntimeOnlyType)
                {
                    return typeA;
                }
                if (typeB.IsApplicationType || typeB.IsRuntimeOnlyType)
                {
                    return typeB;
                }
                var type2 = (SqlType)typeA;
                var type3 = (SqlType)typeB;
                if ((type2.HasPrecisionAndScale && type3.HasPrecisionAndScale) && ((SqlDbType)type.SqlDbType == System.Data.SqlDbType.Decimal))
                {
                    int precision = type2.Precision;
                    int scale = type2.Scale;
                    int num3 = type3.Precision;
                    int num4 = type3.Scale;
                    if (((precision == 0) && (scale == 0)) && ((num3 == 0) && (num4 == 0)))
                    {
                        return Create((SqlDbType)type.SqlDbType);
                    }
                    if ((precision == 0) && (scale == 0))
                    {
                        return Create((SqlDbType)type.SqlDbType, num3, num4);
                    }
                    if ((num3 == 0) && (num4 == 0))
                    {
                        return Create((SqlDbType)type.SqlDbType, precision, scale);
                    }
                    int num5 = Math.Max((int)(precision - scale), (int)(num3 - num4));
                    int num6 = Math.Max(scale, num4);
                    Math.Min(num5 + num6, 0x25);
                    return Create((SqlDbType)type.SqlDbType, num5 + num6, num6);
                }
                int? size = null;
                if (type2.Size.HasValue && type3.Size.HasValue)
                {
                    var nullable4 = type3.Size;
                    var nullable5 = type2.Size;
                    size = ((nullable4.GetValueOrDefault() > nullable5.GetValueOrDefault()) && (nullable4.HasValue & nullable5.HasValue)) ? type3.Size : type2.Size;
                }
                if ((type3.Size.HasValue && (type3.Size.Value == -1)) || (type2.Size.HasValue && (type2.Size.Value == -1)))
                {
                    size = -1;
                }
                return new SqlType((SqlDbType)type.SqlDbType, size);
            }

            protected IProviderType GetBestType(SqlDbType targetType, int? size)
            {
                int num = 0;
                switch (targetType)
                {
                    case System.Data.SqlDbType.Binary:
                    case System.Data.SqlDbType.Char:
                    case System.Data.SqlDbType.VarBinary:
                    case System.Data.SqlDbType.VarChar:
                        num = 0x1f40;
                        break;

                    case System.Data.SqlDbType.NChar:
                    case System.Data.SqlDbType.NVarChar:
                        num = 0xfa0;
                        break;
                }
                if (!size.HasValue)
                {
                    return SqlTypeSystem.Create(targetType, this.SupportsMaxSize ? -1 : num);
                }
                if (size.Value <= num)
                {
                    return SqlTypeSystem.Create(targetType, size.Value);
                }
                return this.GetBestLargeType(SqlTypeSystem.Create(targetType));
            }

            protected virtual object GetParameterValue(SqlType type, object value)
            {
                if (value == null)
                    return DBNull.Value;

                var type2 = value.GetType();
                var closestRuntimeType = type.GetClosestRuntimeType();
                return closestRuntimeType == type2 ? value : DBConvert.ChangeType(value, closestRuntimeType);
            }

            public override void InitializeParameter(IProviderType type, DbParameter parameter, object value)
            {
                var type2 = (SqlType)type;
                if (type2.IsRuntimeOnlyType)
                {
                    throw Error.BadParameterType(type2.GetClosestRuntimeType());
                }
                var parameter2 = parameter as System.Data.SqlClient.SqlParameter;
                if (parameter2 != null)
                {
                    parameter2.SqlDbType = (SqlDbType)type2.SqlDbType;
                    if (type2.HasPrecisionAndScale)
                    {
                        parameter2.Precision = (byte)type2.Precision;
                        parameter2.Scale = (byte)type2.Scale;
                    }
                }
                else
                {
                    PropertyInfo property = parameter.GetType().GetProperty("SqlDbType");
                    if (property != null)
                    {
                        property.SetValue(parameter, type2.SqlDbType, null);
                    }
                    if (type2.HasPrecisionAndScale)
                    {
                        PropertyInfo info2 = parameter.GetType().GetProperty("Precision");
                        if (info2 != null)
                        {
                            info2.SetValue(parameter, Convert.ChangeType(type2.Precision, info2.PropertyType, CultureInfo.InvariantCulture), null);
                        }
                        PropertyInfo info3 = parameter.GetType().GetProperty("Scale");
                        if (info3 != null)
                        {
                            info3.SetValue(parameter, Convert.ChangeType(type2.Scale, info3.PropertyType, CultureInfo.InvariantCulture), null);
                        }
                    }
                }
                parameter.Value = GetParameterValue(type2, value);
                if (!((parameter.Direction == ParameterDirection.Input) && type2.IsFixedSize) && (parameter.Direction == ParameterDirection.Input))
                {
                    return;
                }
                if (type2.Size.HasValue)
                {
                    if (parameter.Size < type2.Size)
                    {
                        goto Label_016B;
                    }
                }
                if (!type2.IsLargeType)
                {
                    return;
                }
            Label_016B:
                parameter.Size = type2.Size.Value;
            }

            public override IProviderType MostPreciseTypeInFamily(IProviderType type)
            {
                var type2 = (SqlType)type;
                switch ((SqlDbType)type2.SqlDbType)
                {
                    case System.Data.SqlDbType.DateTime:
                    case System.Data.SqlDbType.SmallDateTime:
                        return this.From(typeof(DateTime));

                    case System.Data.SqlDbType.Decimal:
                    case System.Data.SqlDbType.Image:
                    case System.Data.SqlDbType.NChar:
                    case System.Data.SqlDbType.NText:
                    case System.Data.SqlDbType.NVarChar:
                    case System.Data.SqlDbType.UniqueIdentifier:
                    case System.Data.SqlDbType.Text:
                    case System.Data.SqlDbType.Timestamp:
                        return type;

                    case System.Data.SqlDbType.Float:
                    case System.Data.SqlDbType.Real:
                        return this.From(typeof(double));

                    case System.Data.SqlDbType.Int:
                    case System.Data.SqlDbType.SmallInt:
                    case System.Data.SqlDbType.TinyInt:
                        return this.From(typeof(int));

                    case System.Data.SqlDbType.Money:
                    case System.Data.SqlDbType.SmallMoney:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Money);
                }
                return type;
            }

            public override IProviderType Parse(string stype)
            {
                string strA = null;
                string s = null;
                string str3 = null;
                int index = stype.IndexOf('(');
                int num2 = stype.IndexOf(' ');
                int length = ((index != -1) && (num2 != -1)) ? Math.Min(num2, index) : ((index != -1) ? index : ((num2 != -1) ? num2 : -1));
                if (length == -1)
                {
                    strA = stype;
                    length = stype.Length;
                }
                else
                {
                    strA = stype.Substring(0, length);
                }
                int startIndex = length;
                if ((startIndex < stype.Length) && (stype[startIndex] == '('))
                {
                    startIndex++;
                    length = stype.IndexOf(',', startIndex);
                    if (length > 0)
                    {
                        s = stype.Substring(startIndex, length - startIndex);
                        startIndex = length + 1;
                        length = stype.IndexOf(')', startIndex);
                        str3 = stype.Substring(startIndex, length - startIndex);
                    }
                    else
                    {
                        length = stype.IndexOf(')', startIndex);
                        s = stype.Substring(startIndex, length - startIndex);
                    }
                    startIndex = length++;
                }
                if (string.Compare(strA, "rowversion", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    strA = "Timestamp";
                }
                if (string.Compare(strA, "numeric", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    strA = "Decimal";
                }
                if (string.Compare(strA, "sql_variant", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    strA = "Variant";
                }
                if (!Enum.GetNames(typeof(SqlDbType)).Select<string, string>(delegate(string n)
                {
                    return n.ToUpperInvariant();
                }).Contains<string>(strA.ToUpperInvariant()))
                {
                    throw Error.InvalidProviderType(strA);
                }
                int size = 0;
                int scale = 0;
                var type = (SqlDbType)Enum.Parse(typeof(SqlDbType), strA, true);
                if (s != null)
                {
                    if (string.Compare(s.Trim(), "max", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        size = -1;
                    }
                    else
                    {
                        size = int.Parse(s, CultureInfo.InvariantCulture);
                        if (size == 0x7fffffff)
                        {
                            size = -1;
                        }
                    }
                }
                if (str3 != null)
                {
                    if (string.Compare(str3.Trim(), "max", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        scale = -1;
                    }
                    else
                    {
                        scale = int.Parse(str3, CultureInfo.InvariantCulture);
                        if (scale == 0x7fffffff)
                        {
                            scale = -1;
                        }
                    }
                }
                switch (type)
                {
                    case System.Data.SqlDbType.Binary:
                    case System.Data.SqlDbType.Char:
                    case System.Data.SqlDbType.NChar:
                    case System.Data.SqlDbType.NVarChar:
                    case System.Data.SqlDbType.VarBinary:
                    case System.Data.SqlDbType.VarChar:
                        return SqlTypeSystem.Create(type, size);

                    case System.Data.SqlDbType.Decimal:
                    case System.Data.SqlDbType.Float:
                    case System.Data.SqlDbType.Real:
                        return SqlTypeSystem.Create(type, size, scale);
                }
                return SqlTypeSystem.Create(type);
            }

            public override IProviderType PredictTypeForBinary(SqlNodeType binaryOp, IProviderType leftType, IProviderType rightType)
            {
                IProviderType bestType;
                if (leftType.IsSameTypeFamily(this.From(typeof(string))) && rightType.IsSameTypeFamily(this.From(typeof(string))))
                {
                    bestType = this.GetBestType(leftType, rightType);
                }
                else
                {
                    bestType = (leftType.ComparePrecedenceTo(rightType) > 0) ? ((SqlTypeSystem.SqlType)leftType) : ((SqlTypeSystem.SqlType)rightType);
                }
                switch (binaryOp)
                {
                    case SqlNodeType.BitAnd:
                    case SqlNodeType.BitOr:
                    case SqlNodeType.BitXor:
                        return bestType;

                    case SqlNodeType.And:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.LE:
                    case SqlNodeType.LT:
                    case SqlNodeType.GE:
                    case SqlNodeType.GT:
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.Or:
                        return theInt;

                    case SqlNodeType.Add:
                        return bestType;

                    case SqlNodeType.Coalesce:
                        return bestType;

                    case SqlNodeType.Concat:
                        {
                            if (!bestType.HasSizeOrIsLarge)
                            {
                                return bestType;
                            }
                            IProviderType type2 = GetBestType((SqlDbType)bestType.SqlDbType, null);
                            if ((leftType.IsLargeType || !leftType.Size.HasValue) || (rightType.IsLargeType || !rightType.Size.HasValue))
                            {
                                return type2;
                            }
                            int num2 = leftType.Size.Value + rightType.Size.Value;
                            int num3 = num2;
                            if ((num3 >= type2.Size) && !type2.IsLargeType)
                            {
                                return type2;
                            }
                            return this.GetBestType((SqlDbType)(bestType).SqlDbType, new int?(num2));
                        }
                    case SqlNodeType.Div:
                        return bestType;

                    case SqlNodeType.Mod:
                    case SqlNodeType.Mul:
                        return bestType;

                    case SqlNodeType.Sub:
                        return bestType;
                }
                throw Error.UnexpectedNode(binaryOp);
            }

            public override IProviderType PredictTypeForUnary(SqlNodeType unaryOp, IProviderType operandType)
            {
                switch (unaryOp)
                {
                    case SqlNodeType.Avg:
                    case SqlNodeType.Covar:
                    case SqlNodeType.Stddev:
                    case SqlNodeType.Sum:
                        return this.MostPreciseTypeInFamily(operandType);

                    case SqlNodeType.BitNot:
                        return operandType;

                    case SqlNodeType.ClrLength:
                        if (operandType.IsLargeType)
                        {
                            return this.From(typeof(long));
                        }
                        return this.From(typeof(int));

                    case SqlNodeType.LongCount:
                        return this.From(typeof(long));

                    case SqlNodeType.Max:
                        return operandType;

                    case SqlNodeType.Count:
                        return this.From(typeof(int));

                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.IsNull:
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                        return SqlTypeSystem.theBit;

                    case SqlNodeType.Negate:
                        return operandType;

                    case SqlNodeType.OuterJoinedValue:
                        return operandType;

                    case SqlNodeType.Min:
                        return operandType;

                    case SqlNodeType.Treat:
                    case SqlNodeType.ValueOf:
                        return operandType;
                }
                throw Error.UnexpectedNode(unaryOp);
            }

            public override IProviderType ReturnTypeOfFunction(SqlFunctionCall functionCall)
            {
                IProviderType[] argumentTypes = this.GetArgumentTypes(functionCall);
                var type = (SqlTypeSystem.SqlType)argumentTypes[0];
                SqlTypeSystem.SqlType type2 = (argumentTypes.Length > 1) ? ((SqlTypeSystem.SqlType)argumentTypes[1]) : null;
                switch (functionCall.Name)
                {
                    case "LEN":
                    case "DATALENGTH":
                        switch ((SqlDbType)type.SqlDbType)
                        {
                            case SqlDbType.VarBinary:
                            case SqlDbType.VarChar:
                            case SqlDbType.NVarChar:
                                if (type.IsLargeType)
                                {
                                    return SqlTypeSystem.Create(SqlDbType.BigInt);
                                }
                                return SqlTypeSystem.Create(SqlDbType.Int);
                        }
                        return SqlTypeSystem.Create(SqlDbType.Int);

                    case "ABS":
                    case "SIGN":
                    case "ROUND":
                    case "CEILING":
                    case "FLOOR":
                    case "POWER":
                        {
                            var sqlDbType = (SqlDbType)type.SqlDbType;
                            //if (sqlDbType > SqlDbType.Real)
                            //{
                            //if ((sqlDbType == SqlDbType.SmallInt) || (sqlDbType == SqlDbType.TinyInt))
                            //{
                            //    //break;
                            //    return Create(SqlDbType.Int);
                            //}
                            //    return type;
                            //}
                            switch (sqlDbType)
                            {
                                case SqlDbType.Float:
                                case SqlDbType.Real:
                                    return SqlTypeSystem.Create(SqlDbType.Float);

                                case SqlDbType.Image:
                                    return type;

                                case SqlDbType.SmallInt:
                                case SqlDbType.TinyInt:
                                    return Create(SqlDbType.Int);
                            }
                            return type;
                        }
                    case "PATINDEX":
                    case "CHARINDEX":
                        if (!type2.IsLargeType)
                        {
                            return SqlTypeSystem.Create(SqlDbType.Int);
                        }
                        return SqlTypeSystem.Create(SqlDbType.BigInt);

                    case "SUBSTRING":
                        {
                            if (functionCall.Arguments[2].NodeType == SqlNodeType.Value)
                            {
                                var value2 = (SqlValue)functionCall.Arguments[2];
                                if ((value2.Value is int))
                                {
                                    switch ((SqlDbType)type.SqlDbType)
                                    {
                                        case SqlDbType.NChar:
                                        case SqlDbType.NVarChar:
                                        case SqlDbType.VarChar:
                                        case SqlDbType.Char:
                                            return SqlTypeSystem.Create((SqlDbType)type.SqlDbType, (int)value2.Value);

                                        case SqlDbType.NText:
                                            //goto Label_02D8;
                                            return null;
                                    }
                                    //goto Label_02D8;
                                    return null;
                                }
                                //goto Label_02DA;
                            }
                            //goto Label_02DA;
                            switch ((SqlDbType)type.SqlDbType)
                            {
                                case SqlDbType.NChar:
                                case SqlDbType.NVarChar:
                                    return SqlTypeSystem.Create(SqlDbType.NVarChar);

                                case SqlDbType.VarChar:
                                case SqlDbType.Char:
                                    return SqlTypeSystem.Create(SqlDbType.VarChar);

                                default:
                                    return null;
                            }
                        }
                    case "STUFF":
                        {
                            var value3 = functionCall.Arguments[2] as SqlValue;

                            if ((functionCall.Arguments.Count != 4) ||
                                ((value3 == null) || (((int)value3.Value) != 0)))
                            {
                                //goto Label_0375;
                                return null;
                            }
                            //if ((value3 == null) || (((int)value3.Value) != 0))
                            //{
                            //    //goto Label_0375;
                            //    return null;
                            //}
                            return this.PredictTypeForBinary(SqlNodeType.Concat, functionCall.Arguments[0].SqlType, functionCall.Arguments[3].SqlType);
                        }
                    case "LOWER":
                    case "UPPER":
                    case "RTRIM":
                    case "LTRIM":
                    case "INSERT":
                    case "REPLACE":
                    case "LEFT":
                    case "RIGHT":
                    case "REVERSE":
                        return type;

                    default:
                        return null;
                }
                return SqlTypeSystem.Create(SqlDbType.Int);
            Label_02D8:
                return null;
            Label_02DA:
                switch ((SqlDbType)type.SqlDbType)
                {
                    case SqlDbType.NChar:
                    case SqlDbType.NVarChar:
                        return SqlTypeSystem.Create(SqlDbType.NVarChar);

                    case SqlDbType.VarChar:
                    case SqlDbType.Char:
                        return SqlTypeSystem.Create(SqlDbType.VarChar);

                    default:
                        return null;
                }
            Label_0375:
                return null;
            }

            // Properties
            protected abstract bool SupportsMaxSize { get; }
        }
        #endregion

        internal class Sql2000Provider : SqlTypeSystem.ProviderBase
        {
            // Methods
            public override IProviderType From(Type type, int? size)
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    type = type.GetGenericArguments()[0];
                }
                TypeCode typeCode = Type.GetTypeCode(type);
                switch (typeCode)
                {
                    case TypeCode.Object:
                        if (type != typeof(Guid))
                        {
                            if ((type == typeof(byte[])) || (type == typeof(Binary)))
                            {
                                return base.GetBestType(System.Data.SqlDbType.VarBinary, size);
                            }
                            if (type == typeof(char[]))
                            {
                                return base.GetBestType(System.Data.SqlDbType.NVarChar, size);
                            }
                            if (type == typeof(TimeSpan))
                            {
                                return SqlTypeSystem.Create(System.Data.SqlDbType.BigInt);
                            }
                            if ((type != typeof(XDocument)) && (type != typeof(XElement)))
                            {
                                return new SqlTypeSystem.SqlType(type);
                            }
                            return SqlTypeSystem.theNText;
                        }
                        return SqlTypeSystem.Create(System.Data.SqlDbType.UniqueIdentifier);

                    case TypeCode.Boolean:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Bit);

                    case TypeCode.Char:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.NChar, 1);

                    case TypeCode.SByte:
                    case TypeCode.Int16:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.SmallInt);

                    case TypeCode.Byte:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.TinyInt);

                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Int);

                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.BigInt);

                    case TypeCode.UInt64:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Decimal, 20, 0);

                    case TypeCode.Single:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Real);

                    case TypeCode.Double:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Float);

                    case TypeCode.Decimal:
                        {
                            int? nullable = size;
                            return SqlTypeSystem.Create(System.Data.SqlDbType.Decimal, 0x1d, nullable.HasValue ? nullable.GetValueOrDefault() : 4);
                        }
                    case TypeCode.DateTime:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.DateTime);

                    case TypeCode.String:
                        return base.GetBestType(System.Data.SqlDbType.NVarChar, size);
                }
                throw Error.UnexpectedTypeCode(typeCode);
            }

            public override IProviderType GetBestLargeType(IProviderType type)
            {
                var type2 = (SqlTypeSystem.SqlType)type;
                switch ((SqlDbType)type2.SqlDbType)
                {
                    case System.Data.SqlDbType.Binary:
                    case System.Data.SqlDbType.VarBinary:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Image);

                    case System.Data.SqlDbType.Bit:
                        return type;

                    case System.Data.SqlDbType.Char:
                    case System.Data.SqlDbType.VarChar:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Text);

                    case System.Data.SqlDbType.NChar:
                    case System.Data.SqlDbType.NVarChar:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.NText);

                    case System.Data.SqlDbType.NText:
                        return type;
                }
                return type;
            }

            // Properties
            protected override bool SupportsMaxSize
            {
                get
                {
                    return false;
                }
            }
        }

        internal class Sql2005Provider : SqlTypeSystem.ProviderBase
        {
            // Methods
            public override IProviderType From(Type type, int? size)
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    type = type.GetGenericArguments()[0];
                }
                TypeCode typeCode = Type.GetTypeCode(type);
                switch (typeCode)
                {
                    case TypeCode.Object:
                        if (type != typeof(Guid))
                        {
                            if ((type == typeof(byte[])) || (type == typeof(Binary)))
                            {
                                return base.GetBestType(System.Data.SqlDbType.VarBinary, size);
                            }
                            if (type == typeof(char[]))
                            {
                                return base.GetBestType(System.Data.SqlDbType.NVarChar, size);
                            }
                            if (type == typeof(TimeSpan))
                            {
                                return SqlTypeSystem.Create(System.Data.SqlDbType.BigInt);
                            }
                            if ((type != typeof(XDocument)) && (type != typeof(XElement)))
                            {
                                return new SqlTypeSystem.SqlType(type);
                            }
                            return SqlTypeSystem.theChar;
                            //return SqlTypeSystem.theXml;
                        }
                        return SqlTypeSystem.Create(System.Data.SqlDbType.UniqueIdentifier);

                    case TypeCode.Boolean:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Bit);

                    case TypeCode.Char:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.NChar, 1);

                    case TypeCode.SByte:
                    case TypeCode.Int16:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.SmallInt);

                    case TypeCode.Byte:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.TinyInt);

                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Int);

                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.BigInt);

                    case TypeCode.UInt64:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Decimal, 20, 0);

                    case TypeCode.Single:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Real);

                    case TypeCode.Double:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.Float);

                    case TypeCode.Decimal:
                        {
                            int? nullable = size;
                            return SqlTypeSystem.Create(System.Data.SqlDbType.Decimal, 0x1d, nullable.HasValue ? nullable.GetValueOrDefault() : 4);
                        }
                    case TypeCode.DateTime:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.DateTime);

                    case TypeCode.String:
                        return base.GetBestType(System.Data.SqlDbType.NVarChar, size);
                }
                throw Error.UnexpectedTypeCode(typeCode);
            }

            public override IProviderType GetBestLargeType(IProviderType type)
            {
                var type2 = (SqlTypeSystem.SqlType)type;
                switch ((SqlDbType)type2.SqlDbType)
                {
                    case System.Data.SqlDbType.Binary:
                    case System.Data.SqlDbType.Image:
                    case System.Data.SqlDbType.VarBinary:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.VarBinary, -1);

                    case System.Data.SqlDbType.Bit:
                        return type;

                    case System.Data.SqlDbType.Char:
                    case System.Data.SqlDbType.Text:
                    case System.Data.SqlDbType.VarChar:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.VarChar, -1);

                    case System.Data.SqlDbType.Int:
                    case System.Data.SqlDbType.Money:
                        return type;

                    case System.Data.SqlDbType.NChar:
                    case System.Data.SqlDbType.NText:
                    case System.Data.SqlDbType.NVarChar:
                        return SqlTypeSystem.Create(System.Data.SqlDbType.NVarChar, -1);

                    case System.Data.SqlDbType.Timestamp:
                    case System.Data.SqlDbType.TinyInt:
                        return type;
                }
                return type;
            }

            // Properties
            protected override bool SupportsMaxSize
            {
                get
                {
                    return true;
                }
            }
        }

        internal class SqlCEProvider : Sql2000Provider
        {
            // Methods
            public override void InitializeParameter(IProviderType type, DbParameter parameter, object value)
            {
                var type2 = (SqlType)type;
                parameter.GetType().GetProperty("SqlDbType").SetValue(parameter, type2.SqlDbType, null);
                if (type2.HasPrecisionAndScale)
                {
                    PropertyInfo property = parameter.GetType().GetProperty("Precision");
                    if (property != null)
                    {
                        property.SetValue(parameter, Convert.ChangeType(type2.Precision, property.PropertyType, CultureInfo.InvariantCulture), null);
                    }
                    PropertyInfo info3 = parameter.GetType().GetProperty("Scale");
                    if (info3 != null)
                    {
                        info3.SetValue(parameter, Convert.ChangeType(type2.Scale, info3.PropertyType, CultureInfo.InvariantCulture), null);
                    }
                }
                parameter.Value = GetParameterValue(type2, value);
                if (((parameter.Direction == ParameterDirection.Input) && type2.IsFixedSize) || (parameter.Direction != ParameterDirection.Input))
                {
                    int size = parameter.Size;
                    if ((size < type2.Size) || type2.IsLargeType)
                    {
                        int? nullable2 = type2.Size;
                        parameter.Size = nullable2.HasValue ? nullable2.GetValueOrDefault() : 0;
                    }
                }
            }
        }

        internal class SqlType : ProviderType<SqlDbType>
        {
            // Fields
            private int? applicationTypeIndex;
            protected int precision;
            private Type runtimeOnlyType;
            protected int scale;
            protected int? size;
            //protected SqlDbType sqlDbType;

            // Methods
            internal SqlType(SqlDbType type)
            {
                applicationTypeIndex = null;
                SqlDbType = type;
            }

            internal SqlType(int applicationTypeIndex)
            {
                this.applicationTypeIndex = applicationTypeIndex;
            }

            internal SqlType(Type type)
            {
                this.applicationTypeIndex = null;
                this.runtimeOnlyType = type;
            }

            internal SqlType(SqlDbType type, int? size)
            {
                this.applicationTypeIndex = null;
                this.SqlDbType = type;// as Enum;//(int)type;
                this.size = size;
            }

            internal SqlType(SqlDbType type, int precision, int scale)
            {
                this.applicationTypeIndex = null;
                this.SqlDbType = type;//(int)type;
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
                SqlDbType sqlDbType = (SqlDbType)this.SqlDbType;
                if (sqlDbType <= System.Data.SqlDbType.NVarChar)
                {
                    switch (sqlDbType)
                    {
                        case System.Data.SqlDbType.NChar:
                        case System.Data.SqlDbType.NVarChar:
                        case System.Data.SqlDbType.Char:
                            goto Label_0039;

                        case System.Data.SqlDbType.NText:
                            goto Label_007D;
                    }
                    goto Label_007D;
                }
                if ((sqlDbType != System.Data.SqlDbType.Text) && (sqlDbType != System.Data.SqlDbType.VarChar))
                {
                    goto Label_007D;
                }
            Label_0039:
                str = o1 as string;
                if (str != null)
                {
                    string str2 = o2 as string;
                    if (str2 != null)
                    {
                        return str.TrimEnd(new char[] { ' ' }).Equals(str2.TrimEnd(new char[] { ' ' }), StringComparison.Ordinal);
                    }
                }
            Label_007D:
                return o1.Equals(o2);
            }

            //public override int ComparePrecedenceTo(IProviderType type)
            public override int ComparePrecedenceTo(ProviderType<SqlDbType> type)
            {
                var type2 = (SqlType)type;
                int num = this.IsTypeKnownByProvider ? GetTypeCoercionPrecedence((SqlDbType)this.SqlDbType) : -2147483648;
                int num2 = type2.IsTypeKnownByProvider ? GetTypeCoercionPrecedence((SqlDbType)type2.SqlDbType) : -2147483648;
                return num.CompareTo(num2);
            }

            public override bool Equals(object obj)
            {
                //if (this == obj)
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                var type = obj as SqlType;
                return (((type != null) && (((runtimeOnlyType == type.runtimeOnlyType) &&
                       ((applicationTypeIndex == type.applicationTypeIndex) && (SqlDbType == type.SqlDbType))) &&
                       ((Size == type.Size) && (precision == type.precision)))) && (scale == type.scale));
            }

            public override Type GetClosestRuntimeType()
            {
                if (this.runtimeOnlyType != null)
                {
                    return this.runtimeOnlyType;
                }
                return SqlTypeSystem.GetClosestRuntimeType((SqlDbType)this.SqlDbType);
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
                return ((((hashCode ^ this.SqlDbType.GetHashCode()) ^ (size.HasValue ? size.GetValueOrDefault() : 0)) ^ this.precision) ^ (this.scale << 8));
            }

            //public override IProviderType GetNonUnicodeEquivalent()
            public override ProviderType<SqlDbType> GetNonUnicodeEquivalent()
            {
                if (this.IsUnicodeType)
                {
                    switch ((SqlDbType)this.SqlDbType)
                    {
                        case System.Data.SqlDbType.NChar:
                            return new SqlTypeSystem.SqlType(System.Data.SqlDbType.Char, this.Size);

                        case System.Data.SqlDbType.NText:
                            return new SqlTypeSystem.SqlType(System.Data.SqlDbType.Text);

                        case System.Data.SqlDbType.NVarChar:
                            return new SqlTypeSystem.SqlType(System.Data.SqlDbType.VarChar, this.Size);
                    }
                }
                return this;
            }

            private static int GetTypeCoercionPrecedence(SqlDbType type)
            {
                switch (type)
                {
                    case System.Data.SqlDbType.BigInt:
                        return 15;

                    case System.Data.SqlDbType.Binary:
                        return 0;

                    case System.Data.SqlDbType.Bit:
                        return 11;

                    case System.Data.SqlDbType.Char:
                        return 3;

                    case System.Data.SqlDbType.DateTime:
                        return 0x16;

                    case System.Data.SqlDbType.Decimal:
                        return 0x12;

                    case System.Data.SqlDbType.Float:
                        return 20;

                    case System.Data.SqlDbType.Image:
                        return 8;

                    case System.Data.SqlDbType.Int:
                        return 14;

                    case System.Data.SqlDbType.Money:
                        return 0x11;

                    case System.Data.SqlDbType.NChar:
                        return 4;

                    case System.Data.SqlDbType.NText:
                        return 10;

                    case System.Data.SqlDbType.NVarChar:
                        return 5;

                    case System.Data.SqlDbType.Real:
                        return 0x13;

                    case System.Data.SqlDbType.UniqueIdentifier:
                        return 6;

                    case System.Data.SqlDbType.SmallDateTime:
                        return 0x15;

                    case System.Data.SqlDbType.SmallInt:
                        return 13;

                    case System.Data.SqlDbType.SmallMoney:
                        return 0x10;

                    case System.Data.SqlDbType.Text:
                        return 9;

                    case System.Data.SqlDbType.Timestamp:
                        return 7;

                    case System.Data.SqlDbType.TinyInt:
                        return 12;

                    case System.Data.SqlDbType.VarBinary:
                        return 1;

                    case System.Data.SqlDbType.VarChar:
                        return 2;

                    case System.Data.SqlDbType.Variant:
                        return 0x18;

                    case System.Data.SqlDbType.Xml:
                        return 0x17;

                    case System.Data.SqlDbType.Udt:
                        return 0x19;
                }
                throw Error.UnexpectedTypeCode(type);
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
                Debug.Assert(type is SqlType);
                var type2 = (SqlType)type;
                if (IsApplicationType)
                {
                    return false;
                }
                if (type2.IsApplicationType)
                {
                    return false;
                }
                if (Category == type2.Category)
                    return true;

                if (Category == TypeCategory.Text && (type2.Category == TypeCategory.Char || type2.Category == TypeCategory.Xml))
                    return true;

                if (Category == TypeCategory.Image && type2.Category == TypeCategory.Binary)
                    return true;

                return false;
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
                return ToQueryString(QueryFormatOptions.None);
            }

            public override string ToQueryString(QueryFormatOptions formatFlags)
            {
                Debug.Assert(formatFlags == QueryFormatOptions.None ||
                             formatFlags == QueryFormatOptions.SuppressSize);
                if (runtimeOnlyType != null)
                {
                    return runtimeOnlyType.ToString();
                }
                var builder = new StringBuilder();
                switch ((SqlDbType)SqlDbType)
                {
                    case System.Data.SqlDbType.BigInt:
                    case System.Data.SqlDbType.Bit:
                    case System.Data.SqlDbType.DateTime:
                    case System.Data.SqlDbType.Image:
                    case System.Data.SqlDbType.Int:
                    case System.Data.SqlDbType.Money:
                    case System.Data.SqlDbType.NText:
                    case System.Data.SqlDbType.UniqueIdentifier:
                    case System.Data.SqlDbType.SmallDateTime:
                    case System.Data.SqlDbType.SmallInt:
                    case System.Data.SqlDbType.SmallMoney:
                    case System.Data.SqlDbType.Text:
                    case System.Data.SqlDbType.Timestamp:
                    case System.Data.SqlDbType.TinyInt:
                    case System.Data.SqlDbType.Xml:
                    case System.Data.SqlDbType.Udt:
                        builder.Append(SqlDbType.ToString());
                        return builder.ToString();

                    case System.Data.SqlDbType.Binary:
                    case System.Data.SqlDbType.Char:
                    case System.Data.SqlDbType.NChar:
                        builder.Append(SqlDbType);
                        //if ((formatFlags & QueryFormatOptions.SuppressSize) == QueryFormatOptions.None)
                        if (formatFlags == QueryFormatOptions.None)
                        {
                            builder.Append("(");
                            builder.Append(size);
                            builder.Append(")");
                        }
                        return builder.ToString();

                    case System.Data.SqlDbType.Decimal:
                    case System.Data.SqlDbType.Float:
                    case System.Data.SqlDbType.Real:
                        builder.Append(this.SqlDbType);
                        if (this.precision != 0)
                        {
                            builder.Append("(");
                            builder.Append(precision);
                            if (this.scale != 0)
                            {
                                builder.Append(",");
                                builder.Append(this.scale);
                            }
                            builder.Append(")");
                        }
                        return builder.ToString();

                    case System.Data.SqlDbType.NVarChar:
                    case System.Data.SqlDbType.VarBinary:
                    case System.Data.SqlDbType.VarChar:
                        builder.Append(SqlDbType);
                        if (!size.HasValue ||
                            //((size == 0) || ((formatFlags & QueryFormatOptions.SuppressSize) != QueryFormatOptions.None)))
                            ((size == 0) || (formatFlags == QueryFormatOptions.SuppressSize)))
                        {
                            return builder.ToString();
                        }
                        builder.Append("(");
                        if (size != -1)
                            builder.Append(size);
                        else
                            builder.Append("MAX");
                        builder.Append(")");
                        return builder.ToString();

                    case System.Data.SqlDbType.Variant:
                        builder.Append("sql_variant");
                        return builder.ToString();

                    default:
                        return builder.ToString();
                }
            }

            public override string ToString()
            {
                return (SingleValue(GetClosestRuntimeType()) + SingleValue(ToQueryString()) + KeyValue("IsApplicationType", IsApplicationType) + KeyValue<bool>("IsUnicodeType", this.IsUnicodeType) + KeyValue<bool>("IsRuntimeOnlyType", this.IsRuntimeOnlyType) + KeyValue<bool>("SupportsComparison", this.SupportsComparison) + KeyValue<bool>("SupportsLength", this.SupportsLength) + KeyValue<bool>("IsLargeType", this.IsLargeType) + KeyValue<bool>("IsFixedSize", this.IsFixedSize) + KeyValue<bool>("IsOrderable", this.IsOrderable) + KeyValue<bool>("IsGroupable", this.IsGroupable) + KeyValue<bool>("IsNumeric", this.IsNumeric) + KeyValue<bool>("IsChar", this.IsChar) + KeyValue<bool>("IsString", this.IsString));
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
                        switch ((SqlDbType)this.SqlDbType)
                        {
                            case System.Data.SqlDbType.Real:
                            case System.Data.SqlDbType.SmallInt:
                            case System.Data.SqlDbType.SmallMoney:
                            case System.Data.SqlDbType.TinyInt:
                            case System.Data.SqlDbType.BigInt:
                            case System.Data.SqlDbType.Bit:
                            case System.Data.SqlDbType.Float:
                            case System.Data.SqlDbType.Int:
                            case System.Data.SqlDbType.Money:
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
                    switch ((SqlDbType)this.SqlDbType)
                    {
                        case System.Data.SqlDbType.BigInt:
                        case System.Data.SqlDbType.Bit:
                        case System.Data.SqlDbType.Decimal:
                        case System.Data.SqlDbType.Float:
                        case System.Data.SqlDbType.Int:
                        case System.Data.SqlDbType.Money:
                        case System.Data.SqlDbType.Real:
                        case System.Data.SqlDbType.SmallInt:
                        case System.Data.SqlDbType.SmallMoney:
                        case System.Data.SqlDbType.TinyInt:
                            return TypeCategory.Numeric;

                        case System.Data.SqlDbType.Binary:
                        case System.Data.SqlDbType.Timestamp:
                        case System.Data.SqlDbType.VarBinary:
                            return TypeCategory.Binary;

                        case System.Data.SqlDbType.Char:
                        case System.Data.SqlDbType.NChar:
                        case System.Data.SqlDbType.NVarChar:
                        case System.Data.SqlDbType.VarChar:
                            return TypeCategory.Char;

                        case System.Data.SqlDbType.Date:
                        case System.Data.SqlDbType.DateTime:
                        case System.Data.SqlDbType.SmallDateTime:
                            return TypeCategory.DateTime;

                        case System.Data.SqlDbType.Image:
                            return TypeCategory.Image;

                        case System.Data.SqlDbType.NText:
                        case System.Data.SqlDbType.Text:
                            return TypeCategory.Text;

                        case System.Data.SqlDbType.UniqueIdentifier:
                            return TypeCategory.UniqueIdentifier;

                        case System.Data.SqlDbType.Variant:
                            return TypeCategory.Variant;

                        case System.Data.SqlDbType.Xml:
                            return TypeCategory.Xml;

                        case System.Data.SqlDbType.Udt:
                            return TypeCategory.Udt;
                    }
                    throw Error.UnexpectedTypeCode(this);
                }
            }

            public override bool HasPrecisionAndScale
            {
                get
                {
                    switch ((SqlDbType)this.SqlDbType)
                    {
                        case System.Data.SqlDbType.Decimal:
                        case System.Data.SqlDbType.Float:
                        case System.Data.SqlDbType.Money:
                        case System.Data.SqlDbType.Real:
                        case System.Data.SqlDbType.SmallMoney:
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
                        switch ((SqlDbType)this.SqlDbType)
                        {
                            case System.Data.SqlDbType.NChar:
                            case System.Data.SqlDbType.NVarChar:
                            case System.Data.SqlDbType.VarChar:
                            case System.Data.SqlDbType.Char:
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
                    switch ((SqlDbType)this.SqlDbType)
                    {
                        case System.Data.SqlDbType.Text:
                        case System.Data.SqlDbType.VarBinary:
                        case System.Data.SqlDbType.VarChar:
                        case System.Data.SqlDbType.Xml:
                        case System.Data.SqlDbType.NText:
                        case System.Data.SqlDbType.NVarChar:
                        case System.Data.SqlDbType.Image:
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
                    var sqlDbType = (SqlDbType)this.SqlDbType;
                    if (sqlDbType <= System.Data.SqlDbType.NText)
                    {
                        switch (sqlDbType)
                        {
                            case System.Data.SqlDbType.Image:
                            case System.Data.SqlDbType.NText:
                                goto Label_002B;
                        }
                        goto Label_002D;
                    }
                    if ((sqlDbType != System.Data.SqlDbType.Text) && (sqlDbType != System.Data.SqlDbType.Xml))
                    {
                        goto Label_002D;
                    }
                Label_002B:
                    return false;
                Label_002D:
                    return true;
                }
            }

            public override bool IsLargeType
            {
                get
                {
                    switch ((SqlDbType)this.SqlDbType)
                    {
                        case System.Data.SqlDbType.Text:
                        case System.Data.SqlDbType.Xml:
                        case System.Data.SqlDbType.NText:
                        case System.Data.SqlDbType.Image:
                            return true;

                        case System.Data.SqlDbType.VarBinary:
                        case System.Data.SqlDbType.VarChar:
                        case System.Data.SqlDbType.NVarChar:
                            return (this.size == -1);
                    }
                    return false;
                }
            }

            public override bool IsNumeric
            {
                get
                {
                    if (!this.IsApplicationType && !this.IsRuntimeOnlyType)
                    {
                        switch ((SqlDbType)(SqlDbType)this.SqlDbType)
                        {
                            case System.Data.SqlDbType.BigInt:
                            case System.Data.SqlDbType.Bit:
                            case System.Data.SqlDbType.Decimal:
                            case System.Data.SqlDbType.Float:
                            case System.Data.SqlDbType.Int:
                            case System.Data.SqlDbType.Money:
                            case System.Data.SqlDbType.Real:
                            case System.Data.SqlDbType.SmallInt:
                            case System.Data.SqlDbType.SmallMoney:
                            case System.Data.SqlDbType.TinyInt:
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
                    SqlDbType sqlDbType = (SqlDbType)this.SqlDbType;
                    if (sqlDbType <= System.Data.SqlDbType.NText)
                    {
                        switch (sqlDbType)
                        {
                            case System.Data.SqlDbType.Image:
                            case System.Data.SqlDbType.NText:
                                goto Label_002B;
                        }
                        goto Label_002D;
                    }
                    if ((sqlDbType != System.Data.SqlDbType.Text) && (sqlDbType != System.Data.SqlDbType.Xml))
                    {
                        goto Label_002D;
                    }
                Label_002B:
                    return false;
                Label_002D:
                    return true;
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
                    int? nullable;
                    if (this.IsApplicationType || this.IsRuntimeOnlyType)
                    {
                        return false;
                    }
                    SqlDbType sqlDbType = (SqlDbType)this.SqlDbType;
                    if (sqlDbType <= System.Data.SqlDbType.NVarChar)
                    {
                        switch (sqlDbType)
                        {
                            case System.Data.SqlDbType.NChar:
                            case System.Data.SqlDbType.NVarChar:
                            case System.Data.SqlDbType.Char:
                                goto Label_0043;

                            case System.Data.SqlDbType.NText:
                                goto Label_0099;
                        }
                        goto Label_009B;
                    }
                    if (sqlDbType == System.Data.SqlDbType.Text)
                    {
                        goto Label_0099;
                    }
                    if (sqlDbType != System.Data.SqlDbType.VarChar)
                    {
                        goto Label_009B;
                    }
                Label_0043:
                    nullable = this.Size;
                    if (((nullable.GetValueOrDefault() != 0) || !nullable.HasValue) && (this.Size <= 1))
                    {
                        return (this.Size == -1);
                    }
                    return true;
                Label_0099:
                    return true;
                Label_009B:
                    return false;
                }
            }

            public override bool IsBinary
            {
                get { return Category == TypeCategory.Binary; }
            }

            public override bool IsDateTime
            {
                get { return Category == TypeCategory.DateTime; }
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
                    switch ((SqlDbType)this.SqlDbType)
                    {
                        case System.Data.SqlDbType.NChar:
                        case System.Data.SqlDbType.NText:
                        case System.Data.SqlDbType.NVarChar:
                            return true;
                    }
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

            public override SqlDbType SqlDbType
            {
                get;
                set;
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

            //public override Enum SqlDbType
            //{
            //    get
            //    {
            //        return this.sqlDbType;
            //    }
            //    set { sqlDbType = (SqlDbType) value; }
            //}

            public override bool SupportsComparison
            {
                get
                {
                    SqlDbType sqlDbType = (SqlDbType)this.SqlDbType;
                    if (sqlDbType <= System.Data.SqlDbType.NText)
                    {
                        switch (sqlDbType)
                        {
                            case System.Data.SqlDbType.Image:
                            case System.Data.SqlDbType.NText:
                                goto Label_0021;
                        }
                        goto Label_0023;
                    }
                    if ((sqlDbType != System.Data.SqlDbType.Text) && (sqlDbType != System.Data.SqlDbType.Xml))
                    {
                        goto Label_0023;
                    }
                Label_0021:
                    return false;
                Label_0023:
                    return true;
                }
            }

            public override bool SupportsLength
            {
                get
                {
                    //SqlDbType sqlDbType = this.sqlDbType;
                    //if (sqlDbType <= System.Data.SqlDbType.NText)
                    //{
                    switch ((SqlDbType)SqlDbType)
                    {
                        case System.Data.SqlDbType.Image:
                        case System.Data.SqlDbType.NText:
                        case System.Data.SqlDbType.Text:
                        case System.Data.SqlDbType.Xml:
                            return false;
                    }
                    return true;
                    //}
                    //if ((sqlDbType == System.Data.SqlDbType.Text) || (sqlDbType == System.Data.SqlDbType.Xml))
                    //{
                    //Label_0021:
                    //    return false;
                    //Label_0023:
                    //    return true;
                    //}
                    //return true;
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







}