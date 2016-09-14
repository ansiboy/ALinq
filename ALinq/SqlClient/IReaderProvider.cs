namespace ALinq.SqlClient
{
    internal interface IReaderProvider : IProvider
    {
        // Properties
        IConnectionManager ConnectionManager { get; }
        IDataServices Services { get; }
    }
}