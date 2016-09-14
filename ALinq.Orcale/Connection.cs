using System.Data;
using System.Data.Common;

namespace ALinq.Oracle
{
    //class Connection : DbConnection
    //{
    //    private readonly DbConnection source;

    //    public Connection(DbConnection source)
    //    {
    //        this.source = source;
    //    }

    //    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    //    {
    //        var sourceTran = source.BeginTransaction(isolationLevel);
    //        return sourceTran;
    //    }

    //    public override void ChangeDatabase(string databaseName)
    //    {
    //        source.ChangeDatabase(databaseName);
    //    }

    //    public override void Close()
    //    {
    //        source.Close();
    //    }

    //    public override string ConnectionString
    //    {
    //        get
    //        {
    //            return source.ConnectionString;
    //        }
    //        set
    //        {
    //            source.ConnectionString = value;
    //        }
    //    }

    //    protected override DbCommand CreateDbCommand()
    //    {
    //        var sourceCommand = source.CreateCommand();
    //        return new Command(sourceCommand, this);
    //    }

    //    public override string DataSource
    //    {
    //        get { return source.DataSource; }
    //    }

    //    public override string Database
    //    {
    //        get { return source.Database; }
    //    }

    //    public override void Open()
    //    {
    //        source.Open();
    //    }

    //    public override string ServerVersion
    //    {
    //        get { return source.ServerVersion; }
    //    }

    //    public override ConnectionState State
    //    {
    //        get { return source.State; }
    //    }

    //    public DbConnection Source
    //    {
    //        get { return source; }
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        if (obj is Connection)
    //            return source == ((Connection)obj).source;
    //        if (obj is DbConnection)
    //            return source == obj;
    //        return base.Equals(obj);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return Source.GetHashCode();
    //    }
    //}
}
