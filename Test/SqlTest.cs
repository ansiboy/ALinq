using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using NorthwindDemo;
using ALinq;
using ALinq.Mapping;
using System.Diagnostics;
using System.Data.Common;

namespace Test
{
    /// <summary>
    /// Summary description for Where
    /// </summary>
    [TestClass]
    public partial class SqlTest
    {
        protected NorthwindDatabase db;

        //protected abstract DbConnection CreateConnection();

        #region Additional test attributes
        public virtual NorthwindDatabase CreateDataBaseInstace()
        {
            throw new NotImplementedException();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            db = CreateDataBaseInstace();
        }

        public void TestInitialize(NorthwindDatabase database)
        {
            this.db = database;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            //db.Connection.Close();
            db.Dispose();
        }
        #endregion

        #region Restriction Operators
        [TestMethod]
        public void Where_Simple1()
        {
            //使用where筛选在伦敦的客户
            var customers = (from c in db.Customers
                             where c.City == "London"
                             select c).ToList();
            Assert.IsTrue(customers.Count > 0);
            var customer = customers[0];
            foreach (var customer1 in customers)
            {
                Console.WriteLine(customer1.CustomerID);
            }
        }

        [TestMethod]
        public void Where_Simple2()
        {
            //筛选1994 年或之后雇用的雇员：
            var employees = (from e in db.Employees
                             where e.HireDate >= new DateTime(1994, 1, 1)
                             select e).ToList();
            Assert.IsTrue(employees.Count > 0);
        }

        [TestMethod]
        public void Where_Simple3()
        {
            //筛选库存量在订货点水平之下但未断货的产品
            var products = (from p in db.Products
                            where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
                            select p).ToList();
            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public void Where_Simple4()
        {
            //下面这个例子是调用两次where以筛选出UnitPrice大于10且已停产的产品
            var products = db.Products.Where(p => p.UnitPrice > 10m).Where(p => p.Discontinued).ToList();
            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public void Where_Drilldown()
        {
            //Where - Drilldown
            //This sample prints a list of customers from the state of Washington along with their orders. 
            //A sequence of customers is created by selecting customers where the region is 'WA'. The sample 
            //uses doubly nested foreach statements to print the order numbers for each customer in the sequence. 
            var waCustomers = (from c in db.Customers
                               where c.Region == "WA"
                               select c).ToList();
            Assert.IsTrue(waCustomers.Count > 0);

            Console.WriteLine("Customers from Washington and their orders:");
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
            Customer cust = db.Customers.First(c => c.CustomerID == "BONAP");
            Assert.IsNotNull(cust);
            //条件：选择运费大于 10.00 的订单：
            Order ord = db.Orders.First(o => o.Freight > 5.00M);
            Assert.IsNotNull(ord);
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            db.Products.Select(o => new { o.ProductID, o.ProductName }).FirstOrDefault();
            var product = db.Products.Select(o => new { o.ProductID, o.ProductName })
                                     .FirstOrDefault(p => p.ProductID == 10);
            Assert.IsNotNull(product);
        }

        [TestMethod]
        public void SingleOrDefault()
        {
            var item = db.Products.Where(o => o.ProductName == "aaa").SingleOrDefault();
            Assert.IsNull(item);

            item = db.Products.SingleOrDefault(o => o.ProductName == "aaa");
            Assert.IsNull(item);

            var reslt = db.Products.Where(o => o.ProductName == "aaa")
                          .Select(o => o.ProductID).SingleOrDefault();
            Assert.IsTrue(reslt == 0);
        }

        [TestMethod]
        public void Single()
        {
            Debug.Assert(db.Products.Count() > 0);
            var item = db.Products.First();
            var id = item.ProductID;
            item = db.Products.Single(o => o.ProductID == id);
        }


        #endregion

        #region Projection Operators

        [TestMethod]
        public void Select_Simple()
        {
            var products = (from p in db.Products
                            select new { p.ProductName }).ToList();
            //var products = db.Products.ToList();
            Assert.IsTrue(products.Count > 0);
            //var categories = (from category in db.Categories
            //                  select category).ToList();
            //Assert.IsTrue(categories.Count > 0);
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

        }

        [TestMethod]
        public void Select_AnonymousType()
        {
            //.匿名类型形式
            IList items = (from p in db.Products
                           select new
                           {
                               p.ProductName,
                               p.UnitsInStock,
                               p.UnitsOnOrder,
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Select_LocalMethodCall()
        {
            //本地方法调用形式(LocalMethodCall)：
            IList items = (from c in db.Customers
                           where c.Country == "UK" || c.Country == "USA"
                           select new
                           {
                               c.CustomerID,
                               c.CompanyName,
                               c.Phone,
                               InternationalPhone =
                               PhoneNumberConverter(c.Country, c.Phone)
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void SelectMany_MultipleFrom()
        {
            DateTime cutoffDate = new DateTime(1997, 1, 1);
            var orders = (from c in db.Customers
                          where c.Region == "WA"
                          from o in c.Orders
                          where o.OrderDate >= cutoffDate
                          select new { c.CustomerID, o.OrderID }).ToList();
            Assert.IsTrue(orders.Count > 0);

        }

        [TestMethod]
        public void Select_Condition()
        {
            var q = from p in db.Products
                    select new
                    {
                        p.ProductName,
                        Availability = p.UnitsInStock - p.UnitsOnOrder < 0 ? "Out Of Stock" : "In Stock"
                    };
            var list = q.ToArray();//q.First();
        }
        #endregion

        #region Set Operators

        [TestMethod]
        public void Distinct1()
        {
            // Distinct形式：说明：筛选字段中不相同的值。用于查询不重复的结果集。生成SQL语句为：SELECT DISTINCT [City] FROM [Customers]
            var q = (from c in db.Customers
                     select c.City).Distinct();
            var items = q.ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Distinct2()
        {
            db.Categories.ToArray();
            var categoryNames = (from p in db.Products
                                 select p.CategoryID).Distinct().ToList();

            Assert.IsTrue(categoryNames.Count > 0);
        }

        [TestMethod]
        public void Union()
        {
            var items = ((from c in db.Customers
                          select c.Country).Union
                         (
                             from e in db.Employees
                             select e.Country
                         )).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        public void Union2()
        {
            var productFirstChars = from p in db.Products
                                    select p.ProductName;
            var customerFirstChars = from c in db.Customers
                                     select c.CompanyName;

            var uniqueFirstChars = productFirstChars.Union(customerFirstChars).ToList();
            Assert.IsTrue(uniqueFirstChars.Count > 0);
        }


        [TestMethod]
        public void Intersect()
        {
            //说明：取相交项；延迟。即是获取不同集合的相同项（交集）。即先遍历第一个集合，
            //找出所有唯一的元素，然后遍历第二个集合，并将每个元素与前面找出的元素作对比，
            //返回所有在两个集合内都出现的元素。
            var items = (from c in db.Customers
                         select c.Country)
                         .Intersect(
                             from e in db.Employees
                             select e.Country
                         ).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Except()
        {
            //Except（与非）
            //说明：排除相交项；延迟。即是从某集合中删除与另一个集合中相同的项。
            //先遍历第一个集合，找出所有唯一的元素，然后再遍历第二个集合，返回
            //第二个集合中所有未出现在前面所得元素集合中的元素。
            //语句描述：查询顾客和职员不同的国家。
            var items = (from c in db.Customers
                         select c.Country)
                         .Except(
                             from e in db.Employees
                             select e.Country
                         ).ToList();
            foreach (var item in items)
                Console.WriteLine(item);
        }
        #endregion

        #region Partitioning Operators

        [TestMethod]
        public void Take_Simple()
        {
            //说明：获取集合的前n个元素；延迟。即只返回限定数量的结果集。
            var items = (from e in db.Employees
                         orderby e.HireDate
                         select e).Take(5).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Take_Nested()
        {
            var items = ((from c in db.Customers
                          from o in c.Orders
                          where c.Region == "WA"
                          select new { c.CustomerID, o.OrderID, o.OrderDate }).Take(3)).ToList();
            Assert.IsTrue(items.Count <= 3);
        }

        [TestMethod]
        public void Skip_Simple()
        {
            int count = (from p in db.Products
                         orderby p.UnitPrice descending
                         select p).Count();
            Assert.IsTrue(count > 10);
            var items = (from p in db.Products
                         orderby p.UnitPrice descending
                         select p).Skip(10).ToList();
            Assert.IsTrue(items.Count > 0);

        }

        [TestMethod]
        public void Skip_Nested()
        {
            var waOrders = from c in db.Customers
                           //from o in c.Orders
                           where c.Region == "WA"
                           select new { c.CustomerID };

            var allButFirst2Orders = waOrders.Skip(2).ToList();
            Assert.IsTrue(allButFirst2Orders.Count > 0);
        }

        [TestMethod]
        public void Skip_Take()
        {
            var items = (from p in db.Products
                         orderby p.ProductID descending
                         select p).Skip(10).Take(5).ToList();

            Assert.AreEqual(5, items.Count);

            items = (from p in db.Products
                     orderby p.ProductID descending
                     select p).Skip(0).Take(5).ToList();
            Assert.AreEqual(5, items.Count);

            items = (from p in db.Products
                     orderby p.ProductID descending
                     select p).Skip(0).Take(0).ToList();
        }
        #endregion

        #region Aggregate Operators
        [TestMethod]
        public void Count_Simple()
        {
            //Count
            //说明：返回集合中的元素个数，返回INT类型；不延迟。生成SQL语句为：SELECT COUNT(*) FROM 
            //1.简单形式：
            //得到数据库中客户的数量：
            db.Log = Console.Out;
            int count = db.Customers.Count();
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public void Count_Conditional()
        {
            //2.带条件形式：
            //得到数据库中未断货产品的数量：
            var count = db.Products.Count(p => !p.Discontinued);
            Assert.IsTrue(count > 0);
            count = db.Products.Where(o => !o.Discontinued).Count();
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public void Count_Empty()
        {
            int count = db.Contacts.OfType<SupplierContact>().Count();
            Assert.IsTrue(count == 0);
            count = (from customer in db.Customers
                     where customer.CustomerID == "XXXX"
                     select customer).Count();
            Assert.IsTrue(count == 0);
        }

        [TestMethod]
        public void Count_Nested()
        {
            var orderCounts = (from c in db.Customers
                               select new { c.CustomerID, OrderCount = c.Orders.Count() }).ToList();
            Assert.IsTrue(orderCounts.Count > 0);
        }

        [TestMethod]
        public void Count_Grouped()
        {
            var categoryCounts = (from p in db.Products
                                  group p by p.Category into g
                                  select new { Category = g.Key, ProductCount = g.Count() }).ToList();
            Assert.IsTrue(categoryCounts.Count > 0);
        }

        [TestMethod]
        public void Sum_Simple()
        {
            //Sum
            //说明：返回集合中数值类型元素之和，集合应为INT类型集合；不延迟。生成SQL语句为：SELECT SUM(…) FROM 

            //1.简单形式：
            //得到所有订单的总运费：
            var sum = db.Orders.Select(o => o.Freight).Sum();
            Assert.IsTrue(sum > 0);

        }

        [TestMethod]
        public void Sum_Empty()
        {
            decimal? sum = (from order in db.Orders
                            where order.CustomerID == "XXXXX"
                            select order.Freight).Sum();
        }

        [TestMethod]
        public void Sum_Projection()
        {
            //2.映射形式：
            //得到所有产品的订货总数：
            var sum = db.Products.Sum(p => p.UnitsOnOrder);
            Assert.IsTrue(sum > 0);
        }

        [TestMethod]
        public void Sum_Grouped()
        {
            var categories = (from p in db.Products
                              group p by p.Category into g
                              select new { Category = g.Key, TotalUnitsInStock = g.Sum(p => p.UnitsInStock) }).ToList();

            Assert.IsTrue(categories.Count > 0);
        }


        [TestMethod]
        public void Min_Simple()
        {
            //Min
            //说明：返回集合中元素的最小值；不延迟。生成SQL语句为：SELECT MIN(…) FROM 

            //1.简单形式：
            //查找任意产品的最低单价：
            decimal? min = db.Products.Select(p => p.UnitPrice).Min();
            Assert.IsTrue(min > 0);

        }

        [TestMethod]
        public void Min_Projection()
        {
            //2.映射形式：
            //查找任意订单的最低运费：
            var min = db.Orders.Min(o => o.Freight);
        }

        [TestMethod]
        public void Min_Elements()
        {
            //3.元素：
            //查找每个类别中单价最低的产品：
            var q = (from p in db.Products
                     group p by p.CategoryID into g
                     select new
                     {
                         CategoryID = g.Key,
                         CheapestProducts = from p2 in g
                                            where p2.UnitPrice == g.Min(p3 => p3.UnitPrice)
                                            select p2
                     });
            var items = q.ToList();
        }

        [TestMethod]
        public void Min_Grouped()
        {
            var categories = (from p in db.Products
                              group p by p.Category into g
                              select new { Category = g.Key, CheapestPrice = g.Min(p => p.UnitPrice) })
                             .ToList();
            Assert.IsTrue(categories.Count > 0);
        }

        [TestMethod]
        public void Max_Simple()
        {
            //Max
            //说明：返回集合中元素的最大值；不延迟。生成SQL语句为：SELECT MAX(…) FROM

            //1.简单形式：
            //查找任意雇员的最近雇用日期：
            var maxHireDate = db.Employees.Select(e => e.HireDate).Max();
            //db.SubmitChanges();
            //var tableName = db.Mapping.GetTable(typeof(DataType)).TableName;
            //if (((SqlProvider)db.Provider).Mode != SqlProvider.ProviderMode.Firebird)
            //    tableName = ((SqlProvider)db.Provider).SqlIdentifier.QuoteIdentifier(tableName);
            //db.ExecuteCommand("DELETE FROM " + tableName);
            //Assert.AreEqual(0, db.DataTypes.Count());
            //var min = db.DataTypes.Min(o => o.ID);
            //Console.WriteLine("maxHireDate: " + maxHireDate);
            db.Customers.Max(o => o.CustomerID);
        }

        [TestMethod]
        public void Max_Projection()
        {
            //2.映射形式：
            //查找任意产品的最大库存量：
            var maxInStock = db.Products.Max(p => p.UnitsInStock);
        }

        [TestMethod]
        public void Max_Elements()
        {
            //3.元素：
            //查找每个类别中单价最高的产品：
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new
                           {
                               g.Key,
                               MostExpensiveProducts =
                                   from p2 in g
                                   where p2.UnitPrice == g.Max(p3 => p3.UnitPrice)
                                   select p2
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Max_Grouped()
        {
            var categories = (from p in db.Products
                              group p by p.Category into g
                              select new { Category = g.Key, MostExpensivePrice = g.Max(p => p.UnitPrice) })
                             .ToList();

            Assert.IsTrue(categories.Count > 0);
        }

        //---------------------------Average-----------------------------
        //说明：返回集合中的数值类型元素的平均值。集合应为数字类型集合，其
        //返回值类型为double；不延迟。生成SQL语句为：SELECT AVG(…) FROM

        //1.简单形式：
        [Timeout(10000), TestMethod]
        public void Avg_Simple()
        {
            //得到所有订单的平均运费：
            decimal? avg = db.Orders.Select(o => o.Freight).Average();
            Assert.IsTrue(avg > 0);
        }

        //2.映射形式：
        [TestMethod]
        public void Avg_Projection()
        {
            //得到所有产品的平均单价：
            var avg = db.Products.Average(p => p.UnitPrice);
            Assert.IsTrue(avg > 0);
        }

        //3.元素：
        [TestMethod]
        public void Avg_Emelents()
        {
            //查找每个类别中单价高于该类别平均单价的产品：
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new
                           {
                               g.Key,
                               ExpensiveProducts =
                                   from p2 in g
                                   where p2.UnitPrice > g.Average(p3 => p3.UnitPrice)
                                   select p2
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        //4.分组：
        [TestMethod]
        public void Avg_Grouped()
        {
            var categories = (from p in db.Products
                              group p by p.Category into g
                              select new { Category = g.Key, AveragePrice = g.Average(p => p.UnitPrice) })
                             .ToList();

            Assert.IsTrue(categories.Count > 0);
        }
        //-----------------------------------------------------------------
        #endregion

        #region Join Operation
        [TestMethod]
        public void Join_OnToMany()
        {
            //1.一对多关系(1 to Many)：
            IList items = (from c in db.Customers
                           from o in c.Orders
                           where c.City == "London"
                           select o).ToList();
            Assert.IsTrue(items.Count > 0);

            items = (from p in db.Products
                     where p.Supplier.Country == "USA" && p.UnitsInStock == 0
                     select p).ToList();
        }

        [TestMethod]
        public void Join_ManyToMany()
        {
            //说明：多对多关系一般会涉及三个表(如果有一个表是自关联的，那有可能只有2个表)。
            //这一句语句涉及Employees, EmployeeTerritories, Territories三个表。
            //它们的关系是1：M：1。Employees和Territories没有很明确的关系。
            //语句描述：这个例子在From子句中使用外键导航筛选在西雅图的雇员，同时列出其所在地区。
            var items = (from e in db.Employees
                         from et in e.EmployeeTerritories
                         where e.City == "Seattle"
                         select new
                         {
                             e.FirstName,
                             e.LastName,
                             et.Territory.TerritoryDescription
                         }).ToList();
            //db.Employees.Join(db.EmployeeTerritories, e => e.EmployeeID, et => et.EmployeeID, (e, et) => e.FirstName);
            foreach (var item in items)
            {
                Console.WriteLine("{0} {1} {2}", item.FirstName, item.LastName, item.TerritoryDescription);
            }
            db.Employees.Where(e => e.City == "Seattle")
                        .SelectMany(e => e.EmployeeTerritories, (e, et) => new { e.FirstName, e.LastName, et.Territory.TerritoryDescription })
                        .ToArray();
        }

        [TestMethod]
        public void Join_SelfJoin()
        {
            //3.自联接关系：
            IList items = (from e1 in db.Employees
                           from e2 in e1.Employees
                           where e1.City == e2.City
                           select new
                           {
                               FirstName1 = e1.FirstName,
                               LastName1 = e1.LastName,
                               FirstName2 = e2.FirstName,
                               LastName2 = e2.LastName,
                               e1.City
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Join_TowWay()
        {
            //1.双向联接(Two way join)：
            //此示例显式联接两个表并从这两个表投影出结果：
            IList items = (from c in db.Customers
                           join o in db.Orders on c.CustomerID
                           equals o.CustomerID into orders
                           select new
                           {
                               c.ContactName,
                               OrderCount = orders.Count()
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Join_ThreeWay()
        {
            //2.三向联接(There way join)：
            //此示例显式联接三个表并分别从每个表投影出结果：
            IList items = (from c in db.Customers
                           join o in db.Orders on c.CustomerID
                           equals o.CustomerID into ords
                           join e in db.Employees on c.City
                           equals e.City into emps
                           select new
                           {
                               c.ContactName,
                               ords = ords.Count(),
                               emps = emps.Count()
                           }).ToList();
        }

        [TestMethod]
        public void Join_LeftOuter()
        {
            //3.左外部联接(Left Outer Join)：
            //此示例说明如何通过使用 此示例说明如何通过使用DefaultIfEmpty() 获取左外部联接。
            //在雇员没有订单时，DefaultIfEmpty()方法返回null：
            var q = (from e in db.Employees
                     join o in db.Orders on e equals o.Employee into ords
                     from o in ords.DefaultIfEmpty()
                     select new
                     {
                         e.FirstName,
                         e.LastName,
                     });
            IList items = q.ToList();
            Assert.IsTrue(items.Count > 0);


            //var q1 = from e in db.Employees
            //         join o in db.Users on e.EmployeeID equals o.ID into users
            //         from o in users.DefaultIfEmpty()
            //         select new
            //         {
            //             e,
            //             o,
            //         };
            //q1.ToList();
        }

        [TestMethod]
        public void Join_LetAssignment()
        {
            //4.投影的Let赋值(Projected let assignment)：
            //说明：let语句是重命名。let位于第一个from和select语句之间。
            //这个例子从联接投影出最终“Let”表达式：
            IList items = (from c in db.Customers
                           join o in db.Orders on c.CustomerID
                           equals o.CustomerID into ords
                           let z = c.City + c.Country
                           from o in ords
                           select new
                           {
                               c.ContactName,
                               o.OrderID,
                               z,
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Join_CompositeKey()
        {
            //5.组合键(Composite Key)：
            //这个例子显示带有组合键的联接：
            IList items = (from o in db.Orders
                           from p in db.Products
                           join d in db.OrderDetails
                               on new
                               {
                                   o.OrderID,
                                   p.ProductID
                               } equals
                                   new
                                   {
                                       d.OrderID,
                                       d.ProductID
                                   }
                               into details
                           from d in details
                           select new
                           {
                               o.OrderID,
                               p.ProductID,
                               d.UnitPrice
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Join_NullableNonnullableKeyRelationship()
        {
            //6.可为null/不可为null的键关系(Nullable/Nonnullable Key Relationship)：
            //这个实例显示如何构造一侧可为 null 而另一侧不可为 null 的联接：
            IList items = (from o in db.Orders
                           join e in db.Employees
                               on o.EmployeeID equals
                               (int?)e.EmployeeID into emps
                           from e in emps
                           select new
                           {
                               o.OrderID,
                               e.FirstName
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Join()
        {
            var items = db.OrderDetails.Select(o => new { A = o.Order.OrderID, B = o.Product.ProductName });//.ToList();

            var item1s = db.Orders.Select(
                    o => new { A = o.Customer.CompanyName, B = o.Shipper.CompanyName, C = o.Employee.FirstName }).Where(o => o.A == "AAA").ToList();

            var items2 = db.Orders.Join(db.OrderDetails, o => o.OrderID, od => od.OrderID, (o, od) => new { o.ShipName, o.OrderDate, od.ProductID })
                                  .Where(o => o.ProductID == 100).ToArray();
            //o => new { A = o.Customer.CompanyName }).Where(o => o.A == "AAA").ToList();
            //var items2 = db.Orders.Select(o => new { o.ShipName, o.Customer.Address }).Where(o => o.Address == "aaaa").ToList();
            var items3 = db.Orders.Join(db.OrderDetails, o => o.OrderID, od => od.OrderID, (o, od) => new { o.OrderID, od.ProductID }).ToList();
            //from st in ctx.SentenceTexts
            //   join s in ctx.Sentences on st.SentenceId equals s.Id
            //               select new
            //               {
            //                   Text = st.Text,
            //                   Code = st.Sentence.Code,
            //               };
            var q = from st in db.OrderDetails
                    join s in db.Orders on st.OrderID equals s.OrderID
                    select new
                    {
                        st.Quantity,
                        st.Product.Discontinued
                    };
            q.ToString();
            q.ToList();

        }
        #endregion

        #region Ordering Operators
        [TestMethod]
        public void OrderBy_Simple()
        {
            //1.简单形式
            //这个例子使用 orderby 按雇用日期对雇员进行排序：
            IList items = (from e in db.Employees
                           orderby e.HireDate
                           select e).ToList();
            Assert.IsTrue(items.Count > 0);
            for (int i = 1; i < items.Count; i++)
            {
                var preItem = (Employee)items[i - 1];
                var current = (Employee)items[i];
                Assert.IsTrue((preItem.HireDate ?? System.DateTime.MinValue) <= (current.HireDate ?? System.DateTime.MinValue));
            }
        }

        [TestMethod]
        public void OrderBy_Conditional()
        {
            // 2.带条件形式
            //注意：Where和Order By的顺序并不重要。而在T-SQL中，Where和Order By有严格的位置限制。
            IList items = (db.Orders.Where(o => o.ShipCity == "London").OrderByDescending(o => o.Freight).Skip(1).Take(2)).ToList();
            Assert.IsTrue(items.Count > 0);
            for (int i = 1; i < items.Count; i++)
            {
                var preItem = (Order)items[i - 1];
                var current = (Order)items[i];
                Assert.IsTrue(string.Compare(preItem.ShipCity, current.ShipCity) <= 0);
            }
        }

        public void OrderBy_Desc()
        {
            //3.降序排序
            IList items = (from p in db.Products
                           orderby p.UnitPrice descending
                           select p).ToList();
            Assert.IsTrue(items.Count > 0);
            for (int i = 1; i < items.Count; i++)
            {
                Product preItem = (Product)items[i - 1];
                Product current = (Product)items[i];
                Assert.IsTrue(preItem.UnitPrice >= current.UnitPrice);
            }
        }

        public void OrderBy_ThenBy()
        {
            //4.ThenBy
            //语句描述：使用复合的 orderby 对客户进行排序，进行排序：
            IList items = (from c in db.Customers
                           orderby c.City, c.ContactName
                           select c).ToList();
            for (int i = 1; i < items.Count; i++)
            {
                var preItem = (Customer)items[i - 1];
                var current = (Customer)items[i];

                Assert.IsTrue(string.Compare(preItem.City, current.City) <= 0);
                if (string.Compare(preItem.City, current.City) == 0)
                    Assert.IsTrue(string.Compare(preItem.ContactName, current.ContactName) <= 0);
            }
            items = db.Customers.OrderBy(c => c.City)
                    .ThenBy(c => c.ContactName).ToList();
            Assert.IsTrue(items.Count > 0);
            items = db.Customers
                    .OrderByDescending(c => c.City)
                    .ThenByDescending(c => c.ContactName).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void OrderBy_ThenByDescending()
        {
            //5.ThenByDescending
            //这两个扩展方式都是用在OrderBy/OrderByDescending之后的，第一个ThenBy/ThenByDescending扩展方法作为第二位排序依据，
            //第二个ThenBy/ThenByDescending则作为第三位排序依据，以此类推
            IList items = (from o in db.Orders
                           where o.EmployeeID == 1
                           orderby o.ShipCountry, o.Freight descending
                           select o).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void OrderBy_GroupBy()
        {
            //6.带GroupBy形式
            //语句描述：使用orderby、Max 和 Group By 得出每种类别中单价最高的产品，并按 CategoryID 对这组产品进行排序
            db.Log = Console.Out;
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           orderby g.Key
                           select new
                           {
                               g.Key,
                               MostExpensiveProducts =
                                   from p2 in g
                                   where p2.UnitPrice == g.Max(p3 => p3.UnitPrice)
                                   select p2
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }
        #endregion

        #region Grouping Operators

        [TestMethod]
        public void GroupBy_Simple()
        {
            //1.简单形式：
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select g).ToList();
            Assert.IsTrue(items.Count > 0);
            foreach (IGrouping<int?, Product> gp in items)
                foreach (var item in gp)
                    ;//DO SOMETHING

            items = (from p in db.Products
                     group p by p.CategoryID).ToList();
            foreach (IGrouping<int?, Product> gp in items)
                foreach (var item in gp)
                    ;//DO SOMETHING
        }

        [TestMethod]
        public void GroupBy_Select()
        {
            //2.Select匿名类：
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new { CategoryID = g.Key, g }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Max()
        {
            //3.最大值
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new
                           {
                               g.Key,
                               MaxPrice = g.Max(p => p.UnitPrice)
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Min()
        {
            //4.最小值
            //语句描述：使用Group By和Min查找每个CategoryID的最低单价。
            //说明：先按CategoryID归类，判断各个分类产品中单价最小的Products。取出CategoryID值，并把UnitPrice值赋给MinPrice。
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new
                           {
                               g.Key,
                               MinPrice = g.Min(p => p.UnitPrice)
                           }).ToList();
            //Assert.IsTrue(items.Count > 0);

            // db.Products.GroupBy(p => p.CategoryID).Select(g => new {g.Key,g.Min(p=>p.C)}).ToArray();
            db.Orders.GroupBy(o => o.EmployeeID).Select(g => new { g.Key, MinDate = g.Min(o => o.OrderDate.Value) }).ToArray();

            Console.WriteLine(DateTime.MinValue.ToString());
        }

        [TestMethod]
        public void GroupBy_Avg()
        {
            //5.平均值
            //语句描述：使用Group By和Average得到每个CategoryID的平均单价。
            //说明：先按CategoryID归类，取出CategoryID值和各个分类产品中单价的平均值。
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new
                           {
                               g.Key,
                               AveragePrice = g.Average(p => p.UnitPrice)
                           }).ToList();
        }


        [TestMethod]
        public void GroupBy_Sum()
        {
            //6.求和
            //语句描述：使用Group By和Sum得到每个CategoryID 的单价总计。
            //说明：先按CategoryID归类，取出CategoryID值和各个分类产品中单价的总和。
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new
                           {
                               g.Key,
                               TotalPrice = g.Sum(p => p.UnitPrice)
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupByCount()
        {
            //7.计数
            //语句描述：使用Group By和Count得到每个CategoryID中产品的数量。
            //说明：先按CategoryID归类，取出CategoryID值和各个分类产品的数量。
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new
                           {
                               g.Key,
                               NumProducts = g.Count()
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_ConditionalCount()
        {
            //8.带条件计数
            //语句描述：使用Group By和Count得到每个CategoryID中断货产品的数量。
            //说明：先按CategoryID归类，取出CategoryID值和各个分类产品的断货数量。 
            //Count函数里，使用了Lambda表达式，Lambda表达式中的p，代表这个组里的一个元素或对象，即某一个产品。
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new
                           {
                               g.Key,
                               NumProducts = g.Count(p => p.Discontinued)
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Where()
        {
            //9.Where限制
            //语句描述：根据产品的―ID分组，查询产品数量大于10的ID和产品数量。这个示例在Group By子句后使用Where子句查找所有至少有10种产品的类别。
            //说明：在翻译成SQL语句时，在最外层嵌套了Where条件。
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           where g.Count() >= 10
                           select new
                           {
                               g.Key,
                               ProductCount = g.Count()
                           }).ToList();
        }

        [TestMethod]
        public void GroupBy_MultipleColumns()
        {
            //10.多列(Multiple Columns)
            //语句描述：使用Group By按CategoryID和SupplierID将产品分组。
            //说明：既按产品的分类，又按供应商分类。在by后面，new出来一个匿名类。这里，Key其实质是一个类的对象，Key包含两个Property：CategoryID、SupplierID。用g.Key.CategoryID可以遍历CategoryID的值。
            IList items = (from p in db.Products
                           group p by new
                           {
                               p.CategoryID,
                               p.SupplierID
                           } into g
                           select new
                           {
                               g.Key,
                               g
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Expression()
        {
            //11.表达式(Expression)
            //语句描述：使用Group By返回两个产品序列。第一个序列包含单价大于10的产品。第二个序列包含单价小于或等于10的产品。
            //说明：按产品单价是否大于10分类。其结果分为两类，大于的是一类，小于及等于为另一类。
            IList items = (from p in db.Products
                           group p by new { Criterion = p.UnitPrice > 10 } into g
                           select g).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Nested()
        {
            var q = from c in db.Customers
                    select new
                    {
                        c.CompanyName,
                        YearGroups = from o in c.Orders
                                     where o.OrderDate != null
                                     group o by o.OrderDate.Value.Year into yg
                                     select new
                                     {
                                         Year = yg.Key,
                                         MonthGroups = from o in yg
                                                       group o by o.OrderDate.Value.Month into mg
                                                       select new { Month = mg.Key, Orders = mg }
                                     }
                    };
            var items = q.ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Year()
        {
            IList items = (from o in db.Orders
                           where o.OrderDate != null
                           group o by o.OrderDate.Value.Year into yg
                           select yg).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Month()
        {
            IList items = (from o in db.Orders
                           where o.OrderDate != null
                           group o by o.OrderDate.Value.Month into yg
                           select yg).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Day()
        {
            IList items = (from o in db.Orders
                           where o.OrderDate != null
                           group o by o.OrderDate.Value.Day into yg
                           select yg).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Date()
        {
            IList items = (from o in db.Orders
                           where o.OrderDate != null
                           group o by o.OrderDate.Value.Date into yg
                           select yg).ToList();
            Assert.IsTrue(items.Count > 0);
        }
        #endregion

        #region Quantifiers
        [TestMethod]
        public void Any_Simple()
        {
            //说明：用于判断集合中所有元素是否都满足某一条件；不延迟
            //1.带条件形式
            //语句描述：这个例子返回所有订单都运往其所在城市的客户或未下订单的客户。
            IList items = (from c in db.Customers
                           where !c.Orders.Any()
                           select c).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Any_Condition()
        {
            //2.带条件形式：
            //仅返回至少有一种产品断货的类别：
            IList items = (from c in db.Categories
                           where c.Products.Any(p => p.Discontinued)
                           select c).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Any_Grouped()
        {
            IList productGroups = (from p in db.Products
                                   group p by p.Category into g
                                   where g.Any(p => p.UnitsInStock == 0)
                                   select new { Category = g.Key, Products = g }).ToList();

            Assert.IsTrue(productGroups.Count > 0);
        }

        [TestMethod]
        public void All_Simple()
        {
            IList items = (from c in db.Customers
                           where c.Orders.All(o => o.ShipCity == c.City)
                           select c).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]

        public void All_Grouped()
        {
            var productGroups = (from p in db.Products
                                 group p by p.Category into g
                                 where g.All(p => p.UnitsInStock > 0)
                                 select new { Category = g.Key, Products = g }).ToList();
            Assert.IsTrue(productGroups.Count > 0);
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
        public void Contains()
        {
            //Contains
            //说明：用于判断集合中是否包含有某一元素；不延迟。它是对两个序列进行连接操作的。
            var customerID_Set = new[] { "AROUT", "BOLID", "FISSA" };
            IList items = (from o in db.Orders
                           where customerID_Set.Contains(o.CustomerID)
                           select o).ToList();
            Assert.IsTrue(items.Count > 0);

            items = (from o in db.Orders
                     where (new[] { "AROUT", "BOLID", "FISSA" }).Contains(o.CustomerID)
                     select o).ToList();
            Assert.IsTrue(items.Count > 0);
            items = (from o in db.Orders
                     where !(new[] { "AROUT", "BOLID", "FISSA" }).Contains(o.CustomerID)
                     select o).ToList();
            Assert.IsTrue(items.Count > 0);

            //1.包含一个对象：
            //语句描述：这个例子使用Contain查找哪个客户包含OrderID为10248的订单。
            var order = (from o in db.Orders
                         where o.OrderID == 10248
                         select o).First();
            items = db.Customers.Where(p => p.Orders.Contains(order)).ToList();
            Assert.IsTrue(items.Count > 0);

            //2.包含多个值：
            //语句描述：这个例子使用Contains查找其所在城市为西雅图、伦敦、巴黎或温哥华的客户
            var cities = new[] { "Seattle", "London", "Vancouver", "Paris" };
            items = db.Customers.Where(p => cities.Contains(p.City)).ToList();
            Assert.IsTrue(items.Count > 0);


        }


        public void Conatin2()
        {
            var result = db.Customers.Select(o => o.CompanyName).Contains("company");
            Assert.IsFalse(result);
        }

        //[TestMethod]
        public void Contains3()
        {
            var item = db.Customers.First();
            var result = db.Customers.Contains(item);
            Assert.IsTrue(result);
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
            //1.Null
            IList items = (from e in db.Employees
                           where e.ReportsToEmployee == null
                           select e).ToList();
            Assert.IsTrue(items.Count > 0);

            //2.Nullable<T>.HasValue
            items = (from e in db.Employees
                     where !e.ReportsTo.HasValue
                     select e).ToList();
            Assert.IsTrue(items.Count > 0);

            //3.Nullable<T>.Value
            items = (from e in db.Employees
                     where e.ReportsTo.HasValue
                     select new
                     {
                         e.FirstName,
                         e.LastName,
                         ReportsTo = e.ReportsTo.Value
                     }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void CollectionConvert()
        {
            //1.AsEnumerable：将类型转换为泛型 IEnumerable 
            //使用 AsEnumerable<TSource> 可返回类型化为泛型 IEnumerable 的参数。
            //在此示例中，LINQ to SQL（使用默认泛型 Query）会尝试将查询转换为 
            //SQL 并在服务器上执行。但 where 子句引用用户定义的客户端方法 (isValidProduct)，
            //此方法无法转换为 SQL。
            //解决方法是指定 where 的客户端泛型 IEnumerable<T> 实现以替换泛型 IQueryable<T>。
            //可通过调用 AsEnumerable<TSource>运算符来执行此操作。
            IEnumerable<Product> items = from p in db.Products.AsEnumerable()
                                         where isValidProduct(p)
                                         select p;
            int count = 0;
            foreach (var item in items)
                count++;
            Assert.IsTrue(count > 0);

            ////2.ToArray：将序列转换为数组
            ////使用 ToArray <TSource>可从序列创建数组。
            var array = (from c in db.Customers
                         where c.City == "London"
                         select c).ToArray();
            Assert.IsTrue(array.Length > 0);

            ////3.ToList：将序列转换为泛型列表 
            ////使用 ToList<TSource>可从序列创建泛型列表。下面的示例使用 ToList<TSource>直接将查询的计算结果放入泛型 List<T>。 
            var list = (from e in db.Employees
                        where e.HireDate >= new DateTime(1994, 1, 1)
                        select e).ToList();
            Assert.IsTrue(list.Count > 0);

            ////4.ToDictionary：将序列转化为字典
            ////使用Enumerable.ToDictionary<TSource, TKey>方法可以将序列转化为字典。TSource表示source
            ////中的元素的类型；TKey表示keySelector返回的键的类型。其返回一个包含键和值的Dictionary<TKey, TValue>。
            var dictionary = (from p in db.Products
                              where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
                              select p).ToDictionary(p => p.ProductID);
            //Dictionary<int, Product> qDictionary =
            //    q.ToDictionary(p => p.ProductID);
            Assert.IsTrue(dictionary.Count > 0);

            var contacts = db.Contacts;
            contacts.OfType<CustomerContact>().ToList();
        }

        [TestMethod]
        public void Concat()
        {
            var customerNames = from c in db.Customers
                                select c.CompanyName;
            var productNames = from p in db.Products
                               select p.ProductName;

            var allNames = customerNames.Concat(productNames);

            Console.WriteLine("Customer and product names:");
            foreach (var n in allNames)
            {
                Console.WriteLine(n);
            }
        }
        #endregion

        #region 字符串函数
        [TestMethod]
        public void StringConcat()
        {
            //1.字符串串联(String Concatenation)
            //语句描述：这个例子使用+运算符在形成经计算得出的客户Location值过程中将字符串字段和字符串串联在一起。
            IList items = (from c in db.Customers
                           select new
                           {
                               c.CustomerID,
                               Location = c.City + ", " + c.Country
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringLength()
        {
            //2.String.Length
            //语句描述：这个例子使用Length属性查找名称短于10个字符的所有产品。
            IList items = (from p in db.Products
                           where p.ProductName.Length < 10
                           select new { p.ProductID, p.ProductName, p.QuantityPerUnit, p.ReorderLevel, p.SupplierID, p.UnitPrice, p.UnitsInStock, p.UnitsOnOrder, p.CategoryID, p.Discontinued }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringContain()
        {
            //3.String.Contains(substring)
            //语句描述：这个例子使用Contains方法查找所有其联系人姓名中包含“Anders”的客户。
            IList items = (from c in db.Customers
                           where c.ContactName.Contains("Anders")
                           select c).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringIndexOf()
        {
            //4.String.IndexOf(substring)
            db.Customers.Select(o => o.ContactName.IndexOf('c')).ToArray();
            Assert.IsTrue(db.Customers.Count() > 0);
            var str = db.Customers.First().ContactName.Substring(1, 1);
            IList items = db.Customers.Where(c => c.ContactName.IndexOf(str) == 1).ToArray();
            //Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringStartsWith()
        {
            //5.String.StartsWith(prefix)
            //语句描述：这个例子使用StartsWith方法查找联系人姓名以“Maria”开头的客户。
            IList items = (from c in db.Customers
                           where c.ContactName.StartsWith("Maria")
                           select c).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringEndsWith()
        {
            //6.String.EndsWith(suffix)
            //语句描述：这个例子使用EndsWith方法查找联系人姓名以“Anders”结尾的客户。
            IList items = (from c in db.Customers
                           where c.ContactName.EndsWith("Anders")
                           select c).ToList();
            //Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringSubstring()
        {
            //7.String.Substring(start)
            //语句描述：这个例子使用Substring方法返回产品名称中从第四个字母开始的部分。
            IList items = (from p in db.Products
                           select p.ProductName.Substring(3)).ToList();
            Assert.IsTrue(items.Count > 0);

            //8.String.Substring(start, length)
            //语句描述：这个例子使用Substring方法查找家庭电话号码第七位到第九位是“555”的雇员。
            items = (from e in db.Employees
                     where e.HomePhone.Substring(0, 3) == "AAA"
                     select e).ToList();
            items = (from e in db.Employees
                     where e.HomePhone.Substring(0) == "AAA"
                     select e).ToList();
        }

        [TestMethod]
        public void StringToUpper()
        {
            //9.String.ToUpper()
            //语句描述：这个例子使用ToUpper方法返回姓氏已转换为大写的雇员姓名。
            var category = db.Categories.First();
            var name = category.CategoryName.ToUpper();
            IList items = db.Categories.Where(o => o.CategoryName.ToUpper() == name).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringToLower()
        {
            //10.String.ToLower()
            //语句描述：这个例子使用ToLower方法返回已转换为小写的类别名称。
            var category = db.Categories.First();
            var name = category.CategoryName.ToLower();
            IList items = db.Categories.Where(o => o.CategoryName.ToLower() == name).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void String_Trim()
        {
            //11.String.Trim()
            //语句描述：这个例子使用Trim方法返回雇员家庭电话号码的前五位，并移除前导和尾随空格。
            IList items = (from e in db.Employees
                           where e.HomePhone != null
                           select e.HomePhone.Trim()).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void String_TrimStart()
        {
            //11.String.Trim()
            //语句描述：这个例子使用Trim方法返回雇员家庭电话号码的前五位，并移除前导和尾随空格。
            var q = from e in db.Employees
                    where e.HomePhone.TrimStart() != "Hello"
                    select e.Address;
            q.ToList();
        }

        [TestMethod]
        public void String_TrimEnd()
        {
            //11.String.Trim()
            //语句描述：这个例子使用Trim方法返回雇员家庭电话号码的前五位，并移除前导和尾随空格。
            var q = from e in db.Employees
                    where e.HomePhone != null
                    select e.HomePhone.TrimEnd();
            var items = q.ToList();
        }

        [TestMethod]
        public void StringInsert()
        {
            //12.String.Insert(pos, str)
            //语句描述：这个例子使用Insert方法返回第五位为 ) 的雇员电话号码的序列，并在 ) 后面插入一个 :。
            IList items = (from e in db.Employees
                           //where e.HomePhone.Substring(4, 1) == ")"
                           where e.HomePhone != null
                           select e.HomePhone.Insert(5, ":")).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringRemove()
        {
            //13.String.Remove(start)
            //语句描述：这个例子使用Remove方法返回第五位为 ) 的雇员电话号码的序列，并移除从第十个字符开始的所有字符。
            IList items = (from e in db.Employees
                           where e.HomePhone.Substring(4, 1) == ")"
                           select e.HomePhone.Remove(9)).ToList();
            //Assert.IsTrue(items.Count > 0);

            //14.String.Remove(start, length)
            //语句描述：这个例子使用Remove方法返回第五位为 ) 的雇员电话号码的序列，并移除前六个字符。
            items = (from e in db.Employees
                     where e.HomePhone.Substring(4, 1) == ")"
                     select e.HomePhone.Remove(0, 6)).ToList();
            //Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringRelace()
        {
            //15.String.Replace(find, replace)
            //语句描述：这个例子使用 Replace 方法返回 Country 字段中UK 被替换为 United Kingdom 以及USA 
            //被替换为 United States of America 的供应商信息。
            IList items = (from s in db.Suppliers
                           select new
                           {
                               s.CompanyName,
                               Country = s.Country
                               .Replace("UK", "United Kingdom")
                               //.Replace("USA", "United States of America")
                           }).ToList();
            Assert.IsTrue(items.Count > 0);
        }
        #endregion

        #region 日期函数
        [TestMethod]
        public void DateTime_Year()
        {
            //16.DateTime.Year
            //语句描述：这个例子使用DateTime 的Year 属性查找1997 年下的订单。
            IList items = (from o in db.Orders
                           where o.OrderDate.Value.Year == 1997
                           select o).ToList();
            //Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void DateTime_Month()
        {
            //DateTime.Month
            //语句描述：这个例子使用DateTime的Month属性查找十二月下的订单。
            var items = (from o in db.Orders
                         where o.OrderDate != null && o.OrderDate.Value.Month == 12
                         select o).ToList();
            //Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void DateTime_Day()
        {
            //DateTime.Day
            var items = (from o in db.Orders
                         where o.OrderDate.Value.Day == 31
                         select o).ToList();
            //Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void DateTimeGT()
        {
            var items = (from o in db.Orders
                         where o.OrderDate.Value > DateTime.Now
                         select o).ToList();
            Assert.IsTrue(items.Count == 0);
        }



        [TestMethod]
        public void DateTimeGE()
        {
            var items = (from o in db.Orders
                         where o.OrderDate.Value >= DateTime.Now
                         select o).ToList();
            Assert.IsTrue(items.Count == 0);
        }

        [TestMethod]
        public void DateTimeLT()
        {
            var items = (from o in db.Orders
                         where o.OrderDate.Value < DateTime.Now
                         select o).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void DateTimeLE()
        {
            var items = (from o in db.Orders
                         where o.OrderDate.Value <= DateTime.Now
                         select o).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void DateTime_Date()
        {
            //var item = db.Orders.First();
            //var date1 = item.OrderDate.Value.Date;
            //var date2 = db.Orders.Where(o => o.OrderID == item.OrderID)
            //                     .Select(o => o.OrderDate).Single();
            //Assert.AreEqual(date1, date2);
            var q = db.Orders.Where(o => o.OrderDate != null)
                             .Select(o => new { o.OrderDate.Value.Date, o.OrderDate }).ToArray();
            foreach (var item in q)
            {
                Assert.AreEqual(item.OrderDate.Value.Date, item.Date);
            }
        }

        [TestMethod]
        public void DateTime_TimeOfDay()
        {
            //var item = db.Orders.First();
            //var date1 = item.OrderDate.Value.Date;
            //var date2 = db.Orders.Where(o => o.OrderID == item.OrderID)
            //                     .Select(o => o.OrderDate).Single();
            //Assert.AreEqual(date1, date2);
            var q = db.Orders.Where(o => o.OrderDate != null)
                             .Select(o => new { o.OrderDate, o.OrderDate.Value.TimeOfDay }).ToArray();
            foreach (var item in q)
            {
                var value = item.OrderDate.Value.TimeOfDay - item.TimeOfDay;
                Assert.IsTrue(value.TotalSeconds < 1);
            }
        }
        #endregion

        #region 增删改

        /// <summary>
        /// 简单形式
        /// <remarks>
        /// new一个对象，使用InsertOnSubmit方法将其加入到对应的集合中，使用SubmitChanges()提交到数据库。
        /// </remarks>
        /// </summary>
        [TestMethod]
        public void Insert_Simple()
        {
            //object item = (from customer in db.Customers
            //               where customer.CustomerID == "MCSFT"
            //               select customer).SingleOrDefault();
            //if (item != null)
            //    db.Customers.DeleteOnSubmit((Customer)item);
            //db.SubmitChanges();

            //var newCustomer = new Customer
            //{
            //    CustomerID = "MCSFT",
            //    CompanyName = "Microsoft",
            //    ContactName = "John Doe",
            //    ContactTitle = "Sales Manager",
            //    Address = "1 Microsoft Way",
            //    City = "Redmond",
            //    Region = "WA",
            //    PostalCode = "98052",
            //    Country = "USA",
            //    Phone = "(425) 555-1234",
            //    Fax = string.Empty,
            //};
            //db.Customers.InsertOnSubmit(newCustomer);
            //db.SubmitChanges();

            //item = (from customer in db.Customers
            //        where customer.CustomerID == "MCSFT"
            //        select customer).FirstOrDefault();
            //Assert.IsNotNull(item);

            var shipper = new Shipper
            {
                CompanyName = "CompanyName",
                Phone = "Phone",
            };
            db.Shippers.InsertOnSubmit(shipper);
            db.SubmitChanges();
            Console.WriteLine("ShipperID = " + shipper.ShipperID);

            //var data = new DataType();
            //data.DateTime = DateTime.Now;
            //db.DataTypes.InsertOnSubmit(data);
            //db.SubmitChanges();
            //Console.WriteLine(db.Connection.ConnectionString);
        }

        /// <summary>
        /// 一对多形式
        /// <remarks>Category与Product是一对多的关系，提交Category(一端)的数据时，LINQ to SQL会自动将Product(多端)的数据一起提交。</remarks>
        /// 使用InsertOnSubmit方法将新类别添加到Categories表中，并将新Product对象添加到与此新Category
        /// 有外键关系的Products表中。调用SubmitChanges将这些新对象及其关系保存到数据库。
        /// </summary>
        [TestMethod]
        public void Insert_OneToMany()
        {
            //先清除原来的数据
            IList items = (from product in db.Products
                           where product.ProductName == "Blue Widget"
                           select product).ToList();
            db.Products.DeleteAllOnSubmit((IList<Product>)items);

            items = (from category in db.Categories
                     where category.CategoryName == "Widgets"
                     select category).ToList();
            db.Categories.DeleteAllOnSubmit((IList<Category>)items);
            db.SubmitChanges();

            int count = (from product in db.Products
                         where product.ProductName == "Blue Widget"
                         select product).Count();
            Assert.AreEqual(0, count);

            count = (from category in db.Categories
                     where category.CategoryName == "Widgets"
                     select category).Count();
            Assert.AreEqual(0, count);

            var newCategory = new Category
            {
                CategoryName = "Widgets",
                Description = "Widgets are the ……"
            };
            var newProduct = new Product
            {
                ProductName = "Blue Widget",
                UnitPrice = 34.56M,
                Category = newCategory
            };
            db.Categories.InsertOnSubmit(newCategory);
            db.SubmitChanges();

            object item = (from category in db.Categories
                           where category.CategoryName == "Widgets"
                           select category).SingleOrDefault();
            Assert.IsNotNull(item);
            item = (from product in db.Products
                    where product.ProductName == "Blue Widget"
                    select product).SingleOrDefault();
            Assert.IsNotNull(item);
        }

        /// <summary>
        /// 多对多关系
        /// <remarks>在多对多关系中，我们需要依次提交。</remarks>
        /// 使用InsertOnSubmit方法将新雇员添加到Employees 表中，
        /// 将新Territory添加到Territories表中，并将新EmployeeTerritory对
        /// 象添加到与此新Employee对象和新Territory对象有外键关系的EmployeeTerritories
        /// 表中。调用SubmitChanges将这些新对象及其关系保持到数据库。
        /// </summary>
        [TestMethod]
        public void Insert_ManyToMany()
        {
            var items1 = (from employeeTerritory in db.EmployeeTerritories
                          where employeeTerritory.TerritoryID == "12345"
                          select employeeTerritory).ToList();
            foreach (var employeeTerritory in items1)
            {
                db.EmployeeTerritories.DeleteOnSubmit(employeeTerritory);
            }
            db.SubmitChanges();

            var items2 = (from employee in db.Employees
                          where employee.FirstName == "Kira" && employee.LastName == "Smith"
                          select employee).ToList();
            foreach (var employee in items2)
            {
                db.Employees.DeleteOnSubmit(employee);
            }
            db.SubmitChanges();

            var items3 = (from territory1 in db.Territories
                          where territory1.TerritoryID == "12345"
                          select territory1).ToList();

            foreach (var territory in items3)
            {
                db.Territories.DeleteOnSubmit(territory);
            }
            db.SubmitChanges();


            var newEmployee = new Employee
            {
                FirstName = "Kira",
                LastName = "Smith"
            };
            Assert.IsTrue(newEmployee.EmployeeID == 0);

            var newTerritory = new Territory
            {
                TerritoryID = "12345",
                TerritoryDescription = "Anytown",
                Region = db.Regions.First()
            };
            var newEmployeeTerritory = new EmployeeTerritory
            {
                Employee = newEmployee,
                Territory = newTerritory
            };

            db.Employees.InsertOnSubmit(newEmployee);
            db.Territories.InsertOnSubmit(newTerritory);
            db.EmployeeTerritories.InsertOnSubmit(newEmployeeTerritory);
            db.SubmitChanges();

            Assert.IsTrue(newEmployee.EmployeeID != 0);

            object item = (from territory in db.Territories
                           where territory.TerritoryID == "12345"
                           select territory).SingleOrDefault();
            Assert.IsTrue(item != null);

            item = (from employeeTerritory in db.EmployeeTerritories
                    where employeeTerritory.EmployeeID == newEmployee.EmployeeID &&
                          employeeTerritory.TerritoryID == newTerritory.TerritoryID
                    select employeeTerritory);
            Assert.IsTrue(item != null);

            db.SubmitChanges();
        }

        [TestMethod]
        public void Delete_ManyToMany()
        {
            var employees = (from employee in db.Employees
                             where employee.FirstName == "Kira" && employee.LastName == "Smith"
                             select employee).ToList();
            var count = employees.Count();
            if (count == 0)
                Insert_ManyToMany();
            foreach (var employee in employees)
            {
                foreach (var employeeTerritory in employee.EmployeeTerritories)
                {
                    db.EmployeeTerritories.DeleteOnSubmit(employeeTerritory);
                    db.Territories.DeleteOnSubmit(employeeTerritory.Territory);
                }
                db.Employees.DeleteOnSubmit(employee);
            }
            db.SubmitChanges();
        }

        /// <summary>
        /// 简单形式
        /// <remarks>更新操作，先获取对象，进行修改操作之后，直接调用SubmitChanges()方法即可提交。注意，这里是在同一个DataContext中，对于不同的DataContex看下面的讲解。</remarks>
        /// <![CDATA[使用SubmitChanges将对检索到的一个Customer对象做出的更新保持回数据库。]]>
        /// </summary>
        [TestMethod]
        public void Update()
        {
            Customer cust = db.Customers.Where(c => c.CustomerID == "ALFKI").Single();
            var sourceTitle = cust.ContactTitle;
            cust.ContactTitle = "Vice President";
            db.SubmitChanges();
            IList items = (from customer in db.Customers
                           where customer.ContactTitle == "Vice President"
                           select customer).ToList();
            Assert.IsTrue(items.Count > 0);

            cust.ContactTitle = sourceTitle;
            db.SubmitChanges();

            //2.多项更改
            //语句描述：使用SubmitChanges将对检索到的进行的更新保持回数据库。
            items = (from p in db.Products
                     where p.CategoryID == 1
                     select p).ToList();
            Assert.IsTrue(items.Count > 0);
            foreach (Product p in items)
            {
                p.UnitPrice = p.UnitPrice + 1.00M;
            }
            db.SubmitChanges();

            items = (from product in db.Products
                     where product.UnitPrice == 1.00M
                     select product).ToList();
            //Assert.IsTrue(items.Count > 0);


        }
        #endregion

        #region 继承
        [TestMethod]
        public void Inherit()
        {
            db.Log = Console.Out;
            IList items;
            //1.一般形式
            //日常我们经常写的形式，对单表查询。
            items = (from c in db.Contacts
                     select c).ToList();

            //2.OfType形式
            //这里我仅仅让其返回顾客的联系方式。
            items = (from c in db.Contacts.OfType<CustomerContact>()
                     select c).ToList();

            //3.IS形式
            //这个例子查找一下发货人的联系方式。
            items = (from c in db.Contacts
                     where c is ShipperContact
                     select c).ToList();

            //4.AS形式
            //这个例子就通吃了，全部查找了一番。
            items = (from c in db.Contacts
                     select c as FullContact).ToList();

            //5.Cast形式
            //使用Case形式查找出在伦敦的顾客的联系方式。
            items = (from c in db.Contacts
                     where c.ContactType == "Customer" &&
                               ((CustomerContact)c).City == "London"
                     select c).ToList();

            //6.UseAsDefault形式
            //当插入一条记录时，使用默认的映射关系了，但是在查询时，使用继承的关系了。
            //具体看看生成的SQL语句就直截了当了。

            //插入一条数据默认使用正常的映射关系
            var contact = new Contact()
            {
                ContactType = "Unknown",
                CompanyName = "Unknown Company",
                Phone = "333-444-5555"
            };
            db.Contacts.InsertOnSubmit(contact);
            db.SubmitChanges();
            //查询一条数据默认使用继承映射关系
            var con = (from c in db.Contacts
                       where c.CompanyName == "Unknown Company" &&
                                              c.Phone == "333-444-5555"
                       select c).FirstOrDefault();

            //7.插入新的记录
            //这个例子说明如何插入发货人的联系方式的一条记录。
            //1.在插入之前查询一下，没有数据
            items = (from sc in db.Contacts.OfType<ShipperContact>()
                     where sc.CompanyName == "Northwind Shipper"
                     select sc).ToList();

        }

        [TestMethod]
        public void Insert_Inherit()
        {
            db.Contacts.Delete(o => true);
            db.Log = Console.Out;
            var guid = new Guid("F7CC0F9F-DD28-4773-9D9F-500195E517D1");
            var customerContact = new CustomerContact()
            {
                GUID = guid, //new Guid("F7CC0F9F-DD28-4773-9D9F-500195E517D1"),
                CompanyName = ""
            };
            db.Contacts.InsertOnSubmit(customerContact);
            db.SubmitChanges();

            customerContact = db.Contacts.OfType<CustomerContact>().First();

            guid = Guid.NewGuid();
            var shipperContact = new ShipperContact();
            shipperContact.GUID = guid;
            db.Contacts.InsertOnSubmit(shipperContact);
            //db = CreateDataBaseInstace();
            //var a = db.Contacts.OfType<CustomerContact>().Select(o => o.GUID).Single();
            //Assert.IsNotNull(a);

            //db.Contacts.Delete(o => true);
            //db.Log = Console.Out;
            //if (db.Contacts.Count() == 0)
            //{
            //var context = new Contact() { CompanyName = "", GUID = Guid.NewGuid() };
            //db.Contacts.Insert(context);
            //db.SubmitChanges();
            //}

            //if (db.Contacts.OfType<FullContact>().Count() == 0)
            //{
            var fullContact = new FullContact()
                                  {
                                      CompanyName = "",
                                      GUID = Guid.NewGuid(),
                                      Address = "KAS",
                                  };
            db.Contacts.InsertOnSubmit(fullContact);
            db.SubmitChanges();
            //}

            // var c =(CustomerContact) db.Contacts.Where(o => o.GUID == guid).First();
            guid = Guid.NewGuid();
            var s = new SupplierContact();
            s.GUID = guid;
            s.CompanyName = "AAA";
            //s.ContactType = "Shipper";
            db.Contacts.InsertOnSubmit(s);
            db.SubmitChanges();

            s = (SupplierContact)db.Contacts.OfType<SupplierContact>().Where(o => o.GUID == guid).First();
            s.HomePage = "BBB";
            db.SubmitChanges();
        }
        #endregion

        #region DataLoadOption
        [TestMethod]
        public void DataLoadOptionsTest()
        {
            var options = new ALinq.DataLoadOptions();
            options.LoadWith<Order>(o => o.OrderDetails);
            //options.LoadWith<OrderDetail>(o => o.Product);
            db.LoadOptions = options;
            db.Log = Console.Out;
            var order = db.Orders.Single(o => o.OrderID == 10248);
            //Assert.IsTrue(order != null);

        }
        #endregion

        [TestMethod]
        public void Null()
        {
            var items = db.Employees.Where(o => o.ReportsTo == null).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        private static bool isValidProduct(Product product)
        {
            return true;
        }

        public string PhoneNumberConverter(string Country, string Phone)
        {
            Phone = Phone.Replace(" ", "").Replace(")", ")-");
            switch (Country)
            {
                case "USA":
                    return "1-" + Phone;
                case "UK":
                    return "44-" + Phone;
                default:
                    return Phone;
            }
        }

        class Name
        {
            internal string FirstName;
            internal string LastName;
        }

        [TestMethod]
        public virtual void CreateDatabase()
        {
            var db = CreateDataBaseInstace();
            db.Log = Console.Out;
            if (db.DatabaseExists())
                db.DeleteDatabase();

            db.CreateDatabase();
        }

        #region 并发测试
        [TestMethod]
        public void ResolveAll()
        {
            List<User> items;
            NorthwindDatabase db2;
            bool raiseException;

            #region OverwriteCurrentValues
            items = db.Users.ToList();
            db.Users.DeleteAllOnSubmit(items);
            db.SubmitChanges();

            var user = new User() { Manager = "Alfreds", Assistant = "Maria", Department = "Sales" };
            db.Users.InsertOnSubmit(user);
            db.SubmitChanges();

            var user1 = db.Users.First();
            user1.Manager = "Alfred";
            user1.Department = "Marketing";

            db2 = CreateDataBaseInstace();
            var user2 = db2.Users.First();
            user2.Assistant = "Mary";
            user2.Department = "Service";

            Assert.AreEqual(user1.ID, user2.ID);
            db2.SubmitChanges();

            raiseException = false;
            try
            {
                db.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
            catch (ChangeConflictException)
            {
                raiseException = true;
                foreach (var conflict in db.ChangeConflicts)
                    conflict.Resolve(RefreshMode.OverwriteCurrentValues);
            }
            Assert.IsTrue(raiseException);
            Assert.AreEqual("Alfreds", user1.Manager);
            Assert.AreEqual("Mary", user1.Assistant);
            Assert.AreEqual("Service", user1.Department);

            items = db.Users.ToList();
            db.Users.DeleteAllOnSubmit(items);
            db.SubmitChanges();
            #endregion

            #region KeepCurrentValues
            items = db.Users.ToList();
            db.Users.DeleteAllOnSubmit(items);
            db.SubmitChanges();

            user = new User() { Manager = "Alfreds", Assistant = "Maria", Department = "Sales" };
            db.Users.InsertOnSubmit(user);
            db.SubmitChanges();

            user1 = db.Users.First();
            user1.Manager = "Alfred";
            user1.Department = "Marketing";

            db2 = CreateDataBaseInstace();
            user2 = db2.Users.First();
            user2.Assistant = "Mary";
            user2.Department = "Service";

            Assert.AreEqual(user1.ID, user2.ID);
            db2.SubmitChanges();

            raiseException = false;
            try
            {
                db.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
            catch (ChangeConflictException)
            {
                raiseException = true;
                foreach (var conflict in db.ChangeConflicts)
                    conflict.Resolve(RefreshMode.KeepCurrentValues);
            }
            Assert.IsTrue(raiseException);
            Assert.AreEqual("Alfred", user1.Manager);
            Assert.AreEqual("Maria", user1.Assistant);
            Assert.AreEqual("Marketing", user1.Department);
            #endregion

            #region KeepChanges
            items = db.Users.ToList();
            db.Users.DeleteAllOnSubmit(items);
            db.SubmitChanges();

            user = new User() { Manager = "Alfreds", Assistant = "Maria", Department = "Sales" };
            db.Users.InsertOnSubmit(user);
            db.SubmitChanges();

            user1 = db.Users.First();
            user1.Manager = "Alfred";
            user1.Department = "Marketing";

            db2 = CreateDataBaseInstace();
            user2 = db2.Users.First();
            user2.Assistant = "Mary";
            user2.Department = "Service";

            Assert.AreEqual(user1.ID, user2.ID);
            db2.SubmitChanges();

            raiseException = false;
            try
            {
                db.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
            catch (ChangeConflictException)
            {
                raiseException = true;
                foreach (var conflict in db.ChangeConflicts)
                    conflict.Resolve(RefreshMode.KeepChanges);
            }
            Assert.IsTrue(raiseException);
            Assert.AreEqual("Alfred", user1.Manager);
            Assert.AreEqual("Mary", user1.Assistant);
            Assert.AreEqual("Marketing", user1.Department);
            #endregion
        }

        #endregion

        [Table(Name = "Categories")]
        class MyCategory
        {
            [Column]
            public byte[] Picture
            {
                get;
                set;
            }
        }

        //[TestMethod]
        public void ByteArrayType()
        {
            var items = db.GetTable<Category>().ToList();
            var pictures = items.Where(o => o.Picture != null).Select(o => o.Picture);
            Assert.IsTrue(pictures.Count() > 0);
        }

        [TestMethod]
        public void ComposeKey()
        {
            db.Log = Console.Out;
            //var item = db.Class1s.Select(o => o.Class2s).ToList();

        }
        #region Procedures

        #endregion



        public string ToLower(string value)
        {
            return value.ToLower();
        }

        [TestMethod]
        public void AutoGuid()
        {
            //var dataType = new DataType();
            //dataType.ID = db.DataTypes.Max(o => o.ID) + 1;
            //db.DataTypes.Insert(dataType);
            //db.SubmitChanges();
        }

        #region MyRegion
        //[TestMethod]
        //public void Procedure_AddCategory()
        //{
        //    var categoryID = db.Categories.Max(o => o.CategoryID) + 1;
        //    db.Log = Console.Out;
        //    db.AddCategory(categoryID, "category", "description");
        //}

        //[TestMethod]
        //public void Procedure_UpdateCategory()
        //{
        //    db.Log = Console.Out;
        //    var categoryID = db.Categories.Max(o => o.CategoryID) + 1;
        //    db.AddCategory(categoryID, "category", "description");
        //}

        //[TestMethod]
        //public void Procedure_DeleteCategory()
        //{
        //    db.Log = Console.Out;
        //    var categoryID = db.Categories.Max(o => o.CategoryID) + 1;
        //    db.AddCategory(categoryID, "category", "description");
        //} 
        #endregion

        [TestMethod]
        public void Association()
        {

        }

        [TestMethod]
        public void Test1()
        {
            // using System.Data.Common;
            var db = new AccessNorthwind(@"c:\northwind.mdb");

            var q =
                from cust in db.Customers
                where cust.City == "London"
                select cust;

            Console.WriteLine("Customers from London:");
            foreach (var z in q)
            {
                Console.WriteLine("\t {0}", z.ContactName);
            }

            DbCommand dc = db.GetCommand(q);
            Console.WriteLine("\nCommand Text: \n{0}", dc.CommandText);
            Console.WriteLine("\nCommand Type: {0}", dc.CommandType);
            Console.WriteLine("\nConnection: {0}", dc.Connection);

            Console.ReadLine();
        }

        [ALinq.Mapping.Table(Name = "Products")]
        public class MyProduct
        {
            [ALinq.Mapping.Column]
            public string ProductName;

            [ALinq.Mapping.Column]
            public int SupplierID;

            [ALinq.Mapping.Column]
            public int CategoryID;

            [ALinq.Mapping.Column]
            public string QuantityPerUnit;



        }

        [TestMethod]
        public void Test2()
        {
            var db = new AccessNorthwind("C:/Northwind.mdb");
            db.Log = Console.Out;
            //IQueryable<Customer> custQuery =
            //    from cust in db.Customers
            //    where cust.City == "London"
            //    select cust;

            //foreach (Customer custObj in custQuery)
            //{
            //    Console.WriteLine(custObj.CustomerID);
            //}

            var d = db.Employees.Where(o => o.EmployeeID == 1).Select(o => o.BirthDate).Min();

            Assert.AreEqual(DateTime.MinValue, d);
            Console.WriteLine(DateTime.Now.ToShortDateString());
            Console.Out.Flush();
        }
    }
}
