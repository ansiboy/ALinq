﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ALinq.Dynamic
{
    [DebuggerDisplay("{ToString()}")]
    public abstract class DataRow : System.Data.IDataRecord
    {


        private int current;
        private Dictionary<string, int> indexes;
        private object[] values;
        private string[] names;

        protected DataRow()
        {
            indexes = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            values = new object[GetFieldCount()];
            names = new string[GetFieldCount()];
        }

        public override string ToString()
        {
            PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                 .Where(o => o.Name != "Item")
                                                 .ToArray();
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < props.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(props[i].Name);
                sb.Append("=");
                sb.Append(props[i].GetValue(this, null));
            }
            sb.Append("}");
            return sb.ToString();
        }

        int IDataRecord.GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        IDataReader IDataRecord.GetData(int i)
        {
            throw new NotImplementedException();
        }

        bool IDataRecord.IsDBNull(int i)
        {
            var value = GetValue(i);
            return value == DBNull.Value;
        }

        int IDataRecord.GetOrdinal(string name)
        {
            return indexes[name];
        }

        string IDataRecord.GetString(int i)
        {

            return (string)values[i];
        }

        bool IDataRecord.GetBoolean(int i)
        {
            return (bool)values[i];
        }

        byte IDataRecord.GetByte(int i)
        {
            return (byte)values[i];
        }

        long IDataRecord.GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            throw new NotImplementedException();
        }

        char IDataRecord.GetChar(int i)
        {
            return (char)values[i];
        }

        long IDataRecord.GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            throw new NotImplementedException();
        }

        Guid IDataRecord.GetGuid(int i)
        {
            return (Guid)values[i];
        }

        short IDataRecord.GetInt16(int i)
        {
            return (short)values[i];
        }

        int IDataRecord.GetInt32(int i)
        {
            return (int)values[i];
        }

        long IDataRecord.GetInt64(int i)
        {
            return (long)values[i];
        }

        string IDataRecord.GetName(int i)
        {
            return names[i];
        }

        string IDataRecord.GetDataTypeName(int i)
        {
            var value = values[i];
            return value.GetType().Name;
        }

        Type IDataRecord.GetFieldType(int i)
        {
            return (Type)values[i];
        }

        float IDataRecord.GetFloat(int i)
        {
            return (float)values[i];
        }

        DateTime IDataRecord.GetDateTime(int i)
        {
            return (DateTime)values[i];
        }

        decimal IDataRecord.GetDecimal(int i)
        {
            return (decimal)values[i];
        }

        double IDataRecord.GetDouble(int i)
        {
            return (double)values[i];
        }

        object IDataRecord.this[int i]
        {
            get
            {
                return values[i];
            }
        }

        int IDataRecord.FieldCount
        {
            get { return this.GetFieldCount(); }
        }

        protected void SetValue(string name, object value)
        {
            if (!indexes.ContainsKey(name))
            {
                indexes.Add(name, indexes.Count);
            }

            values[indexes.Count - 1] = value;
            names[indexes.Count - 1] = name;
        }

        public object GetValue(int i)
        {
            return values[i];
        }

        public abstract int GetFieldCount();

        object IDataRecord.this[string name]
        {
            get
            {
                if (indexes.ContainsKey(name) == false)
                    throw Error.NotPropertyOrField(name);

                int index = indexes[name];
                return values[index];
            }
        }
    }
}