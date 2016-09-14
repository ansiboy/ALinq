using System;
using System.Collections;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;

namespace ALinq.SQLite
{
    //--------------------------------------------------
    //说明：通过Reader实现类型转换。
    internal class DataReader : DbDataReader
    {
        private DbDataReader source;

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
            get { return source.Depth; }
        }

        public override bool IsClosed
        {
            get { return source.IsClosed; }
        }

        public override int RecordsAffected
        {
            get { return source.RecordsAffected; }
        }

        public override int FieldCount
        {
            get { return source.FieldCount; }
        }

        public override object this[int ordinal]
        {
            get
            {
                var value = source[ordinal];
                var type = GetFieldType(ordinal);
                if (type.IsValueType && value == DBNull.Value)
                    return 0;
                return value;
            }
        }

        public override object this[string name]
        {
            get { return source[name]; }
        }

        public override bool HasRows
        {
            get { return source.HasRows; }
        }

        public override bool GetBoolean(int ordinal)
        {
            var value = GetValue(ordinal);
            return Convert.ToBoolean(value);
        }

        public override byte GetByte(int ordinal)
        {
            var value = GetValue(ordinal);
            return Convert.ToByte(value);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return source.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            var result = GetValue(ordinal);
            return Convert.ToChar(result);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return source.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override Guid GetGuid(int ordinal)
        {
            return source.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            var value = source.GetValue(ordinal);
            return Convert.ToInt16(value);
        }

        public override int GetInt32(int ordinal)
        {
            var value = source.GetValue(ordinal);
            return Convert.ToInt32(value);
        }

        public override long GetInt64(int ordinal)
        {
            var value = source.GetValue(ordinal);
            return Convert.ToInt64(value);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            var value = source.GetValue(ordinal);//source.GetString(ordinal);
            return Convert.ToDateTime(value);
        }

        public override string GetString(int ordinal)
        {
            var value = source.GetValue(ordinal);
            return Convert.ToString(value);
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
            var value = source.GetValue(ordinal);
            return Convert.ToDecimal(value);
        }

        public override double GetDouble(int ordinal)
        {
            var value = source.GetValue(ordinal);
            return Convert.ToDouble(value);
        }

        public override float GetFloat(int ordinal)
        {
            var value = source.GetValue(ordinal);
            return Convert.ToSingle(value);
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
