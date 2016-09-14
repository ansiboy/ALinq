using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace ALinq.Oracle.Odp
{
    internal class OdpDataReader : DbDataReader
    {
        private readonly OracleDataReader source;

        public OdpDataReader(OracleDataReader source)
        {
            this.source = source;
        }

        public override void Close()
        {
            source.Close();
        }

        public override DataTable GetSchemaTable()
        {
            return source.GetSchemaTable();
        }

        public override bool NextResult()
        {
            return source.NextResult();
        }

        public override bool Read()
        {
            return source.Read();
        }

        public override int Depth
        {
            get
            {
                return source.Depth;
            }
        }

        public override bool IsClosed
        {
            get
            {
                return source.IsClosed;
            }
        }

        public override int RecordsAffected
        {
            get
            {
                return source.RecordsAffected;
            }
        }

        public override int FieldCount
        {
            get
            {
                return source.FieldCount;
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                return source[ordinal];
            }
        }

        public override object this[string name]
        {
            get
            {
                return source[name];
            }
        }

        public override bool HasRows
        {
            get
            {
                //return source.HasRows;
                var bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;
                return (bool)source.GetType().InvokeMember("HasRows", bf, null, source, null);
            }
        }

        public override bool GetBoolean(int ordinal)
        {
            var value = source.GetDecimal(ordinal);
            return Convert.ToBoolean(value);
        }

        public override byte GetByte(int ordinal)
        {
            return source.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return source.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            var value = source.GetValue(ordinal);
            if (value is string)
                return ((string)value)[0];
            if (value is ValueType)
                return Convert.ToChar(value);

            return source.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return source.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override Guid GetGuid(int ordinal)
        {
            var result = GetValue(ordinal);
            var property = result.GetType().GetProperty("Value");
            if (property != null)
                result = property.GetValue(result, null);

            if (result is Byte[])
                result = new Guid((Byte[])result);
            else if (result is string)
                result = new Guid((string)result);

            return (Guid)result;
            //return source.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            var result = source.GetDecimal(ordinal);
            return Convert.ToInt16(result);
        }

        public override int GetInt32(int ordinal)
        {
            //OracleNumber number = source.GetOracleNumber(ordinal);
            //return Convert.ToInt32(number.Value);
            var result = source.GetDecimal(ordinal);
            return Convert.ToInt32(result);
        }

        public override long GetInt64(int ordinal)
        {
            //OracleNumber number = source.GetOracleNumber(ordinal);
            //return Convert.ToInt64(number.Value);
            var result = source.GetDecimal(ordinal);
            return Convert.ToInt64(result);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            var dateTime = source.GetOracleDate(ordinal);
            return dateTime.Value;
            //return source.GetDateTime(ordinal);
        }

        public override string GetString(int ordinal)
        {
            var result = source.GetOracleValue(ordinal);

            if (result == null)
            {
                return null;
            }

            var property = result.GetType().GetProperty("Value");
            if (property != null)
                result = property.GetValue(result, null);

            return (string)result;
        }

        public override object GetValue(int ordinal)
        {
            var result = source.GetOracleValue(ordinal);

            if (result != null)
            {
                var property = result.GetType().GetProperty("Value");
                if (property != null)
                    return property.GetValue(result, null);
            }

            if (result is OracleBinary)
                return ((OracleBinary)result).Value;

            if (result is OracleBlob)
                return ((OracleBlob)result).Value;

            if (result is OracleClob)
                return ((OracleClob)result).Value;

            return result;
        }

        public override int GetValues(object[] values)
        {
            var result = source.GetOracleValues(values);
            return result;
        }

        public override bool IsDBNull(int ordinal)
        {
            var value = source.IsDBNull(ordinal);
            return value;
        }

        public override decimal GetDecimal(int ordinal)
        {
            /*
            var number = source.GetOracleDecimal(ordinal);
            //不能使用number.Value，会发生溢出错误。
            return Convert.ToDecimal(number.ToDouble());
            */
            var value = source.GetDecimal(ordinal);
            return value;
        }

        public override double GetDouble(int ordinal)
        {
            var number = source.GetOracleDecimal(ordinal);
            return number.ToDouble();//Convert.ToDouble(number.ToString());
        }

        public override float GetFloat(int ordinal)
        {
            var number = source.GetOracleDecimal(ordinal);
            //return Convert.ToSingle(number.Value);
            return number.ToSingle();
        }

        public override string GetName(int ordinal)
        {
            return source.GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return source.GetOrdinal(name);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return source.GetDataTypeName(ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            return source.GetFieldType(ordinal);
        }

        public override IEnumerator GetEnumerator()
        {
            return source.GetEnumerator();
        }
    }
}