using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ALinq.Web.DynamicData;
using NorthwindDemo;

namespace Test.DynamicData
{
    [TestClass]
    public class DataModelProviderTest
    {
        [TestMethod]
        public void TablesTest()
        {
            var dataModelProvider = new ALinqDataModelProvider(delegate()
            {
                return new AccessNorthwind("C:/Northwind.mdb");
            });
            var db = (NorthwindDatabase)dataModelProvider.CreateContext();
            var tables = db.Mapping.GetTables();
            Assert.AreEqual(tables.Count(), dataModelProvider.Tables.Count);
        }

        [TestMethod]
        public void ColumnsTest()
        {
            var dataModelProvider = new ALinqDataModelProvider(delegate()
            {
                return new AccessNorthwind("C:/Northwind.mdb");
            });
            var db = (NorthwindDatabase)dataModelProvider.CreateContext();
            foreach (var tableProvider in dataModelProvider.Tables)
            {
                var table = db.Mapping.GetTable(tableProvider.EntityType);
                Assert.AreEqual(tableProvider.Columns.Count, table.RowType.DataMembers.Count);
            }
        }
    }
}
