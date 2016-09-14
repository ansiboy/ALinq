using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class XmlMappingSourceTest
    {
        //[TestMethod]
        public void TestTableMappingName()
        {
            var xmlMapping = XmlMappingSource.FromUrl("Northwind.Oracle.map");
            var model = xmlMapping.GetModel(typeof(NorthwindDemo.NorthwindDatabase));
            var metaTable = model.GetTable(typeof(NorthwindDemo.Contact));
            Assert.AreEqual("Contacts", metaTable.TableName);
        }

        //[ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            var type = typeof(XmlMappingSourceTest);
            var path = type.Module.FullyQualifiedName;
            var filePath = Path.GetDirectoryName(path) + @"\ALinq.Oracle.OracleProvider.lic";
            File.Copy(@"E:\ALinqs\ALinq1.8\Test\ALinq.Oracle.OracleProvider.lic", filePath);
            filePath = Path.GetDirectoryName(path) + @"\Northwind.Oracle.map";
            File.Copy(@"E:\ALinqs\ALinq1.8\Test\Northwind.Oracle.map", filePath);
        }
    }
}
