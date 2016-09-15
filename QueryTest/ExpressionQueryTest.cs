using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if L2S
using System.Data.Linq;
using SqlMethods = System.Data.Linq.SqlClient.SqlMethods;
using NorthwindDemo;
using System.Reflection;
#else
using ALinq;
using ALinq.Mapping;
using NorthwindDemo;
using SqlMethods = ALinq.SqlClient.SqlMethods;
#endif

namespace ALinq.Dynamic.Test
{
    [TestClass]
    public class ExpressionQueryTest : BaseTest
    {


        /// <summary>
        /// 测试单个属性的访问
        /// </summary>
        [TestMethod]
        public void SelectValues()
        {
            employees.Select("[FirstName], [Values]").ToArray();
            employees.Select("value Orders").Cast<object>().ToArray();
            employees.Where(o => o.Orders.Count > 0)
                     .Select("value Orders[0]").Cast<Order>().ToArray();
            products.Select("value Category.CategoryName").Cast<object>().ToArray();

        }

        /// <summary>
        /// 测试单个属性并返回新对象。
        /// </summary>
        [TestMethod]
        public void SelectRowWithSingleProperty()
        {
            var table = db.GetTable<Employee>();
            table.Select("FirstName as F").Cast<object>().ToArray();
        }

        /// <summary>
        /// 测试一次选择多个属性并返回新对象
        /// </summary>
        [TestMethod]
        public void SelectRowWithMultiProperties()
        {
            var t = db.GetTable<Employee>();
            t.Select("FirstName as F, LastName as L").Cast<object>().ToArray();
        }

        /// <summary>
        /// ROW 嵌套测试
        /// </summary>
        [TestMethod]
        public void SelectRowRecursive()
        {

            employees.Select("row (FirstName + LastName as Name)").Cast<object>().ToArray();

            var items = employees.Select("row (row (FirstName as F, LastName as L) as M)")
                                 .Cast<object>().ToList();
            items.ForEach(Console.WriteLine);

        }

        [TestMethod]
        public void ParameterTest()
        {
            var table = db.GetTable<Employee>();
            table.Where("BirthDate < @0", DateTime.Now).ToArray();
            table.Where("BirthDate.Value.Month < @0.Month", DateTime.Now).ToArray();

        }

        [TestMethod]
        public void Select_Simple()
        {
            var items = (from p in db.Products
                         select new { p.ProductName }).ToList();

            var items1 = db.Products.Select("it.ProductName as ProductName");
        }

        class Name
        {
            internal string FirstName;
            internal string LastName;
        }

        [TestMethod]
        public void Select_SpecialType()
        {
            //指定类型形式
            IList items = (from e in db.Employees
                           select new Name
                           {
                               FirstName = e.FirstName,
                               LastName = e.LastName
                           }).ToList();
            Assert.IsTrue(items.Count > 0);

            //TODO:创建已知类型
            //db.Employees.Select("new (FirstName, LastName)").Cast<Name>().ToArray();
        }

        [TestMethod]
        public void Select_AnonymousType()
        {
            //.匿名类型形式
            var items = (from p in db.Products
                         select new
                         {
                             p.ProductName,
                             p.UnitsInStock,
                             p.UnitsOnOrder,
                         }).ToList();

            var items1 = db.Products.Select("it.ProductName, it.UnitsInStock, it.UnitsOnOrder").Cast<object>().ToArray();

        }

        #region Restriction Operators
        [TestMethod]
        public void Where_Simple1()
        {
            //使用where筛选在伦敦的客户
            db.Customers.Where("City == 'London'").ToArray();
        }

        [TestMethod]
        public void Where_Simple2()
        {
            //筛选1994 年或之后雇用的雇员：
            //db.Employees.Where("HireDate >=  #1994-1-1#").ToArray();


            //var b = db.Employees.Select("value FirstName").Contains("AAA");

            var q = db.Employees.Where("it.LastName in MultiSet('aaa','bbb','ccc')").ToArray();

        }

        [TestMethod]
        public void Where_Simple3()
        {
            //筛选库存量在订货点水平之下但未断货的产品
            db.Products.Where("UnitsInStock <= ReorderLevel && !Discontinued").ToArray();
        }

        [TestMethod]
        public void Where_Simple4()
        {
            //下面这个例子是调用两次where以筛选出UnitPrice大于10且已停产的产品
            db.Products.Where("it.UnitPrice > 10").Where("it.Discontinued").ToArray();
        }

        [TestMethod]
        public void Where_Drilldown()
        {
            var waCustomers = db.Customers.Where("it.Region == 'WA'").ToArray();

            foreach (var customer in waCustomers)
            {
                var orders = customer.Orders.ToList();
            }
        }

        #endregion

        #region Miscellaneous Operators

        [TestMethod]
        public void Contains1()
        {
            db.Orders.Where(o => true).Where("it.CustomerId in {'AROUT', 'BOLId', 'FISSA'}").ToList();
        }

        [TestMethod]
        public void Contains2()
        {
            var CustomerId_Set = new[] { "AROUT", "BOLId", "FISSA" };
            db.Orders.Where("it.CustomerId in @0", new[] { CustomerId_Set }).ToList();

            var CustomerId_Set1 = new List<string> { "AROUT", "BOLId", "FISSA" };
            db.Orders.Where("it.CustomerId in @0", new[] { CustomerId_Set1 }).ToList();
        }

        [TestMethod]
        public void Contains3()
        {
            db.Customers.Where(o => o.CustomerId.Contains("C")).ToArray();
            db.Customers.Where("it.CustomerId.Contains('C')").ToArray();
        }

        [TestMethod]
        public void Contains4()
        {
            db.Orders.Where(o => true).Where("it.CustomerId in {'AROUT', 'BOLId', 'FISSA'}").ToList();
        }

        [TestMethod]
        public void Like()
        {
            //Like
            //自定义的通配表达式。%表示零长度或任意长度的字符串；_表示一个字符；
            //[]表示在某范围区间的一个字符；[^]表示不在某范围区间的一个字符。
            //比如查询消费者Id以“C”开头的消费者。  

            IList items = (from c in db.Customers
                           where SqlMethods.Like(c.CustomerId, "C%")
                           select c).ToList();

            db.Customers.Where("CustomerId like 'C%'").ToArray();
            db.Customers.Where("it.CustomerId like 'C%'").ToArray();
            db.Customers.Select(o => o.CustomerId).Where("it like 'C%'").ToArray();

            //比如查询消费者Id没有“AXOXT”形式的消费者：
            Assert.IsTrue(items.Count > 0);
            items = (from c in db.Customers
                     where !SqlMethods.Like(c.CustomerId, "A%O%T")
                     select c).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void TestNull()
        {
            db.Products.Where("Category == NULL").ToArray();
            db.Products.Where("!it.CategoryId.HasValue").ToArray();
        }

        #endregion

        [TestMethod]
        public void MathTest()
        {
            var table = db.GetTable<Employee>();
            //Math.Abs()
            table.Select("value Math.Abs(EmployeeId)").Cast<int>().ToArray();
        }

        [TestMethod]
        public void DateTimeTypeTest()
        {
            var table = db.GetTable<Employee>();
            table.Where("it.BirthDate.Value.Year < DateTime.Now.Year").Cast<object>().ToArray();
        }

        [TestMethod]
        public void ConvertTest()
        {
            var table = db.GetTable<Employee>();
            table.Select("value Convert.ToString(it.EmployeeId)").Cast<string>().ToArray();
        }

        [TestMethod]
        public void StringTest()
        {
            var table = db.GetTable<Employee>();
            table.Select("value it.FirstName + ' ' + it.LastName").Cast<string>().ToArray();
        }

        [TestMethod]
        public void NumberTest()
        {
            var table = db.GetTable<Employee>();
            table.Where("it.EmployeeId < 100").ToArray();
        }

        [TestMethod]
        public void DateTimeTest()
        {
            var table = db.GetTable<Employee>();
            table.Where("it.BirthDate < #2011-1-1#").ToArray();
        }

        [TestMethod]
        public void PropertyTest()
        {
            var table = db.GetTable<Product>();
            table.Select(o => o.Category.CategoryName).ToArray();
            table.Select("value it.Category.CategoryName").Cast<string>().ToArray();
        }

        [TestMethod]
        public void OperationTest()
        {
            var table = db.GetTable<OrderDetail>();
            table.Select(o => o.UnitPrice + 1).ToArray();
            table.Select("value it.UnitPrice + 1").Cast<decimal>().ToArray();
            table.Select("value it.UnitPrice - 1").Cast<decimal>().ToArray();
            table.Select("value it.UnitPrice * 2").Cast<decimal>().ToArray();
            table.Select("value it.UnitPrice / 2").Cast<decimal>().ToArray();

            table.Where("it.UnitPrice > 0 && it.OrderId > 0").ToArray();
            table.Where("it.UnitPrice > 0 || it.OrderId > 0").ToArray();
            table.Where("!(it.UnitPrice > 0)").ToArray();
        }

        [TestMethod]
        public void TypeSupported()
        {
            var table = db.GetTable<Employee>();
            table.Where(o => o.BirthDate < new DateTime(1999, 1, 1)).ToArray();
            table.Where("it.BirthDate < DateTime(1999,1,1)").ToArray();


        }

        [TestMethod]
        public void TypeStaticMethod()
        {
            var table = db.GetTable<Employee>();
            //table.Select(o => Convert.ToString(o.EmployeeId)).ToArray();
            //table.Select("value Convert.ToString(EmployeeId)").Cast<string>().ToArray();

            table.Where(o => o.BirthDate < DateTime.Now);
            table.Where("it.BirthDate < DateTime.Now").ToArray();
        }

        [TestMethod]
        public void ArrayType()
        {
            var table = db.GetTable<Employee>();
            table.Where(o => new[] { "AAA", "BBB" }.Contains(o.FirstName)).ToArray();
            table.Where("it.FirstName in {'AAA', 'BBB'}").ToArray();
            table.Where("it.FirstName in multiset('AAA', 'BBB')").ToArray();
        }

        [TestMethod]
        public void OpenParenTest()
        {
            var table = db.GetTable<Employee>();
            table.Select("value (it.FirstName)").Cast<string>().ToArray();
            table.Where("!(it.EmployeeId > 0)").ToArray();
        }

        [TestMethod]
        public void Order()
        {
            var table = db.GetTable<Employee>();
            table.OrderBy("FirstName").ToArray();
            table.OrderBy("FirstName asc").ToArray();
            table.OrderBy("FirstName desc").ToArray();

            table.OrderBy("FirstName, LastName desc").ToArray();
        }

        [TestMethod]
        public void GroupBy1()
        {
            var q = db.Products.GroupBy("CategoryId", "CategoryId, count()").Execute();
            foreach (var item in q)
            {
                Console.WriteLine("{0} {1}", item[0], item[1]);
            }
        }

        [TestMethod]
        public void OrderBy1()
        {
            db.GetTable<Product>().OrderBy("it.ProductName").Execute();
            db.GetTable<Product>().OrderBy("it.ProductName desc").Execute();
            db.GetTable<Product>().OrderBy("it.ProductName asc").Execute();
        }

        [TestMethod]
        public void Select1()
        {
            var q = db.GetTable<Product>().Select("it.ProductName, it.UnitsInStock, it.UnitsOnOrder");
            foreach (var item in q)
            {
                Console.WriteLine("{0} {1} {2}", item[0], item[1], item[2]);
            }
        }

        [TestMethod]
        public void Skip1()
        {
            var q =
            db.GetTable<Product>().Skip("10").Execute();
            db.GetTable<Product>().Skip("@skip", new ObjectParameter("skip", 10)).Execute();
        }

        [TestMethod]
        public void Take1()
        {
            db.GetTable<Product>().Take("10").Execute();
            db.GetTable<Product>().Take("@0", 10).Execute();
            db.GetTable<Product>().Take("@take", new ObjectParameter("take", 10)).Execute();
        }

        [TestMethod]
        public void ParserTest()
        {
            //var bf = BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.InvokeMethod;
            //var parser = new ALinq.Dynamic.Parsers.QueryParser();
            //var method = parser.GetType().GetMethod("ParseWhere", bf);

        }

    }
}