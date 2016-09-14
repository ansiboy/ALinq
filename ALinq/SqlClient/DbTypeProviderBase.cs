using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ALinq.SqlClient;
using System.Data;
using System.Xml.Linq;

namespace ALinq.SqlClient
{
    internal abstract class DbTypeProviderBase<SQLTYPE, DBType> : TypeSystemProvider<SQLTYPE> where SQLTYPE : SqlDataType<DBType>
    {
        // Fields
        private static Dictionary<TypeCode, SQLTYPE> sqlTypes;
        private static readonly object sqlTypesLock = new object();
        private static SQLTYPE guidType;
        protected Dictionary<int, SQLTYPE> applicationTypes = new Dictionary<int, SQLTYPE>();
        //private static IProviderType theChar;

        protected const int DEFAULT_DECIMAL_SCALE = SqlDataType<DBType>.DEFAULT_DECIMAL_SCALE;
        protected const int DEFAULT_DECIMAL_PRECISION = SqlDataType<DBType>.DEFAULT_DECIMAL_PRECISION;
        protected const int STRING_SIZE = SqlDataType<DBType>.STRING_SIZE;

        internal virtual SQLTYPE GuidType
        {
            get
            {
                if (guidType == null)
                {
                    guidType = CreateSqlType(TypeMapping[TypeCode.Char], 36);
                }
                return guidType;
            }
        }

        protected abstract Dictionary<TypeCode, DBType> TypeMapping { get; }

        internal virtual Dictionary<TypeCode, SQLTYPE> SqlTypes
        {
            get
            {
                if (sqlTypes == null)
                {
                    lock (sqlTypesLock)
                    {
                        sqlTypes = new Dictionary<TypeCode, SQLTYPE>();
                        foreach (var item in TypeMapping)
                        {
                            SQLTYPE sqlType;
                            switch (item.Key)
                            {
                                case TypeCode.Decimal:
                                    {
                                        var args = new object[] { TypeMapping, item.Value, 
                                                                  DEFAULT_DECIMAL_PRECISION, DEFAULT_DECIMAL_SCALE };
                                        sqlType = (SQLTYPE)Activator.CreateInstance(typeof(SQLTYPE), args);
                                        //sqlType = CreateSqlType(item.Value, DEFAULT_DECIMAL_PRECISION, DEFAULT_DECIMAL_SCALE);
                                    }
                                    break;
                                case TypeCode.Char:
                                    {
                                        //sqlType = CreateSqlType(item.Value, 1);
                                        var args = new object[] { TypeMapping, item.Value, 1 };
                                        sqlType = (SQLTYPE)Activator.CreateInstance(typeof(SQLTYPE), args);
                                    }
                                    break;
                                case TypeCode.String:
                                    {
                                        var args = new object[] { TypeMapping, item.Value, STRING_SIZE };
                                        sqlType = (SQLTYPE)Activator.CreateInstance(typeof(SQLTYPE), args);
                                    }
                                    break;
                                default:
                                    {
                                        var args = new object[] { TypeMapping, item.Value };
                                        sqlType = (SQLTYPE)Activator.CreateInstance(typeof(SQLTYPE), args);
                                        //sqlType = CreateSqlType(item.Value);
                                    }
                                    break;
                            }
                            //sqlType.SqlDbType = item.Value as Enum;//Convert.ToInt32(item.Value);//as Enum;
                            //if (item.Key == TypeCode.Decimal)
                            //{
                            //    sqlType.Precision = DEFAULT_DECIMAL_PRECISION;
                            //    sqlType.Scale = DEFAULT_DECIMAL_SCALE;
                            //}
                            //char is VarChar type.
                            //if (item.Key == TypeCode.Char)// && item.Value == TypeMapping[TypeCode.String])
                            //    sqlType.Size = 1;
                            sqlTypes.Add(item.Key, sqlType);
                        }
                    }
                }
                return sqlTypes;
            }
        }

        // Methods

        internal override SQLTYPE ChangeTypeFamilyTo(SQLTYPE type, SQLTYPE toType)
        {
            if (type.IsSameTypeFamily(toType))
            {
                return type;
            }
            if (type.IsApplicationType || toType.IsApplicationType)
            {
                return toType;
            }
            return toType;
        }

        internal override SQLTYPE From(object o)
        {
            Type type = (o != null) ? o.GetType() : typeof(object);

            Debug.Assert(o != null);

            if (type == typeof(string))
            {
                var str = (string)o;
                return From(type, str.Length);
            }
            //if (type == typeof(bool))
            //{
            //    return From(typeof(int));
            //}
            if (type.IsArray)
            {
                var array = (Array)o;
                return From(type, array.Length);
            }
            //if (type == typeof(decimal))
            //{
            //    var d = (decimal)o;
            //    int num2 = (decimal.GetBits(d)[3] & 0xff0000) >> 0x10;
            //    return From(type, num2);
            //}
            return From(type);
        }

        internal override SQLTYPE From(Type type)
        {
            return From(type, null);
        }

        //internal abstract IProviderType From(Type type, int? size);
        //{
        //    //return this.From(type, size);
        //}

        internal override SQLTYPE GetApplicationType(int index)
        {
            if (index < 0)
            {
                throw ALinq.Error.ArgumentOutOfRange("index");
            }
            SQLTYPE type;
            if (!applicationTypes.TryGetValue(index, out type))
            {
                type = CreateSqlType(index); //new FirebirdType(index);
                applicationTypes.Add(index, type);
            }
            return type;
        }



        protected IProviderType[] GetArgumentTypes(SqlFunctionCall fc)
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

        internal override SQLTYPE GetBestType(SQLTYPE typeA, SQLTYPE typeB)
        {
            var type = (typeA.ComparePrecedenceTo(typeB) > 0) ? typeA : typeB;
            if (typeA.IsApplicationType || typeA.IsRuntimeOnlyType)
            {
                return typeA;
            }
            if (typeB.IsApplicationType || typeB.IsRuntimeOnlyType)
            {
                return typeB;
            }
            var type2 = typeA;
            var type3 = typeB;
            if ((type2.HasPrecisionAndScale && type3.HasPrecisionAndScale) && IsDecimalType((DBType)(type.SqlDbType as object))) //(type.SqlDbType == SqlDbType.Decimal))
            {
                int precision = type2.Precision;
                int scale = type2.Scale;
                int num3 = type3.Precision;
                int num4 = type3.Scale;
                if (((precision == 0) && (scale == 0)) && ((num3 == 0) && (num4 == 0)))
                {
                    return CreateSqlType(type.SqlDbType);
                }
                if ((precision == 0) && (scale == 0))
                {
                    return CreateSqlType(type.SqlDbType, num3, num4);
                }
                if ((num3 == 0) && (num4 == 0))
                {
                    return CreateSqlType(type.SqlDbType, precision, scale);
                }
                int num5 = Math.Max(precision - scale, num3 - num4);
                int num6 = Math.Max(scale, num4);
                Math.Min(num5 + num6, 0x25);
                return CreateSqlType(type.SqlDbType, num5 + num6, num6);
            }
            int? size = null;
            if (type2.Size.HasValue && type3.Size.HasValue)
            {
                int? nullable4 = type3.Size;
                int? nullable5 = type2.Size;
                size = ((nullable4.GetValueOrDefault() > nullable5.GetValueOrDefault()) && (nullable4.HasValue & nullable5.HasValue)) ? type3.Size : type2.Size;
            }
            if ((type3.Size.HasValue && (type3.Size.Value == -1)) || (type2.Size.HasValue && (type2.Size.Value == -1)))
            {
                size = -1;
            }
            //MY Code
            if (size == null)
            {
                return CreateSqlType(type.SqlDbType);
            }

            return CreateSqlType(type.SqlDbType, size.Value);
            //return new FirebirdDbType(type.SqlDbType, size);
        }

        protected virtual SQLTYPE CreateSqlType()
        {
            var item = (SQLTYPE)Activator.CreateInstance(typeof(SQLTYPE), new[] { TypeMapping });
            return item;
        }

        protected SQLTYPE CreateSqlType(DBType sqlDbType)
        {
            var typecode = TypeMapping.Where(o => Equals(o.Value, sqlDbType))
                                      .Select(o => o.Key)
                                      .LastOrDefault();

            if (typecode != TypeCode.Empty)
            {
                return SqlTypes[typecode];
            }
            var args = new object[] { TypeMapping, sqlDbType };
            var item = (SQLTYPE)Activator.CreateInstance(typeof(SQLTYPE), args);
            return item;
        }

        protected SQLTYPE CreateSqlType(DBType sqlDbType, int size)
        {
            var args = new object[] { TypeMapping, sqlDbType, size };
            var item = (SQLTYPE)Activator.CreateInstance(typeof(SQLTYPE), args);
            return item;
        }

        protected virtual SQLTYPE CreateSqlType(DBType sqlDbType, int precision, int scale)
        {
            var args = new object[] { TypeMapping, sqlDbType, precision, scale };
            var item = (SQLTYPE)Activator.CreateInstance(typeof(SQLTYPE), args);
            return item;

            //var item = CreateSqlType();
            //item.SqlDbType = sqlDbType as Enum;
            //item.Precision = precision;
            //item.Scale = scale;
            //return item;
        }

        protected virtual SQLTYPE CreateSqlType(int index)
        {
            var item = CreateSqlType();
            item.ApplicationTypeIndex = index;
            return item;
        }

        protected virtual object GetParameterValue(IProviderType type, object value)
        {
            if (value == null)
                return DBNull.Value;

            var type2 = value.GetType();
            var closestRuntimeType = type.GetClosestRuntimeType();
            return closestRuntimeType == type2 ? value : DBConvert.ChangeType(value, closestRuntimeType);
        }

        internal override void InitializeParameter(SQLTYPE type, DbParameter parameter, object value)
        {
            var type2 = type;
            if (type2.IsRuntimeOnlyType)
            {
                throw Error.BadParameterType(type2.GetClosestRuntimeType());
            }
            SetDbType(parameter, type2.SqlDbType);
            if (type2.HasPrecisionAndScale)
            {
                Debug.Assert(type2.Precision > 0);
                Debug.Assert(type2.Scale >= 0);
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
            //parameter.Size = 16;
#if DEBUG
            if (type2.IsString)
                Debug.Assert(type2.Size > 0);
#endif

            parameter.Value = GetParameterValue(type2, value);
            if (!((parameter.Direction == ParameterDirection.Input) && type2.IsFixedSize) && (parameter.Direction == ParameterDirection.Input))
            {
                return;
            }
            //Debug.Assert(type2.Size != null);
            int size = type2.Size.GetValueOrDefault();
            if ((size != SqlDataType<DBType>.NULL_VALUE && parameter.Size < size) || type2.IsLargeType)
            {
                parameter.Size = System.Math.Max(parameter.Size, size);
                Debug.Assert(parameter.Size >= 0);
            }

        }

        protected virtual void SetDbType(DbParameter parameter, DBType dbType)
        {
            PropertyInfo property = parameter.GetType().GetProperty(typeof(DBType).Name);
            Debug.Assert(property != null);
            property.SetValue(parameter, dbType, null);
        }


        internal override SQLTYPE Parse(string stype)
        {
            if (stype == null)
                throw Error.ArgumentNull(stype);

            stype = stype.ToUpper();
            string typeName;
            string s = null;
            string str3 = null;
            int index = stype.IndexOf('(');
            int num2 = stype.IndexOf(' ');
            int length;

            //get the type name.
            if ((index != -1) && (num2 != -1))
            {
                length = Math.Min(num2, index);
            }
            else if (index != -1)
            {
                length = index;
            }
            else if (num2 != -1)
            {
                length = num2;
            }
            else
            {
                length = -1;
            }

            if (length == -1)
            {
                typeName = stype;
                length = stype.Length;
            }
            else
            {
                typeName = stype.Substring(0, length);
            }

            //
            int startIndex = Math.Max(length, index);
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
                startIndex = length + 1;
            }

            var options = new string[] { };
            if (startIndex < stype.Length - 1)
            {
                var strOptions = stype.Substring(startIndex);
                if (strOptions != string.Empty)
                {
                    options = Regex.Split(strOptions, @"\s+")
                                   .Where(o => !string.IsNullOrEmpty(o)).ToArray();
                }
            }

            int size = SqlDataType<DBType>.NULL_VALUE;
            int scale = SqlDataType<DBType>.NULL_VALUE;
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
            var item = Parse(typeName, size, scale, options);
            if (item == null)
                throw Error.InvalidProviderType(stype);
            return item;
        }

        protected virtual SQLTYPE Parse(string typeName, int size, int scale, string[] options)
        {
            Debug.Assert(typeName == typeName.ToUpper());
            switch (typeName.ToUpper())
            {
                case "CHAR":
                case "VARCHAR":
                    return From(typeof(string), size);
                case "BINARY":
                    return From(typeof(byte[]), size);
                case "NUMBER":
                    if (scale == SqlDataType<DBType>.NULL_VALUE)
                        return From(typeof(decimal), size);
                    return From(typeof(decimal), size, scale);
                case "INT":
                case "INTEGER":
                    return From(typeof(Int32));

            }
            if (!Enum.GetNames(typeof(DBType)).Select(n => n.ToUpperInvariant())
                                              .Contains(typeName.ToUpperInvariant()))
            {
                throw SqlClient.Error.InvalidProviderType(typeName);
            }
            var type = Enum.Parse(typeof(DBType), typeName, true);
            SQLTYPE item;
            if (scale > 0)
                item = CreateSqlType((DBType)type, size, scale);
            else
                item = CreateSqlType((DBType)type, size);
            return item;
        }

        //[Obsolete]
        //internal virtual IProviderType Parse(string typeName, int size, int scale)
        //{
        //    //if (!Enum.GetNames(typeof(DBType)).Select(n => n.ToUpperInvariant()).Contains(typeName.ToUpperInvariant()))
        //    //{
        //    //    throw Error.InvalidProviderType(typeName);
        //    //}
        //    //var type = Enum.Parse(typeof(DBType), typeName, true);
        //    ////DbType a = default(DbType);
        //    ////var b = a as Enum;
        //    ////a =  (DbType)(b as object);

        //    //var item = CreateSqlType((DBType)type);
        //    //item.Size = size;
        //    //item.Scale = scale;
        //    //return item;
        //    return null;
        //}

        internal override SQLTYPE PredictTypeForBinary(SqlNodeType binaryOp, SQLTYPE leftType, SQLTYPE rightType)
        {
            Debug.Assert(leftType != null);
            Debug.Assert(rightType != null);
            SQLTYPE bestType;
            if (leftType.IsSameTypeFamily(From(typeof(string))) && rightType.IsSameTypeFamily(From(typeof(string))))
            {
                bestType = GetBestType(leftType, rightType);
            }
            else
            {
                bestType = (leftType.ComparePrecedenceTo(rightType) > 0) ? (leftType) : (rightType);
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
                    return SqlTypes[TypeCode.Int32];//TheInt;

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
                        SQLTYPE type2 = GetBestType((bestType).SqlDbType, 2000);
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
                        return GetBestType((DBType)((bestType).SqlDbType as object), num2);
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

        internal override SQLTYPE PredictTypeForUnary(SqlNodeType unaryOp, SQLTYPE operandType)
        {
            switch (unaryOp)
            {
                case SqlNodeType.Avg:
                case SqlNodeType.Covar:
                case SqlNodeType.Stddev:
                case SqlNodeType.Sum:
                    return MostPreciseTypeInFamily(operandType);

                case SqlNodeType.BitNot:
                    return operandType;

                case SqlNodeType.ClrLength:
                    if (operandType.IsLargeType)
                    {
                        return From(typeof(long));
                    }
                    return From(typeof(int));

                case SqlNodeType.LongCount:
                    return From(typeof(long));

                case SqlNodeType.Max:
                    return operandType;

                case SqlNodeType.Count:
                    return From(typeof(int));

                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                    return SqlTypes[TypeCode.Boolean];//TheBit;

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

        // Properties
        protected virtual bool SupportsMaxSize
        {
            get { return false; }
        }

        internal override SQLTYPE From(Type type, int? size)
        {
            return From(type, size, null);
        }

        internal virtual SQLTYPE From(Type type, int? size, int? scale)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                type = type.GetGenericArguments()[0];
            }
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.DateTime:
                    //var dbType = TypeMapping[typeCode];
                    //return CreateInstance(dbType);
                    Debug.Assert(SqlTypes.ContainsKey(typeCode));
                    return SqlTypes[typeCode];

                case TypeCode.Object:
                    if (type == typeof(Guid))
                    {
                        return GuidType;
                    }
                    if ((type == typeof(byte[])) || (type == typeof(Binary)))
                    {
                        return GetBestType(TypeMapping[TypeCode.Object], size);
                        //(System.Data.SqlDbType.VarBinary, size);
                    }
                    if (type == typeof(char[]))
                    {
                        return GetBestType(TypeMapping[TypeCode.String], size);
                        //(System.Data.SqlDbType.NVarChar, size);
                    }
                    if (type == typeof(TimeSpan))
                    {
                        return SqlTypes[TypeCode.Int64]; 
                    }
                    if ((type != typeof(XDocument)) && (type != typeof(XElement)))
                    {
                        var item = CreateSqlType();
                        item.RuntimeOnlyType = type;
                        return item;
                    }
                    return SqlTypes[TypeCode.String];


                case TypeCode.Decimal:
                    {
                        var value = size.GetValueOrDefault(0);
                        if (value == DEFAULT_DECIMAL_SCALE || value == 0)
                        {
                            var r = SqlTypes[TypeCode.Decimal];
                            //Debug.Assert(r.Precision == DEFAULT_DECIMAL_PRECISION);
                            Debug.Assert(r.Scale == DEFAULT_DECIMAL_SCALE);
                            return r;
                        }

                        if (scale == null)
                            return CreateSqlType(TypeMapping[TypeCode.Decimal], size.HasValue ? size.Value : 0x1d, 4);

                        return CreateSqlType(TypeMapping[TypeCode.Decimal], size.HasValue ? size.Value : 0x1d, scale.Value);
                        //SqlTypeSystem.Create(System.Data.SqlDbType.Decimal, 0x1d, nullable.HasValue ? nullable.GetValueOrDefault() : 4);
                    }
                case TypeCode.String:
                    return CreateSqlType(TypeMapping[TypeCode.String], size.HasValue ? size.Value : STRING_SIZE);//GetBestType(System.Data.SqlDbType.NVarChar, size);
            }
            throw Error.UnexpectedTypeCode(typeCode);
        }

        internal override SQLTYPE GetBestLargeType(SQLTYPE type)
        {
            //UNDONE:GetBestLargeType
            return type;
        }

        internal override SQLTYPE MostPreciseTypeInFamily(SQLTYPE type)
        {
            //UNDONE:GetBestLargeType
            return type;
        }

        internal override SQLTYPE ReturnTypeOfFunction(SqlFunctionCall functionCall)
        {
            return null;
        }

        protected virtual bool IsDecimalType(DBType sqlDbType)
        {
            if (Equals(sqlDbType, TypeMapping[TypeCode.Decimal]))
                return true;
            return false;
        }

        protected virtual SQLTYPE GetBestType(DBType targeType, int? size)
        {
            var item = CreateSqlType(targeType, size.GetValueOrDefault());
            //item.SqlDbType = (Enum)(targeType as object);
            //item.Size = size;
            return item;
        }

        private static DBType decimalType;


        protected virtual DBType DecimalType
        {
            get
            {
                if (Equals(decimalType, default(DBType)))
                {
                    var obj = Enum.Parse(typeof(DBType), "Decimal");
                    if (obj == null)
                        throw new Exception("Do not recognize the decimal type.");

                    decimalType = (DBType)obj;
                }
                return decimalType;
            }
        }
    }
}