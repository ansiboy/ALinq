#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ALinq;
using ALinq.SqlClient;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class FirebirdDataTypeTest
    {
        [TestMethod]
        public void ParseSqlDataType()
        {
            var typeProvider = CreateDataProvider();

            var dbType = "VarChar(30)";

            var dataType = typeProvider.Parse(dbType);
            Assert.AreEqual(FbDbType.VarChar, dataType.SqlDbType);
            Assert.AreEqual(30, dataType.Size);

            //Test ToQueryString
            dbType = dataType.ToQueryString();
            dataType = typeProvider.Parse(dbType);
            Assert.AreEqual(FbDbType.VarChar, dataType.SqlDbType);
            Assert.AreEqual(30, dataType.Size);

            dbType = "BLOB SUB_TYPE TEXT";
            dataType = typeProvider.Parse(dbType);
        }

        [TestMethod]
        public void FromTypeTest()
        {
            var typeProvider = CreateDataProvider();
            var dataType = typeProvider.From(typeof(Int64));
            Assert.AreEqual(FbDbType.BigInt, dataType.SqlDbType);
            Console.WriteLine("Int 64: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(UInt64));
            Assert.AreEqual(FbDbType.BigInt, dataType.SqlDbType);
            Console.WriteLine("UInt64: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(bool));
            Console.WriteLine("bool: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(byte));
            Console.WriteLine("byte: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(byte[]));
            Console.WriteLine("byte[]: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(char));
            Console.WriteLine("char: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(DateTime));
            Console.WriteLine("DateTime: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(decimal));
            Console.WriteLine("deciaml: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(double));
            Console.WriteLine("double: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(Guid));
            Console.WriteLine("Guid: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(Int16));
            Console.WriteLine("Int16: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(int));
            Console.WriteLine("Int: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(Int64));
            Console.WriteLine("Int64: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(object));
            Console.WriteLine("object: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(SByte));
            Console.WriteLine("SByte: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(Single));
            Console.WriteLine("Single: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(string));
            Console.WriteLine("string: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(TimeSpan));
            Console.WriteLine("TimeSpan: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(UInt16));
            Console.WriteLine("UInt16: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(UInt32));
            Console.WriteLine("UInt32: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(UInt64));
            Console.WriteLine("UInt64: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(XElement));
            Console.WriteLine("XElement: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(XDocument));
            Console.WriteLine("XDocument: {0}", dataType.ToQueryString());

            dataType = typeProvider.From(typeof(Binary));
            Console.WriteLine("Binary: {0}", dataType.ToQueryString());
        }

        [TestMethod]
        public void FromObjectTest()
        {
            var typeProvider = CreateDataProvider();
            var dataType = typeProvider.From(0.0m);
            Assert.AreEqual(FbDbType.Decimal, dataType.SqlDbType);
            Assert.IsTrue(dataType.Precision > 0);
            Assert.IsTrue(dataType.Scale > 0);

        }


        private static ITypeSystemProvider CreateDataProvider()
        {
            return new ALinq.Firebird.FirebirdDataTypeProvider();
        }
    }
}
#endif
