using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ALinq.Dynamic;
using NorthwindDemo;
using System.Linq.Expressions;
using System.Linq;

namespace ALinq.Dynamic.Test
{


    [TestClass]
    public class EntitySqlTest : BaseTest
    {
        #region 标识符
        [TestMethod]
        public void Identifier1()
        {
            string esql;
            IEnumerable q;

            esql = "select e.[FirstName], e.[LastName] from Employee as e";
            q = db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void Identifier2()
        {
            var esql = "select c.ContactName as [Contact Name] from Customers as c";
            var q = db.CreateQuery(esql).Execute();
            //var item = q.ToArray()[0];

        }

        [TestMethod]
        public void Identifier3()
        {
            var esql = "select e.FirstName, e.LastName from [Employee] as e";
            var q = db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void Identifier4()
        {
            var esql = "select e.FirstName as [From], e.LastName as [Select] from Employee as e";
            var q = db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void Identifier5()
        {
            var esql = "select [e].FirstName, [e].LastName from Employee as [e]";
            var q = db.CreateQuery(esql).Execute();
        }

        //[TestMethod]
        //public void Identifier6()
        //{
        //    string esql;
        //    IEnumerable q;

        //    esql = "select FirstName, LastName from Employee as e";
        //    q = db.CreateQuery(esql).Execute();
        //}

        #endregion

        #region 参数

        [TestMethod]
        public void Parameter1()
        {
            var esql = @"select e from Employees as e
                where e.FirstName = @f and e.LastName = @l";
            var q = db.CreateQuery(esql, new ObjectParameter("l", "mak"), new ObjectParameter("f", "mike"));
            q.Execute();
        }

        [TestMethod]
        public void Parameter2()
        {
            var esql = @"select e from Employees as e
                         where e.FirstName = @0 and e.LastName = @1";
            var q = db.CreateQuery(esql, "mike", "mak");
            q.Execute();
        }
        #endregion

        #region 文字
        [TestMethod]
        public void IntegerLiteral()
        {
            string esql;

            esql = "123";
            var value = db.CreateQuery<object>(esql).Single();
            Assert.AreEqual(123, value);

            esql = "123u";
            value = db.CreateQuery<object>(esql).Single();
            Assert.AreEqual(123u, value);

            esql = "123U";
            value = db.CreateQuery<object>(esql).Single();
            Assert.AreEqual(123u, value);

            esql = "123l";
            value = db.CreateQuery<object>(esql).Single();
            Assert.AreEqual(123l, value);

            esql = "123L";
            value = db.CreateQuery<object>(esql).Single();
            Assert.AreEqual(123L, value);

            esql = "123ul";
            value = db.CreateQuery<object>(esql).Single();
            Assert.AreEqual(123ul, value);

            esql = "123UL";
            value = db.CreateQuery<object>(esql).Single();
            Assert.AreEqual(123UL, value);


        }

        [TestMethod]
        public void DecimalLiteral()
        {
            string esql;
            decimal d;

            esql = "123.67m";
            d = db.CreateQuery<decimal>(esql).Single();
            Console.WriteLine(d);

            esql = "123.67M";
            d = db.CreateQuery<decimal>(esql).Single();
            Console.WriteLine(d);
        }

        [TestMethod]
        public void DoubleLiteral()
        {
            string esql;
            double d;

            esql = "123.67";
            d = db.CreateQuery<double>(esql).Single();
            Assert.AreEqual(123.67, d);

            esql = "123.67E+3";
            d = db.CreateQuery<double>(esql).Single();
            Assert.AreEqual(123.67E+3, d);
        }

        [TestMethod]
        public void FloatLiteral()
        {
            string esql;
            float d;

            //esql = "123.67f";
            //d = db.CreateQuery<float>(esql).Single();
            //Assert.AreEqual(123.67f, d);

            //esql = "123.67F";
            //d = db.CreateQuery<float>(esql).Single();
            //Assert.AreEqual(123.67F, d);

            esql = "123f";
            d = db.CreateQuery<float>(esql).Single();
            Assert.AreEqual(123f, d);
        }

        [TestMethod]
        public void StringLiteral()
        {
            var esql = "'hello'";
            var text = db.CreateQuery<string>(esql).Single();
            Console.WriteLine(text);

            esql = "\"hello\"";
            text = db.CreateQuery<string>(esql).Single();
            Console.WriteLine(text);
        }

        [TestMethod]
        public void NullLiteral()
        {
            var esql = "null";
            var obj = db.CreateQuery<object>(esql).Single();
            Assert.AreEqual(null, obj);
        }

        //[TestMethod]
        //public void NullLiteral()
        //{
        //    var esql = "null";
        //    var obj = db.CreateQuery<object>(esql).Single();
        //    Assert.AreEqual(null, obj);
        //}

        [TestMethod]
        public void BooleanLiteral()
        {
            string esql;
            bool obj;

            esql = "true";
            obj = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, obj);

            esql = "false";
            obj = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, obj);
        }

        [TestMethod]
        public void DateTimeLiteral()
        {

            var esql = "#2006-12-25 01:01#";
            var datetime = db.CreateQuery<DateTime>(esql).Single();
            Console.WriteLine(datetime);

            var expected = new DateTime(2006, 12, 25, 1, 1, 0);
            Assert.AreEqual(expected, datetime);

            esql = "DATETIME '2006-12-25 01:01'";
            datetime = db.CreateQuery<DateTime>(esql).Single();
            Console.WriteLine(datetime);

            Assert.AreEqual(expected, datetime);

        }

        [TestMethod]
        public void GuidLiteral()
        {
            string esql;
            Guid guId;
            Guid expected;

            esql = "Guid'CD582A70-863B-4E4B-9530-174A83EEB0C0'";
            guId = db.CreateQuery<Guid>(esql).Single();

            expected = new Guid("CD582A70-863B-4E4B-9530-174A83EEB0C0");
            Assert.AreEqual(expected, guId);

            esql = "G'CD582A70-863B-4E4B-9530-174A83EEB0C0'";
            guId = db.CreateQuery<Guid>(esql).Single();
            //Assert.AreEqual(expected, guId);

        }

        [TestMethod]
        public void BinaryLiteral()
        {
            string esql;
            byte[] bytes;

            esql = "Binary '00ffaabb'";
            bytes = db.CreateQuery<byte[]>(esql).Single();
            Assert.IsTrue(bytes.Length > 0);

            esql = "X'00ffaabb'";
            bytes = db.CreateQuery<byte[]>(esql).Single();
            Assert.IsTrue(bytes.Length > 0);

            esql = "X''";
            bytes = db.CreateQuery<byte[]>(esql).Single();
            Assert.IsTrue(bytes.Length == 0);


        }




        #endregion

        #region 成员访问
        [TestMethod]
        public void MemberVisit1()
        {
            string esql;
            esql = "select o.OrderId, o.OrderDate from Orders as o";
            db.CreateQuery(esql).Execute();

            esql = "select o.OrderDate.Value.Year from Orders as o where o.OrderDate is not null";
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void MemberVisit2()
        {
            string esql;
            esql = "select o.Employee.FirstName from Orders as o";
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void MethodVisit()
        {
            string esql;

            esql = @"select o.OrderDate.Value.ToString('yyyy-MM-dd') as Date 
                     from Orders as o where o.OrderDate is not null";
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void ElementVisit()
        {
            var esql = "select o.OrderId, o.OrderDetails[0] as FirstOrderDetail from Orders as o";
            db.CreateQuery(esql).Execute();
        }
        #endregion

        #region 嵌套查询
        [TestMethod]
        public void NestedQuery1()
        {
            var esql = @"Row('mike' as FirstName, 'mak' as LastName, 
                    Row('ansiboy@163.com' as Email, 
                        '13434126607' as Phone) as Contact)";
#if NET4
            var row = db.CreateQuery<IDataRecord>(esql).Single() as dynamic;
            Console.WriteLine("{0} {1}", row.FirstName, row.LastName);
            Console.WriteLine("{0} {1}", row.Contact.Email, row.Contact.Phone);
#endif

        }

        [TestMethod]
        public void NestedQuery2()
        {
            var esql = @"select c.CategoryId, c.CategoryName, (select value count() from c.Products) as ProductCount
                from Categories as c
                limit 5";
#if NET4
            var items = db.CreateQuery<dynamic>(esql);
            foreach (var item in items)
            {
                Console.WriteLine("Id:{0}, Name：{1}，Products Count:{2}", item.CategoryId, item.CategoryName,
                                    item.ProductCount);
            }
#endif
        }

        #endregion

        #region 自定义函数
        [TestMethod]
        public void CustomFunction1()
        {
            var q = db.Employees.Select(o => db.GetFullName(o.FirstName, o.LastName));

            var esql = "select value GetFullName(e.FirstName, e.LastName) from Employees as e";
            q = db.CreateQuery<string>(esql);
            var items = q.Execute();
            foreach (var item in items)
                Console.WriteLine(item);

        }

        [TestMethod]
        public void CustomFunction2()
        {
            //Expression.MakeMemberAccess()
            var q = db.Employees.Select(o => db.GetFullName(o.FirstName, o.LastName));
            //GetFullName(e.FirstName, e.LastName),
            var esql = "select GetFullName(e.FirstName, e.LastName) as FullName from Employees as e";
            var items = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in items)
                Console.WriteLine(item);
        }
        #endregion

        #region 构造函数
        [TestMethod]
        public void Constructor1()
        {
            var esql = "select value Person(e.EmployeeId){ e.FirstName, e.LastName } from Employees as e";
            var q1 = db.CreateQuery<NorthwindDemo.Person>(esql)
                       .Where(p => p.FirstName == "xxxx");

            q1.Execute();

        }


        [TestMethod]
        public void Constructor2()
        {
            var esql = @"using ALinq.Dynamic.Test;
                         select value Person2{ e.FirstName, e.LastName } 
                         from Employees as e";
            var q1 = db.CreateQuery<ALinq.Dynamic.Test.Person2>(esql);

            q1.Execute();

            esql = @"select value ALinq.Dynamic.Test.Person2{ e.FirstName, e.LastName } 
                     from Employees as e";
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void Constructor3()
        {
            var esql = "System.DateTime(2012,11,5)";
            var result = db.CreateQuery<DateTime>(esql).Single();

        }

        [TestMethod]
        public void Constructor4()
        {
            var EmployeeId = 12345678;
            var esql = "NorthwindDemo.Person(123, 'mike' as FirstName, 'mak' as LastName)";
            var person = db.CreateQuery<NorthwindDemo.Person>(esql, EmployeeId).Single();
        }

        [TestMethod]
        public void Constructor5()
        {
            var esql = @"select value Person(e.EmployeeId, e.FirstName as FirstName, e.LastName as LastName) 
                         from Employees as e";
            var result = db.CreateQuery<Person>(esql).Execute();
        }



        #endregion

        #region 聚合函数
        [TestMethod]
        public void Count1()
        {
            string esql;
            esql = "select value count() from Products as p";
            var count = db.CreateQuery<int>(esql).Execute();
            Queryable.Count<Product>(db.Products);
            db.Categories.Select(c => new { ProudctsCount = Enumerable.Count<Product>(c.Products) });
        }

        [TestMethod]
        public void Count2()
        {
            string esql;
            esql = "select value count(c.Products) from Category as c";
            db.CreateQuery<int>(esql).Execute();
            db.Categories.Select(o => new { Count = Enumerable.Count<Product>(o.Products) }).Execute();
        }

        [TestMethod]
        public void Count3()
        {
            string esql;
            esql = "select c.CategoryId, count(c.Products) as ProductsCount from Categories as c";
            db.CreateQuery<IDataRecord>(esql).Execute();
            //var items = ef.CreateQuery<IDataRecord>(esql).Execute();
            esql = "select c.CategoryId, count(select value p.ProductId from c.Products as p) as ProductsCount from Categories as c";
            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
        }

        [TestMethod]
        public void Count4()
        {
            string esql;
            esql = "select c.CategoryId, count(select p from c.Products as p) as ProductsCount from Category as c";
            db.CreateQuery<IDataRecord>(esql).Execute();


        }

        [TestMethod]
        public void Count5()
        {
            string esql;
            esql = "select Count(p.CategoryId) from Products as p group by p.CategoryId";

            db.CreateQuery<IDataRecord>(esql).Execute();
        }

        [TestMethod]
        public void Count6()
        {
            string esql;
            esql = "select value count(p.CategoryId) from Products as p";
            db.CreateQuery(esql).Execute();
            db.Products.Count();
        }

        [TestMethod]
        public void CountSum1()
        {
            string esql = @"select count(), sum(p.UnitPrice) from Products as p";
            var q = db.CreateQuery<IDataRecord>(esql).Execute();
        }

        [TestMethod]
        public void CountSum2()
        {
            string esql;
            esql = @"select count(), sum(p.UnitPrice) from Products as p
                     group by p.CategoryId";

            var q = db.CreateQuery<IDataRecord>(esql).Execute();
        }

        [TestMethod]
        public void BigCount()
        {
#if L2S
            if (db.Mapping.ProviderType == typeof(System.Data.Linq.SqlClient.SqlProvider))
                return;
#endif
            var esql = "select value bigcount() from Products as p";
            var bigcount = db.CreateQuery<long>(esql).Execute();
            Console.WriteLine(bigcount);
        }

        [TestMethod]
        public void Avg()
        {
            string esql;
            esql = "select value avg(p.UnitPrice) from Products as p";
            var q1 = db.CreateQuery<decimal>(esql);
            foreach (var item in q1)
                Console.WriteLine(item);

            var value = db.CreateQuery<decimal>(esql).Single();
            Console.WriteLine(value);
        }

        [TestMethod]
        public void Max2()
        {
            string esql;
            esql = "select max(p.UnitPrice) from Products as p";
            var q2 = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in q2)
                Console.WriteLine(item[0]);
            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).Single();
            db.Products.GroupBy(o => o.CategoryId).Select(o => new { o.Key, c = o.Count() });
        }

        [TestMethod]
        public void Min1()
        {
            string esql;
            esql = "select value min(p.UnitPrice) from Products as p";
            var q1 = db.CreateQuery<decimal?>(esql);
            foreach (var item in q1)
                Console.WriteLine(item);

            var value = db.CreateQuery<decimal?>(esql).Single();
            Console.WriteLine(value);
        }

        [TestMethod]
        public void Min2()
        {
            string esql;
            esql = "select min(p.UnitPrice) from Products as p";
            var q2 = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in q2)
                Console.WriteLine(item[0]);
            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).Single();
        }

        [TestMethod]
        public void Min3()
        {
            string esql;
            esql = "select value min(p.UnitPrice) from Products as p group by p.CategoryId";
            db.Products.GroupBy(p => p.CategoryId).Select(o => o.Min(p => p.UnitPrice));
            var q1 = db.CreateQuery<decimal?>(esql);
            foreach (var item in q1)
                Console.WriteLine(item);
        }

        [TestMethod]
        public void Min5()
        {
            string esql;
            esql = "select min(select value p.UnitPrice from c.Products as p) from Categories as c";
            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void Min6()
        {
            string esql;
            esql = "select value min(o.ShippedDate) from Orders as o";
            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void Sum1()
        {
            string esql;
            esql = "select sum(o.UnitPrice) from OrderDetails as o";
            //db.OrderDetails.GroupBy(o => o.OrderId).Select(o => new { OrderId = o.Key, SumPrice = o.Select(a => a.UnitPrice).Sum() }).Execute();
            db.CreateQuery<IDataRecord>(esql).Execute();

            db.OrderDetails.Sum(o => o.UnitPrice);
        }

        [TestMethod]
        public void Sum2()
        {
            string esql;
            esql = "select key, sum(o.Quantity) as SumQuantity  from OrderDetails as o group by o.Product as key";
            var items = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
            db.OrderDetails.GroupBy(o => o.Product).Select(a => new { a.Key, s = Enumerable.Sum(a, b => b.UnitPrice) });
        }

        [TestMethod]
        public void Sum3()
        {
            string esql;
            esql = "select value sum(o.Quantity)  from OrderDetails as o group by o.ProductId";
            var items = db.CreateQuery<int>(esql);
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
            db.OrderDetails.GroupBy(o => o.ProductId).Select(o => o.Sum(a => a.Quantity)).Execute();
        }

        [TestMethod]
        public void Sum4()
        {
            string esql;
            esql = "select sum(select value p.UnitPrice from c.Products as p) from Categories as c";
            var items = db.CreateQuery(esql);
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
            db.Categories.Select(o => Enumerable.Sum(o.Products.Select(p => p.UnitPrice)));
        }


        #endregion

        #region 操作符
        [TestMethod]
        public void Add()
        {
            //数值相加
            string esql;
            object value;

            esql = "10 + 5";
            value = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(15, value);
        }

        [TestMethod]
        public void StringContact()
        {
            //字符串连接
            string esql;
            object value;

            esql = "'hello' + 'world'";
            value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("helloworld", value);

            esql = "\"hello\" + \"world\"";
            value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("helloworld", value);
        }

        [TestMethod]
        public void Negative()
        {
            //负号
            string esql;
            object value;

            esql = "-99";
            value = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(-99, value);
        }

        [TestMethod]
        public void Subtract()
        {
            // 相减
            string esql;
            object value;

            esql = "100-99";
            value = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void Multiply()
        {
            //相乘
            string esql;
            object value;

            esql = "3 * 5";
            value = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(15, value);
        }

        [TestMethod]
        public void DivIde()
        {
            //除
            string esql;
            object value;

            esql = "9 / 3";
            value = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(3, value);
        }

        [TestMethod]
        public void Modulo()
        {
            //除模
            var esql = "10 % 3";
            var value = db.CreateQuery<int>(esql).Single();
            Console.WriteLine(value);
        }

        [TestMethod]
        public void And()
        {
            //&& and
            string esql;
            bool value;

            esql = "true && true";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "true && false";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "true and true";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "true and false";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);
        }

        [TestMethod]
        public void Or()
        {
            //or || 
            string esql;
            bool value;

            esql = "true || false";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "false || false";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "true or false";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "false or false";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);
        }

        [TestMethod]
        public void Not()
        {
            //not !
            string esql;
            bool value;

            esql = "!false";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "!true";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "not false";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "not true";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);
        }

        [TestMethod]
        public void Equal()
        {
            // = ==
            var esql = "10 == 5";
            var value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "10 == 10";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "10 = 5";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "10 =10";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);
        }

        [TestMethod]
        public void Greater()
        {
            // >
            var esql = "10 > 5";
            var value = db.CreateQuery<bool>(esql).Single();
            Console.WriteLine(value);

            esql = "5 > 10";
            value = db.CreateQuery<bool>(esql, new ObjectParameter("p1", 5)).Single();
            Console.WriteLine(value);
        }

        [TestMethod]
        public void GreaterEqual()
        {
            // >=
            var esql = "10 >= 5";
            var value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "5 >= 10";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);
        }

        [TestMethod]
        public void Less()
        {
            var esql = "10 < 5";
            var value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "5 < 10";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);
        }

        [TestMethod]
        public void LessEqual()
        {
            var esql = "10 <= 5";
            var value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "10 >= 5";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);
        }

        [TestMethod]
        public void NotEqual()
        {
            // !=
            var esql = "10 != 5";
            var value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "10 != 10";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "10 <> 5";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);

            esql = "10 <> 10";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);
        }

        [TestMethod]
        public void Comment()
        {
            // --
            var esql = "select p from Products as p -- add a comment here";
            db.CreateQuery(esql).Execute();
        }



        #endregion

        #region 数学函数
        [TestMethod]
        public void Abs()
        {
            var esql = "Abs(-10)";
            var value = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(10, value);

            esql = "Abs(10)";
            value = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(10, value);

        }

        [TestMethod]
        public void Ceiling()
        {
            var esql = "Ceiling(12.4)";
            var value = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(13, value);
        }

        [TestMethod]
        public void Floor()
        {
            var esql = "Floor(13.4)";
            var value = db.CreateQuery<object>(esql).Single();

            Assert.AreEqual(Math.Floor(13.4), value);
        }

        [TestMethod]
        public void Power()
        {
            var esql = "Power(133, 4)";
            var value = db.CreateQuery<object>(esql).Single();

            Assert.AreEqual(Math.Pow(133, 4), value);
        }

        [TestMethod]
        public void Round()
        {
            var esql = "Round(748.58)";
            var value = db.CreateQuery<object>(esql).Single();

            Assert.AreEqual(Math.Round(748.58), value);

            esql = "Round(748.58,1)";
            value = db.CreateQuery<object>(esql).Single();

            Assert.AreEqual(Math.Round(748.58, 1), value);
        }

        [TestMethod]
        public void Truncate()
        {
            //var digits = 0;
            //var dec = 5 / Math.Pow(10, digits + 1);
            //Console.WriteLine(dec);
            //var value1 = Math.Round(748.59 + dec, digits) - dec - dec;
            //Console.WriteLine(value1);

            //var value2 = Math.Round(748.55 + dec, digits) - dec - dec;
            //Console.WriteLine(value2);

            //var value3 = Math.Round(748.54 + dec, digits) - dec - dec;
            //Console.WriteLine(value3);

            //var value4 = Math.Round(748.51 + dec, digits) - dec - dec;
            //Console.WriteLine(value4);

            //var value5 = Math.Round(748.50 + dec, digits) - dec - dec;
            //Console.WriteLine(value5);

            var esql = "Truncate(748.58, 1)";
            var value = db.CreateQuery<object>(esql).Single();

            //Assert.AreEqual(Math.Truncate(748.58), value);

        }

        [TestMethod]
        public void PI()
        {
            var esql = "pi()";
            var value = db.CreateQuery<double>(esql).Single();

            Assert.IsTrue(value > 3.14);

        }


        #endregion

        #region 字符串函数
        [TestMethod]
        public void Contact1()
        {
            var esql = "Concat('abc','123')";
            var value = db.CreateQuery<string>(esql).Single();

            Assert.AreEqual("abc123", value);
        }

        [TestMethod]
        public void Contact2()
        {
            var esql = "select value e.FirstName + ' ' + e.LastName from Employees as e";
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void Contains()
        {
            var esql = "Contains('abc','123')";
            var value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "Contains('abc','ab')";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);
        }

        [TestMethod]
        public void EndsWith()
        {
            var esql = "EndsWith('abc','123')";
            var value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(false, value);

            esql = "EndsWith('abc','bc')";
            value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);
        }

        [TestMethod]
        public void IndexOf()
        {
            var esql = "IndexOf('abc','123')";
            var value = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(-1, value);
        }

        [TestMethod]
        public void Left()
        {
            var esql = "left('abcdef',3)";
            var value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("abc", value);
        }

        [TestMethod]
        public void Length()
        {
            var esql = "length('abcd')";
            var value = db.CreateQuery<int>(esql).Single();

            Assert.AreEqual(4, value);
        }

        [TestMethod]
        public void LTrim()
        {
            var esql = "ltrim(' abcd')";
            var value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("abcd", value);
        }

        [TestMethod]
        public void Where1()
        {
            var esql = "select p from Products as p where p.UnitPrice > 10";
            var q = db.CreateQuery(esql);
            q.Execute();
        }

        [TestMethod]
        public void Replace()
        {
            var esql = "replace('abcd','ab','ee')";
            var value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("eecd", value);
        }

        [TestMethod]
        public void Reverse()
        {

            //string str = "abcd".Reverse();
            //var esql = "reverse('abcd')";
            //var value = db.CreateQuery<string>(esql).Single();
            //Assert.AreEqual("dcba", value);
        }

        [TestMethod]
        public void Right()
        {
            var esql = "right('abcxyz',3)";
            var value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("xyz", value);
        }

        [TestMethod]
        public void RTrim()
        {
            var esql = "rtrim('abcd ')";
            var value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("abcd", value);
        }

        [TestMethod]
        public void Substring()
        {
            var esql = "substring('abcdef',1,3)";
            var value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("bcd", value);

        }

        [TestMethod]
        public void Skip()
        {
            var esql = "select p from Products as p skip 10 limit 10";
            var products = db.CreateQuery<Product>(esql).Execute();
        }

        [TestMethod]
        public void StartsWith()
        {
            var esql = "startswith('abcdef','ab')";
            var value = db.CreateQuery<bool>(esql).Single();
            Assert.AreEqual(true, value);
        }

        [TestMethod]
        public void ToLower()
        {
            var esql = "tolower('ABCDE')";
            var value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("abcde", value);
        }

        [TestMethod]
        public void ToUpper()
        {
            var esql = "toupper('abcde')";
            var value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("ABCDE", value);
        }

        [TestMethod]
        public void Trim()
        {
            var esql = "trim(' abcd ')";
            var value = db.CreateQuery<string>(esql).Single();
            Assert.AreEqual("abcd", value);
        }
        #endregion

        #region 日期函数
        [TestMethod]
        public void Day()
        {
            var esql = "day(#2012-10-7#)";
            int day;

            day = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(7, day);

            esql = "day('2012-10-7')";
            day = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(7, day);

            esql = "Day(#2012-10-7#)";
            day = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(7, day);
        }

        [TestMethod]
        public void DayOfYear()
        {
            string esql;
            int day;
            esql = "dayofyear(#2012-10-7#)";
            day = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7).DayOfYear, day);

            esql = "dayofyear('2012-10-7')";
            day = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7).DayOfYear, day);
        }

        [TestMethod]
        public void Hour()
        {
            string esql;
            int hour;
            esql = "hour(#2012-10-7 10:12:32#)";
            hour = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7, 10, 12, 32).Hour, hour);

            esql = "hour('2012-10-7 10:12:32')";
            hour = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7, 10, 12, 32).Hour, hour);
        }

        [TestMethod]
        public void Minute()
        {
            string esql;
            int minute;

            esql = "minute(#2012-10-7 10:12:32#)";
            minute = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7, 10, 12, 32).Minute, minute);

            esql = "minute('2012-10-7 10:12:32')";
            minute = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7, 10, 12, 32).Minute, minute);
        }

        [TestMethod]
        public void Month()
        {
            string esql;
            int month;

            esql = "month(#2012-10-7#)";
            month = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7).Month, month);

            esql = "month('2012-10-7')";
            month = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7).Month, month);
        }

        [TestMethod]
        public void Secnod()
        {
            string esql;
            int second;

            esql = "second(#2012-10-7 10:12:32#)";
            second = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7, 10, 12, 32).Second, second);

            esql = "second('2012-10-7 10:12:32')";
            second = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7, 10, 12, 32).Second, second);
        }

        [TestMethod]
        public void Year()
        {
            string esql;
            int year;

            esql = "year(#2012-10-7#)";
            year = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7).Year, year);

            esql = "year('2012-10-7')";
            year = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(new DateTime(2012, 10, 7).Year, year);
        }
        #endregion

        #region 位函数
        [TestMethod]
        public void BitWiseAnd()
        {
            var esql = "BitWiseAnd(1,3)";
            var result = db.CreateQuery<int>(esql).Single();
            Console.WriteLine(result);
            Assert.AreEqual(1 & 3, result);

            esql = "BitWiseAnd(1u,3u)";
            var value1 = db.CreateQuery<uint>(esql).Single();
            Assert.AreEqual(1u & 3u, value1);
        }

        [TestMethod]
        public void BitWiseNot()
        {
            string esql;
            esql = "BitWiseNot(10)";
            var result = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(~10, result);

            esql = "BitWiseNot(10u)";
            var value1 = db.CreateQuery<uint>(esql).Single();
            Assert.AreEqual(~10u, value1);
        }

        [TestMethod]
        public void BitWiseOr()
        {
            var esql = "BitWiseOr(10,21)";
            var result = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(10 | 21, result);
        }

        [TestMethod]
        public void BitWiseXor()
        {
            var esql = "BitWiseXor(1,3)";
            var result = db.CreateQuery<int>(esql).Single();
            Assert.AreEqual(2, result);
            Assert.AreEqual(1 ^ 3, result);
        }

        #endregion

        #region 集合查询
        [TestMethod]
        public void Union()
        {
            var esql = @"(select p.ProductId, p.ProductName from Products as p where p.UnitPrice < 100 )
                         union
                         (select p.ProductId, p.ProductName from Products as p where p.UnitPrice > 200)";
            var q = db.CreateQuery<IDataRecord>(esql);

            q.Execute();
        }

        [TestMethod]
        public void Except1()
        {
            var esql = @"(select p1.ProductName from Products as p1 
                         where p1.CategoryId = 2)
                         except
                         (select p2.ProductName from Products as p2
                         where p2.CategoryId = 3)";

            db.CreateQuery<object>(esql).Execute();
        }

        [TestMethod]
        public void Except2()
        {
            var esql = @"(select value p1 from Products as p1 
                         where p1.CategoryId = 2)
                         except
                         (select value p2 from Products as p2
                         where p2.CategoryId = 3)";

            db.CreateQuery<object>(esql).Execute();
        }

        [TestMethod]
        public void Except3()
        {
            var esql = @"(select p1 from Products as p1 
                         where p1.CategoryId = 2)
                         except
                         (select p2 from Products as p2
                         where p2.CategoryId = 3)";

            db.CreateQuery<object>(esql).Execute();
        }

        [TestMethod]
        public void ExceptUnion()
        {
            var esql = @"(select p1 from Products as p1 where p1.CategoryId = 1)
                         except
                         (select p2 from Products as p2 where p2.CategoryId = 2)
                         union
                         (select p3 from Products as p3 where p3.CategoryId = 3)";

            var q = db.CreateQuery<object>(esql);
            Console.WriteLine(q.Expression);

            q.Execute();
        }

        [TestMethod]
        public void UnionExcept()
        {
            var esql = @"(select p1 from Products as p1 where p1.CategoryId = 1)
                         union
                         (select p2 from Products as p2 where p2.CategoryId = 2)
                         except
                         (select p3 from Products as p3 where p3.CategoryId = 3)";

            var q = db.CreateQuery<object>(esql);
            Console.WriteLine(q.Expression);

            q.Execute();
        }


        [TestMethod]
        public void Exists()
        {
            var esql = @"select p from Products as p 
                         where exists(select p from Products as p where p.UnitPrice > 0)";
            var items = db.CreateQuery<Product>(esql).Execute();

        }

        [TestMethod]
        public void NotExists()
        {
            var esql = @"select p from Products as p 
                         where not exists(select p from Products as p where p.UnitPrice > 0)";
            var items = db.CreateQuery<Product>(esql).Execute();
        }

        [TestMethod]
        public void INTERSECT()
        {
            var esql = @"(select p.ProductId, p.ProductName from Products as p where p.UnitPrice < 100 )
                intersect
                (select p.ProductId, p.ProductName from Products as p where p.UnitPrice > 200)";
            var q = db.CreateQuery<IDataRecord>(esql);
            q = q.Where(o => (string)o["ProductName"] == "AAAl");
            q.Execute();
        }
        #endregion

        #region Keywords（关键字）

        [TestMethod]
        public void Between()
        {
            db.Products.Where(p => p.ProductId >= 10 && p.ProductId <= 100);

            string esql;
            esql = "select p from Products as p where p.ProductId Between 10 and 100";
            var q = db.CreateQuery<Product>(esql).Execute();

            esql = "select p from Products as p where p.ProductId not between 10 and 100";
            db.CreateQuery<Product>(esql).Execute();
        }

        [TestMethod]
        public void Cast()
        {
            string esql;

            esql = "select value cast(c.CategoryId as string) from Categories as c";
            db.CreateQuery<string>(esql).Execute();

            esql = "select value cast(c.CategoryId as short) from Categories as c";
            db.CreateQuery<short>(esql).Execute();

            esql = "select value cast(c.CategoryId as int) from Categories as c";
            db.CreateQuery<int>(esql).Execute();

            esql = "select value cast(c.CategoryId as long) from Categories as c";
            db.CreateQuery<long>(esql).Execute();


        }

        //[TestMethod]
        //public void Collection()
        //{
        //    var esql = "select o from collection(Product) as o";
        //    ef.CreateQuery<Product>(esql).Execute(MergeOption.NoTracking);
        //}



        [TestMethod]
        public void Flatten()
        {
            var esql = @"flatten(select value o.OrderDetails from Orders as o)";
            //var items = ef.CreateQuery<EF.OrderDetail>(esql).Execute(MergeOption.NoTracking).ToArray();
            //Console.WriteLine(items.Length);
        }

        [TestMethod]
        public void From1()
        {
            var esql = @"(select p from Products as p)";
            var q = db.CreateQuery<Product>(esql);
            q.Execute();
        }

        [TestMethod]
        public void From2()
        {
            var esql = @"select p from Product as p";
            var q = db.CreateQuery<Product>(esql).Execute();
        }

        [TestMethod]
        public void From3()
        {
            var esql = @"select p from Product as p, Orders as o";
            var q = db.CreateQuery(esql);
            q.Execute();
        }

        [TestMethod]
        public void From4()
        {
            var esql = @"select od.o.OrderId as OrderId
                from ( select o, d 
                    from Orders as o 
                    inner join OrderDetails as d on o.OrderId = d.OrderId ) as od";
            var q = db.CreateQuery<object>(esql);
            q.Execute();
        }

        [TestMethod]
        public void From5()
        {
            var esql = @"select p from Products as p";
            var q = db.CreateQuery<Product>(esql).Execute();
        }

        [TestMethod]
        public void Group1()
        {
            string esql;
            IEnumerable<IDataRecord> items;
            //esql = "select SupplierId from Products as p group by p.SupplierId";
            //items = db.CreateQuery<IDataRecord>(esql).Execute();
            //foreach (var item in items)
            //    Console.WriteLine(item["SupplierId"]);

            //db.Products.GroupBy(o => o.SupplierId).Select(a => a.Select(b => b));

            esql = "select SupplierId from Products as p group by p.SupplierId";
            items = db.CreateQuery<IDataRecord>(esql).Execute();
            foreach (var item in items)
                Console.WriteLine(item["SupplierId"]);
            //ef.CreateQuery<string>(esql).Execute(MergeOption.NoTracking);
        }

        [TestMethod]
        public void Group2()
        {
            var q1 = db.Products.GroupBy(o => new { o.SupplierId }).Select(o => new { name = o.Key });

            var esql = "select name from Products as p group by p.SupplierId as name";
            var q = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in q)
                Console.WriteLine(item["name"]);
        }

        [TestMethod]
        public void Group3()
        {

            //var q = db.OrderDetails.GroupBy(c => c.ProductId).Select(o => o.Key);
            //db.OrderDetails.GroupBy(c => new { c.ProductId, c.UnitPrice }).ToArray();
            //return;

            string esql;
            esql = "select value ProductId from OrderDetails as c group by c.ProductId";
            var productKeys = db.CreateQuery<int>(esql);
            foreach (var item in productKeys)
                Console.WriteLine(item);


            esql = "select value c.ProductId from OrderDetails as c group by c.ProductId";
            productKeys = db.CreateQuery<int>(esql);
            foreach (var item in productKeys)
                Console.WriteLine(item);

            db.Orders.GroupBy(o => new { o = o.Customer });
            var x2 = db.Orders.Join(db.OrderDetails, o => o.OrderId, d => d.OrderId, (o, d) => new { o, d })
                      .GroupBy(x => new { x.o.OrderId, x.d.ProductId })
                //.Where(o=>o.Key.OrderId)
                      ;
        }

        [TestMethod]
        public void Group4()
        {
            //            var q = db.OrderDetails.GroupBy(o => o.Product)
            //                                   .Select(o => new { Product = o, SumQuantity = o.Sum(a => a.Quantity) });

            string esql;
            esql = @"select ProductId, sum(o.Quantity) as SumQuantity  
                        from OrderDetails as o 
                        group by o.ProductId";
            db.CreateQuery(esql).Execute();
            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);

            //db.CreateQuery<IDataRecord>(esql).Execute();


            esql = @"select o.Product, sum(o.Quantity) as SumQuantity  
                                     from OrderDetails as o group by o.Product";
            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);

            db.CreateQuery<IDataRecord>(esql).Execute();



        }

        [TestMethod]
        public void Group5()
        {
            var esql = @"select d.OrderId from OrderDetails as d
                         where d.UnitPrice > 1000
                         group by d.OrderId";

            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
            db.CreateQuery<IDataRecord>(esql).Execute();
            //db.OrderDetails.GroupBy(d => d.OrderId).Select(o => new { o.Key });
        }

        [TestMethod]
        public void Group6()
        {
            string esql;
            esql = @"select o.OrderId, sum(o.Freight)
                     from Orders as o
                          inner join OrderDetails as d on o.OrderId = d.OrderId
                          inner join Products as p on d.ProductId = p.ProductId
                     group by o.OrderId";


            db.Orders.Join(db.OrderDetails, o => o.OrderId, o => o.OrderId, (o, d) => new { o, d })
                .Join(db.Products, o => o.d.ProductId, o => o.ProductId, (a, p) => new { a.o, a.d, p })
                .GroupBy(o => o.o.OrderId)
                .Select(a => new { s = a.Sum(j => j.o.Freight) });


            db.CreateQuery<IDataRecord>(esql).Execute();
        }

        [TestMethod]
        public void Group7()
        {
            string esql;
            esql = @"select OrderId
            from ( select o, d 
                from Orders as o 
                        inner join OrderDetails as d on o.OrderId = d.OrderId ) as od
            group by od.o.OrderId";

            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
            var q = db.CreateQuery<object>(esql).Execute();
        }

        [TestMethod]
        public void Group8()
        {
            string esql;
            esql = @"select o
                     from Orders as o
                          inner join OrderDetails as d on o.OrderId = d.OrderId
                     group by o";

            //ef.CreateQuery<object>(esql).Execute(MergeOption.NoTracking);
            var q = db.CreateQuery<object>(esql);
            q.Execute();

            db.Orders.Join(db.OrderDetails, o => o.OrderId, d => d.OrderId, (o, d) => new { o, d })
                     .GroupBy(od => od.o)
                     .Select(c => new { Order = c.Key, OrderDetails = c.Select(b => b.d) })
                     .ToArray();
        }


        [TestMethod]
        public void Group9()
        {
            string esql;
            esql = @"select Product, count()  
                     from OrderDetails as o 
                     group by o.Product";

            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
            db.CreateQuery<IDataRecord>(esql).Execute();
        }

        [TestMethod]
        public void Group10()
        {
            string esql;
            esql = @"select CategoryId, SupplierId  
                     from Products as p 
                     group by p.CategoryId, p.SupplierId";

            db.CreateQuery<IDataRecord>(esql).Execute();

            esql = @"select CategoryId, SupplierId, Count()  
            from Products as p 
            group by p.CategoryId, p.SupplierId";

            db.CreateQuery<IDataRecord>(esql).Execute();
        }

        [TestMethod]
        public void Group11()
        {
            var esql = @"select value row(p.CategoryId, p.UnitPrice) from Products as p 
            group by p.CategoryId, p.UnitPrice
            having max(p.UnitPrice) > 1000";

            var q = db.CreateQuery<IDataRecord>(esql);
            q.Execute();
        }

        [TestMethod]
        public void Group12()
        {
            var esql = @"select avg(d.Quantity) from OrderDetails as d
                         group by d.ProductId";
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]

        public void GroupPartition1()
        {
            var q1 = db.Products.GroupBy(o => new { o.CategoryId }).Select(o => new { o.Key, Products = o.Select(b => b) });

            var esql = @"select CategoryId, GroupPartition(p) as Products
                from Products as p 
                group by p.CategoryId";


            var items = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in items)
            {
                foreach (Product p in (IEnumerable<Product>)item["Products"])
                {
                    Console.WriteLine(p.ProductName);
                }
            }


        }

        [TestMethod]
        public void GroupPartition2()
        {
            var esql = @"select CategoryId, GroupPartition(p.UnitPrice + 1) as ProductUnitPrices
                         from Products as p 
                         group by p.CategoryId";


            var items = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in items)
            {
                foreach (decimal p in (IEnumerable<decimal?>)item["ProductUnitPrices"])
                {
                    Console.WriteLine(p);
                }
            }
        }

        [TestMethod]
        public void GroupPartition3()
        {
            string esql;
            esql = @"select od.o.OrderId as OrderId, GroupPartition(od.d) as OrderDetails
                     from ( select o, d 
                            from Orders as o 
                                 inner join OrderDetails as d on o.OrderId = d.OrderId ) as od
                     group by od.o.OrderId";

            var q = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in q)
            {
                Console.WriteLine(item["OrderId"]);
                foreach (OrderDetail d in (IEnumerable<OrderDetail>)item["OrderDetails"])
                {
                    Console.WriteLine(d.ProductId);
                }
            }

            db.Orders.Join(db.OrderDetails, o => o.OrderId, d => d.OrderId, (o, d) => new { o, d })
                     .GroupBy(od => od.o.OrderId)
                     .Select(x => new { OrderId = x.Key, Ordetails = x.Select(y => y.d) });

        }

        [TestMethod]
        public void GroupPartition4()
        {
            string esql;
            esql = @"select od.o.OrderId as OrderId, 
                           (select o from GroupPartition(od.o) as o limit 1) as Order,
                            GroupPartition(od.d) as OrderDetails
                     from (select o, d 
                           from Orders as o 
                                inner join OrderDetails as d on o.OrderId = d.OrderId ) as od
                     group by od.o.OrderId
                     limit 10";

            var items = db.CreateQuery<IDataRecord>(esql).Execute();
            foreach (var item in items)
            {
                var order = ((IEnumerable<Order>)item["Order"]).Single();
                var orderDetails = (IEnumerable<OrderDetail>)item["OrderDetails"];
                Console.WriteLine(order.OrderId);
                Console.WriteLine(orderDetails.Count());

            }

            //db.Orders.Join(db.OrderDetails, o => o.OrderId, d => d.OrderId, (o, d) => new { o, d })
            //         .GroupBy(od => od.o.OrderId)
            //         .Select(x => new { OrderId = x.Key, Ordetails = Enumerable.Select<Order, Order>(x.Select(y => y.o), o => o) });

        }

        [TestMethod]
        public void GroupPartition5()
        {
            var esql = @"select CategoryId, sum(GroupPartition(p.UnitPrice)) as PriceSum
                from Products as p 
                group by p.CategoryId";


            var items = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in items)
            {
                Console.WriteLine(item["PriceSum"]);
            }
        }

        [TestMethod]
        public void GroupPartition6()
        {
            var esql = @"select CategoryId, GroupPartition(sum(p.UnitPrice)) as PriceSum
    from Products as p 
    group by p.CategoryId";


            var items = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in items)
            {
                Console.WriteLine(item["PriceSum"]);
            }
        }


        [TestMethod]
        public void Having1()
        {
            var esql = @"select value c from Products as p 
                        group by p.CategoryId as c 
                        having c > 1000";

            var item = db.CreateQuery<int?>(esql).Execute();

        }

        [TestMethod]
        public void Having2()
        {
            var esql = @"select c from Products as p 
                        group by p.CategoryId as c, p.SupplierId 
                        having c > 1000";
            var item = db.CreateQuery(esql).Execute();

        }

        [TestMethod]
        public void Having3()
        {
            //var q = db.Products.GroupBy(o => o.CategoryId)
            //                    .Where(o => Enumerable.Max<Product>(o, a => a.UnitPrice) > 1000);

            var esql = @"select value c from Products as p 
                        group by p.CategoryId as c 
                        having max(p.CategoryId) > 1000";

            var item = db.CreateQuery<int?>(esql).Execute();
        }

        [TestMethod]
        public void Like1()
        {
            string esql;
            IQueryable q;

            esql = @"select p from Products as p
            where p.ProductName like '%aaa%'";
            q = db.CreateQuery(esql);
            q.Execute();

            esql = @"select p from Products as p
            where p.ProductName like 'aaa'";
            q = db.CreateQuery(esql);
            q.Execute();


            esql = @"select p from Products as p
            where p.ProductName like '%aaa'";
            q = db.CreateQuery(esql);
            q.Execute();

            esql = @"select p from Products as p
            where p.ProductName not like 'aaa%'";
            q = db.CreateQuery(esql);
            q.Execute();
        }

        [TestMethod]
        public void Like2()
        {
            string esql;
            IQueryable q;

            esql = @"select p from Products as p
                     where p.ProductName like @0";
            q = db.CreateQuery(esql, "%aaa%");
            q.Execute();

            q = db.CreateQuery(esql, "aaa");
            q.Execute();

            q = db.CreateQuery(esql, "%aaa");
            q.Execute();

            q = db.CreateQuery(esql, "aaa%");
            q.Execute();
        }

        [TestMethod]
        public void MultiSet1()
        {
            var esql = "MULTISET(1, 2, 3)";
            var q2 = db.CreateQuery<int>(esql);
            foreach (var item in q2)
                Console.WriteLine(item);

        }

        [TestMethod]
        public void MultiSet2()
        {
            new[] { 1, 2, 3 }.Select(o => o);

            var esql = "{1, 2, 3}";
            var q2 = db.CreateQuery<int>(esql);
            foreach (var item in q2)
                Console.WriteLine(item);

        }

        [TestMethod]
        public void MultiSet3()
        {
            Enumerable.Select(new[] { 1, 2, 3 }, m => m);
            var esql = "select value m from MULTISET(1, 2, 3) as m";
            var q2 = db.CreateQuery<int>(esql);
            foreach (var item in q2)
                Console.WriteLine(item);

        }

        [TestMethod]
        public void In1()
        {

            var esql = "select o from Orders as o where o.OrderId in { 1, 2 ,3 } ";
            db.CreateQuery<Order>(esql).Execute();

        }

        [TestMethod]
        public void In2()
        {
            var esql = @"select o from Orders as o where o.OrderId in 
                                  ( select value d.OrderId from OrderDetails as d ) ";

            db.CreateQuery<Order>(esql).Execute();

        }

        [TestMethod]
        public void In3()
        {
            var q = db.CreateQuery<int>("select value d.OrderId from OrderDetails as d ");
            var orders = db.Orders.Where(o => ((IEnumerable<int>)q).Contains(o.OrderId)).ToArray();
        }

        [TestMethod]
        public void In4()
        {
            var q = db.CreateQuery<int>("select value d.OrderId from OrderDetails as d ");
            var orders = db.Orders.Where("OrderId in @0", q).ToArray();
        }

        [TestMethod]
        public void InnerJoin1()
        {
            var q1 = db.Orders.Join(db.OrderDetails, o => o.OrderId, d => d.OrderId, (o, d) => new { o, d })
                       .Select(o => new { o.o.OrderId, o.d.UnitPrice });

            //new MyVisitor().Visit(q1.Expression);
            //q1.Execute();

            //var q3 = db.Orders.Join(db.OrderDetails, o => o.OrderId, d => d.OrderId, (o, d) => new { o, d })
            //     .GroupBy(a => a.o).Select(a => new { Order = a.Key, OrderDetails = a.Select(b=>b.d) });

            var q2 = db.Orders.GroupJoin(db.OrderDetails, o => o.OrderId, d => d.OrderId, (o, d) => new { o, d });

            var esql = @"select o.OrderId, d.UnitPrice 
                from Orders as o Inner Join 
                    OrderDetails as d on o.OrderId == d.OrderId";

            var q = db.CreateQuery(esql);
            q.Execute();
        }

        [TestMethod]
        public void InnerJoin2()
        {
            var q1 = db.Orders.Join(db.OrderDetails, o => o.OrderId, d => d.OrderId, (o, d) => new { o, d })
                              .Join(db.Products, d => d.d.ProductId, p => p.ProductId, (a, p) => new { a.o, a.d, p })
                              .Select(o => o.o.OrderId);

            var esql = @"select o.OrderId, d.ProductId, p.UnitPrice 
                from Orders as o 
                    Inner Join OrderDetails as d on o.OrderId == d.OrderId
                    Inner Join Products as p on d.ProductId == p.ProductId";

            var q = db.CreateQuery(esql);
            q.Execute();
        }

        [TestMethod]
        public void InnerJoin3()
        {
            var esql = @"select o.OrderId 
                         from Orders as o 
                              join OrderDetails as d on o.OrderId == d.OrderId";

            var q2 = db.CreateQuery<object>(esql);
            q2.Execute();
        }

        [TestMethod]
        public void LeftJoin1()
        {
            db.Orders.GroupJoin(db.OrderDetails, o => o.OrderId, o => o.OrderId, (o, x) => new { o, x })
                     .SelectMany(t => t.x.DefaultIfEmpty(), (a, d) => new { a.o.OrderId, d.UnitPrice })
                     .Execute();

            var esql = @"select o.OrderId, d.UnitPrice 
             from Orders as o 
                  left join OrderDetails as d on o.OrderId == d.OrderId";

            var q2 = db.CreateQuery(esql);
            q2.Execute();
        }

        [TestMethod]
        public void LeftJoin2()
        {

            db.Orders.GroupJoin(db.OrderDetails, o => o.OrderId, d => d.OrderId, (o, x) => new { o, x })
                     .SelectMany(t => t.x.DefaultIfEmpty(), (t, d) => new { t.o, d })
                     .GroupJoin(db.Products, o => o.d.ProductId, p => p.ProductId, (t, x) => new { t.d, t.o, x })
                     .SelectMany(t => t.x.DefaultIfEmpty(), (t, p) => new { t.o, t.d, p })
                     .Select(t => new { t.o.OrderId })
                     .Execute();

            var esql = @"select o.OrderId 
                from Orders as o 
                    left join OrderDetails as d on o.OrderId == d.OrderId
                    left join Products as p on d.ProductId == p.ProductId";

            var q2 = db.CreateQuery(esql);
            q2.Execute();
        }

        [TestMethod]
        public void LeftJoin3()
        {
            var esql = @"select o.OrderId, c.CategoryId, p.ProductId 
                from Orders as o 
                    left join OrderDetails as d on o.OrderId == d.OrderId
                    left join Products as p on d.ProductId == p.ProductId
                    left join Categories as c on p.CategoryId == c.CategoryId";

            //ef.CreateQuery<DbDataRecord>(esql).Execute(MergeOption.NoTracking);

            var q2 = db.CreateQuery<object>(esql);
            q2.Execute();
        }

        [TestMethod]
        public void Limit1()
        {
            var esql = @"select e from Employees as e limit 10";
            db.CreateQuery(esql, 10).Execute();
        }

        [TestMethod]
        public void Limit2()
        {
            var esql = @"select e from Employees as e limit @0";
            db.CreateQuery(esql, 10).Execute();
        }

        [TestMethod]
        public void Take1()
        {
            var esql = @"select e from Employees as e take 10";
            db.CreateQuery(esql, 10).Execute();
        }

        [TestMethod]
        public void Take2()
        {
            var esql = @"select e from Employees as e take @0";
            db.CreateQuery(esql, 10).Execute();
        }

        [TestMethod]
        public void OrderBy1()
        {
            var esql = "select e from Employees as e order by e.FirstName";
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void IsNull()
        {
            var esql = "select p from Products as p where p.Category is null";
            var q = db.CreateQuery<Product>(esql);
            q.Execute();

            esql = "select p from Products as p where p.Category is not null";
            q = db.CreateQuery<Product>(esql);
            q.Execute();

            esql = "select p from Products as p where p.Category not is null";
            q = db.CreateQuery<Product>(esql);
            q.Execute();
        }

        [TestMethod]
        public void IsNull2()
        {
            var esql = @"select c 
                         from Customers as c 
                              left join Orders as o on c.CustomerId = o.CustomerId  
                         where o is not null";
            db.CreateQuery(esql).Execute();
        }

        [TestMethod]
        public void Row()
        {
            var esql = @"Row('mike' as FirstName, 'mak' as LastName, 
        Row('ansiboy@163.com' as Email, 
            '13434126607' as Phone) as Contact)";
#if NET35
var row = db.CreateQuery<IDataRecord>(esql).Single();
Console.WriteLine("{0} {1}", row["FirstName"], row["LastName"]);
Console.WriteLine("{0} {1}", ((IDataRecord)row["Contact"])["Email"], ((IDataRecord)row["Contact"])["Phone"]);
#else
            var row = db.CreateQuery<IDataRecord>(esql).Single() as dynamic;
            Console.WriteLine("{0} {1}", row.FirstName, row.LastName);
            Console.WriteLine("{0} {1}", row.Contact.Email, row.Contact.Phone);
#endif
        }

        [TestMethod]
        public void Top()
        {
            var esql = "select top(1) p from Products as p";
            db.CreateQuery(esql).Execute();
        }

        #endregion

        #region 类型运算符
#if !L2S
        [TestMethod]
        public void IsOf()
        {
            var esql = "select c from Contacts as c where c is of EmployeeContact";
            var q = db.CreateQuery<Contact>(esql);
            q.Execute();

            esql = "select c from Contacts as c where c is not of EmployeeContact";
            db.CreateQuery<Contact>(esql).Execute();

            esql = "select c from Contacts as c where c is of NorthwindDemo.EmployeeContact";
            db.CreateQuery<Contact>(esql).Execute();

            esql = "select c from Contacts as c where c is not of NorthwindDemo.EmployeeContact";
            db.CreateQuery<Contact>(esql).Execute();
        }

        [TestMethod]
        public void IsOfOnly()
        {
            var esql = "select c from Contacts as c where c is of only EmployeeContact";
            var q = db.CreateQuery<Contact>(esql);
            q.Execute();
        }

        [TestMethod]
        public void OfType()
        {
            var esql = "select c from oftype(Contacts, FullContact) as c";
            var q = db.CreateQuery<Contact>(esql);
            q.Execute();
        }

        [TestMethod]
        public void OfTypeOnly()
        {
            string esql = "select c from oftype(Contacts, only FullContact) as c";
            var q = db.CreateQuery<Contact>(esql);
            q.Execute();
        }
#endif
        #endregion

        #region 其它函数
        [TestMethod]
        public void NewGuId()
        {
            var esql = "select newguId() as Guid, e.FirstName from Employees as e";
            var q = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in q)
                Console.WriteLine("{0} {1}", item[0], item[1]);
        }

        [TestMethod]
        public void IIF()
        {
            var esql = "select value iif(e.Country = 'Chinese', 'CN', 'Other') from Employees as e";
            var q = db.CreateQuery(esql);
            foreach (var item in q)
                Console.WriteLine("{0} ", item);
        }
        #endregion

        #region Query Base On Indexer(基于索引器的查询)
        [TestMethod]
        public void IndexerField1()
        {
            var esql = @"select o.OrderId, o.EmployeeId, d.ProductId, p.UnitPrice 
                         from Orders as o 
                              Inner Join OrderDetails as d on o.OrderId == d.OrderId
                              Inner Join Products as p on d.ProductId == p.ProductId";


            var q2 = db.CreateQuery<IDataRecord>(esql);
            var q3 = q2.Select(o => new { OrderId = (int)o["OrderId"], ProductId = (int)o["ProductId"] })
                       .Where(o => o.OrderId > 1000)
                       .Take(10);
            q3.Execute();
        }

        [TestMethod]
        public void IndexerField2()
        {
            var esql = @"select o.OrderId, o.EmployeeId, d.ProductId, p.UnitPrice 
             from Orders as o 
                  Inner Join OrderDetails as d on o.OrderId == d.OrderId
                  Inner Join Products as p on d.ProductId == p.ProductId";


            var q2 = db.CreateQuery<IDataRecord>(esql);
            var q3 = q2.Select(o => new
                        {
                            OrderId = (int)o["OrderId"],
                            EmployeeId = (int)o["EmployeeId"],
                            ProductId = (int)o["ProductId"]
                        })
                       .Where(o => o.OrderId > 1000)
                       .Join(db.Employees, o => o.EmployeeId, e => e.EmployeeId,
                                      (a, b) => new
                                      {
                                          a.EmployeeId,
                                          a.OrderId,
                                          a.ProductId,
                                          b.City,
                                          b.Address
                                      })
                       .Take(10);


            q3.Execute();

        }

        [TestMethod]
        public void IndexerField3()
        {
            var esql = @"select o.OrderId, o.EmployeeId, d.ProductId, p.UnitPrice 
    from Orders as o 
        Inner Join OrderDetails as d on o.OrderId == d.OrderId
        Inner Join Products as p on d.ProductId == p.ProductId";


            var q2 = db.CreateQuery<IDataRecord>(esql);
            var q3 = q2.Where(o => (int)o["OrderId"] > 1000)
                        .Join(db.Employees, o => o["EmployeeId"], e => e.EmployeeId,
                                        (a, b) => new
                                        {
                                            EmployeeId = a["EmployeeId"],
                                            OrderId = (int)a["OrderId"],
                                            ProductId = a["ProductId"],
                                            b.City,
                                            b.Address
                                        })
                        .Take(10);


            q3.Execute();
        }

        [TestMethod]
        public void IndexerField4()
        {
            var esql = @"select e.FirstName, e.LastName from Employees as e";
            var q = db.CreateQuery<IDataRecord>(esql)
                        .Where(o => (string)o["FirstName"] != "Mike");
            q.Execute();
            //ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
        }
        #endregion

        #region Query Base On Interface（基于接口的查询)
        [TestMethod]
        public void InterfaceQuery()
        {
            var esql = "select e from Employees as e";
            var q = db.CreateQuery<IEmployee>(esql)
                        .Where(o => o.FirstName == "F" && o.LastName == "L")
                        .Select(o => new { o.FirstName, o.LastName, o.BirthDate });
            q.Execute();
            //System.Linq.Queryable.Where(db.CreateQuery<IEmployee>(esql), o => o.FirstName == "F" && o.LastName == "L");
        }
        #endregion

        #region Parameterized Data Source(参数化数据源)
        [TestMethod]
        public void ParameterizedSource()
        {
            var employees = db.Employees.Where(o => o.Country == "EN");
            var esql = "select e from @0 as e where e.FirstName = 'f' and e.LastName = 'l'";
            var q = db.CreateQuery(esql, employees);
            q.Execute();

        }
        #endregion

        #region Static Member Access
        [TestMethod]
        public void StaticMethod1()
        {
            var esql = "Guid.NewGuId()";
            var q = db.CreateQuery<Guid>(esql).Single();
            Console.WriteLine(q);
        }

        [TestMethod]
        public void StaticMethod2()
        {
            var esql = "select System.Guid.NewGuId() as Guid, e.FirstName from Employees as e";
            var q = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in q)
                Console.WriteLine("{0} {1}", item[0], item[1]);
        }

        [TestMethod]
        public void StaticMember1()
        {
            var esql = "System.Math.PI";
            var pi = db.CreateQuery<float>(esql).Single();
            Console.WriteLine(pi);
        }

        [TestMethod]
        public void StaticMember2()
        {
            var esql = "select System.Math.PI as PI, e.FirstName from Employees as e";
            var q = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in q)
                Console.WriteLine("{0} {1}", item[0], item[1]);
        }
        #endregion

        #region DataContext Memer Access
        [TestMethod]
        public void DataContextMember()
        {
            var esql = "select Version, e.FirstName from Employees as e";
            var q = db.CreateQuery<IDataRecord>(esql);
            foreach (var item in q)
                Console.WriteLine("{0} {1}", item[0], item[1]);
        }
        #endregion

        #region 逻辑和Case表达式运算符
        [TestMethod]
        public void Case1()
        {
            var esql = @"select value 
                            (case 
                                 when e.Country = 'Chinese' then 'CN'
                                 when e.Country = 'English' then 'EN'
                                 when e.Country = 'HongKong' then 'HK'
                                 else 'other'
                            end)
                         from Employees as e";
            var q = db.CreateQuery(esql);
            q.Execute();


        }

        [TestMethod]
        public void Case2()
        {
            var esql = @"select value 
                            (case 
                                 when e.Country = 'Chinese' then 'CN'
                            end)
                         from Employees as e";
            var q = db.CreateQuery(esql);
            q.Execute();
        }

        [TestMethod]
        public void Case3()
        {
            var esql = @"select value 
                            (case 
                                 when e.Country = 'Chinese' then 'CN'
                                 else 'other'
                            end)
                         from Employees as e";
            var q = db.CreateQuery(esql);
            q.Execute();
        }

        [TestMethod]
        public void Case4()
        {
            var esql = @"select value 
                            (case 
                                 when e.Country = 'Chinese' then 1
                            end)
                         from Employees as e";
            var q = db.CreateQuery(esql);
            q.Execute();
        }

        [TestMethod]
        public void Case5()
        {
            var esql = @"select value 
                            (case 
                                 when e.Country = 'Chinese' then 1F
                            end)
                         from Employees as e";
            var q = db.CreateQuery(esql);
            q.Execute();
        }
        #endregion

        [TestMethod]
        public void Treat()
        {
#if !L2S
            var esql = @"select treat(c as FullContact) from Contacts as c where c is of FullContact";
            var q = db.CreateQuery<FullContact>(esql);
            q.Execute();
#endif
        }

        [TestMethod]
        public void Temp()
        {
            //db.OrderDetails.Where(
            //    o => db.OrderDetails.Where(od => od.ProductId == 123).Select(od => od.OrderId).Contains(o.OrderId));

            //var q1 = (db.Categories.Where(c => c.Products.Count() > 0).Select(c => c.CategoryId));
            //var q = db.Products.Where(p => q1.Contains(p.CategoryId.Value));
            //q.Execute();

            //var c2 = db.Categorie.Single(o => o.CategoryId == 1);
            //db.Refresh(RefreshMode.OverwriteCurrentValues, c2);
            //Console.WriteLine(c1 == c2);

            Console.WriteLine(db.Categories.First().CategoryId);

var c1 = db.Categories.Single(o => o.CategoryId == 1);
c1.CategoryName = "xxx";
db.SubmitChanges();

c1.CategoryName = "xxx";
db.SubmitChanges();
        }


    }

    public class Person2
    {
        public int EmployeeId { get; set; }

        public string FirstName { get; set; }

        //public string LastName { get; set; }
        public string LastName;
    }
}
