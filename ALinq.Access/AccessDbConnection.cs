using System;
using System.Collections;
using System.Data.OleDb;
using System.Data;
using System.Data.Common;

namespace ALinq.Access
{
    #region Connection & Command

    internal class AccessDbConnection : DbConnection
    {
        private readonly OleDbConnection source;

        public AccessDbConnection(OleDbConnection source)
            //: base(source)
        {
            this.source = source;
        }

        public AccessDbConnection(string connectionString)
           // : base(new OleDbConnection(connectionString))
        {
            source = new OleDbConnection(connectionString);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var value = source.BeginTransaction(isolationLevel);
            return new AccessDbTransaction(value);
        }

        public override void Close()
        {
            source.Close();
        }

        public override void ChangeDatabase(string databaseName)
        {
            source.ChangeDatabase(databaseName);
        }

        public override void Open()
        {
            source.Open();
        }

        public override string ConnectionString
        {
            get { return source.ConnectionString; }
            set { source.ConnectionString = value; }
        }

        public override string Database
        {
            get { return source.Database; }
        }

        public override ConnectionState State
        {
            get { return source.State; }
        }

        public override string DataSource
        {
            get { return source.DataSource; }
        }

        public override string ServerVersion
        {
            get { return source.ServerVersion; }
        }

        public DbConnection Source
        {
            get { return source; }
        }

        protected override DbCommand CreateDbCommand()
        {
            return new AccessDbCommand(source);
        }

        public override int GetHashCode()
        {
            return (source != null ? source.GetHashCode() : 0);
        }

        public override bool Equals(object obj)
        {
            if (obj is AccessDbConnection)
                return source == ((AccessDbConnection)obj).source;
            if (obj is DbConnection)
                return source == obj;
            return base.Equals(obj);
        }

        protected override void Dispose(bool disposing)
        {
            source.Dispose();
            base.Dispose(disposing);
        }
    }

    internal class AccessDbCommand : DbCommand
    {
        private readonly DbCommand source;
        //private DbConnection connection;

        public AccessDbCommand(DbConnection connection)
        {
            source = connection.CreateCommand();
            //this.connection = connection;
        }

        public override void Prepare()
        {
            source.Prepare();
        }

        public override string CommandText
        {
            get { return source.CommandText; }
            set { source.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return source.CommandTimeout; }
            set { source.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return source.CommandType; }
            set { source.CommandType = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return source.UpdatedRowSource; }
            set { source.UpdatedRowSource = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return source.Connection; }
            set { source.Connection = value; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return source.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return source.Transaction; }
            set
            {
                source.Transaction = value != null ? ((AccessDbTransaction)value).Source : null;
            }
        }

        public override bool DesignTimeVisible
        {
            get { return source.DesignTimeVisible; }
            set { source.DesignTimeVisible = value; }
        }

        public override void Cancel()
        {
            source.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return source.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var reader = source.ExecuteReader();
            return new AccessDbReader(reader);
        }

        public override int ExecuteNonQuery()
        {
            return source.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            return source.ExecuteScalar();
        }

        protected override void Dispose(bool disposing)
        {
            source.Dispose();
            base.Dispose(disposing);
        }
    }

    internal class AccessDbTransaction : DbTransaction
    {
        //private DbConnection connection;
        private readonly OleDbTransaction source;

        internal AccessDbTransaction(OleDbTransaction source)
        {
            this.source = source;
        }
        public override void Commit()
        {
            source.Commit();
        }

        public override void Rollback()
        {
            source.Rollback();
        }

        protected override DbConnection DbConnection
        {
            get
            {
                return new AccessDbConnection(source.Connection);
            }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return source.IsolationLevel; }
        }

        internal OleDbTransaction Source
        {
            get { return source; }
        }

        public static implicit operator OleDbTransaction(AccessDbTransaction transaction)
        {
            return transaction.Source;
        }

        public static implicit operator AccessDbTransaction(OleDbTransaction transaction)
        {
            return new AccessDbTransaction(transaction);
        }

        public static bool operator ==(AccessDbTransaction value1, AccessDbTransaction value2)
        {
            return value1.Source == value2.Source;
        }



        public static bool operator !=(AccessDbTransaction value1, AccessDbTransaction value2)
        {
            return !(value1.Source == value2.Source);
        }

        public bool Equals(AccessDbTransaction obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.source, source);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(AccessDbTransaction)) return false;
            return Equals((AccessDbTransaction)obj);
        }

        public override int GetHashCode()
        {
            return (source != null ? source.GetHashCode() : 0);
        }

        protected override void Dispose(bool disposing)
        {
            source.Dispose();
            base.Dispose(disposing);
        }
    }

    #endregion

    //--------------------------------------------------
    //说明：用于对数据类型进行转换，如果使用OleDbReader，
    //某些操作会出现类型不匹配的错误。例如：
    //Sum(int)要求返回的是int类型，但实际上数据库返回的是
    //Double类型，因此，必须进行类型的转换。
    internal class AccessDbReader : DbDataReader
    {
        private readonly DbDataReader source;

        public AccessDbReader(DbDataReader source)
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
            var value = GetValue(ordinal);
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
            //return source.GetInt64(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            var value = GetValue(ordinal);
            return Convert.ToDateTime(value);
            //return source.GetDateTime(ordinal);
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
            var value = source.GetValue(ordinal);
            return Convert.ToDecimal(value);
            //return source.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            var value = source.GetValue(ordinal);
            return Convert.ToDouble(value);
            //return source.GetDouble(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            var value = source.GetValue(ordinal);
            return Convert.ToSingle(value);
            //return source.GetFloat(ordinal);
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

        protected override void Dispose(bool disposing)
        {
            source.Dispose();
            base.Dispose(disposing);
        }
    }
}
