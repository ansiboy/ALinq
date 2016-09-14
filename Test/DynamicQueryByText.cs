using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using ALinq.Dynamic;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;
using ALinq;

namespace Test
{
    [TestClass]
    public class DynamicQueryByText
    {
        [Table(Name = "Employees")]
        class MyEmployee
        {
            private Dictionary<string, object> values = new Dictionary<string, object>();
            public object this[string key]
            {
                get
                {
                    object item;
                    if (values.TryGetValue(key, out item))
                        return item;

                    return null;
                }
                set
                {
                    values[key] = value;
                }
            }
        }


        private NorthwindDatabase db;
        private Table<Employee> employees;
        private Table<Product> products;
        private ModuleBuilder moduleBuilder;

        [TestInitialize]
        public void TestInitialize()
        {
            //var xmlMapping = XmlMappingSource.FromStream(typeof(SQLiteTest).Assembly.GetManifestResourceStream("Test.Northwind.Access.map"));
            //db = new AccessNorthwind("C:/Northwind.mdb");
            //db = new AccessNorthwind("C:/Northwind.mdb", xmlMapping);
            db = new SQLiteNorthwind("C:/Northwind.db3");
            db.Log = Console.Out;
            employees = db.GetTable<Employee>();
            products = db.GetTable<Product>();
        }

        /// <summary>
        /// 测试单个属性的访问
        /// </summary>
        [TestMethod]
        public void SelectValues()
        {
            employees.Select("[FirstName]").Cast<string>().ToArray();
            employees.Select("Orders").Cast<dynamic>().ToArray();
            employees.Where(o => o.Orders.Count > 0)
                     .Select("Orders[0]").Cast<Order>().ToArray();
            products.Select("Category.CategoryName").Cast<dynamic>().ToArray();

        }

        /// <summary>
        /// 测试单个属性并返回新对象。
        /// </summary>
        [TestMethod]
        public void SelectNewWithSingleProperty()
        {
            employees.Select("new ([FirstName] as F)").Cast<object>().ToArray();
            employees.Select("[FirstName] as F").Cast<object>().ToArray();
            employees.Select("new ([FirstName])").Cast<object>().ToArray();

            var table = db.GetTable<Employee>();
            table.Select(o => new { F = o.FirstName }).ToArray();

            table.Select("new (FirstName as F)").Cast<object>().ToArray();
            table.Select("FirstName as F").Cast<object>().ToArray();
        }

        /// <summary>
        /// 测试一次选择多个属性并返回新对象
        /// </summary>
        [TestMethod]
        public void SelectNewWithMultiProperties()
        {
            var table = db.GetTable<MyEmployee>();
            table.Select(o => new { FisrtName = o["FisrtName"], LastName = o["LastName"] }).ToArray();

            table.Select("new (['FirstName'] as FirstName, ['LastName'] as FirstName)").Cast<object>().ToArray();
            table.Select("[FirstName] as F, [LastName] as L").Cast<object>().ToArray();
            table.Select("[FirstName] as F, [LastName]");
            table.Select("[FirstName], [LastName]");

            var t = db.GetTable<Employee>();
            t.Select("FirstName as F, LastName as L").Cast<object>().ToArray();
        }

        /// <summary>
        /// NEW 嵌套测试
        /// </summary>
        [TestMethod]
        public void SelectNewRecursive()
        {
            var items = employees.Select("new (new ([FirstName] as F, [LastName] as L) as M)")
                                 .Cast<dynamic>().ToList();
            items.ForEach(Console.WriteLine);

            items = employees.Select("new (new ([FirstName], [LastName]) as M)")
                             .Cast<dynamic>().ToList();
            items.ForEach(Console.WriteLine);
        }

        [TestMethod]
        public void ParameterTest()
        {
            var table = db.GetTable<Employee>();
            table.Where("BirthDate < @0", DateTime.Now).ToArray();
            table.Where("BirthDate.Value.Month < @0.Month", DateTime.Now).ToArray();

            var names = new[] { "AAA", "BBB", "CCC" };
            table.Where(o => names.Contains(o.FirstName)).ToArray();
            table.Where("@0.Contains(FirstName)", new object[] { names }).ToArray();
        }

        [TestMethod]
        public void Aggregate_Null()
        {
            //TODO:
            employees.Select("FirstName").Min();
            employees.Min("FirstName");

            employees.Select("FirstName").Max();
            employees.Max("FirstName");

            products.Count(o => o.UnitPrice < 1000);
            products.Count("UnitPrice < 1000");

            products.Average("ProductID");
            products.Sum("ProductID");

            employees.Select(o => o.FirstName).Min();
            employees.Min(o => o.FirstName);
        }

        [TestMethod]
        public void Select_Simple()
        {
            var items = (from p in db.Products
                         select new { p.ProductName }).ToList();

            var items1 = db.Products.Select("ProductName as ProductName");
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

            var items1 = db.Products.Select("ProductName, UnitsInStock, UnitsOnOrder").Cast<object>().ToArray();

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
            db.Employees.Where("HireDate >=  DateTime(1994, 1, 1)").ToArray();


            var b = db.Employees.Select("FirstName").Contains("AAA");
            var q = db.Employees.Where("string['aaa','bbb','ccc'].Contains(LastName)").ToArray();
            ////var b = q.ToArray();

            //db.Employees.Count("FirstName < 'xxxx'");
            //Console.WriteLine(b);
        }

        [TestMethod]
        public void Where_Simple3()
        {
            //筛选库存量在订货点水平之下但未断货的产品
            //var products = (from p in db.Products
            //                where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
            //                select p).ToList();
            //Assert.IsTrue(products.Count > 0);
            db.Products.Where("UnitsInStock <= ReorderLevel && !Discontinued").ToArray();
        }

        [TestMethod]
        public void Where_Simple4()
        {
            //下面这个例子是调用两次where以筛选出UnitPrice大于10且已停产的产品
            db.Products.Where("UnitPrice > 10").Where("Discontinued").ToArray();
        }

        [TestMethod]
        public void Where_Drilldown()
        {
            //Where - Drilldown
            //This sample prints a list of customers from the state of Washington along with their orders. 
            //A sequence of customers is created by selecting customers where the region is 'WA'. The sample 
            //uses doubly nested foreach statements to print the order numbers for each customer in the sequence. 
            var waCustomers = db.Customers.Where("Region == 'WA'").ToArray();

            foreach (var customer in waCustomers)
            {
                var orders = customer.Orders.ToList();
            }
        }
        #endregion

        #region Element Operators
        [Priority(20), TestMethod]
        public void First()
        {
            //简单用法：选择表中的第一个发货方。
            Shipper shipper = db.Shippers.First();
            Assert.IsNotNull(shipper);
            //元素：选择CustomerID 为“BONAP”的单个客户
            Customer cust = db.Customers.First("CustomerID == 'BONAP'");
            Assert.IsNotNull(cust);
            //条件：选择运费大于 10.00 的订单：
            Order ord = db.Orders.First("Freight > 5.00");
            Assert.IsNotNull(ord);
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            db.Products.Select("ProductID, ProductName").FirstOrDefault();
            var product = db.Products.Select("ProductID, ProductName")
                                     .FirstOrDefault("ProductID == 10");
            Assert.IsNotNull(product);
        }

        [TestMethod]
        public void SingleOrDefault()
        {
            var item = db.Products.Where("ProductName == 'aaa'").SingleOrDefault();
            Assert.IsNull(item);

            item = db.Products.SingleOrDefault("ProductName == 'aaa'");
            Assert.IsNull(item);

            var reslt = (int)db.Products.Where("ProductName == 'aaa'")
                                        .Select("ProductID").SingleOrDefault();
            Assert.IsTrue(reslt == 0);
        }

        [TestMethod]
        public void Single()
        {
            Debug.Assert(db.Products.Count() > 0);
            var item = db.Products.First();
            var id = item.ProductID;
            item = db.Products.Single("ProductID == " + item.ProductID);
        }


        #endregion

        #region Miscellaneous Operators
        //[TestMethod]
        //public void CreateDataBase()
        //{
        //    if (database.DatabaseExists())
        //        database.DeleteDatabase();
        //    database.CreateDatabase();
        //}

        [TestMethod]
        public void Contains1()
        {
            db.Orders.Select("CustomerID").Contains("BOLID");
            db.Orders.Where("string['AROUT', 'BOLID', 'FISSA'].Contains(CustomerID)").ToList();
        }

        [TestMethod]
        public void Contains2()
        {
            var customerID_Set = new[] { "AROUT", "BOLID", "FISSA" };
            db.Orders.Where("@0.Contains(CustomerID)", new[] { customerID_Set }).ToList();

            var customerID_Set1 = new List<string> { "AROUT", "BOLID", "FISSA" };
            db.Orders.Where("@0.Contains(CustomerID)", new[] { customerID_Set1 }).ToList();
        }

        [TestMethod]
        public void Contains3()
        {
            db.Customers.Where(o => o.CustomerID.Contains("C")).ToArray();
            db.Customers.Where("CustomerID.Contains('C')").ToArray();
        }

        [TestMethod]
        public void Like()
        {
            //Like
            //自定义的通配表达式。%表示零长度或任意长度的字符串；_表示一个字符；
            //[]表示在某范围区间的一个字符；[^]表示不在某范围区间的一个字符。
            //比如查询消费者ID以“C”开头的消费者。  

            //比如查询消费者ID没有“AXOXT”形式的消费者：
            IList items = (from c in db.Customers
                           where ALinq.SqlClient.SqlMethods.Like(c.CustomerID, "C%")
                           select c).ToList();
            Assert.IsTrue(items.Count > 0);
            items = (from c in db.Customers
                     where !ALinq.SqlClient.SqlMethods.Like(c.CustomerID, "A_O_T")
                     select c).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void TestNull()
        {
            db.Employees.Where("ReportsToEmployee == NULL").ToArray();
            db.Employees.Where("!ReportsTo.HasValue").ToArray();
        }

        #endregion

        [TestMethod]
        public void Update()
        {
            Expression<Func<Employee, dynamic>> exp1 = o => new { FirstName = "AAA", LastName = "BBB" };
            Expression<Func<Employee, bool>> predicte = o => o.EmployeeID == -1;
            var table = db.GetTable<Employee>();
            table.Update(exp1, predicte);

            table.Update(o => new Employee { FirstName = "AAA", LastName = "BBB" }, o => o.EmployeeID == -1);

            table.Update("FirstName = 'AAA', LastName = 'BBB'", "EmployeeID == -1");
            table.Update("FirstName = 'AAA', LastName = 'BBB'", o => o.EmployeeID == -1);
            table.Update("FirstName = @0, LastName = @1", "EmployeeID == @2", "AAA", "BBB", -1);
            table.Update(o => new { FirstName = "AAA", LastName = "BBB" }, "EmployeeID == -1");
            table.Update(o => new Employee { FirstName = "AAA", LastName = "BBB" }, "EmployeeID == -1");
        }

        [TestMethod]
        public void Insert()
        {
            Expression<Func<Employee, dynamic>> exp1 = o => new { FirstName = "AAA", LastName = "BBB" };
            Expression<Func<Employee, bool>> predicte = o => o.EmployeeID == -1;
            var table = db.GetTable<Employee>();
            table.Update(exp1, predicte);
            var employeeID = table.Insert<int>(o => new Employee { FirstName = "AAA", LastName = "BBB" });
            Assert.IsTrue(employeeID > 0);
            //table.Insert(o => new Employee { FirstName = "AAA", LastName = "BBB" });
            employeeID = table.Insert("FirstName = 'AAA', LastName = 'BBB'");
            Console.WriteLine(employeeID);
            table.Insert("FirstName = @0, LastName = @1", "AAA", "BBB");
        }

        [TestMethod]
        public void Delete()
        {
            var table = db.GetTable<Employee>();
            table.Delete(o => o.EmployeeID == -1);
            table.Delete("EmployeeID == -1");
            table.Delete("EmployeeID == @0", -1);
        }

        [TestMethod]
        public void TypeCast()
        {
            var table = db.GetTable<Employee>();
            table.Where("(string)[FirstName] == 'AAA'").ToArray();
        }

        [TestMethod]
        public void MathTest()
        {
            var table = db.GetTable<Employee>();
            //Math.Abs()
            table.Select("Math.Abs(EmployeeID)").Cast<int>().ToArray();
        }

        [TestMethod]
        public void DateTimeTypeTest()
        {
            var table = db.GetTable<Employee>();
            table.Where("BirthDate.Value.Year < DateTime.get_Now().Year").Cast<object>().ToArray();
        }

        [TestMethod]
        public void ConvertTest()
        {
            var table = db.GetTable<Employee>();
            table.Select("Convert.ToString(EmployeeID)").Cast<string>().ToArray();
        }

        [TestMethod]
        public void StringTest()
        {
            var table = db.GetTable<Employee>();
            table.Select("FirstName + ' ' + LastName").Cast<string>().ToArray();
        }

        [TestMethod]
        public void NumberTest()
        {
            var table = db.GetTable<Employee>();
            table.Where("EmployeeID < 100").ToArray();
        }

        [TestMethod]
        public void DateTimeTest()
        {
            var table = db.GetTable<Employee>();
            table.Where("Birthdate < #2011-1-1#").ToArray();
        }

        [TestMethod]
        public void PropertyTest()
        {
            var table = db.GetTable<Employee>();
            table.Select(o => o.ReportsToEmployee.FirstName).ToArray();
            table.Select("ReportsToEmployee.FirstName").Cast<string>().ToArray();
        }

        [TestMethod]
        public void OperationTest()
        {
            var table = db.GetTable<OrderDetail>();
            table.Select(o => o.UnitPrice + 1).ToArray();
            table.Select("UnitPrice + 1").Cast<decimal>().ToArray();
            table.Select("UnitPrice - 1").Cast<decimal>().ToArray();
            table.Select("UnitPrice * 2").Cast<decimal>().ToArray();
            table.Select("UnitPrice / 2").Cast<decimal>().ToArray();

            table.Where("UnitPrice > 0 && OrderID > 0").ToArray();
            table.Where("UnitPrice > 0 || OrderID > 0").ToArray();
            table.Where("!(UnitPrice > 0)").ToArray();
        }

        [TestMethod]
        public void TypeSupported()
        {
            var table = db.GetTable<Employee>();
            table.Where(o => o.BirthDate < new DateTime(1999, 1, 1)).ToArray();
            table.Where("Birthdate < DateTime(1999,1,1)").ToArray();


        }

        [TestMethod]
        public void TypeStaticMethod()
        {
            var table = db.GetTable<Employee>();
            table.Select(o => Convert.ToString(o.EmployeeID)).ToArray();
            table.Select("Convert.ToString(EmployeeID)").Cast<string>().ToArray();

            table.Where(o => o.BirthDate < DateTime.Now);
            table.Where("BirthDate < DateTime.get_Now()").ToArray();
        }

        [TestMethod]
        public void ArrayType()
        {
            var table = db.GetTable<Employee>();
            table.Where(o => new[] { "AAA", "BBB" }.Contains(o.FirstName)).ToArray();
            table.Where("string['AAA', 'BBB'].Contains(FirstName)").ToArray();
            table.Where("(string['AAA', 'BBB']).Contains(FirstName)").ToArray();
        }

        [TestMethod]
        public void OpenParenTest()
        {
            var table = db.GetTable<Employee>();
            table.Select("(FirstName)").Cast<string>().ToArray();
            table.Where("(string['AAA', 'BBB']).Contains(FirstName)").ToArray();
            table.Where("!(EmployeeID > 0)").ToArray();
        }

        [TestMethod]
        public void IndexerInsert()
        {
            var table = db.GetTable<MyEmployee>();
            table.Insert("FirstName = 'AAA', LastName = 'BBB'");
        }

 
    }
}