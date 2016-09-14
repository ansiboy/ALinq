#define FREE
#if !FREE
using System;
using ALinq;
using ALinq.Mapping;
using ALinq.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{

    [TestClass]
    public class LicenseTest
    {
#if DEBUG
        [TestMethod]
        public void AttributeLicense()
        {
            DataContext context = new AccessDataContext { Log = Console.Out };
            Assert.IsNotNull(((SqlProvider)context.Provider).License);
            var license = (ALinqLicenseProvider.LicFileLicense)((SqlProvider)context.Provider).License;
            Assert.IsFalse(license.IsTrial);

            context = new SQLiteDataContext();
            Assert.IsNotNull(((SqlProvider)context.Provider).License);
            license = (ALinqLicenseProvider.LicFileLicense)((SqlProvider)context.Provider).License;
            Assert.IsFalse(license.IsTrial);

            context = new MySqlDataContext();
            Assert.IsNotNull(((SqlProvider)context.Provider).License);
            license = (ALinqLicenseProvider.LicFileLicense)((SqlProvider)context.Provider).License;
            Assert.IsFalse(license.IsTrial);

            context = new OracleDataContext();
            Assert.IsNotNull(((SqlProvider)context.Provider).License);
            license = (ALinqLicenseProvider.LicFileLicense)((SqlProvider)context.Provider).License;
            Assert.IsFalse(license.IsTrial);

            context = new OracleOdpDataContext();
            Assert.IsNotNull(((SqlProvider)context.Provider).License);
            license = (ALinqLicenseProvider.LicFileLicense)((SqlProvider)context.Provider).License;
            Assert.IsFalse(license.IsTrial);

            //context = new DataContext()

        }

        [TestMethod]
        public void AttributeLicense1()
        {
            DataContext context;
            var mapping = XmlMappingSource.FromXml(
@"<Database Name=""Northwind"" Provider=""ALinq.Firebird.FirebirdProvider, ALinq.Firebird, Version=1.2.7.0, Culture=Neutral, PublicKeyToken=2b23f34316d38f3a"" xmlns=""http://schemas.microsoft.com/linqtosql/mapping/2007"">
</Database>
");
            context = new MyDataContext("", mapping);
            Assert.IsNotNull(((SqlProvider)context.Provider).License);
            var license = (ALinqLicenseProvider.LicFileLicense)((SqlProvider)context.Provider).License;
            Assert.IsFalse(license.IsTrial);

            mapping = XmlMappingSource.FromXml(
@"<Database Name=""Northwind"" Provider=""ALinq.Access.AccessDbProvider, ALinq.Access, Version=1.7.9.0, Culture=neutral, PublicKeyToken=2b23f34316d38f3a"" xmlns=""http://schemas.microsoft.com/linqtosql/mapping/2007"">
</Database>
");
            context = new MyDataContext("", mapping);
            Assert.IsNotNull(((SqlProvider)context.Provider).License);
            license = (ALinqLicenseProvider.LicFileLicense)((SqlProvider)context.Provider).License;
            Assert.IsFalse(license.IsTrial);

            mapping = XmlMappingSource.FromXml(
@"<Database Name=""Northwind"" Provider=""ALinq.SQLite.SQLiteProvider, ALinq.SQLite, Version=1.7.9.0, Culture=neutral, PublicKeyToken=2b23f34316d38f3a"" xmlns=""http://schemas.microsoft.com/linqtosql/mapping/2007"">
</Database>
");
            context = new MyDataContext("", mapping);
            Assert.IsNotNull(((SqlProvider)context.Provider).License);
            license = (ALinqLicenseProvider.LicFileLicense)((SqlProvider)context.Provider).License;
            Assert.IsFalse(license.IsTrial);
        }
#endif
        [TestMethod]
        public void LicenseTest1()
        {
            var access = new MyDataContext("C:/Northwind.mdb", typeof(ALinq.Access.AccessDbProvider));
            var q = access.GetTable<Customer>().ToList();
            var sqlLite = new MyDataContext("C:/Northwind.db3", typeof(ALinq.SQLite.SQLiteProvider));
            q = sqlLite.GetTable<Customer>().ToList();
            var conn = MySqlNorthwind.CreateConnection("root", "test", "Northwind", NorthwindDatabase.DB_HOST, 3306).ConnectionString;
            //var mysql = new MyDataContext(conn, typeof(ALinq.MySQL.MySqlProvider));
            //q = mysql.GetTable<Customer>().ToList();

            conn = OracleNorthwind.CreateConnection("Northwind", "test", "vpc1").ConnectionString;
            var oracle = new MyDataContext(conn, typeof(ALinq.Oracle.OracleProvider));
            q = oracle.GetTable<Customer>().ToList();

            conn = OdpOracleNorthwind.CreateConnection("Northwind", "test", "vpc1").ConnectionString;
            var odpOracle = new MyDataContext(conn, typeof(ALinq.Oracle.Odp.OracleProvider));
            q = oracle.GetTable<Customer>().ToList();

            conn = "User=SYSDBA;Password=masterkey;Database=Northwind;DataSource=vpc1;ServerType=0";
            var firebird = new MyDataContext(conn, typeof(ALinq.Firebird.FirebirdProvider));
            q = firebird.GetTable<Customer>().ToList();

            //conn = "DataBase=SAMPLE;USER ID=db2admin;Password=test;Server=VPC1";
            //var db2 = new MyDataContext(conn, typeof(ALinq.DB2.DB2Provider));
            //q = db2.GetTable<Customer>().ToList();

            //conn = "HOST=VPC1;User ID=postgres;PASSWORD=test;DATABASE=northwind";
            //var pgsql = new MyDataContext(conn, typeof(ALinq.PostgreSQL.PgsqlProvider));
            //pgsql.Connection.Open();
            //q = pgsql.GetTable<Customer>().ToList();
        }

        [TestMethod]
        public void LicenseTest2()
        {
            var access = new ALinq.DataContext("C:/Northwind.mdb", typeof(ALinq.Access.AccessDbProvider)) {
                Log = Console.Out
            };
            var q = access.GetTable<Customer>().ToList();

            var sqlLite = new ALinq.DataContext("C:/Northwind.db3", typeof(ALinq.SQLite.SQLiteProvider)) {
                Log = Console.Out
            };
            q = sqlLite.GetTable<Customer>().ToList();

            var conn = MySqlNorthwind.CreateConnection("root", "test", "Northwind", NorthwindDatabase.DB_HOST, 3306).ConnectionString;
            var mysql = new ALinq.DataContext(conn, typeof(ALinq.MySQL.MySqlProvider)) {
                Log = Console.Out
            };
            q = mysql.GetTable<Customer>().ToList();

            conn = OracleNorthwind.CreateConnection("Northwind", "test", "vpc1").ConnectionString;
            var oracle = new ALinq.DataContext(conn, typeof(ALinq.Oracle.OracleProvider)) {
                Log = Console.Out
            };
            q = oracle.GetTable<Customer>().ToList();

            conn = OdpOracleNorthwind.CreateConnection("Northwind", "test", "vpc1").ConnectionString;
            var odpOracle = new ALinq.DataContext(conn, typeof(ALinq.Oracle.Odp.OracleProvider)) {
                Log = Console.Out
            };
            q = oracle.GetTable<Customer>().ToList();

            conn = "User=SYSDBA;Password=masterkey;Database=Northwind;DataSource=vpc1;ServerType=0";
            var firebird = new ALinq.DataContext(conn, typeof(ALinq.Firebird.FirebirdProvider)) {
                Log = Console.Out
            };
            q = firebird.GetTable<Customer>().ToList();

            conn = "DataBase=SAMPLE;USER ID=db2admin;Password=test;Server=VPC1";
            var db2 = new ALinq.DataContext(conn, typeof(ALinq.DB2.DB2Provider)) {
                Log = Console.Out
            };
            q = db2.GetTable<Customer>().ToList();
        }

        [ALinq.License("ansiboy", "YO37C09AC579139220", typeof(ALinq.Access.AccessDbProvider))]
        [ALinq.License("ansiboy", "IG7C68697A92B7298D", typeof(ALinq.SQLite.SQLiteProvider))]
        [ALinq.License("ansiboy", "YOB4DD332AA17ABF595", typeof(ALinq.MySQL.MySqlProvider))]
        [ALinq.License("ansiboy", "YOF1FBE483ED5A8071", typeof(ALinq.Oracle.OracleProvider))]
        [ALinq.License("ansiboy", "YOF1FBE483ED5A8071", typeof(ALinq.Oracle.Odp.OracleProvider))]
        [ALinq.License("ansiboy", "4ECF2126023457FC", typeof(ALinq.Firebird.FirebirdProvider))]
        [ALinq.License("ansiboy", "YOBIH0747AE4A18FE78E4", typeof(ALinq.DB2.DB2Provider))]
        [ALinq.License("Steen Aagaard Sørensen", "HD7D009FB41AAFD704D26B7837C0479CDC5605AFAAA73A225F", typeof(ALinq.PostgreSQL.PgsqlProvider))]
        class MyDataContext : ALinq.DataContext
        {
            public MyDataContext(string fileOrServerOrConnection, Type provider)
                : base(fileOrServerOrConnection, provider)
            {
                Log = Console.Out;
            }

            public MyDataContext(string fileOrServerOrConnection, XmlMappingSource mapping)
                : base(fileOrServerOrConnection, mapping)
            {
                Log = Console.Out;
            }
        }

        //class LicenseFileDataContext : ALinq.DataContext
        //{
        //    public LicenseFileDataContext(string fileOrServerOrConnection, Type provider)
        //        : base(fileOrServerOrConnection, provider)
        //    {
        //        Log = Console.Out;
        //    }
        //}

        [Provider(typeof(ALinq.Access.AccessDbProvider))]
        [License("ansiboy", "QO77626437CFA2FE9E")]
        class AccessDataContext : ALinq.DataContext
        {
            public AccessDataContext()
                : base("C:/ConnectionString")
            {
            }
        }

        [Provider(typeof(ALinq.SQLite.SQLiteProvider))]
        [License("ansiboy", "JQ793C8427B6C0AE6E")]
        class SQLiteDataContext : ALinq.DataContext
        {
            public SQLiteDataContext()
                : base("C:/ConnectionString")
            {
            }
        }

        [Provider(typeof(ALinq.MySQL.MySqlProvider))]
        [License("ansiboy", "DLCE7A78A3D52567D5D")]
        class MySqlDataContext : ALinq.DataContext
        {
            public MySqlDataContext()
                : base("Server=localhost;Port=3306;User Id=root;Password=test;Database=Northwind")
            {
            }
        }

        [Provider(typeof(ALinq.Oracle.OracleProvider))]
        [License("ansiboy", "AV7D47FBDE5376DFA5")]
        class OracleDataContext : ALinq.DataContext
        {
            public OracleDataContext()
                : base("Data Source=localhost;Persist Security Info=True;User ID=Northwind;Password=Test")
            {
            }
        }

        [Provider(typeof(ALinq.Oracle.Odp.OracleProvider))]
        [License("ansiboy", "AV7D47FBDE5376DFA5")]
        class OracleOdpDataContext : ALinq.DataContext
        {
            public OracleOdpDataContext()
                : base("Data Source=localhost;Persist Security Info=True;User ID=Northwind;Password=Test")
            {
            }
        }
    }


}
#endif
