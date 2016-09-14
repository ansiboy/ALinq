using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ALinq.SqlClient;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace ALinq.Oracle.Odp
{
    internal class OracleDataTypeProvider : SqlClient.DbTypeProviderBase<OracleDataType, OracleDbType>
    {
        private static Dictionary<TypeCode, OracleDbType> typeMapping;
        protected override Dictionary<TypeCode, OracleDbType> TypeMapping
        {
            get
            {
                if (typeMapping == null)
                {
                    typeMapping = new Dictionary<TypeCode, OracleDbType>()
                                      {
                                          { TypeCode.Boolean, OracleDbType.Int32 },
                                          { TypeCode.Byte, OracleDbType.Byte },
                                          { TypeCode.Char, OracleDbType.Char },
                                          { TypeCode.DateTime, OracleDbType.Date },
                                          { TypeCode.Decimal, OracleDbType.Decimal },
                                          { TypeCode.Double, OracleDbType.Double },
                                          { TypeCode.Int16, OracleDbType.Int16 },
                                          { TypeCode.Int32, OracleDbType.Int32},
                                          { TypeCode.Int64, OracleDbType.Int64 },
                                          { TypeCode.Object, OracleDbType.Blob },
                                          { TypeCode.SByte, OracleDbType.Int16 },
                                          { TypeCode.Single, OracleDbType.Single },
                                          { TypeCode.String, OracleDbType.Varchar2 },
                                          { TypeCode.UInt16, OracleDbType.Decimal },
                                          { TypeCode.UInt32, OracleDbType.Decimal },
                                          { TypeCode.UInt64, OracleDbType.Decimal },
                                      };

                }
                return typeMapping;
            }

        }

        private static Dictionary<Type, OracleDataType> nativeDbTypeMapping;
        Dictionary<Type, OracleDataType> NativeDbTypeMapping
        {
            get
            {
                if (nativeDbTypeMapping == null)
                {
                    nativeDbTypeMapping = new Dictionary<Type, OracleDataType>
                                              {
                                                  { typeof (OracleRefCursor), new OracleDataType(TypeMapping, OracleDbType.RefCursor) }
                                              };
                }
                return nativeDbTypeMapping;
            }
        }

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
        private static OracleDataTypeProvider instance;

        internal override OracleDataType GuidType
        {
            get
            {
                if (guidType == null)
                {
                    guidType = new OracleDataType(TypeMapping, OracleDbType.Raw, 16);
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

            //if (stype == "RAW(16)")
            //    return GuidType;

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

            int size = SqlDataType<OracleDbType>.NULL_VALUE;
            int scale = SqlDataType<OracleDbType>.NULL_VALUE;
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
            OracleDbType type;
            switch (typeName)
            {
                case "CURSOR":
                case "REF CURSOR":
                    type = OracleDbType.RefCursor;
                    break;

                //Begin 数值
                case "FLOAT":
                    type = OracleDbType.Single;
                    break;

                case "INTEGER":
                    type = OracleDbType.Int32;
                    break;

                case "SMALLINT":
                    type = OracleDbType.Int16;
                    break;

                case "DECIMAL":
                case "NUMBER":
                    type = OracleDbType.Decimal;
                    if (scale == SqlDataType<OracleDbType>.NULL_VALUE)
                        scale = 0;
                    break;

                case "REAL":
                    type = OracleDbType.Double;
                    break;

                case "DOUBLE PRECISION":
                    type = OracleDbType.Double;
                    break;
                //END

                //Begin 时间
                case "TIMESTAMP WITH LOCAL TIME ZONE":
                    type = OracleDbType.TimeStampLTZ;
                    break;

                case "TIMESTAMP WITH TIME ZONE":
                    type = OracleDbType.TimeStampTZ;
                    break;

                case "INTERVAL DAY TO SECOND":
                    type = OracleDbType.IntervalDS;
                    break;

                case "INTERVAL YEAR TO MONTH":
                    type = OracleDbType.IntervalYM;
                    break;
                //End

                //Begin 字符串
                case "CHAR":
                    type = OracleDbType.Char;
                    break;

                case "NCHAR":
                case "NATIONAL CHARACTER":
                case "NATIONAL CHAR":
                    type = OracleDbType.NChar;
                    break;

                case "NATIONAL CHARACTER VARYING":
                case "NATIONAL CHAR VARYING":
                case "NVARCHAR":
                case "NCHAR VARYING":
                    type = OracleDbType.NVarchar2;
                    break;

                case "VARCHAR2":
                case "VARCHAR":
                    type = OracleDbType.Varchar2;
                    break;
                case "LONG":
                    type = OracleDbType.Long;
                    break;
                //END

                //Begin 二进制
                case "LONG RAW":
                    type = OracleDbType.LongRaw;
                    break;
                case "RAW":
                    if (size == 16)
                        return this.GuidType;

                    type = OracleDbType.Raw;
                    break;

                //End
                default:
                    if (!Enum.GetNames(typeof(OracleDbType)).Select(n => n.ToUpperInvariant())
                                           .Contains(typeName.ToUpperInvariant()))
                    {
                        throw SqlClient.Error.InvalidProviderType(typeName);
                    }
                    type = (OracleDbType)Enum.Parse(typeof(OracleDbType), typeName, true);
                    break;
            }
            OracleDataType item;
            //if (size == SqlDataType<OracleDbType>.NULL_VALUE)
            //    item = CreateSqlType(type);
            //else if (scale == SqlDataType<OracleDbType>.NULL_VALUE)
            //    item = CreateSqlType(type, size);
            //else
            if (size != SqlDataType<OracleDbType>.NULL_VALUE && scale != SqlDataType<OracleDbType>.NULL_VALUE)
                item = CreateSqlType(type, size, scale);
            else if (size != SqlDataType<OracleDbType>.NULL_VALUE)
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
            //if (type2 == typeof(Guid))
            if(type == GuidType)
                return DBConvert.ChangeType(value, typeof(byte[]));

            var closestRuntimeType = type.GetClosestRuntimeType();
            if (closestRuntimeType == typeof(OracleDecimal))
            {
                return new OracleDecimal(Convert.ToDecimal(value));
            }
            if (type2 == typeof(char) && closestRuntimeType == typeof(decimal))
            {
                value = DBConvert.ChangeType(value, typeof(int));
                return DBConvert.ChangeType(value, closestRuntimeType);
            }
            return closestRuntimeType == type2 ? value : DBConvert.ChangeType(value, closestRuntimeType);
        }

        const int NULL_VALUE = SqlDataType<OracleDbType>.NULL_VALUE;

        internal override OracleDataType From(Type type, int? size, int? scale)
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
                        return SqlTypes[TypeCode.Int64]; //SqlTypeSystem.Create(System.Data.SqlDbType.BigInt);
                    }
                    if (type == typeof(OracleBFile))
                    {
                        return CreateSqlType(OracleDbType.BFile, size.GetValueOrDefault(NULL_VALUE));
                    }

                    if (type == typeof(OracleBlob))
                        return CreateSqlType(OracleDbType.Blob, size.GetValueOrDefault(NULL_VALUE));

                    if (type == typeof(OracleClob))
                        return CreateSqlType(OracleDbType.Clob, size.GetValueOrDefault(NULL_VALUE));

                    if (type == typeof(OracleDate))
                        return CreateSqlType(OracleDbType.Date);

                    if (type == typeof(OracleDecimal))
                        return CreateSqlType(OracleDbType.Decimal, size.GetValueOrDefault(NULL_VALUE),
                                             scale.GetValueOrDefault(NULL_VALUE));

                    if (type == typeof(OracleIntervalDS))
                        return CreateSqlType(OracleDbType.IntervalDS);

                    if (type == typeof(OracleIntervalYM))
                        return CreateSqlType(OracleDbType.IntervalYM);

                    //if (type == typeof(OracleRef))
                    //    return CreateSqlType(OracleDbType.Ref);

                    if (type == typeof(OracleRefCursor))
                        return CreateSqlType(OracleDbType.RefCursor);

                    if (type == typeof(OracleString))
                        return CreateSqlType(OracleDbType.Varchar2, size.GetValueOrDefault(NULL_VALUE));

                    if (type == typeof(OracleTimeStamp))
                        return CreateSqlType(OracleDbType.TimeStamp);

                    if (type == typeof(OracleTimeStampLTZ))
                        return CreateSqlType(OracleDbType.TimeStampLTZ);

                    if (type == typeof(OracleTimeStampTZ))
                        return CreateSqlType(OracleDbType.TimeStampTZ);

                    if (type == typeof(OracleXmlType))
                        return CreateSqlType(OracleDbType.XmlType);

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
                            Debug.Assert(r.Precision == DEFAULT_DECIMAL_PRECISION);
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
            throw SqlClient.Error.UnexpectedTypeCode(typeCode);
        }
    }
}
