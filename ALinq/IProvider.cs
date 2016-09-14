using System;
using System.Collections;
using System.Data.Common;
using System.IO;
using System.Linq.Expressions;
using ALinq.Mapping;

namespace ALinq
{
    internal interface IProvider : IDisposable
    {
        // Methods
        void ClearConnection();

        ICompiledQuery Compile(Expression query);
        /**/
        void CreateDatabase();
        bool DatabaseExists();
        void DeleteDatabase();

        //bool CreateTable(MetaTable metaTable);

        IExecuteResult Execute(Expression query);
        DbCommand GetCommand(Expression query);
        string GetQueryText(Expression query);
        void Initialize(IDataServices dataServices, object connection);
        IMultipleResults Translate(DbDataReader reader);
        IEnumerable Translate(Type elementType, DbDataReader reader);

        // Properties
        int CommandTimeout { get; set; }
        DbConnection Connection { get; }
        TextWriter Log { get; set; }
        DbTransaction Transaction { get; set; }
    }

    interface IProviderExtend
    {
        void CreateTable(MetaTable metaTable);
        //void CreateColumn(MetaDataMember metaMember);

        bool TableExists(MetaTable metaTable);
        void DeleteTable(MetaTable metaTable);
        void CreateForeignKeys(MetaTable metaTable);
    }

    internal interface ICompiledQuery
    {
        IExecuteResult Execute(IProvider provider, object[] arguments);
    }
}
