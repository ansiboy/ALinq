using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ALinq.SqlClient;
using System.Data.OracleClient;

namespace ALinq.Oracle
{
    internal class OracleDataTypeProvider : DbTypeProviderBase<OracleDataType, OracleType>
    {
        private static readonly object objLock = new object();
        private static Dictionary<TypeCode, OracleType> typeMapping;
        protected override Dictionary<TypeCode, OracleType> TypeMapping
        {
            get
            {
                if (typeMapping == null)
                {
                    lock (objLock)
                    {
                        typeMapping = new Dictionary<TypeCode, OracleType>()
                                          {
                                              { TypeCode.Boolean, OracleType.Int16 },
                                              { TypeCode.Byte, OracleType.Int16 },
                                              { TypeCode.Char, OracleType.Char },
                                              { TypeCode.DateTime, OracleType.DateTime },
                                              { TypeCode.Decimal, OracleType.Number },
                                              { TypeCode.Double, OracleType.Double },
                                              { TypeCode.Int16, OracleType.Int16 },
                                              { TypeCode.Int32, OracleType.Int32 },
                                              { TypeCode.Int64, OracleType.Number },
                                              { TypeCode.Object, OracleType.Blob },
                                              { TypeCode.SByte, OracleType.Int16 },
                                              { TypeCode.Single, OracleType.Float },
                                              { TypeCode.String, OracleType.VarChar },
                                              { TypeCode.UInt16, OracleType.UInt16 },
                                              { TypeCode.UInt32, OracleType.UInt32 },
                                              { TypeCode.UInt64, OracleType.Number }
                                          };

                    }
                }
                return typeMapping;
            }
        }

        private static OracleDataTypeProvider instance;
        internal static OracleDataTypeProvider Instance
        {
            get
            {
                if (instance == null)
                    instance = new OracleDataTypeProvider();
                return instance;
            }
        }

        private static OracleDataType guidType;
        internal override OracleDataType GuidType
        {
            get
            {
                if (guidType == null)
                {
                    guidType = new OracleDataType(TypeMapping, OracleType.Raw, 16);
                    //guidType.SqlDbType = OracleDbType.Raw;
                    //guidType.Size = 16;
                }
                return guidType;
            }
        }

        internal override OracleDataType Parse(string stype)
        {
            //    switch (stype)
            //    {
            //        case "REF CURSOR":
            //            return CreateSqlType(OracleDbType.RefCursor);
            //        case "TIMESTAMP WITH LOCAL TIME ZONE":
            //            return CreateSqlType(OracleDbType.TimeStampLTZ);
            //        case "TIMESTAMP WITH TIME ZONE":
            //            return CreateSqlType(OracleDbType.TimeStampTZ);
            //        case "LONG RAW":
            //            return CreateSqlType(OracleDbType.LongRaw);
            //        case "INTERVAL DAY TO SECOND":
            //            return CreateSqlType(OracleDbType.IntervalDS);
            //        case "INTERVAL YEAR TO MONTH":
            //            return CreateSqlType(OracleDbType.IntervalYM);
            //    }
            //    return base.Parse(stype);
            //if (stype == null)
            //    throw Error.ArgumentNull(stype);
            //stype = Regex.Replace(stype, @"\s+", " ");

            if (stype == null)
                throw Error.ArgumentNull(stype);

            stype = Regex.Replace(stype, @"\s+", " ");
            stype = stype.ToUpper();

            string typeName;
            string s = null;
            string str3 = null;
            int index = stype.IndexOf('(');
            //int num2 = stype.IndexOf(' ');
            int length;

            if (index != -1)
                length = index;
            else
                length = -1;

            if (length == -1)
            {
                typeName = stype;
                length = stype.Length;
            }
            else
            {
                typeName = stype.Substring(0, length);
            }
            typeName = typeName.Trim();
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

            //var options = new string[] { };
            //if (startIndex < stype.Length - 1)
            //{
            //    var strOptions = stype.Substring(startIndex);
            //    if (strOptions != string.Empty)
            //    {
            //        options = Regex.Split(strOptions, @"\s+")
            //                       .Where(o => !string.IsNullOrEmpty(o)).ToArray();
            //    }
            //}

            int size = SqlDataType<OracleType>.NULL_VALUE;
            int scale = SqlDataType<OracleType>.NULL_VALUE;
            if (s != null)
            {
                size = int.Parse(s, CultureInfo.InvariantCulture);
            }

            if (str3 != null)
            {
                scale = int.Parse(str3, CultureInfo.InvariantCulture);
            }
            var item = Parse(typeName, size, scale, null);
            if (item == null)
                throw SqlClient.Error.InvalidProviderType(stype);
            return item;
        }

        protected override OracleDataType Parse(string typeName, int size, int scale, string[] options)
        {
            OracleType type;
            switch (typeName)
            {
                case "CURSOR":
                case "REF CURSOR":
                case "REFCURSOR":
                    type = OracleType.Cursor;
                    break;

                //Begin 数值
                case "FLOAT":
                    type = OracleType.Float;
                    break;

                case "INTEGER":
                    type = OracleType.Int32;
                    break;

                case "SMALLINT":
                    type = OracleType.Int16;
                    break;

                case "DECIMAL":
                case "NUMBER":
                    type = OracleType.Number;
                    if (scale == SqlDataType<OracleType>.NULL_VALUE)
                        scale = 0;
                    break;

                case "REAL":
                    type = OracleType.Double;
                    break;

                case "DOUBLE PRECISION":
                    type = OracleType.Double;
                    break;
                //END

                //Begin 时间
                case "TIMESTAMP WITH LOCAL TIME ZONE":
                    type = OracleType.TimestampLocal;
                    break;

                case "TIMESTAMP WITH TIME ZONE":
                    type = OracleType.TimestampWithTZ;
                    break;

                case "INTERVAL DAY TO SECOND":
                    type = OracleType.IntervalDayToSecond;
                    break;

                case "INTERVAL YEAR TO MONTH":
                    type = OracleType.IntervalYearToMonth;
                    break;

                case "DATE":
                    type = OracleType.DateTime;
                    break;
                //End

                //Begin 字符串
                case "CHAR":
                    type = OracleType.Char;
                    break;

                case "NCHAR":
                case "NATIONAL CHARACTER":
                case "NATIONAL CHAR":
                    type = OracleType.NChar;
                    break;

                case "NATIONAL CHARACTER VARYING":
                case "NATIONAL CHAR VARYING":
                case "NVARCHAR":
                case "NCHAR VARYING":
                    type = OracleType.NVarChar;
                    break;

                case "VARCHAR2":
                case "VARCHAR":
                    type = OracleType.VarChar;
                    break;

                case "NVARCHAR2":
                    type = OracleType.NVarChar;
                    break;

                case "LONG":
                    type = OracleType.LongVarChar;
                    break;

                case "XMLTYPE":
                    type = OracleType.VarChar;
                    break;
                //END

                //Begin 二进制
                case "LONG RAW":
                    type = OracleType.LongRaw;
                    break;
                case "RAW":
                    if (size == 16)
                        return this.GuidType;

                    type = OracleType.Raw;
                    break;
                //End
                default:
                    if (!Enum.GetNames(typeof(OracleType)).Select(n => n.ToUpperInvariant())
                                           .Contains(typeName.ToUpperInvariant()))
                    {
                        throw SqlClient.Error.InvalidProviderType(typeName);
                    }
                    type = (OracleType)Enum.Parse(typeof(OracleType), typeName, true);
                    break;
            }
            OracleDataType item;
            //if (size == SqlDataType<OracleDbType>.NULL_VALUE)
            //    item = CreateSqlType(type);
            //else if (scale == SqlDataType<OracleDbType>.NULL_VALUE)
            //    item = CreateSqlType(type, size);
            //else
            if (size != SqlDataType<OracleType>.NULL_VALUE && scale != SqlDataType<OracleType>.NULL_VALUE)
                item = CreateSqlType(type, size, scale);
            else if (size != SqlDataType<OracleType>.NULL_VALUE)
                item = CreateSqlType(type, size);
            else
                item = CreateSqlType(type);
            //if (type == OracleDbType.NChar || type == OracleDbType.NClob || type == OracleDbType.NVarchar2)
            //    item.IsUnicodeType = true;

            Debug.Assert(item.IsRuntimeOnlyType == false);
            return item;
        }

        protected override object GetParameterValue(IProviderType type, object value)
        {
            if (value == null)
                return DBNull.Value;

            var type2 = value.GetType();
            var closestRuntimeType = type.GetClosestRuntimeType();
            if (closestRuntimeType == typeof(OracleNumber))
            {
                return new OracleNumber(Convert.ToDecimal(value));
            }
            return closestRuntimeType == type2 ? value : DBConvert.ChangeType(value, closestRuntimeType);
        }

        internal override OracleDataType ReturnTypeOfFunction(SqlFunctionCall functionCall)
        {
            switch (functionCall.Name)
            {
                case "COSH":
                    return base.CreateSqlType(OracleType.Number);
                case "SUBSTR":
                    return From(typeof(string));
            }
            return base.ReturnTypeOfFunction(functionCall);
        }
    }
}
