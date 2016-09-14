using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using ALinq.Mapping;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class FirebirdTest : SqlTest
    {
        protected static DbConnection CreateConnection()
        {
            var builder = new FbConnectionStringBuilder()
            {
                Database = "C:/Northwind.FDB",
                DataSource = "localhost",
                ServerType = FbServerType.Default,
                UserID = "SYSDBA",
                Password = "masterkey"
            };
            return new FbConnection(builder.ToString());
        }


        public override NorthwindDemo.NorthwindDatabase CreateDataBaseInstace()
        {
            var writer = Console.Out;
            //var xmlMapping = XmlMappingSource.FromUrl("Northwind.Firebird.map");
            var xmlMapping = XmlMappingSource.FromStream(GetType().Assembly.GetManifestResourceStream("Test.Northwind.Firebird.map"));
            //return new FirebirdNorthwind("C:/Northwind.FDB") { Log = writer };
            return new FirebirdNorthwind("User=SYSDBA;Password=masterkey;Database=Northwind;DataSource=localhost;ServerType=0") { Log = writer };
        }

        [TestMethod]
        public void TransactionTest()
        {
            using (var conn = new FbConnection("User=SYSDBA;Password=masterkey;Database=Northwind;DataSource=localhost;ServerType=0"))
            {
                var dc = new FirebirdNorthwind(conn.ConnectionString);
                conn.Open();                
                
                var tran = conn.BeginTransaction();
                dc.Transaction = tran;

                try
                {
                    //do something
                    dc.SubmitChanges();
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                }
            }

            
        }

        [TestMethod]
        public override void CreateDatabase()
        {
            //var conn = CreateDataBaseInstace().Connection;
            //FbConnection.DropDatabase(conn.ConnectionString);
            base.CreateDatabase();
        }

        //[ClassInitialize]
        //public static void Initialize(TestContext testContext)
        //{
            //var type = typeof(SQLiteTest);
            //var path = type.Module.FullyQualifiedName;
            //var filePath = Path.GetDirectoryName(path) + @"\fbembed.dll";
            //File.Copy(@"E:\ALinqs\ALinq1.8\ConsoleApplication\bin\Debug\fbembed.dll", filePath);
            //filePath = Path.GetDirectoryName(path) + @"\ib_util.dll";
            //File.Copy(@"E:\ALinqs\ALinq1.8\ConsoleApplication\bin\Debug\ib_util.dll", filePath);
            //filePath = Path.GetDirectoryName(path) + @"\icudt30.dll";
            //File.Copy(@"E:\ALinqs\ALinq1.8\ConsoleApplication\bin\Debug\icudt30.dll", filePath);
            //filePath = Path.GetDirectoryName(path) + @"\icuin30.dll";
            //File.Copy(@"E:\ALinqs\ALinq1.8\ConsoleApplication\bin\Debug\icuin30.dll", filePath);
            //filePath = Path.GetDirectoryName(path) + @"\icuuc30.dll";
            //File.Copy(@"E:\ALinqs\ALinq1.8\ConsoleApplication\bin\Debug\icuuc30.dll", filePath);

            //filePath = Path.GetDirectoryName(path) + @"\ALinq.Firebird.lic";
            //File.Copy(@"E:\ALinqs\ALinq1.8\Test\ALinq.Firebird.lic", filePath);

            //filePath = Path.GetDirectoryName(path) + @"\Northwind.Firebird.map";
            //File.Copy(@"E:\ALinqs\ALinq1.8\Test\Northwind.Firebird.map", filePath);

            //writer = new StreamWriter(LogFileName, false);
            //var database = new SQLiteNorthwind(DbFileName) { Log = writer };
            //if (!database.DatabaseExists())
            //{
            //    database.CreateDatabase();
            //    database.Connection.Close();
            //}

            //var db = new AccessNorthwind("C:/Nrothwind.mdb");
            //foreach (var metaTable in db.Mapping.GetTables())
            //    db.CreateTable(metaTable);

            //foreach (var metaTable in db.Mapping.GetTables())
            //    db.CreateForeignKeys(metaTable);
        //}

        //存储过程
        //[TestMethod]
        //public void Procedure_AddCategory()
        //{
        //    db.Log = Console.Out;
        //    ((FirebirdNorthwind)db).AddCategory("category", "description");
        //}


        //1、标量返回
        //[TestMethod]
        //public void Procedure_GetCustomersCountByRegion()
        //{
        //    var regions = db.Regions.ToList();
        //    var groups = db.Customers.GroupBy(o => o.Region)
        //                   .Select(g => new { Count = g.Count(), Region = g.Key }).ToArray();
        //    foreach (var group in groups)
        //    {
        //        if (group.Region == null)
        //            continue;
        //        var count1 = group.Count;
        //        var count2 = ((FirebirdNorthwind)db).GetCustomersCountByRegion(group.Region);
        //        Assert.AreEqual(count1, count2);
        //    }
        //}

        //2、单一结果集返回
        //[TestMethod]
        //public void Procedure_GetCustomersByCity()
        //{
        //    var groups = db.Customers.GroupBy(o => o.City).Select(o => new { o.Key, Count = o.Count() }).ToList();
        //    foreach (var group in groups)
        //    {
        //        var result = ((FirebirdNorthwind)db).GetCustomersByCity(group.Key);
        //        Assert.AreEqual(result.Count(), group.Count);
        //    }
        //}

        //3.多个可能形状的单一结果集
        //[TestMethod]
        //public void Procedure_SingleRowset_MultiShape()
        //{
        //    var count = db.Customers.Where(o => o.Region == "WA").Count();
        //    //返回全部Customer结果集
        //    var result = ((FirebirdNorthwind)db).SingleRowset_MultiShape(1);
        //    var shape1 = result.GetResult<Customer>();
        //    foreach (var compName in shape1)
        //    {
        //        Console.WriteLine(compName.CompanyName);
        //    }
        //    //Assert.AreEqual(count, shape1.Count());

        //    //返回部分Customer结果集
        //    result = ((FirebirdNorthwind)db).SingleRowset_MultiShape(2);
        //    var shape2 = result.GetResult<PartialCustomersSetResult>();
        //    foreach (var con in shape2)
        //    {
        //        Console.WriteLine(con.ContactName);
        //    }
        //    //Assert.AreEqual(count, shape2.Count());

        //}

    }
}
