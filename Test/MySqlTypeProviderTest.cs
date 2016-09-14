using System;
using System.Linq;
using ALinq;
using ALinq.MySQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ALinq.SqlClient;
using MySql.Data.MySqlClient;
using NorthwindDemo;

namespace Test
{

#if DEBUG
    /// <summary>
    ///This is a test class for MySqlTypeProviderTest and is intended
    ///to contain all MySqlTypeProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MySqlTypeProviderTest
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
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseSqlDataType()
        {
            var target = new MySqlDataTypeProvider();
            var types = new[] { "BIT", "BOOL", "BOOLEAN", "SMALLINT", "MEDIUMINT", 
                                "INT", "INTEGER", "BIGINT", "FLOAT", "DOUBLE",
                                "DOUBLE PRECISION", "FLOAT", "DECIMAL", "DEC",
                                "NUMERIC", "FIXED"};
            for (int i = 0; i < types.Length; i++)
            {
                target.Parse(types[i]);
            }

            types = new[] { "DATE", "DATETIME", "TIMESTAMP", "TIME", "YEAR" };
            for (int i = 0; i < types.Length; i++)
            {
                target.Parse(types[i]);
            }

            types = new[] { "CHAR", "VARCHAR", "TEXT", "BINARY", "VARBINARY",
                            "TINYBLOB", "TINYTEXT", "BLOB", "TEXT", "MEDIUMBLOB",
                            "MEDIUMTEXT", "LONGBLOB", "LONGTEXT", "ENUM", "SET"};
            for (int i = 0; i < types.Length; i++)
            {
                target.Parse(types[i]);
            }

            var dataType = target.Parse("CHAR");
            Assert.AreEqual(typeof(string), dataType.GetClosestRuntimeType());

            dataType = target.Parse("CHAR(1)");
            Assert.AreEqual(typeof(char), dataType.GetClosestRuntimeType());

            dataType = target.Parse("Char (100)");
            Assert.AreEqual(typeof(string), dataType.GetClosestRuntimeType());
            Assert.AreEqual(100, dataType.Size);
            Assert.IsFalse(dataType.IsUnicodeType);

            dataType = target.Parse("NChar");
            Assert.AreEqual(typeof(string), dataType.GetClosestRuntimeType());
            Assert.IsTrue(dataType.IsUnicodeType);

            dataType = target.Parse("NChar(100)");
            Assert.AreEqual(typeof(string), dataType.GetClosestRuntimeType());
            Assert.IsTrue(dataType.IsUnicodeType);
            Assert.AreEqual(100, dataType.Size);

            dataType = target.Parse("VarCHAR");
            Assert.AreEqual(typeof(string), dataType.GetClosestRuntimeType());

            dataType = target.Parse("VarCHAR(1)");
            Assert.AreEqual(typeof(char), dataType.GetClosestRuntimeType());

            dataType = target.Parse("VarChar (100)");
            Assert.AreEqual(typeof(string), dataType.GetClosestRuntimeType());
            Assert.AreEqual(100, dataType.Size);
            Assert.IsFalse(dataType.IsUnicodeType);

            dataType = target.Parse("NVarChar");
            Assert.AreEqual(typeof(string), dataType.GetClosestRuntimeType());
            Assert.IsTrue(dataType.IsUnicodeType);

            dataType = target.Parse("NVarChar(100)");
            Assert.AreEqual(typeof(string), dataType.GetClosestRuntimeType());
            Assert.IsTrue(dataType.IsUnicodeType);
            Assert.AreEqual(100, dataType.Size);

            dataType = target.Parse("VarChar Binary");
            Assert.AreEqual(typeof(string), dataType.GetClosestRuntimeType());



            var stype = "BIT";
            var actual = target.Parse(stype);
            Assert.AreEqual(actual.SqlDbType, MySqlDbType.Bit);

            stype = "TINYINT";
            actual = target.Parse(stype);
            Assert.AreEqual(actual.SqlDbType, MySqlDbType.Byte);

            stype = "TINYINT(1)";
            actual = target.Parse(stype);
            Assert.AreEqual(actual.SqlDbType, MySqlDbType.Byte);
            Assert.AreEqual(actual.Size, 1);

            stype = "TINYINT(1) UNSIGNED";
            actual = target.Parse(stype);
            Assert.AreEqual(actual.SqlDbType, MySqlDbType.UByte);
            Assert.AreEqual(actual.Size, 1);

            stype = "TINYINT  (1)   UNSIGNED";
            actual = target.Parse(stype);
            Assert.AreEqual(actual.SqlDbType, MySqlDbType.UByte);
            Assert.AreEqual(actual.Size, 1);

            stype = "VarChar(100) Unicode";
            actual = target.Parse(stype);
            Assert.AreEqual(actual.SqlDbType, MySqlDbType.VarChar);
            Assert.AreEqual(actual.Size, 100);
            Assert.IsTrue(actual.IsUnicodeType);

            stype = "Decimal";
            actual = target.Parse(stype);
            Assert.AreEqual(SqlDataType<MySqlDbType>.DEFAULT_DECIMAL_SCALE, actual.Scale);
            Assert.AreEqual(SqlDataType<MySqlDbType>.DEFAULT_DECIMAL_PRECISION, actual.Precision);
        }

        [TestMethod]
        public void FromType()
        {
            var typeProvider = new MySqlDataTypeProvider();
            var dbType = typeProvider.From(typeof(Binary));

            dbType = typeProvider.From(typeof(byte[]));
            Console.WriteLine(dbType.ToQueryString());

        }

        [TestMethod]
        public void ToQueryString()
        {
            var target = new MySqlDataTypeProvider();
            var dataType = target.From(typeof(string));
            var dbType = dataType.ToQueryString();
            Assert.AreEqual("VARCHAR(4000)", dbType);

            dataType = target.From(typeof(string), 100);
            dbType = dataType.ToQueryString();
            Assert.AreEqual("VARCHAR(100)", dbType);

            var sqlType = target.From(typeof(Binary));
            dbType = sqlType.ToQueryString();
            Console.WriteLine(dbType);
        }

        [TestMethod]
        public void temp()
        {
            //var formatFlags = QueryFormatOptions.None;
            //Assert.AreEqual(formatFlags & QueryFormatOptions.SuppressSize, QueryFormatOptions.None);
            var context = new ALinq.DataContext("C:/Northwind.mdb", typeof(ALinq.Access.AccessDbProvider));
            var product = context.GetTable<Product>();
            var tables = context.Mapping.GetTables().ToArray();
            Console.WriteLine(tables.Count());
        }

    }
#endif
}
