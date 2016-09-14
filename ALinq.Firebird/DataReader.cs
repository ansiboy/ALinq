using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace ALinq.Firebird
{
    internal class DataReader : DbDataReader
    {
        private readonly DbDataReader source;

        public DataReader(DbDataReader source)
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
            return source.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return source.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override Guid GetGuid(int ordinal)
        {
            var value = GetValue(ordinal);
            if (value == null)
                return Guid.Empty;

            if (value is Byte[])
            {
                var buffer = (Byte[])value;
                source.GetBytes(ordinal, 0, buffer, 0, 16);
                return new Guid(buffer);
            }
            if (value is string)
                return new Guid((string) value);

            return Guid.Empty;
        }

        public override short GetInt16(int ordinal)
        {
           return source.GetInt16(ordinal);
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
            return source.GetInt64(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            //OracleDateTime dateTime = source.GetOracleDateTime(ordinal);
            //return dateTime.Value;
            return source.GetDateTime(ordinal);
        }

        public override string GetString(int ordinal)
        {
            return source.GetString(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return source.GetValue(ordinal);
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
            return source.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            //var number = source.GetOracleNumber(ordinal);
            //return Convert.ToDouble(number.Value);
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
    }
}