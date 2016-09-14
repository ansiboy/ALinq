using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;

namespace ALinq.Oracle
{
    internal class DataReader : DbDataReader
    {
        private readonly OracleDataReader source;

        public DataReader(OracleDataReader source)
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
                return source.HasRows;
            }
        }

        public override bool GetBoolean(int ordinal)
        {
            //OracleNumber number = source.GetOracleNumber(ordinal);
            //return Convert.ToBoolean(number.Value);
            var result = source.GetDecimal(ordinal);
            return Convert.ToBoolean(result);
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
            return source.GetString(ordinal)[0];
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return source.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override Guid GetGuid(int ordinal)
        {
            var buffer = new Byte[16];
            source.GetBytes(ordinal, 0, buffer, 0, 16);
            return new Guid(buffer);
        }

        public override short GetInt16(int ordinal)
        {
            var result = source.GetDecimal(ordinal);
            return Convert.ToInt16(result);
        }

        public override int GetInt32(int ordinal)
        {
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
            //OracleDateTime dateTime = source.GetOracleDateTime(ordinal);
            //return dateTime.Value;
            var result = source.GetDateTime(ordinal);
            return result;
        }

        public override string GetString(int ordinal)
        {
            return source.GetString(ordinal);
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

            if (result is OracleBFile)
                return ((OracleBFile)result).Value;

            if (result is OracleBinary)
                return ((OracleBinary)result).Value;

            if (result is OracleLob)
                return ((OracleLob)result).Value;


            return result;
        }

        public override int GetValues(object[] values)
        {
            return source.GetValues(values);
        }

        public override bool IsDBNull(int ordinal)
        {
            return source.IsDBNull(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            //var number = source.GetOracleNumber(ordinal);
            ////不能使用number.Value，会发生溢出错误。
            //return Convert.ToDecimal(number.ToString());
            return source.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            //var number = ((OracleDataReader)source).GetOracleNumber(ordinal);
            //return Convert.ToDouble(number.ToString());
            return source.GetDouble(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            //var number = source.GetOracleNumber(ordinal);
            //return Convert.ToSingle(number.Value);
            return source.GetFloat(ordinal);
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

        public byte[] GetOracleBFile(int i)
        {
            return source.GetOracleBFile(i).Value as byte[];
        }
    }
}