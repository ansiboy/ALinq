#if DEBUG
using System;
using System.Data.OracleClient;
using ALinq.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class NorOracleDataTypeProviderTest : OracleDataTypeProviderTest
    {
        #region MyRegion
        //[TestMethod]
        //public void ParseSqlDataType()
        //{
        //    var typeProvider = new OracleDataTypeProvider();

        //    var dbType = "CHAR(1)";
        //    var sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(char), sqlType.GetClosestRuntimeType());
        //    Assert.AreEqual(1, sqlType.Size);

        //    dbType = "NCHAR ( 20 )";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
        //    Assert.IsTrue(sqlType.IsUnicodeType);
        //    Assert.AreEqual(sqlType.Size, 20);

        //    dbType = "NVARCHAR2(20)";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
        //    Assert.IsTrue(sqlType.IsUnicodeType);
        //    Assert.AreEqual(sqlType.Size, 20);

        //    dbType = "NUMBER(10)";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(decimal), sqlType.GetClosestRuntimeType());
        //    Assert.AreEqual(sqlType.Precision, 10);

        //    dbType = "NUMBER(10,2)";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(decimal), sqlType.GetClosestRuntimeType());
        //    Assert.AreEqual(sqlType.Precision, 10);
        //    Assert.AreEqual(sqlType.Scale, 2);
        //    //Assert.AreEqual(sqlType.Size, 10);

        //    dbType = "NUMBER(10,0)";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(decimal), sqlType.GetClosestRuntimeType());
        //    Assert.AreEqual(sqlType.Precision, 10);
        //    Assert.AreEqual(sqlType.Scale, 0);

        //    dbType = "NUMBER";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(decimal), sqlType.GetClosestRuntimeType());
        //    Assert.IsTrue(sqlType.Precision > 0);
        //    Assert.IsTrue(sqlType.Scale > 0);

        //    dbType = "FLOAT";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(float), sqlType.GetClosestRuntimeType());

        //    dbType = "FLOAT(10)";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(float), sqlType.GetClosestRuntimeType());
        //    Assert.AreEqual(sqlType.Size, 10);

        //    dbType = "RAW";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(byte[]), sqlType.GetClosestRuntimeType());

        //    dbType = "BLOB";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(byte[]), sqlType.GetClosestRuntimeType());
        //    Assert.IsTrue(sqlType.IsLargeType);

        //    dbType = "CLOB";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
        //    Assert.IsTrue(sqlType.IsLargeType);

        //    dbType = "NCLOB";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(string), sqlType.GetClosestRuntimeType());
        //    Assert.IsTrue(sqlType.IsLargeType);
        //    Assert.IsTrue(sqlType.IsUnicodeType);

        //    dbType = "CURSOR";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(object), sqlType.GetClosestRuntimeType());


        //    dbType = "DATE";
        //    sqlType = typeProvider.Parse(dbType);
        //    Assert.AreEqual(typeof(DateTime), sqlType.GetClosestRuntimeType());



        //} 
        #endregion

        internal override ITypeSystemProvider CreateDataProvider()
        {
            return new ALinq.Oracle.OracleDataTypeProvider();
        }

        internal override Type GetProviderType()
        {
            return typeof(ALinq.Oracle.OracleProvider);
        }

        [TestMethod]
        public override void ParseSqlDataType()
        {
            base.ParseSqlDataType();

            var typeProvider = CreateDataProvider();
            var dbType = "DATETIME";
            var sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(DateTime), sqlType.GetClosestRuntimeType());
            Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(DateTime))));

            dbType = "BFILE";
            sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(Byte[]), sqlType.GetClosestRuntimeType());
            //Assert.IsTrue(sqlType.IsSameTypeFamily(typeProvider.From(typeof(Binary))));
        }

        [TestMethod]
        public void FromTypeTest()
        {
            var typeProvider = CreateDataProvider();

            var dbType = typeProvider.From(typeof(int));
            Assert.AreEqual(OracleType.Int32, dbType.SqlDbType);

            dbType = typeProvider.From(typeof(string));
            Assert.AreEqual(OracleType.VarChar, dbType.SqlDbType);
            Assert.AreEqual(4000, dbType.Size);

            dbType = typeProvider.From(typeof(DateTime));
            Assert.AreEqual(OracleType.DateTime, dbType.SqlDbType);

            dbType = typeProvider.From(typeof (byte[]));
            Console.WriteLine(dbType.ToQueryString());
        }
    }

    [TestClass]
    public class Sql2000DataTypeProvider
    {
        [TestMethod]
        public void ParseSqlDataType()
        {
            var typeProvider = CreateDataProvider();

            var dbType = "CHAR(1)";
            var sqlType = typeProvider.Parse(dbType);
            //Assert.AreEqual(typeof (char), sqlType.GetClosestRuntimeType());
            //Assert.AreEqual(1, sqlType.Size);

            dbType = "CHAR";
            sqlType = typeProvider.Parse(dbType);
            //Assert.AreEqual(typeof(char), sqlType.GetClosestRuntimeType());

            dbType = "BigInt";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Binary";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Bit";
            sqlType = typeProvider.Parse(dbType);

            dbType = "DateTime";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Decimal";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Float";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Image";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Int";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Money";
            sqlType = typeProvider.Parse(dbType);

            dbType = "NChar";
            sqlType = typeProvider.Parse(dbType);

            dbType = "NText";
            sqlType = typeProvider.Parse(dbType);

            dbType = "NVarChar";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Real";
            sqlType = typeProvider.Parse(dbType);

            dbType = "UniqueIdentifier";
            sqlType = typeProvider.Parse(dbType);

            dbType = "SmallDateTime";
            sqlType = typeProvider.Parse(dbType);

            dbType = "SmallInt";
            sqlType = typeProvider.Parse(dbType);

            dbType = "SmallMoney";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Text";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Timestamp";
            sqlType = typeProvider.Parse(dbType);

            dbType = "TinyInt";
            sqlType = typeProvider.Parse(dbType);

            dbType = "VarBinary";
            sqlType = typeProvider.Parse(dbType);

            dbType = "VarChar";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Variant";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Xml";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Udt";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Structured";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Date";
            sqlType = typeProvider.Parse(dbType);

            dbType = "Time";
            sqlType = typeProvider.Parse(dbType);

            dbType = "DateTime2";
            sqlType = typeProvider.Parse(dbType);

            dbType = "DateTimeOffset";
            sqlType = typeProvider.Parse(dbType);
        }

        internal ITypeSystemProvider CreateDataProvider()
        {
            return new ALinq.SqlClient.SqlTypeSystem.Sql2000Provider();
        }
    }

    [TestClass]
    public class Sql2005DataTypeProvider
    {
        [TestMethod]
        public void ParseSqlDataType()
        {
            var typeProvider = CreateDataProvider();

            var dbType = "CHAR(1)";
            var sqlType = typeProvider.Parse(dbType);
            Assert.AreEqual(typeof(char), sqlType.GetClosestRuntimeType());
            Assert.AreEqual(1, sqlType.Size);
        }

        internal ITypeSystemProvider CreateDataProvider()
        {
            return new ALinq.SqlClient.SqlTypeSystem.Sql2000Provider();
        }
    }
}
#endif