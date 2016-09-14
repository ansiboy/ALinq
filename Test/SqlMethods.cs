using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;
using ALinq;

namespace Test
{
    [TestClass]
    public class SqlMethodsTest
    {
        [TestMethod]
        public void DateDiffYear()
        {
            var db = CreateDataContext();
            var q = db.Orders.Select(o => new { o.OrderDate, o.ShippedDate }).ToArray();
            var values = db.Orders.Select(o => new { o.OrderDate, o.ShippedDate, Result = ALinq.SqlClient.SqlMethods.DateDiffYear(o.OrderDate, o.ShippedDate) }).ToArray();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                Console.WriteLine("{0} {1} {2}", value.OrderDate, value.ShippedDate, value.Result);
                Assert.AreEqual(ALinq.SqlClient.SqlMethods.DateDiffYear(value.OrderDate, value.ShippedDate), value.Result);
            }
        }

        [TestMethod]
        public void DateDiffMonth()
        {
            var db = CreateDataContext();
            db.Orders.Select(o => ALinq.SqlClient.SqlMethods.DateDiffMonth(o.OrderDate, o.ShippedDate)).ToList();

            var values = db.Orders.Select(o => new { o.OrderDate, o.ShippedDate, Result = ALinq.SqlClient.SqlMethods.DateDiffMonth(o.OrderDate, o.ShippedDate) }).ToArray();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                Console.WriteLine("{0} {1} {2}", value.OrderDate, value.ShippedDate, value.Result);
                Assert.AreEqual(ALinq.SqlClient.SqlMethods.DateDiffMonth(value.OrderDate, value.ShippedDate), value.Result);
            }
        }

        [TestMethod]
        public void DateDiffDay()
        {
            var db = CreateDataContext();
            db.Orders.Select(o => ALinq.SqlClient.SqlMethods.DateDiffDay(o.OrderDate, o.ShippedDate)).ToList();

            var values = db.Orders.Select(o => new { o.OrderDate, o.ShippedDate, Result = ALinq.SqlClient.SqlMethods.DateDiffDay(o.OrderDate, o.ShippedDate) }).ToArray();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                Console.WriteLine("{0} {1} {2}", value.OrderDate, value.ShippedDate, value.Result);
                //Assert.AreEqual(ALinq.SqlClient.SqlMethods.DateDiffDay(value.OrderDate, value.ShippedDate), value.Result);
            }

            //var mysqlDB = db as MySqlNorthwind;
            //if (mysqlDB != null)
            //{
            //    var v1 = db.Orders.Select(o => mysqlDB.Datediff(o.OrderDate, o.ShippedDate)).ToArray();
            //    for (var i = 0; i < v1.Length; i++)
            //    {
            //        Console.WriteLine("{0}", v1[i]);
            //    }
            //}
        }

        [TestMethod]
        public void DateDiffHour()
        {
            var db = CreateDataContext();
            db.Orders.Select(o => ALinq.SqlClient.SqlMethods.DateDiffHour(o.OrderDate, o.ShippedDate)).ToList();

            var values = db.Orders.Select(o => new { o.OrderDate, o.ShippedDate, Result = ALinq.SqlClient.SqlMethods.DateDiffHour(o.OrderDate, o.ShippedDate) }).ToArray();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                Console.WriteLine("{0} {1} {2}", value.OrderDate, value.ShippedDate, value.Result);
                Assert.AreEqual(ALinq.SqlClient.SqlMethods.DateDiffHour(value.OrderDate, value.ShippedDate), value.Result);
            }
        }

        [TestMethod]
        public void DateDiffMinute()
        {
            var db = CreateDataContext();
            db.Orders.Select(o => ALinq.SqlClient.SqlMethods.DateDiffMinute(o.OrderDate, o.ShippedDate)).ToList();

            var values = db.Orders.Select(o => new { o.OrderDate, o.ShippedDate, Result = ALinq.SqlClient.SqlMethods.DateDiffMinute(o.OrderDate, o.ShippedDate) }).ToArray();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                Console.WriteLine("{0} {1} {2}", value.OrderDate, value.ShippedDate, value.Result);
                Assert.AreEqual(ALinq.SqlClient.SqlMethods.DateDiffMinute(value.OrderDate, value.ShippedDate), value.Result);
            }
        }

        [TestMethod]
        public void DateDiffMillisecond()
        {
            var db = CreateDataContext();
            db.Orders.Select(o => ALinq.SqlClient.SqlMethods.DateDiffMillisecond(o.OrderDate, o.ShippedDate)).ToList();

            var values = db.Orders.Select(o => new { o.OrderDate, o.ShippedDate, Result = ALinq.SqlClient.SqlMethods.DateDiffMillisecond(o.OrderDate, o.ShippedDate) }).ToArray();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                Console.WriteLine("{0} {1} {2} {3}", value.OrderDate, value.ShippedDate, value.Result, ALinq.SqlClient.SqlMethods.DateDiffSecond(value.OrderDate, value.ShippedDate));
                Assert.AreEqual(ALinq.SqlClient.SqlMethods.DateDiffMillisecond(value.OrderDate, value.ShippedDate), value.Result);
            }
        }

        [TestMethod]
        public void DateDiffSecond()
        {
            var db = CreateDataContext();
            db.Orders.Select(o => ALinq.SqlClient.SqlMethods.DateDiffSecond(o.OrderDate, o.ShippedDate)).ToList();

            var values = db.Orders.Select(o => new { o.OrderDate, o.ShippedDate, Result = ALinq.SqlClient.SqlMethods.DateDiffSecond(o.OrderDate, o.ShippedDate) }).ToArray();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                Console.WriteLine("{0} {1} {2}", value.OrderDate, value.ShippedDate, value.Result);
                Assert.AreEqual(ALinq.SqlClient.SqlMethods.DateDiffSecond(value.OrderDate, value.ShippedDate), value.Result);
            }
        }

        protected virtual NorthwindDatabase CreateDataContext()
        {
            throw new NotImplementedException();
        }

    }

    [TestClass]
    public class AccessSqlMethods : SqlMethodsTest
    {
        protected override NorthwindDatabase CreateDataContext()
        {
            return new AccessNorthwind("C:/Northwind.mdb") { Log = Console.Out };
        }
    }

    [TestClass]
    public class MySqlMethods : SqlMethodsTest
    {
        protected override NorthwindDatabase CreateDataContext()
        {
            return new MySqlNorthwind(MySqlNorthwind.CreateConnection("root", "test", "Northwind", NorthwindDatabase.DB_HOST, 3306).ConnectionString) { Log = Console.Out };
        }
    }

    [TestClass]
    public class DB2SqlMethods : SqlMethodsTest
    {
        protected override NorthwindDatabase CreateDataContext()
        {
            return new DB2Northwind() { Log = Console.Out };
        }
    }
}
