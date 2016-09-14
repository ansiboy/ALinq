using System;
using System.Collections;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SQLite;
using System.Linq;
using ALinq.Access;
using ALinq.Mapping;
//using MySql.Data.MySqlClient;
using FirebirdSql.Data.FirebirdClient;
using IBM.Data.DB2;
using MySql.Data.MySqlClient;
using NorthwindDemo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;
using ALinq;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using Npgsql;

namespace Test
{


    /// <summary>
    ///This is a test class for DataContextTest and is intended
    ///to contain all DataContextTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataContextTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Connection
        ///</summary>
        public void TestConnection(DataContext context, DbConnection connection)
        {
            //IProvider provider = context.Provider;
            //Assert.IsTrue(connection == provider.Connection);

            //connection.Open();
            //var tran = connection.BeginTransaction();
            //context.Transaction = tran;
            //tran.Commit();
            //connection.Close();
            //Assert.IsTrue(context.Connection.State == ConnectionState.Closed);
        }

        //[TestMethod]
        //public void SQLiteConnectionTest()
        //{
        //    var builder = new SQLiteConnectionStringBuilder()
        //                      {
        //                          DataSource = "C:/Northwind.db",
        //                      };
        //    var conn = new SQLiteConnection(builder.ToString());
        //    var context = new DataContext(conn, typeof(SQLiteProvider));
        //    TestConnection(context, conn);
        //}

        [TestMethod]
        public void AccessConnectionTest()
        {
            var builder = new OleDbConnectionStringBuilder()
            {
                DataSource = "C:/Northwind.mdb",
                Provider = "Microsoft.Jet.OLEDB.4.0",
            };
            var conn = new OleDbConnection(builder.ToString());
            var context = new AccessNorthwind(conn);
            TestConnection(context, conn);
        }

        //[TestMethod]
        //public void MySqlConnectionTest()
        //{
        //    var builder = new MySqlConnectionStringBuilder()
        //    {
        //        Server = "localhost",
        //        Port = 3306,
        //        UserID = "root",
        //        Password = "test",
        //        Database = "Northwind",
        //    };
        //    var conn = new MySqlConnection(builder.ToString());
        //    var context = new DataContext(conn, typeof(MySqlProvider));
        //    TestConnection(context, conn);
        //}

        [TestMethod]
        public void OracleConnectionTest()
        {
            var builder = new OracleConnectionStringBuilder()
            {
                DataSource = "localhost",
                UserID = "Northwind",
                Password = "test",
                PersistSecurityInfo = true,
            };
            var conn = new OracleConnection(builder.ToString());
            var context = new OracleNorthwind(conn);
            TestConnection(context, conn);
        }

        //[TestMethod]
        //public void SQLiteTransactionTest()
        //{
        //    var builder = new SQLiteConnectionStringBuilder()
        //    {
        //        DataSource = "C:/Northwind.db",
        //    };
        //    var conn = new SQLiteConnection(builder.ToString());
        //    conn.Open();
        //    var tran = conn.BeginTransaction();
        //    var context = new DataContext(conn, typeof(SQLiteProvider));
        //    context.Transaction = tran;
        //    conn.Close();
        //}

        [TestMethod]
        public void TestAutoClose()
        {
            var builder = new OleDbConnectionStringBuilder()
            {
                DataSource = "C:/Northwind.mdb",
                Provider = "Microsoft.Jet.OLEDB.4.0",
            };
            var conn = new OleDbConnection(builder.ToString());
            conn.Open();
            using (var context = new DataContext(conn, typeof(AccessDbProvider)))
            {
                var customers = context.GetTable<Customer>();
                var q = from customer in customers
                        select customer;
                q.ToList();
                Assert.IsTrue(context.Connection.State == ConnectionState.Open);
                foreach (var customer in q)
                {
                    IList items = customer.Orders.ToList();
                    Assert.IsTrue(items.Count >= 0);
                    Assert.IsTrue(context.Connection.State == ConnectionState.Open);
                }
            }
            conn.Close();

            using (var context = new DataContext(conn, typeof(AccessDbProvider)))
            {
                var customers = context.GetTable<Customer>();
                var q = from customer in customers
                        select customer;
                Assert.IsTrue(context.Connection.State == ConnectionState.Closed);
                foreach (var customer in q)
                {
                    IList items = customer.Orders.ToList();
                    Assert.IsTrue(items.Count >= 0);
                    Assert.IsTrue(context.Connection.State == ConnectionState.Open);
                }
                Assert.IsTrue(context.Connection.State == ConnectionState.Closed);
            }
        }

        [TestMethod]
        public void InsertMethod()
        {
            var contact = new SqlMethod();
            var db = new AccessNorthwind("C:/Northwind.mdb");
            db.SqlMethods.InsertOnSubmit(contact);
            db.SubmitChanges();
            Assert.AreEqual(NorthwindDatabase.SqlMethodAction.InsertContact, db.SqlAction);
        }

        [TestMethod]
        public void UpdateMethod()
        {
            //var db = new AccessNorthwind("C:/Northwind.mdb");
            //Assert.IsNotNull(db.Services.Model.GetMetaType(typeof(SqlMethod)).Table.UpdateMethod);
        }

        [TestMethod]
        public void DeleteMethod()
        {
            //var db = new AccessNorthwind("C:/Northwind.mdb");
            //Assert.IsNotNull(db.Services.Model.GetMetaType(typeof(SqlMethod)).Table.DeleteMethod);
        }

        [TestMethod]
        public void Attach()
        {
            var db = new AccessNorthwind("C:/Northwind.mdb");
            var shipperID = db.Shippers.Max(o => o.ShipperID);
            var shipper = new Shipper { ShipperID = shipperID };
            db.Shippers.Attach(shipper, true);
            var changeSet = db.GetChangeSet();

            Exception exc = null;
            var db2 = new AccessNorthwind("C:/Northwind.mdb");

            try
            {
                db2.Shippers.Attach(shipper);
            }
            catch (Exception e)
            {
                exc = e;
            }
            Assert.IsNotNull(exc);
            Console.WriteLine(exc.GetType());

            //db.Shippers.Detach(shipper);
            db2.Shippers.Attach(shipper);
        }

        [TestMethod]
        public void ObjectIdentity()
        {
            var db = new AccessNorthwind("C:/Northwind.mdb") { Log = Console.Out };
            var shipperID = db.Shippers.Max(o => o.ShipperID);
            Assert.IsTrue(shipperID != 0);
            var shipper1 = db.Shippers.Where(o => o.ShipperID == shipperID).Single();
            var shipper2 = db.Shippers.OrderByDescending(o => o.ShipperID).First();
            Assert.AreSame(shipper1, shipper2);
        }


        [TestMethod]
        public void TranslateTest()
        {
            var db = new AccessNorthwind("C:/Northwind.mdb") { Log = Console.Out };
            var command = db.GetCommand(db.Products.OrderBy(o => o.ProductName));
            command.Connection.Open();
            Console.WriteLine(command.CommandText);
            var reader = command.ExecuteReader();

            var items = db.Translate<P>(reader).ToList();
            foreach (var item in items)
            {
                Console.WriteLine(item.ProductName + " " + item.ID);
            }

            command.Connection.Close();
        }

        [ALinq.Mapping.Table(Name = "Products")]
        public class P
        {
            ALinq.Link<decimal> _UnitPrice;

            [Column]
            public string ProductName
            {
                get;
                set;
            }

            [Column(Name = "ProductID", IsPrimaryKey = true)]
            public int ID
            {
                get;
                set;
            }

            [Column(Storage = "_UnitPrice")]
            public decimal UnitPrice
            {
                get { return this._UnitPrice.Value; }
                set { this._UnitPrice.Value = value; }
            }
        }

        [TestMethod]
        public void SetDataContextTest()
        {
            var db = new AccessNorthwind("C:/Northwind.mdb");
            var order = db.Orders.First();
            Assert.IsNotNull(order.DataContext);

            var customer = db.Customers.SingleOrDefault(o => o.CustomerID == "MCSFT");
            if (customer != null)
            {
                Assert.AreEqual(db, customer.DataContext);

                db.Customers.DeleteOnSubmit(customer);
                db.SubmitChanges();
                //Assert.AreEqual(null, customer.DataContext);
            }

            var newCustomer = new Customer
            {
                CustomerID = "MCSFT",
                CompanyName = "Microsoft",
                ContactName = "John Doe",
                ContactTitle = "Sales Manager",
                Address = "1 Microsoft Way",
                City = "Redmond",
                Region = "WA",
                PostalCode = "98052",
                Country = "USA",
                Phone = "(425) 555-1234",
                Fax = string.Empty,
            };
            db.Customers.InsertOnSubmit(newCustomer);
            db.SubmitChanges();

            Assert.AreEqual(db, newCustomer.DataContext);

            db.Contacts.Delete(o => true);
            db.Log = Console.Out;
            if (db.Contacts.Count() == 0)
            {
                var contact = new Contact();
                db.Contacts.InsertOnSubmit(contact);
                db.SubmitChanges();
                Assert.AreEqual(db, contact.DataContext);
            }

            if (db.Contacts.OfType<FullContact>().Count() == 0)
            {
                var contact = new FullContact();
                db.Contacts.InsertOnSubmit(contact);
                db.SubmitChanges();
                Assert.AreEqual(db, contact.DataContext);
                Assert.IsTrue(db.Contacts.OfType<FullContact>().Count() > 0);
            }

            db = new AccessNorthwind("C:/Northwind.mdb");
            var c = db.Contacts.First();
            Assert.AreEqual(db, c.DataContext);

            var f = db.Contacts.OfType<FullContact>().First();
            Assert.AreEqual(db, f.DataContext);
        }

        //[TestMethod]
        //public void SerializerTest()
        //{
        //    var serializer = new Newtonsoft.Json.JsonSerializer();
        //    var db = new AccessNorthwind("C:/Northwind.mdb");
        //    var order = db.Orders.FirstOrDefault();
        //    Debug.Assert(order != null);

        //    var sb = new StringBuilder();
        //    serializer.Serialize(new StringWriter(sb), order);

        //    order = (Order)serializer.Deserialize(new StringReader(sb.ToString()), typeof(Order));
        //    Assert.IsNotNull(order.Customer);
        //    Console.WriteLine(order.Customer.CompanyName);
        //    Console.WriteLine(order.Customer.ContactName);
        //}

        [TestMethod]
        public void GetChangeText()
        {
            var db = new AccessNorthwind("C:/Northwind.mdb");

        }


        [TestMethod]
        public void TableCountTest()
        {
            DbConnection conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Northwind.mdb");
            //var count = OutputRowsCount(conn);

            conn = new SQLiteConnection(@"Data Source=C:\Northwind.db3;Version=3;");

            conn = new MySqlConnection(@"server=localhost;password=test;User Id=root;Persist Security Info=True;database=northwind");

            conn = new FbConnection(@"data source=vpc1;initial catalog=northwind;user id=sysdba;password=masterkey");

            conn = new OracleConnection("DATA SOURCE=vpc1;PERSIST SECURITY INFO=True;USER ID=NORTHWIND;password=test");

            conn = new Oracle.DataAccess.Client.OracleConnection("DATA SOURCE=vpc1;PERSIST SECURITY INFO=True;USER ID=NORTHWIND;password=test");

            conn = new NpgsqlConnection("HOST=localhost;User ID=postgres;PASSWORD=test;DATABASE=northwind");

            conn = new DB2Connection(@"DataBase=SAMPLE;USER ID=db2admin;Password=test;Server=VPC1");
            conn.Open();
            try
            {
                OutputRowsCount(conn);

            }
            finally
            {
                conn.Close();
            }
        }

        static int OutputRowsCount(DbConnection conn)
        {

            string tableType;
            string typeColumn = "TABLE_TYPE";

            //注意：不区分大小写
            switch (conn.GetType().Name)
            {
                case "FbConnection":
                case "OleDbConnection":
                case "SQLiteConnection":
                case "DB2Connection":
                    tableType = "TABLE";
                    break;

                case "MySqlConnection":
                case "NpgsqlConnection":
                    tableType = "BASE TABLE";
                    break;

                case "OracleConnection":
                    tableType = "User";
                    typeColumn = "TYPE";
                    break;
                default:
                    return 10000;
            }

            var dt = conn.GetSchema("Tables");
            var rows = dt.Rows.Cast<DataRow>().Where(o => o[typeColumn] as string == tableType);
            return rows.Count();


        }

#if DEBUG
        [TestMethod]
        public void TrialTest()
        {
            var context = new DataContext("C:/Northwind.mdb") { Log = Console.Out };
            var table1 = context.GetTable<Product>();
            var count1 = table1.GetMetaTablesCount();
            Assert.IsTrue(count1 > 0);

            context = new DataContext("C:/Northwind.mdb");
            var table2 = context.GetTable<Employee>();
            var count2 = table2.GetMetaTablesCount();
            Assert.IsTrue(count2 > count1);
        }



        [TestMethod]
        public void LinkTest()
        {
            var context = new DataContext("C:/Northwind.mdb") { Log = Console.Out };
            var p = context.GetTable<P>().First();
            p.ProductName = p.ProductName + "t";
            //p.UnitPrice = 10;
            context.SubmitChanges();
            //var v = context.GetTable<P>().First().UnitPrice;

        }
#endif
    }
}
