using System;
using System.Collections.Generic;
using System.Data.Common;
using ALinq;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;
using IDbConnection = System.Data.IDbConnection;

namespace Test
{
    [TestClass]
    public class SQL2005Test : SqlTest
    {
        private static TextWriter writer;
        private static readonly string SQL2005 = string.Format(@"Data Source=WIN-RG88VACR8GJ\SQLEXPRESS;Initial Catalog=Northwind;Integrated Security=True", NorthwindDatabase.DB_HOST);

        public override NorthwindDatabase CreateDataBaseInstace()
        {
            writer = Console.Out;
            var builder = new SqlConnectionStringBuilder(SQL2005);
            return new Sql2005Northwind(new SqlConnection(builder.ToString())) { Log = writer };
        }

        protected DbConnection CreateConnection()
        {
            return new SqlConnection(SQL2005);
        }

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            //"Data Source=.\SQLEXPRESS;AttachDbFilename=D:\NORTHWND.MDF;Integrated Security=True;Connect Timeout=30;User Instance=True"
            writer = new StreamWriter("c:/SQL2005.txt", false);
            var builder = new SqlConnectionStringBuilder(SQL2005);
            var database = new Sql2005Northwind(new SqlConnection(builder.ToString())) { Log = writer };
            if (!database.DatabaseExists())
            {
                database.CreateDatabase();
                database.Connection.Close();
            }
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
            var context = new ALinq.DataContext(connstr, typeof(ALinq.SqlClient.Sql2005Provider));
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
