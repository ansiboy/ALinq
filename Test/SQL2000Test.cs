using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using ALinq;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;
using IDbConnection = System.Data.IDbConnection;

namespace Test
{

    [TestClass]
    public class SQL2000Test : SqlTest
    {
        private static TextWriter writer;

        public override NorthwindDatabase CreateDataBaseInstace()
        {
            var xmlMapping = XmlMappingSource.FromUrl("Northwind.SQL2000.map");
            writer = Console.Out;
            var constr = @"Data Source=vpc1\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa;PASSWORD=test";
            var builder = new SqlConnectionStringBuilder(constr);
            return new Sql2000Northwind(new SqlConnection(builder.ToString())) { Log = writer };//, xmlMapping
        }

        protected DbConnection CreateConnection()
        {
            return new SqlConnection("Data Source=vpc1;Initial Catalog=Northwind;User ID=sa;PASSWORD=test");
        }

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            var type = typeof(SQLiteTest);
            var path = type.Module.FullyQualifiedName;
            var filePath = Path.GetDirectoryName(path) + @"\Northwind.SQL2000.map";
            File.Copy(@"E:\ALinqs\ALinq1.8\Test\Northwind.SQL2000.map", filePath);

            //writer = Console.Out;//new StreamWriter("c:/SQL2000.txt", false);
            //var builder = new SqlConnectionStringBuilder(@"Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True");
            //var database = new Sql2000Northwind(new SqlConnection(builder.ToString())) { Log = writer };
            //if (!database.DatabaseExists())
            //{
            //    database.CreateDatabase();
            //    database.Connection.Close();
            //}
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            writer.Flush();
            writer.Close();
        }

        [TestMethod]
        public void StringConnect()
        {
            var connstr = CreateConnection().ConnectionString;
            var context = new DataContext(connstr, typeof(ALinq.SqlClient.Sql2000Provider));
            context.Connection.Open();
            context.Connection.Close();
        }
        // Call the stored procedure.
        //[TestMethod]
        public void StoredProcedures()
        {
            //Northwnd db = new Northwnd(@"c:\northwnd.mdf");

            ISingleResult<Sql2005Northwind.CustomersByCityResult> result =
                ((Sql2005Northwind)db).CustomersByCity("London");

            foreach (Sql2005Northwind.CustomersByCityResult cust in result)
            {
                Console.WriteLine("CustID={0}; City={1}", cust.CustomerID,
                    cust.City);
            }
        }



        
    }
}
