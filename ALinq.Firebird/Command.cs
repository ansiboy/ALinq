using System.Data;
using System.Data.Common;

namespace ALinq.Firebird
{
    //class Command : DbCommand
    //{
    //    private readonly DbCommand source;
    //    private DbConnection connection;
    //    //private ParameterCollection parameters;

    //    public Command(DbCommand source, DbConnection connection)
    //    {
    //        this.source = source;
    //        this.connection = connection;
    //        //this.parameters = new ParameterCollection(source.Parameters);
    //    }

    //    public override void Prepare()
    //    {
    //        source.Prepare();
    //    }

    //    public override string CommandText
    //    {
    //        get
    //        {
    //            return source.CommandText;
    //        }
    //        set
    //        {
    //            source.CommandText = value;
    //        }
    //    }

    //    public override int CommandTimeout
    //    {
    //        get { return source.CommandTimeout; }
    //        set { source.CommandTimeout = value; }
    //    }

    //    public override CommandType CommandType
    //    {
    //        get { return source.CommandType; }
    //        set { source.CommandType = value; }
    //    }

    //    public override UpdateRowSource UpdatedRowSource
    //    {
    //        get { return source.UpdatedRowSource; }
    //        set { source.UpdatedRowSource = value; }
    //    }

    //    protected override DbConnection DbConnection
    //    {
    //        get { return connection; }
    //        set { connection = value; }
    //    }

    //    protected override DbParameterCollection DbParameterCollection
    //    {
    //        get
    //        {
    //            return source.Parameters;
    //        }
    //    }

    //    protected override DbTransaction DbTransaction
    //    {
    //        get
    //        {
    //            return source.Transaction;
    //        }
    //        set
    //        {
    //            source.Transaction = value;
    //        }
    //    }

    //    public override bool DesignTimeVisible
    //    {
    //        get { return source.DesignTimeVisible; }
    //        set { source.DesignTimeVisible = value; }
    //    }

    //    public DbCommand Source
    //    {
    //        get { return source; }
    //    }

    //    public override void Cancel()
    //    {
    //        source.Cancel();
    //    }

    //    protected override DbParameter CreateDbParameter()
    //    {
    //        var sourceParameter = source.CreateParameter();
    //        return sourceParameter;
    //    }

    //    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    //    {
    //        var sourceReader = source.ExecuteReader(behavior);
    //        return new DataReader(sourceReader);
    //    }

    //    public override int ExecuteNonQuery()
    //    {
    //        return source.ExecuteNonQuery();
    //    }

    //    public override object ExecuteScalar()
    //    {
    //        return source.ExecuteScalar();
    //    }
    //}
}