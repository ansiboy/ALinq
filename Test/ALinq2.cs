using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;
using System.Globalization;
using ALinq.Mapping;
using ALinq;
using System.Collections.Generic;

namespace Test
{
    partial class SqlTest
    {


        [TestMethod]
        public void Aggregate_Null()
        {
            db.Log = Console.Out;
            var tableName = db.Mapping.GetTable(typeof(DataType)).TableName;
            db.ExecuteCommand("DELETE FROM " + tableName);

            //if (((ALinq.SqlClient.SqlProvider)db.Provider).Mode == ALinq.SqlClient.SqlProvider.ProviderMode.EffiProz)
            //    
#if DEBUG
            if (((ALinq.SqlClient.SqlProvider)db.Provider).Mode != ALinq.SqlClient.SqlProvider.ProviderMode.EffiProz)
                Assert.AreEqual(0, db.DataTypes.Count());
#endif
            int min = db.Products.Where(o => o.SupplierID == -1000).Min(o => o.ProductID);
            Assert.AreEqual(0, min);
            int max = db.Products.Where(o => o.SupplierID == -1000).Max(o => o.ProductID);
            Assert.AreEqual(0, max);
            var avg = db.Products.Where(o => o.SupplierID == -1000).Average(o => o.ProductID);
            Assert.AreEqual(0, avg);
            var sum = db.Products.Where(o => o.SupplierID == -1000).Sum(o => o.ProductID);
            Assert.AreEqual(0, sum);
            var count = db.DataTypes.Count();
#if DEBUG
            if (((ALinq.SqlClient.SqlProvider)db.Provider).Mode != ALinq.SqlClient.SqlProvider.ProviderMode.EffiProz)
                Assert.AreEqual(0, count);
#endif
            decimal? min1 = db.Products.Where(o => o.SupplierID == -1000).Min(o => o.UnitPrice);
            Assert.AreEqual(0, min1);
            decimal? max1 = db.Products.Where(o => o.SupplierID == -1000).Max(o => o.UnitPrice);
            Assert.AreEqual(0, max1);
            decimal? avg1 = db.Products.Where(o => o.SupplierID == -1000).Average(o => o.UnitPrice);
            Assert.AreEqual(0, avg1);
            decimal? sum1 = db.Products.Where(o => o.SupplierID == -1000).Sum(o => o.UnitPrice);
            Assert.AreEqual(0, sum1);
        }

        [TestMethod]
        public void GetDefaultValue()
        {
            var q = db.Products.Where(o => o.UnitPrice.GetValueOrDefault(0) == 0).ToList();
            q = db.Products.Where(o => o.UnitPrice.GetValueOrDefault() == 0).ToList();
        }


        //public void Temp()
        //{
        //    var qa = from c in db.OrderDetails
        //             where c.OrderID == 10248
        //             select c;
        //    var t1 = qa.Take(1).Single();
        //    Assert.IsNotNull(t1.Order);
        //}

        #region Math Method

        [TestMethod]
        public void Math_Abs()
        {
            var q = db.Orders.Select(o => Math.Abs(o.EmployeeID)).Take(5).ToArray();
        }

        [TestMethod]
        public void Math_Acos()
        {
            var q = db.OrderDetails.Where(o => o.Discount <= 1)
                                   .Select(o => new { Value1 = o.Discount, Value2 = Math.Acos(o.Discount) }).Take(5);//.ToArray();
            var command = db.GetCommand(q);
            Assert.IsTrue(q.ToArray().Length > 0);
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Acos(item.Value1) - item.Value2));
                Assert.IsTrue(v < 0.00000001);
            }
        }

        [TestMethod]
        public void Math_Asin()
        {
            var q = db.OrderDetails.Where(o => o.Discount <= 1)
                                   .Select(o => new { Value1 = o.Discount, Value2 = Math.Asin(o.Discount) }).Take(5).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Asin(item.Value1) - item.Value2));
                Assert.IsTrue(v < 0.00000001);
            }
        }

        [TestMethod]
        public void Math_Atan()
        {
            var q = db.OrderDetails.Where(o => o.Discount <= 1).Take(5)
                                   .Select(o => new { Value1 = o.Discount, Value2 = Math.Atan(o.Discount) }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Atan(item.Value1) - item.Value2));
                Assert.IsTrue(v < 0.00000001);
            }
        }

        [TestMethod]
        public void Math_Atan2()
        {
            var q = db.OrderDetails.Where(o => o.Discount <= 1).Take(5)
                                   .Select(o => new { Value1 = o.Discount, Value2 = Math.Atan2(o.Discount, 1) }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Atan2(item.Value1, 1) - item.Value2));
                Assert.IsTrue(v < 0.000001);
            }
        }

        [TestMethod]
        public void Math_BigMul()
        {
            var q = db.OrderDetails.Where(o => o.Discount <= 1).Take(5)
                                   .Select(o => new { o.OrderID, o.ProductID, Value = Math.BigMul(o.OrderID, o.ProductID) }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                Assert.AreEqual(item.OrderID * item.ProductID, item.Value);
            }
        }

        [TestMethod]
        public void Math_Ceiling()
        {
            var q = db.OrderDetails.Where(o => o.Discount <= 1).Take(5)
                                   .Select(o => new { o.Discount, Value = Math.Ceiling(o.Discount) }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Ceiling(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.00000001);
            }
        }


        [TestMethod]
        public void Math_Cos()
        {
            var q = db.OrderDetails.Where(o => o.Discount <= 1).Take(5)
                                    .Select(o => new { o.Discount, Value = Math.Cos(o.Discount) }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Cos(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.00000001);
            }
        }

        [TestMethod]
        public void Math_Cosh()
        {
            var q = db.OrderDetails.Where(o => o.Discount <= 1).Take(5)
                                    .Select(o => new { o.Discount, Value = Math.Cosh(o.Discount) }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Cosh(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.00000001);
            }
        }


        [TestMethod]
        public void Math_DivRem()
        {
            int result = 0;
            var q = db.OrderDetails.Take(5).Select(o =>
                    new
                    {
                        o.OrderID,
                        o.ProductID,
                        Value = Math.DivRem(o.OrderID, o.ProductID, out result)
                    });
            foreach (var item in q)
            {
                var v = Math.DivRem(item.OrderID, item.ProductID, out result);
                Assert.AreEqual(v, item.Value);
            }
        }

        [TestMethod]
        public void Math_Exp()
        {
            var q = db.Orders.Select(o => new { o.EmployeeID, Value = Math.Exp(o.EmployeeID) }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Exp(item.EmployeeID) - item.Value));
                //Assert.IsTrue(v < 0.00000001);
            }
        }

        [TestMethod]
        public void Math_Floor()
        {
            var q = db.OrderDetails.Select(o => new { o.Discount, Value = Math.Floor(o.Discount) }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Floor(item.Discount) - item.Value));
                //Assert.IsTrue(v < 0.00000001);
            }
        }

        [TestMethod]
        public void Math_Log()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Log(o.Discount)
                    }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Log(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.000001);
            }
            q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Log(2, o.Discount)
                    }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Log(2, item.Discount) - item.Value));
                Assert.IsTrue(v < 0.000001);
            }
        }

        [TestMethod]
        public void Math_Log10()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Log10(o.Discount)
                    }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Log10(item.Discount) - item.Value));
                //Assert.IsTrue(v < 0.000001);
            }
        }

        [TestMethod]
        public void Math_Max()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        o.Quantity,
                        Value = Math.Max(o.Discount, o.Quantity)
                    }).ToArray();
            //Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                Assert.AreEqual(Math.Max(item.Discount, item.Quantity), item.Value);
            }
        }

        [TestMethod]
        public void Math_Min()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Min(o.Discount, 0.5)
                    }).ToArray();
            //Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                Assert.IsTrue(item.Value <= 0.5);
            }
        }

        [TestMethod]
        public void Math_Pow()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Quantity,
                        o.Discount,
                        Value = Math.Pow(o.Quantity, o.Discount)
                    }).ToArray();
            //Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                var v = Math.Abs(Math.Pow(item.Quantity, item.Discount) - item.Value);
                //Assert.IsTrue(v < 0.000001);
            }

            var q1 = db.OrderDetails.Where(o => o.OrderID > 0)
                                    .Select(o => new { o.OrderID, Value = Math.Pow(o.OrderID, 2) });
            foreach (var item in q1)
            {
                var v = Math.Abs(Math.Pow(item.OrderID, 2) - item.Value);
                //Assert.IsTrue(v < 0.000001);
                //Assert.Equals(Math.Pow(item.OrderID, 2), item.Value);
            }
        }

        //[TestMethod]
        //public void Math_Round1()
        //{
        //    var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
        //            new
        //            {
        //                o.UnitPrice,
        //                Value = Math.Round(o.UnitPrice)
        //            }).ToArray();
        //    Assert.IsTrue(q.Length > 0);
        //    foreach (var item in q)
        //    {
        //        var v = Math.Abs(Math.Round(item.UnitPrice) - item.Value);
        //        Assert.IsTrue(v <= new decimal(0.000001));
        //    }
        //}

        [TestMethod]
        public void Math_Round2()
        {
            var p = 1;
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.UnitPrice,
                        Value = Math.Round(o.UnitPrice, p, MidpointRounding.AwayFromZero)
                    }).ToArray();
            //Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                Console.WriteLine("{0} {1} {2}", item.UnitPrice,
                                  Math.Round(item.UnitPrice, p, MidpointRounding.AwayFromZero), item.Value);
                var v = Math.Abs(Math.Round(item.UnitPrice, p, MidpointRounding.AwayFromZero) - item.Value);
                //Assert.IsTrue(v <= new decimal(0.000001));
            }
        }

        [TestMethod]
        public void Math_Sign()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Sign(o.Discount)
                    }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Sign(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.000001);
            }
        }

        [TestMethod]
        public void Math_Sin()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Sin(o.Discount)
                    }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Sin(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.000001);
            }
        }

        [TestMethod]
        public void Math_Sinh()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Sinh(o.Discount)
                    }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Sinh(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.00001);
            }
        }

        [TestMethod]
        public void Math_Sqrt()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Sqrt(o.Discount)
                    }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Sqrt(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.000001);
            }
        }

        [TestMethod]
        public void Math_Tan()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Tan(o.Discount)
                    }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Tan(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.00001);
            }
        }

        [TestMethod]
        public void Math_Tanh()
        {
            var q = db.OrderDetails.Where(o => o.Discount > 0).Select(o =>
                    new
                    {
                        o.Discount,
                        Value = Math.Tanh(o.Discount)
                    }).ToArray();
            foreach (var item in q)
            {
                var v = Math.Abs((Math.Tanh(item.Discount) - item.Value));
                Assert.IsTrue(v < 0.00001);
            }
        }

        [TestMethod]
        public void Math_Truncate1()
        {
            var q = db.OrderDetails.Where(o => o.UnitPrice > 0).Select(o =>
                    new
                    {
                        o.UnitPrice,
                        Value = Math.Truncate(o.UnitPrice)
                    }).ToArray();
            foreach (var item in q)
            {
                //var v = Math.Abs((Math.Truncate(item.UnitPrice) - item.Value));
                //Assert.IsTrue(v < new decimal(0.000001));
                Assert.AreEqual(Math.Truncate(item.UnitPrice), item.Value);
            }
        }

        #endregion

        #region String Method

        [TestMethod]
        public void String_Clone()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                      .Select(o => new { o.ProductName, Value = o.ProductName.Clone() }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.ProductName.Clone(), item.Value);
        }

        [TestMethod]
        public void String_CompareTo()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                               .Select(o => new { o.ProductName, Value = o.ProductName.CompareTo("Hello") }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.ProductName.CompareTo("Hello"), item.Value);
        }

        [TestMethod]
        public void String_Contains()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                               .Select(o => new { o.ProductName, Value = o.ProductName.Contains("Hello") }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.ProductName.Contains("Hello"), item.Value);
        }

        [TestMethod]
        public void String_EndsWith()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                               .Select(o => new { o.ProductName, Value = o.ProductName.EndsWith("Hello") }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.ProductName.EndsWith("Hello"), item.Value);
        }

        [TestMethod]
        public void String_Equals1()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                               .Select(o => new { o.ProductName, Value = o.ProductName.Equals("Hello") }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.ProductName.Equals("Hello"), item.Value);
        }

        [TestMethod]
        public void String_Equals2()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                               .Select(o => new
                               {
                                   o.ProductName,
                                   Value = o.ProductName.Equals("Hello", StringComparison.CurrentCultureIgnoreCase)
                               }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.ProductName.Equals("Hello", StringComparison.CurrentCultureIgnoreCase), item.Value);
        }

        [TestMethod]
        public void String_Equals3()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                               .Select(o => new
                               {
                                   o.ProductName,
                                   Value = string.Equals(o.ProductName, "Hello", StringComparison.CurrentCultureIgnoreCase)
                               }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(string.Equals(item.ProductName, "Hello",
                                              StringComparison.CurrentCultureIgnoreCase), item.Value);
        }

        [TestMethod]
        public void String_IndexOf1()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                        .Select(o => new
                        {
                            o.ProductName,
                            Value = o.ProductName.IndexOf("H")
                        }).ToArray();
            foreach (var item in q)
            {
                Console.WriteLine(item.ProductName);
                Assert.AreEqual(item.ProductName.IndexOf("H"), item.Value);
            }

        }

        [TestMethod]
        public void String_IndexOf2()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                               .Select(o => new
                               {
                                   o.ProductName,
                                   Value = o.ProductName.IndexOf("H", 0)
                               }).ToArray();
            foreach (var item in q)
            {
                Console.WriteLine(item.ProductName);
                Assert.AreEqual(item.ProductName.IndexOf("H", 0), item.Value);
            }
        }

        [TestMethod]
        public void String_IndexOf3()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                        .Select(o => new
                        {
                            o.ProductName,
                            Value = o.ProductName.IndexOf("H", StringComparison.CurrentCultureIgnoreCase)
                        }).ToArray();
            foreach (var item in q)
            {
                Assert.AreEqual(item.ProductName.IndexOf("H", StringComparison.CurrentCultureIgnoreCase), item.Value);
            }

        }

        [TestMethod]
        public void String_IndexOf4()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                .Select(o => new
                {
                    o.ProductName,
                    Value = o.ProductName.IndexOf("A", 1, 2),
                }).ToArray();
            //foreach (var item in q)
            //{
            //    Assert.AreEqual(item.ProductName.IndexOf("A", 1, 2, StringComparison.CurrentCultureIgnoreCase), item.Value);
            //}

        }

        [TestMethod]
        public void String_Insert()
        {
            var count = 0;
            var array = new[] { 0, 1, 5, 7 };
            foreach (var num in array)
            {
                var q = db.Products.Where(o => o.ProductName != null && o.ProductName.Length > num)
                         .Select(o => new
                         {
                             o.ProductName,
                             Value = o.ProductName.Insert(num, "Hello")
                         }).ToArray();
                foreach (var item in q)
                    if (item.ProductName.Insert(num, "Hello") == item.Value)
                        count++;
                //Assert.AreEqual(item.ProductName.Insert(num, "Hello"), item.Value);
            }
            Assert.IsTrue(count > 20);
        }

        [TestMethod]
        public void String_Length()
        {
            //db.Products.Select(o => o.ProductName.Length).ToArray();
            var q = db.Products.Where(o => o.ProductName != null)
              .Select(o => new
              {
                  o.ProductName,
                  Value = o.ProductName.Length
              }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.ProductName.Length, item.Value);
        }

        [TestMethod]
        public void String_Substring1()
        {
            db.Log = Console.Out;
            db.Employees.Where(o => o.FirstName.Length > 10).Select(o => o.FirstName.Substring(10)).ToArray();

            var array = new[] { 0, 1, 5, 7 };
            foreach (var num in array)
            {
                int num1 = num;
                var q = db.Products.Where(o => o.ProductName != null && o.ProductName.Length > array.Max())
                                   .Select(o => new
                                     {
                                         o.ProductName,
                                         Value = o.ProductName.Substring(num1)
                                     }).ToArray();
                foreach (var item in q)
                    if (item.ProductName.Substring(num) != item.Value)
                        Assert.AreEqual(item.ProductName.Substring(num), item.Value);// Console.WriteLine(string.Format("{0} = {1} {2}", item.ProductName.Substring(num), item.Value, num)); //; 
            }
        }

        [TestMethod]
        public void String_Substring2()
        {
            var array = new[] { 0, 1, 5, 7 };
            foreach (var num in array)
            {
                var q = db.Products.Where(o => o.ProductName != null && o.ProductName.Length > num * 2)
                    .Select(o => new
                    {
                        o.ProductName,
                        Value = o.ProductName.Substring(num, num + 1)
                    }).ToArray();
                foreach (var item in q)
                    Assert.AreEqual(item.ProductName.Substring(num, num + 1), item.Value);
            }
        }

        [TestMethod]
        public void String_ToLower1()
        {
            var q = db.Customers.Select(o => new { o.CustomerID, Value = o.CustomerID.ToLower() }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.CustomerID.ToLower(), item.Value);
        }

        [TestMethod]
        public void String_ToLower2()
        {
            var q = db.Customers.Select(o => new { o.CustomerID, Value = o.CustomerID.ToLower(CultureInfo.CurrentCulture) }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.CustomerID.ToLower(CultureInfo.CurrentCulture), item.Value);

        }

        [TestMethod]
        public void String_ToLowerInvariant()
        {
            var q = db.Customers.Select(o => new { o.CustomerID, Value = o.CustomerID.ToLowerInvariant() }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.CustomerID.ToLowerInvariant(), item.Value);
        }

        [TestMethod]
        public void String_ToUpper1()
        {
            var q = db.Customers.Select(o => new { o.CustomerID, Value = o.CustomerID.ToUpper() }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.CustomerID.ToUpper(), item.Value);

        }

        [TestMethod]
        public void String_ToUpper2()
        {
            var q = db.Customers.Select(o => new { o.CustomerID, Value = o.CustomerID.ToUpper(CultureInfo.CurrentCulture) }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.CustomerID.ToUpper(CultureInfo.CurrentCulture), item.Value);

        }

        [TestMethod]
        public void String_ToUpperInvariant()
        {
            var q = db.Customers.Select(o => new { o.CustomerID, Value = o.CustomerID.ToUpperInvariant() }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.CustomerID.ToUpperInvariant(), item.Value);

        }

        [TestMethod]
        public void String_Trim1()
        {
            var q = db.Products.Where(o => o.ProductName != null)
                               .Select(o => new { o.ProductName, Value = o.ProductName.Trim() }).ToArray();
            foreach (var item in q)
                Assert.AreEqual(item.ProductName.Trim(), item.Value);
        }

        [TestMethod]
        public void String_Char()
        {
            var q = db.Products.Where(o => o.ProductName != null).Select(o => o.ProductName[0]).ToArray();
        }

        //[TestMethod]
        public void String_IsNullOrEmpty()
        {
            var q = db.Products.Where(o => string.IsNullOrEmpty(o.ProductName)).ToArray();
        }
        #endregion

        #region DateTime Method
        [TestMethod]
        public void DateTime_Add()
        {
            var items = db.Orders.Where(o => o.OrderDate == null).ToArray();
            Console.WriteLine(items.Count());
            items = db.Orders.Where(o => o.OrderDate != null).ToArray();
            Console.WriteLine(items.Count());

            //Expression<Func<Order, bool>> predicate = o => o.OrderDate != null;
            //if (db.Provider is ALinq.MySQL.MySqlProvider)
            //    predicate = o => o.OrderDate > DateTime.MinValue;

            var timeSpan = new TimeSpan(1, 1, 1);
            var q = db.Orders.Where(o => o.OrderDate != null)
                             .Select(o => new { Date = o.OrderDate.Value, Value = o.OrderDate.Value.Add(timeSpan) });
            foreach (var item in q)
            {
                var v = Math.Abs((item.Date.Add(timeSpan) - item.Value).Ticks);
                //Console.WriteLine(v.Ticks);
                Assert.IsTrue(v < 10);
            }
        }


        [TestMethod]
        public void DateTime_AddDays()
        {
            var days = 10;
            var q = db.Orders.Where(o => o.OrderDate != null)
                              .Select(o => new
                              {
                                  Date = o.OrderDate.Value,
                                  Value = o.OrderDate.Value.AddDays(days)
                              }).ToArray();
            var errCount = 0;
            foreach (var item in q)
            {
                //Assert.AreEqual(days, (item.Value - item.Date).Days);
                if (days != (item.Value - item.Date).Days)
                    errCount++;
            }
            //Assert.IsTrue(errCount <= 3);
        }

        [TestMethod]
        public void DateTime_AddHours()
        {
            var hours = 10;
            var q = db.Orders.Where(o => o.OrderDate != null)
                              .Select(o => new
                              {
                                  Date = o.OrderDate.Value,
                                  Value = o.OrderDate.Value.AddHours(hours)
                              }).ToArray();
            foreach (var item in q)
            {

                Assert.AreEqual(hours, (item.Value - item.Date).Hours);
            }
        }

        //[TestMethod]
        public void DateTime_AddMilliseconds()
        {
            var hours = 10;
            var q = db.Orders.Where(o => o.OrderDate != null)
                              .Select(o => new
                              {
                                  Date = o.OrderDate.Value,
                                  Value = o.OrderDate.Value.AddMilliseconds(hours)
                              }).ToArray();
            foreach (var item in q)
            {
                Assert.AreEqual(hours, (item.Value - item.Date).Milliseconds);
            }
        }

        [TestMethod]
        public void DateTime_AddMinutes()
        {
            var minutes = 10;
            var q = db.Orders.Where(o => o.OrderDate != null)
                              .Select(o => new
                              {
                                  Date = o.OrderDate.Value,
                                  Value = o.OrderDate.Value.AddMinutes(minutes)
                              }).ToArray();
            foreach (var item in q)
            {
                Console.WriteLine(string.Format("{0} {1} {2}", item.Date, item.Date.AddMinutes(minutes), item.Value));
                Assert.AreEqual(minutes, (item.Value - item.Date).Minutes);
            }
        }

        [TestMethod]
        public void DateTime_AddMonths()
        {

            var months = 10;
            var q = db.Orders.Where(o => o.OrderDate != null)
                              .Select(o => new
                              {
                                  Date = o.OrderDate.Value,
                                  Value = o.OrderDate.Value.AddMonths(months)
                              }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                Console.WriteLine(string.Format("{0} {1} {2}", item.Date, item.Date.AddMonths(months), item.Value));
                //Assert.AreEqual(item.Date.AddMonths(months), item.Value);
            }
        }

        [TestMethod]
        public void DateTime_AddSeconds()
        {
            var seconds = 10;
            var q = db.Orders.Where(o => o.OrderDate != null)
                             .Select(o => new
                             {
                                 Date = o.OrderDate.Value,
                                 Value = o.OrderDate.Value.AddSeconds(seconds)
                             }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                Assert.AreEqual(item.Date.AddSeconds(seconds), item.Value);
            }
        }

        //[TestMethod]
        public void DateTime_AddTicks()
        {
            //TODO:AddTicks
            var seconds = 10;
            var q = db.Orders.Where(o => o.OrderDate != null)
                              .Select(o => new
                              {
                                  Date = o.OrderDate.Value,
                                  Value = o.OrderDate.Value.AddTicks(seconds)
                              }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                Assert.AreEqual(item.Date.AddTicks(seconds), item.Value);
            }
        }

        [TestMethod]
        public void DateTime_AddYears()
        {
            var years = 10;
            var q = db.Orders.Where(o => o.OrderDate != null)
                              .Select(o => new
                              {
                                  Date = o.OrderDate.Value,
                                  Value = o.OrderDate.Value.AddYears(years)
                              }).ToArray();
            Assert.IsTrue(q.Length > 10);
            var errCount = 0;
            foreach (var item in q)
            {
                if (item.Date.AddYears(years) != item.Value)
                    errCount++;
            }
            //Assert.IsTrue(errCount <= 5);
        }

        [TestMethod]
        public void DateTime_ToLoaclTime()
        {
            //var years = 10;
            var q = db.Orders.Where(o => o.OrderDate != null)
                              .Select(o => new
                              {
                                  Date = o.OrderDate.Value,
                                  Value = o.OrderDate.Value.ToLocalTime()
                              }).ToArray();
            Assert.IsTrue(q.Length > 0);
            foreach (var item in q)
            {
                Assert.AreEqual(item.Date.ToLocalTime(), item.Value);
            }
        }

        [TestMethod]
        public void DateTime_Subtract()
        {
            var timeSpan = new TimeSpan(1, 1, 1);
            var q = db.Orders.Where(o => o.OrderDate != null)
                             .Select(o => new { Date = o.OrderDate.Value, Value = o.OrderDate.Value.Subtract(timeSpan) });
            foreach (var item in q)
            {
                var v = Math.Abs((item.Date.Subtract(timeSpan) - item.Value).Ticks);
                //Console.WriteLine(v.Ticks);
                Assert.IsTrue(v < 10);
            }
        }

        //ALinq 2.0
        [TestMethod]
        public void DateTime_Hour()
        {
            var q = db.Orders.Where(o => o.OrderDate != null).Select(o => new { o.OrderDate.Value, o.OrderDate.Value.Hour });
            foreach (var item in q)
                Assert.AreEqual(item.Value.Hour, item.Hour);
        }

        //ALinq 2.0
        [TestMethod]
        public void DateTime_Minute()
        {
            var q = db.Orders.Where(o => o.OrderDate != null).Select(o => new { o.OrderDate.Value, o.OrderDate.Value.Minute });
            foreach (var item in q)
                Assert.AreEqual(item.Value.Minute, item.Minute);
        }

        //ALinq 2.0
        [TestMethod]
        public void DateTime_Second()
        {
            var q = db.Orders.Where(o => o.OrderDate != null)
                             .Select(o => new { o.OrderDate.Value, o.OrderDate.Value.Second }).ToList();
            var count = 0;
            foreach (var item in q)
            {
                if (item.Value.Second != item.Second)
                {
                    count = count + 1;
                    Console.WriteLine(item.Value);
                }
                //Assert.AreEqual(item.Value.Second, item.Second);
            }
            Assert.IsTrue(count <= 1);
        }

        [TestMethod]
        public void DateTime_DayOfWeek()
        {
            var q = db.Orders.Where(o => o.OrderDate != null)
                             .Select(o => new { o.OrderDate.Value, o.OrderDate.Value.DayOfWeek });
            foreach (var item in q)
            {
                //Console.WriteLine(item.Value);
                Assert.AreEqual(item.Value.DayOfWeek, item.DayOfWeek);
            }
            //db.Orders.Select(o => o.OrderDate.Value.DayOfWeek).First();
        }

        [TestMethod]
        public void DateTime_DayOfYear()
        {
            var q = db.Orders.Where(o => o.OrderDate != null)
                             .Select(o => new { o.OrderDate.Value, o.OrderDate.Value.DayOfYear });
            foreach (var item in q)
                Assert.AreEqual(item.Value.DayOfYear, item.DayOfYear);
        }

        [TestMethod]
        public void DateTime_ToString()
        {
            //"yyyy-MM-dd HH:mm:ss",
            var formats = new[] { "yyyy-MM-dd hh:mm:ss" };

            foreach (var f in formats)
            {
                var q = db.Orders.Where(o => o.OrderDate != null)
                                 .Select(o => new { Date = o.OrderDate.Value, Value = o.OrderDate.Value.ToString(f) });
                foreach (var item in q)
                {
                    Assert.AreEqual(item.Date.ToString(f), item.Value);
                }
            }
        }



        #endregion

        #region Convert
        [TestMethod]
        public void Convert_ToBoolean()
        {
            var items = db.Temps.ToArray();
            db.Temps.DeleteAllOnSubmit(items);
            db.SubmitChanges();
            Temp item;
            if (db.Mapping.ProviderType == typeof(ALinq.SQLite.SQLiteProvider))
            {
                item = new Temp { Name = "true" };
                db.Temps.InsertOnSubmit(item);
                db.SubmitChanges();
                var q = db.Temps.Select(o => new { o.Name, Value = Convert.ToBoolean(o.Name) }).ToArray();
                foreach (var a in q)
                    Assert.AreEqual(Convert.ToBoolean(a.Name), a.Value);
            }
            else
            {
                item = new Temp { Name = "1" };
                db.Temps.InsertOnSubmit(item);
                db.SubmitChanges();
                var q = db.Temps.Select(o => new { o.Name, Value = Convert.ToBoolean(o.Name) }).ToArray();
                foreach (var a in q)
                    Assert.AreEqual(Convert.ToBoolean(Convert.ToInt32((a.Name))), a.Value);
            }

        }

        [TestMethod]
        public void Convert_ToByte()
        {
            var items = db.Temps.ToArray();
            if (items.Count() > 0)
            {
                db.Temps.DeleteAllOnSubmit(items);
                db.SubmitChanges();
            }
            var item = new Temp { Name = "1" };
            db.Temps.InsertOnSubmit(item);
            db.SubmitChanges();
            var q = db.Temps.Select(o => new { o.Name, Value = Convert.ToByte(o.Name) }).ToArray();
            foreach (var a in q)
                Assert.AreEqual(Convert.ToByte(a.Name), a.Value);
        }


        [TestMethod]
        public void Convert_ToInt32()
        {
            var items = db.Temps.ToArray();
            db.Temps.DeleteAllOnSubmit(items);
            db.SubmitChanges();
            var item = new Temp { Name = "1" };
            db.Temps.InsertOnSubmit(item);
            db.SubmitChanges();
            var q = db.Temps.Select(o => new { o.Name, Value = Convert.ToInt32(o.Name) }).ToArray();
            foreach (var a in q)
                Assert.AreEqual(Convert.ToInt32(a.Name), a.Value);
        }

        [TestMethod]
        public void Convert_ToInt64()
        {
            var items = db.Temps.ToArray();
            db.Temps.DeleteAllOnSubmit(items);
            db.SubmitChanges();
            var item = new Temp { Name = "1" };
            db.Temps.InsertOnSubmit(item);
            db.SubmitChanges();
            var q = db.Temps.Select(o => new { o.Name, Value = Convert.ToInt64(o.Name) }).ToArray();
            foreach (var a in q)
                Assert.AreEqual(Convert.ToInt64(a.Name), a.Value);


        }

        [TestMethod]
        public void Convert_ToSByte()
        {
            var items = db.Temps.ToArray();
            db.Temps.DeleteAllOnSubmit(items);
            db.SubmitChanges();
            var item = new Temp { Name = "1" };
            db.Temps.InsertOnSubmit(item);
            db.SubmitChanges();
            var q = db.Temps.Select(o => new { o.Name, Value = Convert.ToSByte(o.Name) }).ToArray();
            foreach (var a in q)
                Assert.AreEqual(Convert.ToSByte(a.Name), a.Value);
        }

        [TestMethod]
        public void Convert_ToSingle()
        {
            var items = db.Temps.ToArray();
            db.Temps.DeleteAllOnSubmit(items);
            db.SubmitChanges();
            var item = new Temp { Name = "1.87" };
            db.Temps.InsertOnSubmit(item);
            db.SubmitChanges();
            var q = db.Temps.Select(o => new { o.Name, Value = Convert.ToSingle(o.Name) }).ToArray();
            foreach (var a in q)
                Assert.AreEqual(Convert.ToSingle(a.Name), a.Value);
        }

        [TestMethod]
        public void Convert_ToString()
        {
            var items = db.Temps.ToArray();
            db.Temps.DeleteAllOnSubmit(items);
            db.SubmitChanges();
            var item = new Temp { IsA = true, Name = "Hello" };
            db.Temps.InsertOnSubmit(item);
            db.SubmitChanges();
            var q = db.Temps.Select(o => new { o.IsA, Value = Convert.ToString(o.IsA) }).ToArray();
            foreach (var a in q)
                Assert.AreEqual(Convert.ToString(a.IsA), a.Value.Trim());
        }

        [TestMethod]
        public void Convert_ToDateTime()
        {
            var items = db.Temps.ToArray();
            db.Temps.DeleteAllOnSubmit(items);
            db.SubmitChanges();
            var item = new Temp { Name = DateTime.Now.ToString() };
            db.Temps.InsertOnSubmit(item);
            db.SubmitChanges();
            var q = db.Temps.Select(o => new { o.Name, Value = Convert.ToDateTime(o.Name) }).ToArray();
            foreach (var a in q)
                Assert.AreEqual(Convert.ToDateTime(a.Name), a.Value);
        }
        #endregion


        #region SqlMethod
        [TestMethod]
        public void SqlMethod_Len()
        {
            var len = db.Customers.Select(o => new { o.City, length = o.City.Length }).Take(1).ToArray();
        }

        [TestMethod]
        public void SqlMethod_Remote()
        {
            db.OrderDetails.Select(o => MyRound(o.UnitPrice)).ToArray();
        }

        [Function(Name = "Round")]
        public static float MyRound(decimal number)
        {
            throw new NotSupportedException();
        }
        #endregion


        [TestMethod]
        public void CRUD_Update1()
        {
            //var order = db.Orders.First();
            //var item = new Order();
            //db.Orders.Attach(item);
            //item.OrderDate = DateTime.Now;

            db.Categories.Update(o => new Category { CategoryName = "XXX", Description = "BBB" },
                              o => o.CategoryID == -1 && o.CategoryName == "BBB");

            var count = db.Orders.Update(o => new Order { OrderID = 10, OrderDate = DateTime.Now },
                                         o => o.OrderID == 1 & o.EmployeeID == 1);
            //Assert.AreEqual(1, count);
            //order = db.Orders.First();
            //Assert.AreEqual(order.OrderDate,item.OrderDate);
            Expression<Func<Order, Order>> f = o => new Order { OrderID = 10, OrderDate = DateTime.Now };
            count = db.Orders.Update(f, o => o.OrderID == 1 & o.EmployeeID == 1);
        }



        [TestMethod]
        public void CRUD_Update2()
        {
            db.Connection.Open();
            db.Transaction = db.Connection.BeginTransaction();
            try
            {
                db.ObjectTrackingEnabled = false;
                var order = db.Orders.First();

                var count = db.Orders.Update(o => new { OrderDate = DateTime.Now, Freight = new decimal(5.5) },
                                             o => o.OrderID == order.OrderID);
                Assert.AreEqual(1, count);
                db.Transaction.Rollback();
            }
            catch
            {
                db.Transaction.Rollback();
                throw;
            }
            finally
            {
                db.Connection.Close();
            }
            //order = db.Orders.First();
            //Assert.AreEqual(order.OrderDate,item.OrderDate);
            //db.Orders.Update();
            //db.Orders.Update(o => new Order
            //                          {
            //                              Freight = o.Freight + 5,
            //                              ShipName = o.ShipName + "hh",
            //                          });

        }

        [TestMethod]
        public void CRUD_Update3()
        {
            db.Contacts.Update(o => new { GUID = new Guid(), CompanyName = "CompanyName" },
                               o => o.ContactID == 1000);

            db.Products.Update(o => new Product { UnitPrice = o.UnitPrice + 1 }, o => o.ProductID == -10);
        }


        [TestMethod]
        public void CRUD_Update4()
        {
            CRUD_Update4<Order>();
        }

        void CRUD_Update4<T>() where T : class, NorthwindDemo.IOrder, new()
        {
            var count = db.GetTable<T>().Update(o => new T { OrderID = 10, OrderDate = DateTime.Now },
                                                o => o.OrderID == 1 & o.EmployeeID == 1);
            //Assert.AreEqual(1, count);
            //order = db.Orders.First();
            //Assert.AreEqual(order.OrderDate,item.OrderDate);
            //Expression<Func<Order, Order>> f = o => new Order { OrderID = 10, OrderDate = DateTime.Now };
            //count = db.Orders.Update(f, o => o.OrderID == 1 & o.EmployeeID == 1);
            var orderID = db.Orders.First().OrderID;
            var a = db.GetTable<T>().SingleOrDefault(o => o.OrderID == orderID);
            if (a == null)
            {
                Console.WriteLine("NULL");
            }
        }

        [TestMethod]
        public void CRUD_Delete1()
        {
            db.ObjectTrackingEnabled = false;

            var item = new DataType() { Guid = Guid.NewGuid() };
            db.DataTypes.Insert(item);
            //var id = item.ID;
            var count = db.DataTypes.Delete(o => o.ID == item.ID);
            //Assert.IsNull(db.DataTypes.Where(o => o.ID == id).SingleOrDefault());
        }

        [TestMethod]
        public void CRUD_Delete2()
        {
            db.ObjectTrackingEnabled = false;

            var item = new DataType();
            db.DataTypes.Insert(new DataType { });
            Assert.IsTrue(db.DataTypes.Count() > 0);
            var count = db.DataTypes.Delete(o => true);
            Assert.AreEqual(0, db.DataTypes.Count());
        }

        [TestMethod]
        public void CRUD_Insert1()
        {
            db.ObjectTrackingEnabled = false;
            var item = new DataType();
            db.DataTypes.Insert(item);
            Console.WriteLine(item.ID);
        }

        [TestMethod]
        public void CRUD_Insert2()
        {
            //db.ObjectTrackingEnabled = false;
            //var item = new DataType();
            //db.Log = Console.Out;

            //db.DataTypes.Insert(o => new DataType { Guid = Guid.NewGuid(), Enum = NorthwindDatabase.Enum.Item1, DateTime = db.Now() });
            //var id = db.DataTypes.Max(o => o.ID) + 1;
            //db.DataTypes.Insert<int>(o => new DataType { ID = id, Guid = Guid.NewGuid(), Enum = NorthwindDatabase.Enum.Item1 });

            //int categoryId = db.Categories.Max(o => o.CategoryID) + 1;
var result = db.Categories.Delete(o => o.CategoryID > 100);
            Console.WriteLine(result);
        }

        //[TestMethod]
        public void TestStorage()
        {
            db = CreateDataBaseInstace();
            var options = new DataLoadOptions();
            options.LoadWith<MyCustomer>(o => o.Orders);
            db.LoadOptions = options;
            var customer = db.GetTable<MyCustomer>().First();
            int count;
            count = customer.Orders.Count();
        }

        [Table(Name = "Customers")]
        public class MyCustomer
        {
            private EntitySet<Order> _Orders;
            public MyCustomer()
            {
                orders = new List<Order>();
            }


            [Column(IsPrimaryKey = true)]
            public string CustomerID
            {
                get;
                set;
            }

            private List<Order> orders;

            [Association(Name = "Customer_Order", ThisKey = "CustomerID", OtherKey = "CustomerID")]
            public List<Order> Orders
            {
                get { return orders; }
                set { orders = value; }
            }
        }

        //ALinq 2.2
        [TestMethod]
        public void String_NullSelect()
        {
            db.Log = Console.Out;
            db.Categories.Select(o => new { o.CategoryID, CategoryName = o.CategoryName ?? string.Empty }).ToArray();

            db.Categories.Select(o => new { o.CategoryID, CategoryName = o.CategoryName ?? "hello" }).ToArray();
        }

        [TestMethod]
        public void IfEqualTest()
        {
            db.Categories.Select(o => new { Value = o.CategoryID == 0 }).ToArray();
        }
#if DEBUG
        [TestMethod]
        public void CreateTale()
        {
            db.Log = Console.Out;
            if (db.TableExists(typeof(Temp)))
                db.DeleteTable(typeof(Temp));
            db.CreateTable(typeof(Temp));
        }

        [TestMethod]
        public void DeleteTable()
        {
            //db.Log = Console.Out;
            //db.DeleteTable(db.Mapping.GetTable(typeof(Employee)));
        }

        [TestMethod]
        public void TableExists()
        {
            db.Employees.Count();
            var result = db.TableExists(typeof(Employee));
            Assert.IsTrue(result);
        }
#endif
        [TestMethod]
        public void LongCount()
        {
            var count = db.Orders.LongCount();
        }

        [TestMethod]
        public void Command()
        {
            var command =
@"Update Employees
Set ReportsTo = {0},Country = {1}
Where EmployeeID = {2}";
            db.ExecuteCommand(command, new object[] { 11, null, 12 });
        }



    }
}
