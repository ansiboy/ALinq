using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class DynamicMappingTest
    {
        private AccessNorthwind db;

        [TestInitialize]
        public void TestInitialize()
        {
            var xmlMapping = XmlMappingSource.FromStream(typeof(SQLiteTest).Assembly.GetManifestResourceStream("Test.Northwind.Access.map"));
            db = new AccessNorthwind("C:/Northwind.mdb");
            //db = new AccessNorthwind("C:/Northwind.mdb", xmlMapping);
            //db = new SQLiteNorthwind("C:/Northwind.db3");
            //db = new MySqlNorthwind(MySqlNorthwind.CreateConnection("root", "test", "Northwind", "localhost", 3306).ConnectionString);
            db.Log = Console.Out;
            //this.mappingSource = new DynamicMappingSource()
        }

        [TestMethod]
        public void TestMetaMembersCount()
        {
            var table = db.GetTable<Employee>();

            var metaTable = db.Mapping.GetTable(typeof(Employee));
            var dataMembers = metaTable.RowType.DataMembers;
            var count = dataMembers.Count;
            Assert.IsTrue(count > 0);

            
            var q = table.Select(o => o["XXXXXX"]).ToArray();
            //db.GetCommand(q);
            //Assert.AreEqual(count + 1, dataMembers.Count);

        }
    }
}
