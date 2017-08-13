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
            //ʹ��whereɸѡ���׶صĿͻ�
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
            //ɸѡ1994 ���֮����õĹ�Ա��
            var employees = (from e in db.Employees
                             where e.HireDate >= new DateTime(1994, 1, 1)
                             select e).ToList();
            Assert.IsTrue(employees.Count > 0);
        }

        [TestMethod]
        public void Where_Simple3()
        {
            //ɸѡ������ڶ�����ˮƽ֮�µ�δ�ϻ��Ĳ�Ʒ
            var products = (from p in db.Products
                            where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
                            select p).ToList();
            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public void Where_Simple4()
        {
            //������������ǵ�������where��ɸѡ��UnitPrice����10����ͣ���Ĳ�Ʒ
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
            //���÷���ѡ����еĵ�һ����������
            Shipper shipper = db.Shippers.First();
            Assert.IsNotNull(shipper);
            //Ԫ�أ�ѡ��CustomerID Ϊ��BONAP���ĵ����ͻ�
            Customer cust = db.Customers.First(c => c.CustomerID == "BONAP");
            Assert.IsNotNull(cust);
            //������ѡ���˷Ѵ��� 10.00 �Ķ�����
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
            //ָ��������ʽ
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
            //.����������ʽ
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
            //���ط���������ʽ(LocalMethodCall)��
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
            // Distinct��ʽ��˵����ɸѡ�ֶ��в���ͬ��ֵ�����ڲ�ѯ���ظ��Ľ����������SQL���Ϊ��SELECT DISTINCT [City] FROM [Customers]
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
            //˵����ȡ�ཻ��ӳ١����ǻ�ȡ��ͬ���ϵ���ͬ������������ȱ�����һ�����ϣ�
            //�ҳ�����Ψһ��Ԫ�أ�Ȼ������ڶ������ϣ�����ÿ��Ԫ����ǰ���ҳ���Ԫ�����Աȣ�
            //�������������������ڶ����ֵ�Ԫ�ء�
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
            //Except����ǣ�
            //˵�����ų��ཻ��ӳ١����Ǵ�ĳ������ɾ������һ����������ͬ���
            //�ȱ�����һ�����ϣ��ҳ�����Ψһ��Ԫ�أ�Ȼ���ٱ����ڶ������ϣ�����
            //�ڶ�������������δ������ǰ������Ԫ�ؼ����е�Ԫ�ء�
            //�����������ѯ�˿ͺ�ְԱ��ͬ�Ĺ��ҡ�
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
            //˵������ȡ���ϵ�ǰn��Ԫ�أ��ӳ١���ֻ�����޶������Ľ������
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
            //˵�������ؼ����е�Ԫ�ظ���������INT���ͣ����ӳ١�����SQL���Ϊ��SELECT COUNT(*) FROM 
            //1.����ʽ��
            //�õ����ݿ��пͻ���������
            db.Log = Console.Out;
            int count = db.Customers.Count();
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public void Count_Conditional()
        {
            //2.��������ʽ��
            //�õ����ݿ���δ�ϻ���Ʒ��������
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
            Assert.AreEqual(0, count);
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
            //˵�������ؼ�������ֵ����Ԫ��֮�ͣ�����ӦΪINT���ͼ��ϣ����ӳ١�����SQL���Ϊ��SELECT SUM(��) FROM 

            //1.����ʽ��
            //�õ����ж��������˷ѣ�
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
            //2.ӳ����ʽ��
            //�õ����в�Ʒ�Ķ���������
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
            //˵�������ؼ�����Ԫ�ص���Сֵ�����ӳ١�����SQL���Ϊ��SELECT MIN(��) FROM 

            //1.����ʽ��
            //���������Ʒ����͵��ۣ�
            decimal? min = db.Products.Select(p => p.UnitPrice).Min();
            Assert.IsTrue(min > 0);

        }

        [TestMethod]
        public void Min_Projection()
        {
            //2.ӳ����ʽ��
            //�������ⶩ��������˷ѣ�
            var min = db.Orders.Min(o => o.Freight);
        }

        [TestMethod]
        public void Min_Elements()
        {
            //3.Ԫ�أ�
            //����ÿ������е�����͵Ĳ�Ʒ��
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
            //˵�������ؼ�����Ԫ�ص����ֵ�����ӳ١�����SQL���Ϊ��SELECT MAX(��) FROM

            //1.����ʽ��
            //���������Ա������������ڣ�
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
            //2.ӳ����ʽ��
            //���������Ʒ�����������
            var maxInStock = db.Products.Max(p => p.UnitsInStock);
        }

        [TestMethod]
        public void Max_Elements()
        {
            //3.Ԫ�أ�
            //����ÿ������е�����ߵĲ�Ʒ��
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
        //˵�������ؼ����е���ֵ����Ԫ�ص�ƽ��ֵ������ӦΪ�������ͼ��ϣ���
        //����ֵ����Ϊdouble�����ӳ١�����SQL���Ϊ��SELECT AVG(��) FROM

        //1.����ʽ��
        [Timeout(10000), TestMethod]
        public void Avg_Simple()
        {
            //�õ����ж�����ƽ���˷ѣ�
            decimal? avg = db.Orders.Select(o => o.Freight).Average();
            Assert.IsTrue(avg > 0);
        }

        //2.ӳ����ʽ��
        [TestMethod]
        public void Avg_Projection()
        {
            //�õ����в�Ʒ��ƽ�����ۣ�
            var avg = db.Products.Average(p => p.UnitPrice);
            Assert.IsTrue(avg > 0);
        }

        //3.Ԫ�أ�
        [TestMethod]
        public void Avg_Emelents()
        {
            //����ÿ������е��۸��ڸ����ƽ�����۵Ĳ�Ʒ��
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

        //4.���飺
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
            //1.һ�Զ��ϵ(1 to Many)��
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
            //˵������Զ��ϵһ����漰������(�����һ�������Թ����ģ����п���ֻ��2����)��
            //��һ������漰Employees, EmployeeTerritories, Territories������
            //���ǵĹ�ϵ��1��M��1��Employees��Territoriesû�к���ȷ�Ĺ�ϵ��
            //������������������From�Ӿ���ʹ���������ɸѡ������ͼ�Ĺ�Ա��ͬʱ�г������ڵ�����
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
            //3.�����ӹ�ϵ��
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
            //1.˫������(Two way join)��
            //��ʾ����ʽ��������������������ͶӰ�������
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
            //2.��������(There way join)��
            //��ʾ����ʽ�����������ֱ��ÿ����ͶӰ�������
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
            //3.���ⲿ����(Left Outer Join)��
            //��ʾ��˵�����ͨ��ʹ�� ��ʾ��˵�����ͨ��ʹ��DefaultIfEmpty() ��ȡ���ⲿ���ӡ�
            //�ڹ�Աû�ж���ʱ��DefaultIfEmpty()��������null��
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
            //4.ͶӰ��Let��ֵ(Projected let assignment)��
            //˵����let�������������letλ�ڵ�һ��from��select���֮�䡣
            //������Ӵ�����ͶӰ�����ա�Let�����ʽ��
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
            //5.��ϼ�(Composite Key)��
            //���������ʾ������ϼ������ӣ�
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
            //6.��Ϊnull/����Ϊnull�ļ���ϵ(Nullable/Nonnullable Key Relationship)��
            //���ʵ����ʾ��ι���һ���Ϊ null ����һ�಻��Ϊ null �����ӣ�
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
            //1.����ʽ
            //�������ʹ�� orderby ���������ڶԹ�Ա��������
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
            // 2.��������ʽ
            //ע�⣺Where��Order By��˳�򲢲���Ҫ������T-SQL�У�Where��Order By���ϸ��λ�����ơ�
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
            //3.��������
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
            //���������ʹ�ø��ϵ� orderby �Կͻ��������򣬽�������
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
            //��������չ��ʽ��������OrderBy/OrderByDescending֮��ģ���һ��ThenBy/ThenByDescending��չ������Ϊ�ڶ�λ�������ݣ�
            //�ڶ���ThenBy/ThenByDescending����Ϊ����λ�������ݣ��Դ�����
            IList items = (from o in db.Orders
                           where o.EmployeeID == 1
                           orderby o.ShipCountry, o.Freight descending
                           select o).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void OrderBy_GroupBy()
        {
            //6.��GroupBy��ʽ
            //���������ʹ��orderby��Max �� Group By �ó�ÿ������е�����ߵĲ�Ʒ������ CategoryID �������Ʒ��������
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
            //1.����ʽ��
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
            //2.Select�����ࣺ
            IList items = (from p in db.Products
                           group p by p.CategoryID into g
                           select new { CategoryID = g.Key, g }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void GroupBy_Max()
        {
            //3.���ֵ
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
            //4.��Сֵ
            //���������ʹ��Group By��Min����ÿ��CategoryID����͵��ۡ�
            //˵�����Ȱ�CategoryID���࣬�жϸ��������Ʒ�е�����С��Products��ȡ��CategoryIDֵ������UnitPriceֵ����MinPrice��
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
            //5.ƽ��ֵ
            //���������ʹ��Group By��Average�õ�ÿ��CategoryID��ƽ�����ۡ�
            //˵�����Ȱ�CategoryID���࣬ȡ��CategoryIDֵ�͸��������Ʒ�е��۵�ƽ��ֵ��
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
            //6.���
            //���������ʹ��Group By��Sum�õ�ÿ��CategoryID �ĵ����ܼơ�
            //˵�����Ȱ�CategoryID���࣬ȡ��CategoryIDֵ�͸��������Ʒ�е��۵��ܺ͡�
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
            //7.����
            //���������ʹ��Group By��Count�õ�ÿ��CategoryID�в�Ʒ��������
            //˵�����Ȱ�CategoryID���࣬ȡ��CategoryIDֵ�͸��������Ʒ��������
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
            //8.����������
            //���������ʹ��Group By��Count�õ�ÿ��CategoryID�жϻ���Ʒ��������
            //˵�����Ȱ�CategoryID���࣬ȡ��CategoryIDֵ�͸��������Ʒ�Ķϻ������� 
            //Count�����ʹ����Lambda���ʽ��Lambda���ʽ�е�p��������������һ��Ԫ�ػ���󣬼�ĳһ����Ʒ��
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
            //9.Where����
            //������������ݲ�Ʒ�ĨDID���飬��ѯ��Ʒ��������10��ID�Ͳ�Ʒ���������ʾ����Group By�Ӿ��ʹ��Where�Ӿ��������������10�ֲ�Ʒ�����
            //˵�����ڷ����SQL���ʱ���������Ƕ����Where������
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
            //10.����(Multiple Columns)
            //���������ʹ��Group By��CategoryID��SupplierID����Ʒ���顣
            //˵�����Ȱ���Ʒ�ķ��࣬�ְ���Ӧ�̷��ࡣ��by���棬new����һ�������ࡣ���Key��ʵ����һ����Ķ���Key��������Property��CategoryID��SupplierID����g.Key.CategoryID���Ա���CategoryID��ֵ��
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
            //11.���ʽ(Expression)
            //���������ʹ��Group By����������Ʒ���С���һ�����а������۴���10�Ĳ�Ʒ���ڶ������а�������С�ڻ����10�Ĳ�Ʒ��
            //˵��������Ʒ�����Ƿ����10���ࡣ������Ϊ���࣬���ڵ���һ�࣬С�ڼ�����Ϊ��һ�ࡣ
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
            //˵���������жϼ���������Ԫ���Ƿ�����ĳһ���������ӳ�
            //1.��������ʽ
            //���������������ӷ������ж��������������ڳ��еĿͻ���δ�¶����Ŀͻ���
            IList items = (from c in db.Customers
                           where !c.Orders.Any()
                           select c).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Any_Condition()
        {
            //2.��������ʽ��
            //������������һ�ֲ�Ʒ�ϻ������
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
            //˵���������жϼ������Ƿ������ĳһԪ�أ����ӳ١����Ƕ��������н������Ӳ����ġ�
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

            //1.����һ������
            //����������������ʹ��Contain�����ĸ��ͻ�����OrderIDΪ10248�Ķ�����
            var order = (from o in db.Orders
                         where o.OrderID == 10248
                         select o).First();
            items = db.Customers.Where(p => p.Orders.Contains(order)).ToList();
            Assert.IsTrue(items.Count > 0);

            //2.�������ֵ��
            //����������������ʹ��Contains���������ڳ���Ϊ����ͼ���׶ء�������¸绪�Ŀͻ�
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
            //�Զ����ͨ����ʽ��%��ʾ�㳤�Ȼ����ⳤ�ȵ��ַ�����_��ʾһ���ַ���
            //[]��ʾ��ĳ��Χ�����һ���ַ���[^]��ʾ����ĳ��Χ�����һ���ַ���
            //�����ѯ������ID�ԡ�C����ͷ�������ߡ�  

            //�����ѯ������IDû�С�AXOXT����ʽ�������ߣ�
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
            //1.AsEnumerable��������ת��Ϊ���� IEnumerable 
            //ʹ�� AsEnumerable<TSource> �ɷ������ͻ�Ϊ���� IEnumerable �Ĳ�����
            //�ڴ�ʾ���У�LINQ to SQL��ʹ��Ĭ�Ϸ��� Query���᳢�Խ���ѯת��Ϊ 
            //SQL ���ڷ�������ִ�С��� where �Ӿ������û�����Ŀͻ��˷��� (isValidProduct)��
            //�˷����޷�ת��Ϊ SQL��
            //���������ָ�� where �Ŀͻ��˷��� IEnumerable<T> ʵ�����滻���� IQueryable<T>��
            //��ͨ������ AsEnumerable<TSource>�������ִ�д˲�����
            IEnumerable<Product> items = from p in db.Products.AsEnumerable()
                                         where isValidProduct(p)
                                         select p;
            int count = 0;
            foreach (var item in items)
                count++;
            Assert.IsTrue(count > 0);

            ////2.ToArray��������ת��Ϊ����
            ////ʹ�� ToArray <TSource>�ɴ����д������顣
            var array = (from c in db.Customers
                         where c.City == "London"
                         select c).ToArray();
            Assert.IsTrue(array.Length > 0);

            ////3.ToList��������ת��Ϊ�����б� 
            ////ʹ�� ToList<TSource>�ɴ����д��������б������ʾ��ʹ�� ToList<TSource>ֱ�ӽ���ѯ�ļ��������뷺�� List<T>�� 
            var list = (from e in db.Employees
                        where e.HireDate >= new DateTime(1994, 1, 1)
                        select e).ToList();
            Assert.IsTrue(list.Count > 0);

            ////4.ToDictionary��������ת��Ϊ�ֵ�
            ////ʹ��Enumerable.ToDictionary<TSource, TKey>�������Խ�����ת��Ϊ�ֵ䡣TSource��ʾsource
            ////�е�Ԫ�ص����ͣ�TKey��ʾkeySelector���صļ������͡��䷵��һ����������ֵ��Dictionary<TKey, TValue>��
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

        #region �ַ�������
        [TestMethod]
        public void StringConcat()
        {
            //1.�ַ�������(String Concatenation)
            //����������������ʹ��+��������γɾ�����ó��Ŀͻ�Locationֵ�����н��ַ����ֶκ��ַ���������һ��
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
            //����������������ʹ��Length���Բ������ƶ���10���ַ������в�Ʒ��
            IList items = (from p in db.Products
                           where p.ProductName.Length < 10
                           select new { p.ProductID, p.ProductName, p.QuantityPerUnit, p.ReorderLevel, p.SupplierID, p.UnitPrice, p.UnitsInStock, p.UnitsOnOrder, p.CategoryID, p.Discontinued }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringContain()
        {
            //3.String.Contains(substring)
            //����������������ʹ��Contains����������������ϵ�������а�����Anders���Ŀͻ���
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
            //����������������ʹ��StartsWith����������ϵ�������ԡ�Maria����ͷ�Ŀͻ���
            IList items = (from c in db.Customers
                           where c.ContactName.StartsWith("Maria")
                           select c).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringEndsWith()
        {
            //6.String.EndsWith(suffix)
            //����������������ʹ��EndsWith����������ϵ�������ԡ�Anders����β�Ŀͻ���
            IList items = (from c in db.Customers
                           where c.ContactName.EndsWith("Anders")
                           select c).ToList();
            //Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringSubstring()
        {
            //7.String.Substring(start)
            //����������������ʹ��Substring�������ز�Ʒ�����дӵ��ĸ���ĸ��ʼ�Ĳ��֡�
            IList items = (from p in db.Products
                           select p.ProductName.Substring(3)).ToList();
            Assert.IsTrue(items.Count > 0);

            //8.String.Substring(start, length)
            //����������������ʹ��Substring�������Ҽ�ͥ�绰�������λ���ھ�λ�ǡ�555���Ĺ�Ա��
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
            //����������������ʹ��ToUpper��������������ת��Ϊ��д�Ĺ�Ա������
            var category = db.Categories.First();
            var name = category.CategoryName.ToUpper();
            IList items = db.Categories.Where(o => o.CategoryName.ToUpper() == name).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringToLower()
        {
            //10.String.ToLower()
            //����������������ʹ��ToLower����������ת��ΪСд��������ơ�
            var category = db.Categories.First();
            var name = category.CategoryName.ToLower();
            IList items = db.Categories.Where(o => o.CategoryName.ToLower() == name).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void String_Trim()
        {
            //11.String.Trim()
            //����������������ʹ��Trim�������ع�Ա��ͥ�绰�����ǰ��λ�����Ƴ�ǰ����β��ո�
            IList items = (from e in db.Employees
                           where e.HomePhone != null
                           select e.HomePhone.Trim()).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void String_TrimStart()
        {
            //11.String.Trim()
            //����������������ʹ��Trim�������ع�Ա��ͥ�绰�����ǰ��λ�����Ƴ�ǰ����β��ո�
            var q = from e in db.Employees
                    where e.HomePhone.TrimStart() != "Hello"
                    select e.Address;
            q.ToList();
        }

        [TestMethod]
        public void String_TrimEnd()
        {
            //11.String.Trim()
            //����������������ʹ��Trim�������ع�Ա��ͥ�绰�����ǰ��λ�����Ƴ�ǰ����β��ո�
            var q = from e in db.Employees
                    where e.HomePhone != null
                    select e.HomePhone.TrimEnd();
            var items = q.ToList();
        }

        [TestMethod]
        public void StringInsert()
        {
            //12.String.Insert(pos, str)
            //����������������ʹ��Insert�������ص���λΪ ) �Ĺ�Ա�绰��������У����� ) �������һ�� :��
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
            //����������������ʹ��Remove�������ص���λΪ ) �Ĺ�Ա�绰��������У����Ƴ��ӵ�ʮ���ַ���ʼ�������ַ���
            IList items = (from e in db.Employees
                           where e.HomePhone.Substring(4, 1) == ")"
                           select e.HomePhone.Remove(9)).ToList();
            //Assert.IsTrue(items.Count > 0);

            //14.String.Remove(start, length)
            //����������������ʹ��Remove�������ص���λΪ ) �Ĺ�Ա�绰��������У����Ƴ�ǰ�����ַ���
            items = (from e in db.Employees
                     where e.HomePhone.Substring(4, 1) == ")"
                     select e.HomePhone.Remove(0, 6)).ToList();
            //Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void StringRelace()
        {
            //15.String.Replace(find, replace)
            //����������������ʹ�� Replace �������� Country �ֶ���UK ���滻Ϊ United Kingdom �Լ�USA 
            //���滻Ϊ United States of America �Ĺ�Ӧ����Ϣ��
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

        #region ���ں���
        [TestMethod]
        public void DateTime_Year()
        {
            //16.DateTime.Year
            //����������������ʹ��DateTime ��Year ���Բ���1997 ���µĶ�����
            IList items = (from o in db.Orders
                           where o.OrderDate.Value.Year == 1997
                           select o).ToList();
            //Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void DateTime_Month()
        {
            //DateTime.Month
            //����������������ʹ��DateTime��Month���Բ���ʮ�����µĶ�����
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

        #region ��ɾ��

        /// <summary>
        /// ����ʽ
        /// <remarks>
        /// newһ������ʹ��InsertOnSubmit����������뵽��Ӧ�ļ����У�ʹ��SubmitChanges()�ύ�����ݿ⡣
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
        /// һ�Զ���ʽ
        /// <remarks>Category��Product��һ�Զ�Ĺ�ϵ���ύCategory(һ��)������ʱ��LINQ to SQL���Զ���Product(���)������һ���ύ��</remarks>
        /// ʹ��InsertOnSubmit�������������ӵ�Categories���У�������Product������ӵ������Category
        /// �������ϵ��Products���С�����SubmitChanges����Щ�¶������ϵ���浽���ݿ⡣
        /// </summary>
        [TestMethod]
        public void Insert_OneToMany()
        {
            //�����ԭ��������
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
                Description = "Widgets are the ����"
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
        /// ��Զ��ϵ
        /// <remarks>�ڶ�Զ��ϵ�У�������Ҫ�����ύ��</remarks>
        /// ʹ��InsertOnSubmit�������¹�Ա��ӵ�Employees ���У�
        /// ����Territory��ӵ�Territories���У�������EmployeeTerritory��
        /// ����ӵ������Employee�������Territory�����������ϵ��EmployeeTerritories
        /// ���С�����SubmitChanges����Щ�¶������ϵ���ֵ����ݿ⡣
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
        /// ����ʽ
        /// <remarks>���²������Ȼ�ȡ���󣬽����޸Ĳ���֮��ֱ�ӵ���SubmitChanges()���������ύ��ע�⣬��������ͬһ��DataContext�У����ڲ�ͬ��DataContex������Ľ��⡣</remarks>
        /// <![CDATA[ʹ��SubmitChanges���Լ�������һ��Customer���������ĸ��±��ֻ����ݿ⡣]]>
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

            //2.�������
            //���������ʹ��SubmitChanges���Լ������Ľ��еĸ��±��ֻ����ݿ⡣
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

        #region �̳�
        [TestMethod]
        public void Inherit()
        {
            db.Log = Console.Out;
            IList items;
            //1.һ����ʽ
            //�ճ����Ǿ���д����ʽ���Ե����ѯ��
            items = (from c in db.Contacts
                     select c).ToList();

            //2.OfType��ʽ
            //�����ҽ������䷵�ع˿͵���ϵ��ʽ��
            items = (from c in db.Contacts.OfType<CustomerContact>()
                     select c).ToList();

            //3.IS��ʽ
            //������Ӳ���һ�·����˵���ϵ��ʽ��
            items = (from c in db.Contacts
                     where c is ShipperContact
                     select c).ToList();

            //4.AS��ʽ
            //������Ӿ�ͨ���ˣ�ȫ��������һ����
            items = (from c in db.Contacts
                     select c as FullContact).ToList();

            //5.Cast��ʽ
            //ʹ��Case��ʽ���ҳ����׶صĹ˿͵���ϵ��ʽ��
            items = (from c in db.Contacts
                     where c.ContactType == "Customer" &&
                               ((CustomerContact)c).City == "London"
                     select c).ToList();

            //6.UseAsDefault��ʽ
            //������һ����¼ʱ��ʹ��Ĭ�ϵ�ӳ���ϵ�ˣ������ڲ�ѯʱ��ʹ�ü̳еĹ�ϵ�ˡ�
            //���忴�����ɵ�SQL����ֱ���˵��ˡ�

            //����һ������Ĭ��ʹ��������ӳ���ϵ
            var contact = new Contact()
            {
                ContactType = "Unknown",
                CompanyName = "Unknown Company",
                Phone = "333-444-5555"
            };
            db.Contacts.InsertOnSubmit(contact);
            db.SubmitChanges();
            //��ѯһ������Ĭ��ʹ�ü̳�ӳ���ϵ
            var con = (from c in db.Contacts
                       where c.CompanyName == "Unknown Company" &&
                                              c.Phone == "333-444-5555"
                       select c).FirstOrDefault();

            //7.�����µļ�¼
            //�������˵����β��뷢���˵���ϵ��ʽ��һ����¼��
            //1.�ڲ���֮ǰ��ѯһ�£�û������
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

        #region ��������
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

        //[TestMethod]
        //public void Test1()
        //{
        //    // using System.Data.Common;
        //    var db = new AccessNorthwind(@"c:\northwind.mdb");

        //    var q =
        //        from cust in db.Customers
        //        where cust.City == "London"
        //        select cust;

        //    Console.WriteLine("Customers from London:");
        //    foreach (var z in q)
        //    {
        //        Console.WriteLine("\t {0}", z.ContactName);
        //    }

        //    DbCommand dc = db.GetCommand(q);
        //    Console.WriteLine("\nCommand Text: \n{0}", dc.CommandText);
        //    Console.WriteLine("\nCommand Type: {0}", dc.CommandType);
        //    Console.WriteLine("\nConnection: {0}", dc.Connection);

        //    Console.ReadLine();
        //}

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
    }
}
