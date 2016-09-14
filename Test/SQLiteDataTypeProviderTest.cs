#if DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using ALinq.Access;
using ALinq.SQLite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class SQLiteDataTypeProviderTest
    {
        [TestMethod]
        public void ToQueryString()
        {
            var dataTypeProvider = new SQLiteDataTypeProvider();
            var dataType = dataTypeProvider.From(typeof(string));
            Assert.IsTrue(dataType.Size > 0);
            Console.WriteLine(dataType.ToQueryString());

            dataType = dataTypeProvider.From(typeof(byte[]));
            Console.WriteLine(dataType.ToQueryString());

            dataType = dataTypeProvider.From(typeof(decimal));
            Console.WriteLine(dataType.ToQueryString());

            dataType = dataTypeProvider.From(typeof(Int32));
            Console.WriteLine(dataType.ToQueryString());

            dataType = dataTypeProvider.From(typeof(long));
            Console.WriteLine(dataType.ToQueryString());

            dataType = dataTypeProvider.From(typeof(Single));
            Console.WriteLine(dataType.ToQueryString());

            dataType = dataTypeProvider.From(typeof(Double));
            Console.WriteLine(dataType.ToQueryString());

            dataType = dataTypeProvider.From(typeof(Byte[]));
            Console.WriteLine(dataType.ToQueryString());
        }

        [TestMethod]
        public void ParseSqlDataType()
        {
            var typeProvider = new SQLiteDataTypeProvider();
            var dataType = typeProvider.Parse("Binary");
            Assert.AreEqual(DbType.Binary, dataType.SqlDbType);

            dataType = typeProvider.Parse("Bit");
            Assert.AreEqual(DbType.Boolean, dataType.SqlDbType);

            dataType = typeProvider.Parse("DateTime");
            Assert.AreEqual(DbType.Date, dataType.SqlDbType);

            dataType = typeProvider.Parse("Date");
            Assert.AreEqual(DbType.Date, dataType.SqlDbType);
            Console.WriteLine(dataType.ToQueryString());

            dataType = typeProvider.Parse("VarChar(30)");
            Assert.AreEqual(dataType.Size, 30);

            dataType = typeProvider.Parse("(30)");
            Assert.AreEqual(dataType.Size, 30);

            dataType = typeProvider.Parse("integer");
            Assert.AreEqual(DbType.Int32, dataType.SqlDbType);
        }

        [TestMethod]
        public void GetSqlDataType()
        {
            var typeProvider = new SQLiteDataTypeProvider();
            var dataType = typeProvider.From(typeof(byte[]));
            Assert.AreEqual(OleDbType.Binary, dataType.SqlDbType);

            dataType = typeProvider.From(typeof(string));
            Assert.AreEqual(OleDbType.VarChar, dataType.SqlDbType);
            dataType.Size = 50;

            Assert.AreEqual("VARCHAR(50)", dataType.ToQueryString().ToUpper());

            dataType = typeProvider.From(typeof(ALinq.Binary));
        }
    }
}
#endif