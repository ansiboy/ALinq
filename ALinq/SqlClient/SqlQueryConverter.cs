namespace ALinq.SqlClient
{
    class SqlQueryConverter : QueryConverter
    {
        public SqlQueryConverter(IDataServices services, ITypeSystemProvider provider, Translator translator, SqlFactory sql)
            : base(services, provider, translator, sql)
        {
        }
    }
}