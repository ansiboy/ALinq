using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class PgsqlTest : SqlTest
    {
        public override NorthwindDemo.NorthwindDatabase CreateDataBaseInstace()
        {
            return new PgsqlNorthwind() { Log = Console.Out };
        }

        [TestMethod]
        public void CreateDatabase()
        {
            base.CreateDatabase();
        }
    }
}