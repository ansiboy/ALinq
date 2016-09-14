using System.Data.Common;
using System.Data.OleDb;
using System.Reflection;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    //[License("ansiboy", "80F24A2987BF4910EAAD69CC8C5F9E39")]
    [Provider(typeof(ALinq.SQLite.SQLiteProvider))]
    public class SQLiteNorthwind : NorthwindDatabase
    {
        public SQLiteNorthwind(string fileName)
            : base(fileName)
        {
        }

        public SQLiteNorthwind(OleDbConnection connection)
            : base(connection)
        {

        }

        public SQLiteNorthwind(string conn, XmlMappingSource mapping)
            : base(conn, mapping)
        {

        }
    }
}