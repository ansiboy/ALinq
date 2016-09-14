using System;
using System.Collections.Generic;
using System.Data.EffiProz;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class EfzTest : SqlTest
    {
        public override NorthwindDemo.NorthwindDatabase CreateDataBaseInstace()
        {
            //var str = "Connection Type=Memory ; Database=Northwind; Data Source=TestDB; User=sa; Password=;";
            var builder = new EfzConnectionStringBuilder()
                              {
                                  ConnectionType = "file",
                                  User = "sa",
                                  Password = "",
                                  InitialCatalog = "C:\\Northwind",
                              };
            var conn = new EfzConnection(builder.ToString());
            var mapping = ALinq.Mapping.XmlMappingSource.FromStream(GetType().Assembly.GetManifestResourceStream("Test.Northwind.Efz.map"));
            return new EfzNorthwind(builder.ToString(), mapping) { Log = Console.Out };
        }

        [TestMethod]
        public override void CreateDatabase()
        {
            var db = CreateDataBaseInstace();
            db.Connection.Open();
            db.Log = Console.Out;
            db.CreateDatabase();
            db.ExecuteCommand("SHUTDOWN");
            db.Connection.Close();
        }
    }
}
