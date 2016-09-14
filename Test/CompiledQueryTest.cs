using System;
using ALinq;
using ALinq.Mapping;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    /// <summary>
    /// Summary description for CompiledQuery
    /// </summary>
    [TestClass]
    public class CompiledQueryTest
    {
        public class SimpleCustomer
        {
            public string ContactName { get; set; }
        }

        [TestMethod]
        public void CompileTest()
        {
            var CustomersByCity =
              ALinq.CompiledQuery.Compile<NorthwindDatabase, string, IEnumerable<SimpleCustomer>>((db, city) =>
                            from c in db.Customers
                            where c.City == city
                            select new SimpleCustomer { ContactName = c.ContactName });
            var context = new AccessNorthwind("C:/Northwind.mdb");
            var result = CustomersByCity(context, "London").ToList();
            Assert.IsTrue(result.Count > 0);
        }
    }
}
