using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;
using ALinq.Mapping;
using ALinq.Dynamic;

namespace Test
{
    /// <summary>
    /// ALinq 3.0 新版功能测试。
    /// </summary>
    partial class SqlTest
    {
        [TestMethod]
        public void DynamicPropertyTest()
        {
            db.Orders.Select(o => new { o.OrderID, C = (DateTime?)o["OrderDate"] }).ToArray();
            db.Orders.Where(o => (int)o["OrderID"] < 10).ToArray();
            db.Orders.Where(o => (string)o["ShipName"] == "AAAA").ToArray();

            var d = DateTime.Now;
            db.Orders.Where(o => (DateTime)o["OrderDate"] == d).Select(o => o.OrderDate).ToArray();
        }

        [Function]
        public static string GetColumn()
        {
            return "OrderDate";
        }

        [TestMethod]
        public void NewItemTest()
        {
            db.Orders.Select(o => new Order { OrderDate = o.OrderDate, OrderID = o.OrderID });
        }

        [Table(Name = "Employees")]
        public class E
        {
            private System.Collections.Generic.Dictionary<string, object> _this;
            public E()
            {
                _this = new System.Collections.Generic.Dictionary<string, object>();
            }

            public object this[string name]
            {
                get
                {
                    object item;
                    if (_this.TryGetValue(name, out item))
                        return item;

                    return null;
                }
                set
                {
                    _this[name] = value;
                }
            }

            public string Test()
            {
                return "HELLO";
            }
        }

        [TestMethod]
        public void DynamicColumnSelect()
        {
            //var q1 = db.GetTable<E>().Select(o => new { FirstName = o["FirstName"], LastName = o["LastName"] })
            //                         .ToList();
            var q = db.GetTable<E>().Select("new (this['FirstName'] as F, this['LastName'] as L)");
            q.Cast<dynamic>().ToList().ForEach(delegate(dynamic o)
            {
                Console.WriteLine(o.FirstName);
            });
        }

        [TestMethod]
        public void DynamicColumnSelect1()
        {
            var q = db.Employees.Select(o => o["FirstName"])
                                .Cast<dynamic>().ToArray();

        }

        [TestMethod]
        public void DynamicColumnWhere()
        {
            var q1 = db.Orders.Where("OrderDate < @0", DateTime.Now).ToArray();
            var q2 = db.Orders.Where("OrderDate < #2011-10-4#").ToArray();
            //var q3 = db.Orders.Where("this['OrderDate'] < @0", DateTime.Now).ToArray();
        }

        [TestMethod]
        public void ItemLocalTest()
        {
            var table = db.GetTable<E>();
            var q = table.Select(o => new { A = o["EmployeeID"], T = o.Test() }).ToList();
            q.ForEach(o => Console.WriteLine(o));
            //db.Orders.Select(o => o["OrderID"]).ToArray();
        }
    }
}
