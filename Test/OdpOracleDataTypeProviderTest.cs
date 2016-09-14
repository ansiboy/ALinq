#if DEBUG
using System;
using ALinq.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.DataAccess.Client;

namespace Test
{
    [TestClass]
    public class OdpOracleDataTypeProviderTest : OracleDataTypeProviderTest//<ALinq.Oracle.Odp.OracleProvider>
    {
        internal override ITypeSystemProvider CreateDataProvider()
        {
            return new ALinq.Oracle.Odp.OracleDataTypeProvider();
        }

        internal override Type GetProviderType()
        {
            return typeof(ALinq.Oracle.Odp.OracleProvider);
        }


        [TestMethod]
        public void FromTypeTest()
        {
            var typeProvider = CreateDataProvider();

            var dbType = typeProvider.From(typeof(int));
            Assert.AreEqual(OracleDbType.Int32, dbType.SqlDbType);

            dbType = typeProvider.From(typeof (long));
            Assert.AreEqual(OracleDbType.Int64, dbType.SqlDbType);

            dbType = typeProvider.From(typeof (Int16));
            Assert.AreEqual(OracleDbType.Int16, dbType.SqlDbType);

            dbType = typeProvider.From(typeof(Oracle.DataAccess.Types.OracleRefCursor));
            Assert.AreEqual(OracleDbType.RefCursor, dbType.SqlDbType);

            dbType = typeProvider.From(typeof(string));
            Assert.AreEqual(OracleDbType.Varchar2, dbType.SqlDbType);
            Assert.AreEqual(4000, dbType.Size);

            dbType = typeProvider.From(typeof(DateTime));
            Assert.AreEqual(OracleDbType.Date, dbType.SqlDbType);
        }
    }
}
#endif