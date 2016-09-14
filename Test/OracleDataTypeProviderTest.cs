#if DEBUG
using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ALinq;
using ALinq.Mapping;

using ALinq.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;
using Oracle.DataAccess.Client;
using System.Data.OracleClient;

//using ALinq.Oracle.Odp;

namespace Test
{
    [TestClass]
    public abstract class OracleDataTypeProviderTest //<T> where T : SqlProvider //: OracleDataTypeProviderTest
    {
        internal abstract ITypeSystemProvider CreateDataProvider();

        internal abstract Type GetProviderType();

        [TestMethod]
        public virtual void ParseSqlDataType()
        {
            var typeProvider = CreateDataProvider();

            var dbType = "CHAR(1)";
            var sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(char), sqlType.GetClosestRuntimeType());
            Assert.AreEqual(1, sqlType.Size);

            dbType = "NCHAR ( 20 )";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsUnicodeType);
            Assert.AreEqual(sqlType.Size, 20);

            dbType = "NVARCHAR2(20)";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsUnicodeType);
            Assert.AreEqual(sqlType.Size, 20);

            dbType = "NUMBER(10)";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(decimal), sqlType.GetClosestRuntimeType());
            Assert.AreEqual(10, sqlType.Precision);

            dbType = "NUMBER(10,2)";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(decimal), sqlType.GetClosestRuntimeType());
            Assert.AreEqual(sqlType.Precision, 10);
            Assert.AreEqual(sqlType.Scale, 2);
            //Assert.AreEqual(sqlType.Size, 10);

            dbType = "NUMBER(10,0)";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(decimal), sqlType.GetClosestRuntimeType());
            Assert.AreEqual(sqlType.Precision, 10);
            Assert.AreEqual(sqlType.Scale, 0);

            dbType = "NUMBER";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(decimal), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.Precision > 0);
            Assert.IsTrue(sqlType.Scale > 0);

            dbType = "FLOAT";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(float), sqlType.GetClosestRuntimeType());

            dbType = "FLOAT(10)";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(float), sqlType.GetClosestRuntimeType());
            Assert.AreEqual(sqlType.Size, 10);

            dbType = "RAW";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(byte[]), sqlType.GetClosestRuntimeType());

            dbType = "RAW ( 2000 ) ";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(byte[]), sqlType.GetClosestRuntimeType());
            Assert.AreEqual(2000, sqlType.Size);

            dbType = "BLOB";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(byte[]), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsLargeType);

            dbType = "CLOB";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsLargeType);

            dbType = "NCLOB";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsLargeType);
            Assert.IsTrue(sqlType.IsUnicodeType);

            dbType = "CURSOR";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(object), sqlType.GetClosestRuntimeType());


            dbType = "DATE";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(DateTime), sqlType.GetClosestRuntimeType());

            dbType = "REFCURSOR";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(object), sqlType.GetClosestRuntimeType());
            sqlType.IsSameTypeFamily(typeProvider.From(typeof(object)));

            dbType = "REF CURSOR";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(object), sqlType.GetClosestRuntimeType());

            //容错检测
            dbType = "REF   CURSOR";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(object), sqlType.GetClosestRuntimeType());

            dbType = "CURSOR";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(object), sqlType.GetClosestRuntimeType());

            dbType = "VARCHAR2";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());

            //时间
            dbType = "TIMESTAMP";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(DateTime), sqlType.GetClosestRuntimeType());

            dbType = "TIMESTAMP WITH LOCAL TIME ZONE";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(DateTime), sqlType.GetClosestRuntimeType());

            dbType = "TIMESTAMP WITH TIME ZONE";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(DateTime), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(DateTime))));

            dbType = "INTERVAL DAY TO SECOND";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(TimeSpan), sqlType.GetClosestRuntimeType());

            dbType = "INTERVAL YEAR TO MONTH";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(Int32), sqlType.GetClosestRuntimeType());

            dbType = "DATE";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(DateTime), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(DateTime))));

            //数值
            dbType = "NUMBER";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(decimal), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.Precision > 0);
            Assert.IsTrue(sqlType.Scale > 0);
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(float))));
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(double))));
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(int))));
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(Int16))));
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(Int64))));

            dbType = "INTEGER";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(int), sqlType.GetClosestRuntimeType());

            //字符串
            dbType = "NCHAR";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsUnicodeType);

            dbType = "NVARCHAR2";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsUnicodeType);

            dbType = "XMLTYPE";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(string))));
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(XElement))));

            //二进制
            dbType = "LONG";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(string))));

            dbType = "LONG RAW";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(byte[]), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(Binary))));

            dbType = "BLOB";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(byte[]), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(Binary))));


        }




        [TestMethod]
        public void TestGuid()
        {
            var db = new TestDB(GetProviderType());
            var guid1 = new Guid("{35450085-954D-4ed2-9C72-8F12293DB0F8}");
            var guid2 = new Guid("{F9EE3130-72A7-4f66-93BB-50B2FFE3A27F}");
            var guid3 = new Guid("{507DD15B-0F8E-44a9-9C26-F9E72835B14B}");
            var guid4 = new Guid("{00D93FDD-1C8D-461a-89CA-5B89DF42FFD3}");

            var item = new DotNetDataType
                           {
                               Guid1 = guid1,
                               Guid2 = guid2,
                               Guid3 = guid3,
                               Guid4 = guid4,
                           };
            db.DotNetDataType.InsertOnSubmit(item);
            db.SubmitChanges();

            var id = db.DotNetDataType.Max(o => o.ID);
            item = db.DotNetDataType.Where(o => o.ID == id).Single();
            Assert.AreEqual(item.Guid1, guid1);
            Assert.AreEqual(item.Guid2, guid2);
            Assert.AreEqual(item.Guid3, guid3);
            Assert.AreEqual(item.Guid4, guid4);

            //Console.WriteLine(guid4.ToString().Length);
        }

        [TestMethod]
        public void CreateDatabase()
        {
            var db = new TestDB("DATA SOURCE=vpc1;PERSIST SECURITY INFO=True;USER ID=SYSTEM;PASSWORD=TEST", GetProviderType())
                     {
                         Log = Console.Out
                     };
            if (db.DatabaseExists())
            {
                db.DeleteDatabase();
            }
            db.CreateDatabase();
        }

        [TestMethod]
        public void DataTypeTest()
        {
            TestDB db;
            //db = new TestDB("DATA SOURCE=vpc1;PERSIST SECURITY INFO=True;USER ID=SYSTEM;PASSWORD=TEST")
            //         {
            //             Log = Console.Out
            //         };
            //if (db.DatabaseExists())
            //{
            //    db.DeleteDatabase();
            //}
            //db.CreateDatabase();

            db = new TestDB(GetProviderType()) { Log = Console.Out };
            //var guid = new Guid("0ce0823c-a758-4da2-8aa2-25aa3830684d");
            //var item = new T1
            //               {
            //                   PK = db.T1.Max(o => o.PK) + 1,
            //                   Blob = Encoding.Default.GetBytes("Hello World"),
            //                   Guid1 = guid,
            //                   Guid2 = guid,
            //                   OracleTimeStampTZ = new DateTime(1999, 11, 1),
            //                   OracleTimeStampLTZ = new DateTime(1999, 11, 1, 1, 1, 1)
            //               };
            //db.T1.InsertOnSubmit(item);
            //db.SubmitChanges();
            //var id = item.PK;
            //item = db.T1.Where(o => o.PK == id).Single();

            //Assert.AreEqual(item.OracleTimeStampTZ, new DateTime(1999, 11, 1));
            var contactsXML = new XElement("Contacts",
                new XElement("Contact", new XElement("Name", "Patrick Hines"),
                    new XElement("Phone", "206-555-0144")),
                    new XElement("Contact",
                        new XElement("Name", "Ellen Adams"),
                        new XElement("Phone", "206-555-0155")));

            {
                var item = new TString();
                item.XmlType = contactsXML;
                db.TString.InsertOnSubmit(item);
                db.SubmitChanges();
                //db.TString.Where(o => o.NVARCHAR2 != null).Select(o => o.NVARCHAR2.Substring(10)).ToList();
                var id = db.TString.Max(o => o.ID);
                //var result = db.TString.Where(o => o.XmlType != null)
                //                       .Select(o => o.XmlType.Element("Contact")).ToList();
            }

            {
                var item = new TNumber();
                item.Char = 'A';
                db.TNumber.InsertOnSubmit(item);
                db.SubmitChanges();

                item = db.TNumber.Where(o => o.PK == item.PK).Single();
                Assert.AreEqual('A', item.Char);
            }
        }

        [License("ansiboy", "AV7D47FBDE5376DFA5")]
        [Database(Name = "TestDB3")]
        public class TestDB : ALinq.DataContext
        {
            public TestDB(Type providerType)
                : base("DATA SOURCE=vpc1;USER ID=TestDB3;PASSWORD=TEST", providerType)
            {
            }

            public TestDB(string conn, Type providerType)
                : base(conn, providerType)
            {

            }

            public ALinq.Table<TNumber> TNumber
            {
                get { return GetTable<TNumber>(); }
            }

            public Table<TString> TString
            {
                get { return GetTable<TString>(); }
            }


            public Table<TBinary> TBinary
            {
                get { return GetTable<TBinary>(); }
            }

            public Table<TTime> TTime
            {
                get { return GetTable<TTime>(); }
            }

            public Table<DotNetDataType> DotNetDataType
            {
                get { return GetTable<DotNetDataType>(); }
            }

        }



        [Table]
        public class DotNetDataType
        {
            [Column(DbType = "Integer", IsPrimaryKey = true, IsDbGenerated = true)]
            public int ID
            {
                get;
                private set;
            }

            #region Guid 类型
            [Column(DbType = "Char(36)")]
            public Guid Guid1
            {
                get;
                set;
            }

            [Column(DbType = "Raw(16)")]
            public Guid Guid2
            {
                get;
                set;
            }

            [Column(DbType = "Blob")]
            public Guid Guid3
            {
                get;
                set;
            }

            [Column(DbType = "CLOB")]
            public Guid Guid4
            {
                get;
                set;
            }
            #endregion

            [Column(DbType = "RAW(2000)")]
            public Binary Binary1
            {
                get;
                set;
            }

            [Column(DbType = "Blob")]
            public Binary Binary2
            {
                get;
                set;
            }

            [Column(DbType = "VarChar(2000)")]
            public XElement XElement1
            {
                get;
                set;
            }

            [Column(DbType = "CLOB")]
            public XElement XElement2
            {
                get;
                set;
            }

            #region DateTime Type
            [Column(DbType = "TIMESTAMP")]
            public DateTime DateTime1
            {
                get;
                set;
            }


            [Column(DbType = "TIMESTAMP WITH LOCAL TIME ZONE")]
            public DateTime DateTime2
            {
                get;
                set;
            }

            [Column(DbType = "TIMESTAMP WITH TIME ZONE")]
            public DateTime DateTime3
            {
                get;
                set;
            }
            #endregion
        }

        [Table]
        public class TNumber
        {
            [Column(DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
            public int PK
            {
                get;
                set;
            }



            [Column(DbType = "Long")]
            public long Long
            {
                get;
                set;
            }

            [Column(DbType = "SmallInt")]
            public short SmallInt
            {
                get;
                set;
            }

            [Column(DbType = "Integer")]
            public int Integer
            {
                get;
                set;
            }

            [Column(DbType = "Real")]
            public float Real
            {
                get;
                set;
            }

            [Column(DbType = "DOUBLE PRECISION")]
            public double Double
            {
                get;
                set;
            }

            [Column(DbType = "Number")]
            public decimal OracleDecimal
            {
                get;
                set;
            }

            [Column(DbType = "SMALLINT")]
            public char Char
            {
                get;
                set;
            }
        }

        [Table]
        public class TBinary
        {
            [Column(DbType = "BLOB")]
            public byte[] Blob
            {
                get;
                set;
            }

            [Column(DbType = "CLOB")]
            public Guid Guid2
            {
                get;
                set;
            }
        }

        [Table]
        public class TTime
        {
            [Column(DbType = "TIMESTAMP")]
            public DateTime TimeStamp
            {
                get;
                set;
            }


            [Column(DbType = "TIMESTAMP WITH LOCAL TIME ZONE")]
            public DateTime OracleTimeStampLTZ
            {
                get;
                set;
            }

            [Column(DbType = "TIMESTAMP WITH TIME ZONE")]
            public DateTime OracleTimeStampTZ
            {
                get;
                set;
            }
        }

        [Table]
        public class TString
        {
            [Column(DbType = "Integer", IsDbGenerated = true, IsPrimaryKey = true)]
            public int ID
            {
                get;
                private set;
            }

            [Column(DbType = "NCHAR(50)")]
            public string NCHAR
            {
                get;
                set;
            }

            [Column(DbType = "NVARCHAR2(50)", Name = "\"NVARCHAR2\"")]
            public string NVARCHAR2
            {
                get;
                set;
            }

            [Column(DbType = "XMLTYPE")]
            public XElement XmlType
            {
                get;
                set;
            }

            [Column(DbType = "VarChar(36)")]
            public Guid Guid1
            {
                get;
                set;
            }
        }
    }

    [TestClass]
    public class TranslateTest
    {
        [Database(Name = "TranslateContext2")]
        [Provider(typeof(ALinq.Oracle.Odp.OracleProvider))]
        class TranslateContext : ALinq.DataContext
        {
            public TranslateContext(string fileOrServerOrConnection)
                : base(fileOrServerOrConnection)
            {
            }

            public TranslateContext()
                : base(@"DATA SOURCE=vpc1;PERSIST SECURITY INFO=True;USER ID=TranslateContext2;Password=TEST")
            {

            }

            public ALinq.Table<User> Users
            {
                get { return GetTable<User>(); }
            }
        }

        [Table]
        class User
        {
            [Column(Name = "\"ID\"", DbType = "Integer", IsPrimaryKey = true)]
            public int ID
            {
                get;
                set;
            }

            [Column(Name = "\"Name\"", DbType = "VarChar(20)")]
            public string Name
            {
                get;
                set;
            }
        }

        class T
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void Test()
        {
            var db = new TranslateContext() { Log = System.Console.Out };
            db.Users.ToList();
            db.Users.Insert(new User { Name = "User", ID = db.Users.Max(o => o.ID) + 1 });
            var q = db.Users.Where(o => o.ID < 10);
            var command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);
            db.Connection.Open();
            try
            {
                var reader = command.ExecuteReader();
                var items = db.Translate<T>(reader);
                foreach (var item in items)
                    Console.Write(item.Name);
            }
            finally
            {
                db.Connection.Close();
            }
        }

        [TestMethod]
        public void Test2()
        {
            var db = new TranslateContext();
            db.Users.Insert(new User { Name = "User", ID = db.Users.Max(o => o.ID) + 1 });
            var q = db.Users.Where(o => o.ID < 10);
            var command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);
            var items = db.ExecuteQuery<T>(command.CommandText, 10);
            foreach (var item in items)
                Console.WriteLine(item.ID);
        }

        [TestMethod]
        public void Test1()
        {
            var db = new AccessNorthwind("C:/Northwind.mdb");

            var q = db.Users.Where(o => o.ID < 100);
            var command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);
            command.Connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                var items = db.Translate<T>(reader);
                foreach (var item in items)
                    Console.Write(item.ID);
            }
            finally
            {
                db.Connection.Close();
            }
        }

        [TestMethod]
        public void CreateDatabase()
        {
            var db = new TranslateContext()
            {
                Log = Console.Out,
            };
            if (db.DatabaseExists())
                db.DeleteDatabase();
            db.CreateDatabase();
        }
    }
}
#endif