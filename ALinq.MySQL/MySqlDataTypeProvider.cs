using System;
using System.Collections.Generic;
using System.Linq;
using ALinq.SqlClient;
using MySql.Data.MySqlClient;

namespace ALinq.MySQL
{
    internal class MySqlDataTypeProvider : DbTypeProviderBase<MySqlDataType, MySqlDbType>
    {
        private static Dictionary<TypeCode, MySqlDbType> typeMapping;


        protected override Dictionary<TypeCode, MySqlDbType> TypeMapping
        {
            get
            {
                if (typeMapping == null)
                {
                    typeMapping = new Dictionary<TypeCode, MySqlDbType>
                              {
                                  {TypeCode.Boolean, MySqlDbType.Bit},
                                  {TypeCode.Byte, MySqlDbType.UByte},
                                  {TypeCode.Char, MySqlDbType.String},
                                  {TypeCode.DateTime, MySqlDbType.DateTime},
                                  {TypeCode.Decimal, MySqlDbType.Decimal},
                                  {TypeCode.Double, MySqlDbType.Double},
                                  {TypeCode.Int16, MySqlDbType.Int16},
                                  {TypeCode.Int32, MySqlDbType.Int32},
                                  {TypeCode.Int64, MySqlDbType.Int64},
                                  {TypeCode.Object, MySqlDbType.Binary},
                                  {TypeCode.SByte,MySqlDbType.Byte},
                                  {TypeCode.Single,MySqlDbType.Float},
                                  {TypeCode.String,MySqlDbType.VarChar},
                                  {TypeCode.UInt16,MySqlDbType.UInt16},
                                  {TypeCode.UInt32,MySqlDbType.UInt32},
                                  {TypeCode.UInt64,MySqlDbType.UInt64},
                              };
                }
                return typeMapping;
            }
        }

        protected override MySqlDataType Parse(string typeName, int size, int scale, string[] options)
        {
            MySqlDbType type;
            var unsigned = options.Contains("UNSIGNED");
            var zeroFill = options.Contains("ZEROFILL");
            var unicode = options.Contains("UNICODE");
            switch (typeName.ToUpper())
            {
                case "BIT":
                case "BOOL":
                case "BOOLEAN":
                    type = MySqlDbType.Bit;
                    break;
                case "TINYINT":
                    type = unsigned ? MySqlDbType.UByte : MySqlDbType.Byte;
                    break;
                case "SMALLINT":
                    type = unsigned ? MySqlDbType.UInt16 : MySqlDbType.Int16;
                    break;
                case "MEDIUMINT":
                    type = unsigned ? MySqlDbType.UInt24 : MySqlDbType.Int24;
                    break;
                case "INT":
                case "INTEGER":
                    type = unsigned ? MySqlDbType.UInt32 : MySqlDbType.Int32;
                    break;
                case "BIGINT":
                    type = unsigned ? MySqlDbType.UInt64 : MySqlDbType.Int64;
                    break;
                case "DOUBLE":
                    type = MySqlDbType.Double;
                    break;
                case "FLOAT":
                    type = MySqlDbType.Float;
                    break;
                case "DECIMAL":
                case "DEC":
                case "NUMERIC":
                case "FIXED":
                    type = MySqlDbType.Decimal;
                    break;

                //string type
                case "CHAR":
                    type = MySqlDbType.String;
                    break;
                case "NCHAR":
                    type = MySqlDbType.String;
                    unicode = true;
                    break;
                case "VARCHAR":
                    type = MySqlDbType.VarChar;
                    break;
                case "NVARCHAR":
                    type = MySqlDbType.VarChar;
                    unicode = true;
                    break;
                case "BINARY":
                    type = MySqlDbType.Binary;
                    break;
                case "VARBINARY":
                    type = MySqlDbType.VarBinary;
                    break;
                case "TINYBLOB":
                    type = MySqlDbType.Blob;
                    size = 255;
                    break;
                case "TINYTEXT":
                    type = MySqlDbType.TinyText;
                    break;
                case "BLOB":
                    type = MySqlDbType.Blob;
                    break;
                case "TEXT":
                    type = MySqlDbType.Text;
                    break;
                case "MEDIUMBLOB":
                    type = MySqlDbType.MediumBlob;
                    break;
                case "MEDIUMTEXT":
                    type = MySqlDbType.MediumText;
                    break;
                case "LONGBLOB":
                    type = MySqlDbType.LongBlob;
                    break;
                case "LONGTEXT":
                    type = MySqlDbType.LongText;
                    break;
                case "DATETIME":
                    type = MySqlDbType.DateTime;
                    break;
                case "DATE":
                    type = MySqlDbType.Date;
                    break;
                case "TIMESTAMP":
                    type = MySqlDbType.Timestamp;
                    break;
                case "TIME":
                    type = MySqlDbType.Time;
                    break;
                case "YEAR":
                    type = MySqlDbType.Year;
                    break;
                //date type
                default:
                    if (!Enum.GetNames(typeof(MySqlDbType)).Select(n => n.ToUpperInvariant())
                                                           .Contains(typeName.ToUpperInvariant()))
                    {
                        throw SqlClient.Error.InvalidProviderType(typeName);
                    }
                    type = (MySqlDbType)Enum.Parse(typeof(MySqlDbType), typeName, true);
                    break;

            }


            var item = CreateSqlType(type);

            if (type == MySqlDbType.Decimal)
            {
                item.Precision = size == SqlDataType<MySqlDbType>.NULL_VALUE
                                                     ? SqlDataType<MySqlDbType>.DEFAULT_DECIMAL_PRECISION : size;
                item.Scale = scale == SqlDataType<MySqlDataType>.NULL_VALUE
                                                     ? SqlDataType<MySqlDbType>.DEFAULT_DECIMAL_SCALE : scale;
            }
            else if (item.IsString)
            {
                item.Size = size == SqlDataType<MySqlDbType>.NULL_VALUE ? SqlDataType<MySqlDbType>.STRING_SIZE : size;
            }
            else
            {
                item.Size = size;
                item.Scale = scale;
            }

            //item.Unsigned = unsigned;
            item.ZeroFill = zeroFill;
            item.SetUnicodeType(unicode);
            return item;
        }

        internal override MySqlDataType From(Type type)
        {
            if (type.IsGenericType)
            {
                var argTypes = type.GetGenericArguments();
                if (argTypes.Length == 1)
                {
                    if (argTypes[0] == typeof(MySql.Data.Types.MySqlDateTime))
                    {
                        var dataType = new MySqlDataType(TypeMapping, MySqlDbType.DateTime, argTypes[0]);
                        return dataType;
                    }
                }
            }
            return base.From(type);
        }

    }
}
