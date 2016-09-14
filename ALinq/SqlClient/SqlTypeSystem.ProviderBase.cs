namespace ALinq.SqlClient
{
    //partial class SqlTypeSystem
    //{
    //    internal abstract class ProviderBase : TypeSystemProvider
    //    {
    //        // Fields
    //        protected readonly Dictionary<int, SqlType> applicationTypes = new Dictionary<int, SqlType>();

    //        // Methods
    //        protected ProviderBase(object source)
    //            : base(source)
    //        {
    //        }

    //        internal override ProviderType ChangeTypeFamilyTo(ProviderType type, ProviderType toType)
    //        {
    //            if (type.IsSameTypeFamily(toType))
    //            {
    //                return type;
    //            }
    //            if (type.IsApplicationType || toType.IsApplicationType)
    //            {
    //                return toType;
    //            }
    //            var type2 = (SqlType)toType;
    //            var type3 = (SqlType)type;
    //            if ((type2.Category != SqlType.TypeCategory.Numeric) || (type3.Category != SqlType.TypeCategory.Char))
    //            {
    //                return toType;
    //            }
    //            SqlDbType sqlDbType = type3.SqlDbType;
    //            if (sqlDbType != SqlDbType.Char)
    //            {
    //                if (sqlDbType == SqlDbType.NChar)
    //                {
    //                    return Create(SqlDbType.Int);
    //                }
    //                return toType;
    //            }
    //            return Create(SqlDbType.SmallInt);
    //        }

    //        internal override ProviderType From(object o)
    //        {
    //            Type type = (o != null) ? o.GetType() : typeof(object);
    //            if (type == typeof(string))
    //            {
    //                var str = (string)o;
    //                return From(type, str.Length);
    //            }
    //            if (type == typeof(bool))
    //            {
    //                return this.From(typeof(int));
    //            }
    //            if (type.IsArray)
    //            {
    //                var array = (Array)o;
    //                return this.From(type, new int?(array.Length));
    //            }
    //            if (type == typeof(decimal))
    //            {
    //                var d = (decimal)o;
    //                int num2 = (decimal.GetBits(d)[3] & 0xff0000) >> 0x10;
    //                return this.From(type, new int?(num2));
    //            }
    //            return this.From(type);
    //        }

    //        internal override ProviderType From(Type type)
    //        {
    //            return this.From(type, null);
    //        }

    //        internal abstract override ProviderType From(Type type, int? size);
    //        //{
    //        //    return this.From(type, size);
    //        //}

    //        internal override ProviderType GetApplicationType(int index)
    //        {
    //            if (index < 0)
    //            {
    //                throw Error.ArgumentOutOfRange("index");
    //            }
    //            SqlTypeSystem.SqlType type = null;
    //            if (!this.applicationTypes.TryGetValue(index, out type))
    //            {
    //                type = new SqlTypeSystem.SqlType(index);
    //                this.applicationTypes.Add(index, type);
    //            }
    //            return type;
    //        }

    //        private static ProviderType[] GetArgumentTypes(SqlFunctionCall fc)
    //        {
    //            var typeArray = new ProviderType[fc.Arguments.Count];
    //            int index = 0;
    //            int length = typeArray.Length;
    //            while (index < length)
    //            {
    //                typeArray[index] = fc.Arguments[index].SqlType;
    //                index++;
    //            }
    //            return typeArray;
    //        }

    //        internal override ProviderType GetBestType(ProviderType typeA, ProviderType typeB)
    //        {
    //            SqlTypeSystem.SqlType type = (typeA.ComparePrecedenceTo(typeB) > 0) ? ((SqlTypeSystem.SqlType)typeA) : ((SqlTypeSystem.SqlType)typeB);
    //            if (typeA.IsApplicationType || typeA.IsRuntimeOnlyType)
    //            {
    //                return typeA;
    //            }
    //            if (typeB.IsApplicationType || typeB.IsRuntimeOnlyType)
    //            {
    //                return typeB;
    //            }
    //            var type2 = (SqlType)typeA;
    //            var type3 = (SqlType)typeB;
    //            if ((type2.HasPrecisionAndScale && type3.HasPrecisionAndScale) && (type.SqlDbType == SqlDbType.Decimal))
    //            {
    //                int precision = type2.Precision;
    //                int scale = type2.Scale;
    //                int num3 = type3.Precision;
    //                int num4 = type3.Scale;
    //                if (((precision == 0) && (scale == 0)) && ((num3 == 0) && (num4 == 0)))
    //                {
    //                    return Create(type.SqlDbType);
    //                }
    //                if ((precision == 0) && (scale == 0))
    //                {
    //                    return Create(type.SqlDbType, num3, num4);
    //                }
    //                if ((num3 == 0) && (num4 == 0))
    //                {
    //                    return Create(type.SqlDbType, precision, scale);
    //                }
    //                int num5 = Math.Max((int)(precision - scale), (int)(num3 - num4));
    //                int num6 = Math.Max(scale, num4);
    //                int num7 = Math.Min(num5 + num6, 0x26);
    //                return Create(type.SqlDbType, num7, num6);
    //            }
    //            int? size = null;
    //            if (type2.Size.HasValue && type3.Size.HasValue)
    //            {
    //                int? nullable4 = type3.Size;
    //                int? nullable5 = type2.Size;
    //                size = ((nullable4.GetValueOrDefault() > nullable5.GetValueOrDefault()) && (nullable4.HasValue & nullable5.HasValue)) ? type3.Size : type2.Size;
    //            }
    //            if ((type3.Size.HasValue && (type3.Size.Value == -1)) || (type2.Size.HasValue && (type2.Size.Value == -1)))
    //            {
    //                size = -1;
    //            }
    //            return new SqlType(type.SqlDbType, size);
    //        }

    //        protected ProviderType GetBestType(SqlDbType targetType, int? size)
    //        {
    //            int num = 0;
    //            switch (targetType)
    //            {
    //                case SqlDbType.Binary:
    //                case SqlDbType.Char:
    //                case SqlDbType.VarBinary:
    //                case SqlDbType.VarChar:
    //                    num = 0x1f40;
    //                    break;

    //                case SqlDbType.NChar:
    //                case SqlDbType.NVarChar:
    //                    num = 0xfa0;
    //                    break;
    //            }
    //            if (!size.HasValue)
    //            {
    //                return Create(targetType, SupportsMaxSize ? -1 : num);
    //            }
    //            if (size.Value <= num)
    //            {
    //                return SqlTypeSystem.Create(targetType, size.Value);
    //            }
    //            return this.GetBestLargeType(Create(targetType));
    //        }

    //        protected static object GetParameterValue(SqlType type, object value)
    //        {
    //            if (value == null)
    //            {
    //                return DBNull.Value;
    //            }
    //            Type type2 = value.GetType();
    //            Type closestRuntimeType = type.GetClosestRuntimeType();
    //            if (closestRuntimeType == type2)
    //            {
    //                return value;
    //            }
    //            return DBConvert.ChangeType(value, closestRuntimeType);
    //        }

    //        internal override void InitializeParameter(ProviderType type, DbParameter parameter, object value)
    //        {
    //            var type2 = (SqlType)type;
    //            if (type2.IsRuntimeOnlyType)
    //            {
    //                throw Error.BadParameterType(type2.GetClosestRuntimeType());
    //            }
    //            var parameter2 = parameter as System.Data.SqlClient.SqlParameter;
    //            if (parameter2 != null)
    //            {
    //                parameter2.SqlDbType = type2.SqlDbType;
    //                if (type2.HasPrecisionAndScale)
    //                {
    //                    parameter2.Precision = (byte)type2.Precision;
    //                    parameter2.Scale = (byte)type2.Scale;
    //                }
    //            }
    //            else
    //            {
    //                PropertyInfo property = parameter.GetType().GetProperty("SqlDbType");
    //                if (property != null)
    //                {
    //                    property.SetValue(parameter, type2.SqlDbType, null);
    //                }
    //                if (type2.HasPrecisionAndScale)
    //                {
    //                    PropertyInfo info2 = parameter.GetType().GetProperty("Precision");
    //                    if (info2 != null)
    //                    {
    //                        info2.SetValue(parameter, Convert.ChangeType(type2.Precision, info2.PropertyType, CultureInfo.InvariantCulture), null);
    //                    }
    //                    PropertyInfo info3 = parameter.GetType().GetProperty("Scale");
    //                    if (info3 != null)
    //                    {
    //                        info3.SetValue(parameter, Convert.ChangeType(type2.Scale, info3.PropertyType, CultureInfo.InvariantCulture), null);
    //                    }
    //                }
    //            }
    //            parameter.Value = GetParameterValue(type2, value);
    //            if (!((parameter.Direction == ParameterDirection.Input) && type2.IsFixedSize) && (parameter.Direction == ParameterDirection.Input))
    //            {
    //                return;
    //            }
    //            if (type2.Size.HasValue)
    //            {
    //                if (parameter.Size < type2.Size)
    //                {
    //                    goto Label_016B;
    //                }
    //            }
    //            if (!type2.IsLargeType)
    //            {
    //                return;
    //            }
    //        Label_016B:
    //            parameter.Size = type2.Size.Value;
    //        }

    //        internal override ProviderType MostPreciseTypeInFamily(ProviderType type)
    //        {
    //            var type2 = (SqlType)type;
    //            switch (type2.SqlDbType)
    //            {
    //                case SqlDbType.DateTime:
    //                case SqlDbType.SmallDateTime:
    //                case SqlDbType.Date:
    //                case SqlDbType.Time:
    //                case SqlDbType.DateTime2:
    //                    return this.From(typeof(DateTime));

    //                case SqlDbType.Decimal:
    //                case SqlDbType.Image:
    //                case SqlDbType.NChar:
    //                case SqlDbType.NText:
    //                case SqlDbType.NVarChar:
    //                case SqlDbType.UniqueIdentifier:
    //                case SqlDbType.Text:
    //                case SqlDbType.Timestamp:
    //                    return type;

    //                case SqlDbType.Float:
    //                case SqlDbType.Real:
    //                    return this.From(typeof(double));

    //                case SqlDbType.Int:
    //                case SqlDbType.SmallInt:
    //                case SqlDbType.TinyInt:
    //                    return this.From(typeof(int));

    //                case SqlDbType.Money:
    //                case SqlDbType.SmallMoney:
    //                    return SqlTypeSystem.Create(SqlDbType.Money);

    //                case SqlDbType.DateTimeOffset:
    //                    return this.From(typeof(DateTimeOffset));
    //            }
    //            return type;
    //        }

    //        internal override ProviderType Parse(string stype)
    //        {
    //            string strA = null;
    //            string s = null;
    //            string str3 = null;
    //            int index = stype.IndexOf('(');
    //            int num2 = stype.IndexOf(' ');
    //            int length = ((index != -1) && (num2 != -1)) ? Math.Min(num2, index) : ((index != -1) ? index : ((num2 != -1) ? num2 : -1));
    //            if (length == -1)
    //            {
    //                strA = stype;
    //                length = stype.Length;
    //            }
    //            else
    //            {
    //                strA = stype.Substring(0, length);
    //            }
    //            int startIndex = length;
    //            if ((startIndex < stype.Length) && (stype[startIndex] == '('))
    //            {
    //                startIndex++;
    //                length = stype.IndexOf(',', startIndex);
    //                if (length > 0)
    //                {
    //                    s = stype.Substring(startIndex, length - startIndex);
    //                    startIndex = length + 1;
    //                    length = stype.IndexOf(')', startIndex);
    //                    str3 = stype.Substring(startIndex, length - startIndex);
    //                }
    //                else
    //                {
    //                    length = stype.IndexOf(')', startIndex);
    //                    s = stype.Substring(startIndex, length - startIndex);
    //                }
    //                startIndex = length++;
    //            }
    //            if (string.Compare(strA, "rowversion", StringComparison.OrdinalIgnoreCase) == 0)
    //            {
    //                strA = "Timestamp";
    //            }
    //            if (string.Compare(strA, "numeric", StringComparison.OrdinalIgnoreCase) == 0)
    //            {
    //                strA = "Decimal";
    //            }
    //            if (string.Compare(strA, "sql_variant", StringComparison.OrdinalIgnoreCase) == 0)
    //            {
    //                strA = "Variant";
    //            }
    //            if (string.Compare(strA, "filestream", StringComparison.OrdinalIgnoreCase) == 0)
    //            {
    //                strA = "Binary";
    //            }
    //            if (!Enum.GetNames(typeof(SqlDbType)).Select<string, string>(delegate(string n)
    //            {
    //                return n.ToUpperInvariant();
    //            }).Contains<string>(strA.ToUpperInvariant()))
    //            {
    //                throw Error.InvalidProviderType(strA);
    //            }
    //            int size = 0;
    //            int scale = 0;
    //            SqlDbType type = (SqlDbType)Enum.Parse(typeof(SqlDbType), strA, true);
    //            if (s != null)
    //            {
    //                if (string.Compare(s.Trim(), "max", StringComparison.OrdinalIgnoreCase) == 0)
    //                {
    //                    size = -1;
    //                }
    //                else
    //                {
    //                    size = int.Parse(s, CultureInfo.InvariantCulture);
    //                    if (size == 0x7fffffff)
    //                    {
    //                        size = -1;
    //                    }
    //                }
    //            }
    //            if (str3 != null)
    //            {
    //                if (string.Compare(str3.Trim(), "max", StringComparison.OrdinalIgnoreCase) == 0)
    //                {
    //                    scale = -1;
    //                }
    //                else
    //                {
    //                    scale = int.Parse(str3, CultureInfo.InvariantCulture);
    //                    if (scale == 0x7fffffff)
    //                    {
    //                        scale = -1;
    //                    }
    //                }
    //            }
    //            switch (type)
    //            {
    //                case SqlDbType.Binary:
    //                case SqlDbType.Char:
    //                case SqlDbType.NChar:
    //                case SqlDbType.NVarChar:
    //                case SqlDbType.VarBinary:
    //                case SqlDbType.VarChar:
    //                    return SqlTypeSystem.Create(type, size);

    //                case SqlDbType.Decimal:
    //                case SqlDbType.Float:
    //                case SqlDbType.Real:
    //                    return SqlTypeSystem.Create(type, size, scale);
    //            }
    //            return SqlTypeSystem.Create(type);
    //        }

    //        internal override ProviderType PredictTypeForBinary(SqlNodeType binaryOp, ProviderType leftType, ProviderType rightType)
    //        {
    //            SqlTypeSystem.SqlType bestType;
    //            if (leftType.IsSameTypeFamily(this.From(typeof(string))) && rightType.IsSameTypeFamily(this.From(typeof(string))))
    //            {
    //                bestType = (SqlTypeSystem.SqlType)this.GetBestType(leftType, rightType);
    //            }
    //            else
    //            {
    //                bestType = (leftType.ComparePrecedenceTo(rightType) > 0) ? ((SqlTypeSystem.SqlType)leftType) : ((SqlTypeSystem.SqlType)rightType);
    //            }
    //            switch (binaryOp)
    //            {
    //                case SqlNodeType.BitAnd:
    //                case SqlNodeType.BitOr:
    //                case SqlNodeType.BitXor:
    //                    return bestType;

    //                case SqlNodeType.And:
    //                case SqlNodeType.EQ:
    //                case SqlNodeType.EQ2V:
    //                case SqlNodeType.LE:
    //                case SqlNodeType.LT:
    //                case SqlNodeType.GE:
    //                case SqlNodeType.GT:
    //                case SqlNodeType.NE:
    //                case SqlNodeType.NE2V:
    //                case SqlNodeType.Or:
    //                    return SqlTypeSystem.theInt;

    //                case SqlNodeType.Add:
    //                    return bestType;

    //                case SqlNodeType.Coalesce:
    //                    return bestType;

    //                case SqlNodeType.Concat:
    //                    {
    //                        if (!bestType.HasSizeOrIsLarge)
    //                        {
    //                            return bestType;
    //                        }
    //                        ProviderType type2 = this.GetBestType(bestType.SqlDbType, null);
    //                        if ((leftType.IsLargeType || !leftType.Size.HasValue) || (rightType.IsLargeType || !rightType.Size.HasValue))
    //                        {
    //                            return type2;
    //                        }
    //                        int num2 = leftType.Size.Value + rightType.Size.Value;
    //                        int num3 = num2;
    //                        if ((num3 >= type2.Size) && !type2.IsLargeType)
    //                        {
    //                            return type2;
    //                        }
    //                        return this.GetBestType(bestType.SqlDbType, new int?(num2));
    //                    }
    //                case SqlNodeType.Div:
    //                    return bestType;

    //                case SqlNodeType.Mod:
    //                case SqlNodeType.Mul:
    //                    return bestType;

    //                case SqlNodeType.Sub:
    //                    return bestType;
    //            }
    //            throw Error.UnexpectedNode(binaryOp);
    //        }

    //        internal override ProviderType PredictTypeForUnary(SqlNodeType unaryOp, ProviderType operandType)
    //        {
    //            switch (unaryOp)
    //            {
    //                case SqlNodeType.Avg:
    //                case SqlNodeType.Covar:
    //                case SqlNodeType.Stddev:
    //                case SqlNodeType.Sum:
    //                    return this.MostPreciseTypeInFamily(operandType);

    //                case SqlNodeType.BitNot:
    //                    return operandType;

    //                case SqlNodeType.ClrLength:
    //                    if (operandType.IsLargeType)
    //                    {
    //                        return this.From(typeof(long));
    //                    }
    //                    return this.From(typeof(int));

    //                case SqlNodeType.LongCount:
    //                    return this.From(typeof(long));

    //                case SqlNodeType.Max:
    //                    return operandType;

    //                case SqlNodeType.Count:
    //                    return this.From(typeof(int));

    //                case SqlNodeType.IsNotNull:
    //                case SqlNodeType.IsNull:
    //                case SqlNodeType.Not:
    //                case SqlNodeType.Not2V:
    //                    return SqlTypeSystem.theBit;

    //                case SqlNodeType.Negate:
    //                    return operandType;

    //                case SqlNodeType.OuterJoinedValue:
    //                    return operandType;

    //                case SqlNodeType.Min:
    //                    return operandType;

    //                case SqlNodeType.Treat:
    //                case SqlNodeType.ValueOf:
    //                    return operandType;
    //            }
    //            throw Error.UnexpectedNode(unaryOp);
    //        }

    //        internal override ProviderType ReturnTypeOfFunction(SqlFunctionCall functionCall)
    //        {
    //            ProviderType[] argumentTypes = GetArgumentTypes(functionCall);
    //            var type = (SqlType)argumentTypes[0];
    //            SqlTypeSystem.SqlType type2 = (argumentTypes.Length > 1) ? ((SqlType)argumentTypes[1]) : null;
    //            switch (functionCall.Name)
    //            {
    //                case "LEN":
    //                case "DATALENGTH":
    //                    switch (type.SqlDbType)
    //                    {
    //                        case SqlDbType.VarBinary:
    //                        case SqlDbType.VarChar:
    //                        case SqlDbType.NVarChar:
    //                            if (type.IsLargeType)
    //                            {
    //                                return SqlTypeSystem.Create(SqlDbType.BigInt);
    //                            }
    //                            return SqlTypeSystem.Create(SqlDbType.Int);
    //                    }
    //                    return SqlTypeSystem.Create(SqlDbType.Int);

    //                case "ABS":
    //                case "SIGN":
    //                case "ROUND":
    //                case "CEILING":
    //                case "FLOOR":
    //                case "POWER":
    //                    {
    //                        SqlDbType sqlDbType = type.SqlDbType;
    //                        if (sqlDbType > SqlDbType.Real)
    //                        {
    //                            if ((sqlDbType != SqlDbType.SmallInt) && (sqlDbType != SqlDbType.TinyInt))
    //                            {
    //                                return type;
    //                            }
    //                            break;
    //                        }
    //                        switch (sqlDbType)
    //                        {
    //                            case SqlDbType.Float:
    //                            case SqlDbType.Real:
    //                                return SqlTypeSystem.Create(SqlDbType.Float);

    //                            case SqlDbType.Image:
    //                                return type;
    //                        }
    //                        return type;
    //                    }
    //                case "PATINDEX":
    //                case "CHARINDEX":
    //                    if (!type2.IsLargeType)
    //                    {
    //                        return Create(SqlDbType.Int);
    //                    }
    //                    return Create(SqlDbType.BigInt);

    //                case "SUBSTRING":
    //                    {
    //                        if (functionCall.Arguments[2].NodeType != SqlNodeType.Value)
    //                        {
    //                            goto Label_02DA;
    //                        }
    //                        var value2 = (SqlValue)functionCall.Arguments[2];
    //                        if (!(value2.Value is int))
    //                        {
    //                            goto Label_02DA;
    //                        }
    //                        switch (type.SqlDbType)
    //                        {
    //                            case SqlDbType.NChar:
    //                            case SqlDbType.NVarChar:
    //                            case SqlDbType.VarChar:
    //                            case SqlDbType.Char:
    //                                return SqlTypeSystem.Create(type.SqlDbType, (int)value2.Value);

    //                            case SqlDbType.NText:
    //                                goto Label_02D8;
    //                        }
    //                        goto Label_02D8;
    //                    }
    //                case "STUFF":
    //                    {
    //                        if (functionCall.Arguments.Count != 4)
    //                        {
    //                            goto Label_0375;
    //                        }
    //                        SqlValue value3 = functionCall.Arguments[2] as SqlValue;
    //                        if ((value3 == null) || (((int)value3.Value) != 0))
    //                        {
    //                            goto Label_0375;
    //                        }
    //                        return this.PredictTypeForBinary(SqlNodeType.Concat, functionCall.Arguments[0].SqlType, functionCall.Arguments[3].SqlType);
    //                    }
    //                case "LOWER":
    //                case "UPPER":
    //                case "RTRIM":
    //                case "LTRIM":
    //                case "INSERT":
    //                case "REPLACE":
    //                case "LEFT":
    //                case "RIGHT":
    //                case "REVERSE":
    //                    return type;

    //                default:
    //                    return null;
    //            }
    //            return SqlTypeSystem.Create(SqlDbType.Int);
    //        Label_02D8:
    //            return null;
    //        Label_02DA:
    //            switch (type.SqlDbType)
    //            {
    //                case SqlDbType.NChar:
    //                case SqlDbType.NVarChar:
    //                    return SqlTypeSystem.Create(SqlDbType.NVarChar);

    //                case SqlDbType.VarChar:
    //                case SqlDbType.Char:
    //                    return SqlTypeSystem.Create(SqlDbType.VarChar);

    //                default:
    //                    return null;
    //            }
    //        Label_0375:
    //            return null;
    //        }

    //        // Properties
    //        protected abstract bool SupportsMaxSize { get; }
    //    }


    //}
}
