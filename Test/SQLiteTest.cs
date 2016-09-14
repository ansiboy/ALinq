using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class SQLiteTest : SqlTest
    {
        private const string DbFileName = "C:/Northwind.db3";
        private const string LogFileName = "C:/SQLite.txt";

        private static TextWriter writer;


        public override NorthwindDatabase CreateDataBaseInstace()
        {
            var xmlMapping = XmlMappingSource.FromStream(typeof(SQLiteTest).Assembly.GetManifestResourceStream("Test.Northwind.SQLite.map"));
            writer = Console.Out;
            return new SQLiteNorthwind(DbFileName) { Log = writer };//xmlMapping
        }

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            //var type = typeof(SQLiteTest);
            //var path = type.Module.FullyQualifiedName;
            //var filePath = Path.GetDirectoryName(path) + @"\ALinq.SQLite.lic";
            //File.Copy(@"E:\ALinqs\ALinq1.8\Test\ALinq.SQLite.lic", filePath);

            var xmlMapping = XmlMappingSource.FromStream(typeof(SQLiteTest).Assembly.GetManifestResourceStream("Test.Northwind.SQLite.map"));
            writer = new StreamWriter(LogFileName, false);
            var database = new SQLiteNorthwind(DbFileName) { Log = writer };//, xmlMapping
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

        public static Func<SQLiteNorthwind, string, IQueryable<Customer>> CustomersByCity =
            ALinq.CompiledQuery.Compile((SQLiteNorthwind db, string city) =>
                                               from c in db.Customers
                                               where c.City == city
                                               select c);
        public static Func<SQLiteNorthwind, string, Customer> CustomersById =
            ALinq.CompiledQuery.Compile((SQLiteNorthwind db, string id) =>
                                               Enumerable.Where(db.Customers, c => c.CustomerID == id).First());

        [TestMethod]
        public void StoreAndReuseQuery()
        {
            var customers = CustomersByCity((SQLiteNorthwind)db, "London").ToList();
            Assert.IsTrue(customers.Count() > 0);

            var id = customers.First().CustomerID;
            var customer = CustomersById((SQLiteNorthwind)db, id);
            Assert.AreEqual("London", customer.City);
        }
    }
}