using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using ALinq.Access;
using ALinq.Mapping;
using ALinq.MySQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;
using ALinq.Dynamic;
using ALinq;

namespace Test
{
    [TestClass]
    public class DynamicQuery
    {
        private NorthwindDatabase db;

        [TestInitialize]
        public void TestInitialize()
        {
            var xmlMapping = XmlMappingSource.FromStream(typeof(SQLiteTest).Assembly.GetManifestResourceStream("Test.Northwind.Access.map"));
            //db = new AccessNorthwind("C:/Northwind.mdb");
            //db = new AccessNorthwind("C:/Northwind.mdb", xmlMapping);
            //db = new SQLiteNorthwind("C:/Northwind.db3");

            db = new MySqlNorthwind(MySqlNorthwind.CreateConnection("root", "test", "Northwind", "localhost", 3306).ConnectionString);
            db.Log = Console.Out;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            db.Log.Flush();
            db.Dispose();
        }

        [TestMethod]
        public void SelectNew()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => o["FirstName"])
                       .Column(o => o["LastName"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            db.GetTable<MyEmployee>()
                .Select(o => new { F = o["FirstName"], L = (string)o["LastName"] })
                .ToArray();
            db.GetTable<MyEmployee>().ToArray();
        }

        [TestMethod]
        public void SelectNew1()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => o["LastName"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            db.GetTable<MyEmployee>().Select(o => new { Item = o })
                                      .Select(o => o.Item["LastName"])
                                      .ToArray();
        }

        [TestMethod]
        public void SelectNew2()
        {

            db.Employees.Select(o => new { o.FirstName, Item = o })
                        .Select(o => o.Item.LastName)
                        .ToArray();

        }

        [TestMethod]
        public void Select3()
        {
            var table = db.GetTable<Employee>();
            table.Select(o => new { FirstName = o["FirstName"], LastName = o["LastName"] }).ToArray();

            table.Select("['FirstName'] as FirstName, ['LastName'] as LastName").Cast<object>().ToArray();
            table.Select("['FirstName'], ['LastName']").Cast<object>().ToArray();
            table.Select("[FirstName], [LastName]").Cast<object>().ToArray();
        }

        [TestMethod]
        public void SelectValue()
        {
            //Expression<Func<Employee
            //db.GetTable<Employee>().Select(o => o["FirstName"]).Cast<string>().ToArray();
            db.GetTable<Employee>().Select("[FirstName]").Cast<string>().ToArray();
        }

        [TestMethod]
        public void SelectNew4()
        {

        }

        [TestMethod]
        public void SelectWhere()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => (DateTime)o["BirthDate"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            var table = db.GetTable<MyEmployee>().Where(o => (DateTime?)o["BirthDate"] < DateTime.Now);
            table.Where(o => (DateTime?)o["BirthDate"] < DateTime.Now).ToArray();
        }

        [TestMethod]
        public void SelectSingleProperty()
        {
            db.GetTable<MyEmployee1>().Select(o => (string)o["LastName"]).ToArray();
            db.GetTable<MyEmployee1>().Select(o => o["LastName"]).ToArray();
        }


        [TestMethod]
        public void Where()
        {
            db.Employees.Where(o => o.ReportsTo == 10).Select(o => new { o.FirstName, o.LastName }).ToList();
            db.Employees.Where("ReportsTo == @0", 10).Select(o => new { o.FirstName, o.LastName }).ToList();
            db.Employees.Where("ReportsTo == 10").Select(o => new { o.FirstName, o.LastName }).ToList();
            db.Employees.Where("ReportsTo == NULL").Select(o => new { o.FirstName, o.LastName }).ToList();
            db.Employees.Where("ReportsTo != NULL").Select(o => new { o.FirstName, o.LastName }).ToList();
            var items = db.Employees.Where("ReportsTo == 1").Select(o => new { o.FirstName, o.LastName }).ToList();
            Assert.AreEqual(items.Count, db.Employees.Count(o => o.ReportsTo == 1));
            items = db.Employees.Where("ReportsTo == 1").Skip(0).Take(20).Select(o => new { o.FirstName, o.LastName }).ToList();
        }

        [TestMethod]
        public void DynamicDateTimeGE()
        {
            var items = (db.Orders.Where("OrderDate.Value >= #1990-1-1#")).ToList();
            //Assert.IsTrue(items.Count == 0);
        }

        [TestMethod]
        public void DynamicDateTimeBetween()
        {
            var items = (db.Orders.Where("OrderDate.Value >= #1990-1-1# &&  OrderDate.Value <= #2010-1-1#")).ToList();
            items = (db.Orders.Where("#1990-1-1# < OrderDate.Value &&  #2010-1-1# > OrderDate.Value")).ToList();
        }

        [TestMethod]
        public void ExpressionParserTest()
        {
            var p = Expression.Parameter(typeof(Employee));
            var methodInfo = typeof(Employee).GetMethod("get_Item");
            var body = Expression.Call(p, methodInfo, new[] { Expression.Constant("ReportsTo") });
            var p_ReportsTo = Expression.Lambda(body, p);

            var source = (IQueryable)db.Employees;
            var q1 = source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Select",
                                                 new[] { source.ElementType, typeof(object) },
                                                 source.Expression, Expression.Quote(p_ReportsTo)));

            Console.WriteLine(db.GetCommand(q1).CommandText);
            Console.WriteLine();

            Expression<Func<Employee, object>> exp0 = o => o["ReportsTo"];
            var q = db.Employees.Select(exp0);
            Console.WriteLine(db.GetCommand(q).CommandText);
            Console.WriteLine();

            Expression<Func<Employee, object>> exp5 = o => new { o.FirstName, ReportsTo = o["ReportsTo"] };
            q = db.Employees.Select(exp5);
            Console.WriteLine(db.GetCommand(q).CommandText);
            Console.WriteLine();

            var items = db.Employees.Select("FirstName").Cast<string>()
                          .ToArray();

            //var parser = new ExpressionParser(new[] { Expression.Parameter(typeof(Employee)) },
            //                                  "FirstName", new object[] { });

            //var exp = parser.Parse();
            //Expression<Func<Employee, string>> exp1 = o => o.FirstName;

            //var exp2 = ExpressionParser.Parse<Employee>("FirstName");
            //var exp3 = ExpressionParser.Parse<Employee>("new (FirstName + LastName as Name)");
            //var exp4 = ExpressionParser.Parse<Employee>("FirstName + LastName");
        }

        [TestMethod]
        public void DynamicProperty1()
        {
            Expression<Func<Employee, object>> exp0 = o => o.FirstName;
            var q = db.Employees.Select(exp0);
            Console.WriteLine(db.GetCommand(q).CommandText);
            Console.WriteLine();

            var p = Expression.Parameter(typeof(Employee));
            var body = Expression.MakeMemberAccess(p, typeof(Employee).GetMember("FirstName")[0]);
            var p_FirstName = Expression.Lambda(body, p);
            var q1 = db.Employees.Select(p_FirstName);
            Console.WriteLine(db.GetCommand(q1).CommandText);
            Console.WriteLine();
        }

        [TestMethod]
        public void DynamicProperty3()
        {
            //var exp1 = ExpressionParser.Parse<Order>("OrderDetails[0]");
            //var exp2 = ExpressionParser.Parse<Employee>("this['FirstName']");

            var pi = typeof(MyEmployee1).GetProperty("Item");
            Console.WriteLine(pi);
            //var q1 = db.Employees.Select(o => new { o.BirthDate, FirstName = o["FirstName"], LastName = o["LastName"] });
            //q1.Cast<dynamic>().ToList().ForEach(Console.WriteLine);

            //var q = db.Employees.Select("new (this['FirstName'] as FirstName, this['LastName'] as LastName)");
            //q.Cast<dynamic>().ToList().ForEach(Console.WriteLine);
        }

        class MyEmployee
        {
            private Dictionary<string, object> values;

            public MyEmployee()
            {
                this.values = new Dictionary<string, object>();
            }

            //[Column(Name = "EmployeeID", IsDbGenerated = true, IsPrimaryKey = true)]


            public object this[string key]
            {
                get
                {
                    switch (key)
                    {
                        default:
                            object value;
                            if (values.TryGetValue(key, out value))
                                return value;

                            return null;
                    }
                }
                set
                {
                    switch (key)
                    {
                        default:
                            values[key] = value;
                            break;
                    }
                }
            }
        }

        class MyCustomer
        {
            private Dictionary<string, object> values;

            public MyCustomer()
            {
                this.values = new Dictionary<string, object>();
            }

            [Column(Storage = "values")]
            public object this[string key]
            {
                get
                {
                    switch (key)
                    {
                        default:
                            object value;
                            if (values.TryGetValue(key, out value))
                                return value;

                            return null;
                    }
                }
                set
                {
                    switch (key)
                    {
                        default:
                            values[key] = value;
                            break;
                    }
                }
            }
        }

        class MyOrder
        {
            private Dictionary<string, object> values;

            public MyOrder()
            {
                this.values = new Dictionary<string, object>();
            }

            //[Column(Name = "OrderID", IsPrimaryKey = true)]
            //public int ID
            //{
            //    get;
            //    set;
            //}

            public object this[string key]
            {
                get
                {
                    switch (key)
                    {
                        default:
                            object value;
                            if (values.TryGetValue(key, out value))
                                return value;

                            return null;
                    }
                }
                set
                {
                    switch (key)
                    {
                        default:
                            values[key] = value;
                            break;
                    }
                }
            }
        }

        [Table(Name = "OrderDetails")]
        class MyOrderDetail
        {
            private Dictionary<string, object> values;

            public MyOrderDetail()
            {
                this.values = new Dictionary<string, object>();
            }

            [Column(Name = "OrderID")]
            public int OrderID
            {
                get;
                set;
            }

            [Column(Name = "ProductID")]
            public int ProductID
            {
                get;
                set;
            }

            [Column(Storage = "values")]
            public object this[string key]
            {
                get
                {
                    switch (key)
                    {
                        case "OrderID":
                            return OrderID;
                        case "ProductID":
                            return ProductID;
                        default:
                            object value;
                            if (values.TryGetValue(key, out value))
                                return value;

                            return null;
                    }
                }
                set
                {
                    switch (key)
                    {
                        case "ID":
                            OrderID = Convert.ToInt32(value);
                            break;
                        case "ProductID":
                            ProductID = Convert.ToInt32(value);
                            break;
                        default:
                            values[key] = value;
                            break;
                    }
                }
            }
        }

        [Table(Name = "Employees")]
        class MyEmployee1 : IEntityObject
        {
            private readonly ExpandoObject propertyies;

            public MyEmployee1()
            {
                //System.Dynamic.ExpandoObject ;
                this.propertyies = new ExpandoObject();
            }

            [Column]
            public int EmployeeID { get; set; }

            [Column]
            public string FirstName { get; set; }

            [Column]
            public string LastName { get; set; }

            private Dictionary<string, object> values = new Dictionary<string, object>();
            [Column]
            public object this[string key]
            {
                get { return values[key]; }
                set
                {
                    Console.WriteLine("Great,SUCCESS!!!");
                    values[key] = value;
                }
            }
        }

        class MyProduct
        {
            private Dictionary<string, object> values = new Dictionary<string, object>();
            public object this[string key]
            {
                get { return values[key]; }
                set
                {
                    Console.WriteLine("Great,SUCCESS!!!");
                    values[key] = value;
                }
            }

        }

        [TestMethod]
        public void DynamicProperty2()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var contextMapping = new DataContextMapping<DataContext>();
                contextMapping.Provider = new ProviderAttribute(typeof(ALinq.MySQL.MySqlProvider));

                var employeeMapping = new EntityMapping<MyEmployee>();
                employeeMapping.Table("Employees")
                               .Column(o => o["EmployeeID"], o => o.PrimaryKey().AutoSyncOnInsert())
                               .Column(o => o["City"])
                               .Column(o => o["FirstName"])
                               .Column(o => o["LastName"]);

                contextMapping.Add(employeeMapping);
                return contextMapping;
            });

            var conn = "server=localhost;User Id=root;Password=test;Persist Security Info=True;database=northwind";
            var db = new DataContext(conn, mappingSource) { Log = Console.Out };

            var table = db.GetTable<MyEmployee>();

            table.Select(o => o["FirstName"]).Cast<dynamic>().ToArray();
            table.Select("['FirstName']").Cast<dynamic>().ToArray();

            table.Select(o => new { OrderDate = o["FirstName"] }).ToArray();
            table.Select("new (['FirstName'] as F, ['LastName'] as L)").Cast<dynamic>().ToList()
                 .ForEach(o => Console.WriteLine(o.F + " " + o.L));

        }

        [TestMethod]
        public void DynamicColumGroup()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var contextMapping = new DataContextMapping<DataContext>();
                contextMapping.Provider = new ProviderAttribute(typeof(ALinq.MySQL.MySqlProvider));

                var employeeMapping = new EntityMapping<MyEmployee>();
                employeeMapping.Table("Employees")
                               .Column(o => o["EmployeeID"], o => o.PrimaryKey().AutoSyncOnInsert())
                               .Column(o => o["City"])
                               .Column(o => o["FirstName"])
                               .Column(o => o["LastName"]);

                contextMapping.Add(employeeMapping);
                return contextMapping;
            });

            var conn = "server=localhost;User Id=root;Password=test;Persist Security Info=True;database=northwind";
            var db = new DataContext(conn, mappingSource);
            var table = db.GetTable<MyEmployee>();

            var keys1 = table.GroupBy(o => o["City"])
                             .Select(o => o.Key);
            keys1.ToList().ForEach(Console.WriteLine);

            var keys2 = table.GroupBy(o => new { F = o["FirstName"], L = o["LastName"] })
                             .Select(o => o.Key);
            keys2.ToList().ForEach(Console.WriteLine);


        }

        [TestMethod]
        public void GroupWhere1()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => (DateTime)o["BirthDate"])
                       .Column(o => o["FirstName"])
                       .Column(o => o["LastName"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource);
            var table = db.GetTable<MyEmployee>();
            var keys3 = table.Where(o => (DateTime)o["BirthDate"] < DateTime.Now)
                             .GroupBy(o => new { F = o["FirstName"], L = o["LastName"] })
                             .Select(o => o.Key);
            keys3.ToList().ForEach(Console.WriteLine);
        }

        [TestMethod]
        public void GroupWhere2()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => (DateTime)o["BirthDate"])
                       .Column(o => (string)o["FirstName"])
                       .Column(o => (string)o["LastName"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource);
            var table = db.GetTable<MyEmployee>();
            var keys3 = table.Where(o => (DateTime?)o["BirthDate"] < DateTime.Now)
                             .GroupBy(o => new { F = o["FirstName"], L = o["LastName"] })
                             .Select(o => o.Key);
            keys3.ToList().ForEach(Console.WriteLine);
        }

        [TestMethod]
        public void GroupWhere3()
        {
            var table = db.GetTable<Employee>();
            var keys3 = table.Where(o => (DateTime?)o.BirthDate < DateTime.Now)//(DateTime?)o["BirthDate}"]
                             .GroupBy(o => o.FirstName)
                             .Select(o => o.Key);
            keys3.ToList().ForEach(Console.WriteLine);
        }

        [TestMethod]
        public void GroupWhere4()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => (DateTime)o["BirthDate"])
                       .Column(o => o["FirstName"])
                       .Column(o => o["LastName"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource);
            var table = db.GetTable<MyEmployee>();
            var keys3 = table.Where(o => (DateTime)o["BirthDate"] < DateTime.Now)
                             .GroupBy(o => o["FirstName"])
                             .Select(o => o.Key);
            keys3.ToList().ForEach(Console.WriteLine);
        }

        [TestMethod]
        public void DynamicColumJoin()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => o["EmployeeID"])
                       .Column(o => o["BirthDate"])
                       .Column(o => o["FirstName"])
                       .Column(o => o["LastName"]);

                mapping.Map<MyOrder>()
                       .Table("Orders")
                       .Column(o => o["OrderID"])
                       .Column(o => o["OrderDate"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            var employees = db.GetTable<MyEmployee>();
            var orders = db.GetTable<MyOrder>();
            employees.Join(orders, o => o["EmployeeID"], o => o["OrderID"],
                             (a, b) => new { FirstName = a["FirstName"], OrderDate = b["OrderDate"] }).ToArray();
        }

        //ParseElementAccess(expr);
        class c1
        {
            static c1()
            {

            }

            public c1(string FirstName, object ReportsTo)
            {
                this.FirstName = FirstName;
                this.ReportsTo = ReportsTo;
            }

            public string FirstName { get; set; }

            public object ReportsTo { get; set; }
        }

        [TestMethod]
        public void DynamicNew()
        {
            Expression<Func<Employee, object>> exp0 = o => new { o.FirstName };
            var q = db.Employees.Select(exp0);
            Console.WriteLine(db.GetCommand(q).CommandText);
            Console.WriteLine();

            db.Employees.Select("new (FirstName)");
            var p = Expression.Parameter(typeof(Employee));
            var p_FirstName = Expression.MakeMemberAccess(p, typeof(Employee).GetMember("FirstName")[0]);

            var methodInfo = typeof(Employee).GetMethod("get_Item");
            var body = Expression.Call(p, methodInfo, new[] { Expression.Constant("ReportsTo") });
            var p_ReportsTo = Expression.Lambda<Func<Employee, object>>(body, p);

            var exp1 = Expression.New(typeof(c1).GetConstructor(new[] { typeof(string), typeof(int) }),
                                      new Expression[] { p_FirstName, p_ReportsTo },
                                      new[] { typeof(c1).GetMember("FirstName")[0], typeof(c1).GetMember("ReportsTo")[0] });
            var predicate = Expression.Lambda<Func<Employee, c1>>(exp1, new[] { p });
            var q1 = db.Employees.Select(predicate);
            Console.WriteLine(db.GetCommand(q1).CommandText);
            Console.WriteLine();
        }

        [TestMethod]
        public void Aggregate_Null()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                mapping.Map<MyProduct>()
                       .Table("Products")
                       .Column(o => (int)o["ProductID"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource);
            var table = db.GetTable<MyProduct>();
            table.Select(o => (int)o["ProductID"]).ToArray();

            var min = table.Min(o => (int)o["ProductID"]);
            var max = table.Max(o => (int)o["ProductID"]);
        }

        [TestMethod]
        public void ConvertTest()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyProduct>().Table("Products").Column(o => o["ProductID"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            var table = db.GetTable<MyProduct>();
            table.Select(o => o["ProductID"]).ToArray();
            table.Select(o => (int)o["ProductID"]).ToArray();
            table.Select(o => Convert.ToString((int)o["ProductID"])).ToArray();

            //var min = table.Min(o => (int)o["ProductID"]);
            //var max = table.Max(o => (int)(o["ProductID"]));
        }

        public interface IEntityObject
        {
            //[Column(IsPrimaryKey = true)]
            //int EmployeeID { get; set; }

            //string FirstName { get; set; }

            //string LastName { get; set; }

            object this[string key] { get; set; }
        }


        public class BaseEntity
        {
            private Dictionary<string, object> items;

            public BaseEntity()
            {
                items = new Dictionary<string, object>();
            }

            public T GetValue<T>(string key)
            {
                var value = this[key];
                if (value == null)
                    return default(T);

                return (T)value;
            }

            public void SetValue<T>(string key, T value)
            {
                this[key] = value;
            }


            public object this[string key]
            {
                get
                {
                    object value;
                    if (items.TryGetValue(key, out value))
                        return value;

                    return null;
                }
                set
                {
                    items[key] = value;
                }
            }
        }

        [TestMethod]
        public void DynamicTable()
        {
            //var table = db.GetTable<IEntityObject>("EMPLOYEES",typeof(BaseEntity));
            //var q1 = table.Cast<IEntityObject>().ToArray();
            //var q2 = table.Cast<IEntityObject>().Select(o => o.FirstName).ToArray();

            //var item = table.Cast<IEntityObject>().First();
            //Console.WriteLine(item.FirstName);
            //item["Key"] = "hello";
            //Console.WriteLine(item["Key"]);
            //Console.WriteLine(item["FirstName"]);

            //var products = db.GetTable<IEntityObject>("PRODUCTS").Cast<IEntityObject>();
            //products.Select(o => o["ProductID"]).ToArray();
            //products.Where(o => (int)o["ProductID"] == 10).Cast<int>().ToArray();
            //var employees = db.GetTable<IEntityObject>("EMPLOYEES").Cast<IEntityObject>();
            //employees.Where(o => (int)o["EmployeeID"] == 10).Cast<dynamic>().ToArray();

            //db.GetTable<IEntityObject>("EMPLOYEES").Cast<IEntityObject>()
            //  .Where(o => o.FirstName == "XXXX")
            //  .Select(o => o.FirstName)
            //  .ToArray();

            //    db.GetTable<IEntityObject>("EMPLOYEES").Cast<IEntityObject>()
            //.Where(o => o.EmployeeID == 29)
            //.Select(o => o.FirstName)
            //.ToArray(); 


            //db.GetTable<IEntityObject>("EMPLOYEES").Cast<IEntityObject>()
            //  .Where(o => (string)o["FirstName"] == "XXXX")
            //  .ToArray(); 

            //db.GetTable(typeof(MyEmployee1)).Cast<IEntityObject>()
            //  .Where(o => o.FirstName == "XXXX")
            //  .Select(o => o.FirstName)
            //  .ToArray();

            //          db.GetTable<IEntityObject>("EMPLOYEES").Cast<IEntityObject>()
            //.Where(o => o.EmployeeID == 20)
            //.Select(o => o.FirstName)
            //.ToArray();

            //          db.GetTable(typeof(MyEmployee1)).Cast<IEntityObject>()
            //.Where(o => o.EmployeeID == 20)
            //.Select(o => o.FirstName)
            //.ToArray();
            //table.Where("EmployeeID == 10").Cast<dynamic>().ToArray();
        }

        [TestMethod]
        public void Update()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => (int)o["EmployeeID"]);
                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            var table = db.GetTable<MyEmployee>();
            table.Update(o => new { FirstName = "AAA", LastName = "BBB" },
                            o => (int)o["EmployeeID"] == -1);


        }

        [TestMethod]
        public void Insert()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => (int)o["EmployeeID"]);
                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            var table = db.GetTable<MyEmployee>();
            var id = table.Insert(o => new { FirstName = "AAA", LastName = "BBB" });
        }

        [TestMethod]
        public void Delete()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => (int)o["EmployeeID"])
                       .Column(o => (DateTime)o["BirthDate"])
                       .Column(o => (string)o["FirstName"])
                       .Column(o => (string)o["LastName"]);

                mapping.Map<MyOrder>()
                       .Table("Orders")
                       .Column(o => (int)o["OrderID"])
                       .Column(o => (DateTime)o["OrderDate"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource);
            var table = db.GetTable<MyEmployee>();
            table.Delete(o => (int)o["EmployeeID"] == -1);
        }

        [TestMethod]
        public void UpdateWithSubmit()
        {
            //var table = db.GetTable<MyEmployee>();
            //var item = table.OrderByDescending(o => o.ID)
            //                .First();

            //item["FirstName"] = "AAA";
            //item["LastName"] = "BBB";

            //db.SubmitChanges();

            //item["FirstName"] = "AAA";
            //item["LastName"] = "BBB";
            //db.SubmitChanges();
        }

        [TestMethod]
        public void InsertWithSubmit()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => (int)o["EmployeeID"], o => o.PrimaryKey().AutoSyncOnInsert().DbGenerated())
                       .Column(o => (string)o["FirstName"])
                       .Column(o => (string)o["LastName"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            var item = new MyEmployee();

            item["FirstName"] = "CCC";
            item["LastName"] = "DDD";

            var table = db.GetTable<MyEmployee>();
            table.InsertOnSubmit(item);

            db.SubmitChanges();
        }

        [TestMethod]
        public void DeleteWithSubmit()
        {
            //var table = db.GetTable<MyEmployee>();
            //var item = table.OrderByDescending(o => o.ID).First();
            //table.DeleteOnSubmit(item);
            //db.SubmitChanges();
        }

        #region Restriction Operators
        [TestMethod]
        public void Where_Simple1()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyCustomer>()
                       .Table("Customers")
                       .Column(o => (string)o["City"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            //使用where筛选在伦敦的客户
            var table = db.GetTable<MyCustomer>();
            var customers = table.Where(o => (string)o["City"] == "London").ToArray();
            Assert.IsTrue(customers.Length > 0);
        }

        [TestMethod]
        public void Where_Simple2()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => (DateTime)o["HireDate"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };

            //筛选1994 年或之后雇用的雇员：
            //var employees = (from e in db.Employees
            //                 where e.HireDate >= new DateTime(1994, 1, 1)
            //                 select e).ToList();
            //Assert.IsTrue(employees.Count > 0);
            var table = db.GetTable<MyEmployee>();
            var employees = table.Where(o => (DateTime?)o["HireDate"] >= new DateTime(1994, 1, 1)).ToList();
            Assert.IsTrue(employees.Count > 0);
        }

        [TestMethod]
        public void Where_Simple3()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyProduct>()
                       .Table("Products")
                       .Column(o => (int)o["UnitsInStock"])
                       .Column(o => (bool)o["Discontinued"])
                       .Column(o => (int)o["ReorderLevel"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            //筛选库存量在订货点水平之下但未断货的产品
            var table = db.GetTable<MyProduct>();
            var products = table.Where(o => (short?)o["UnitsInStock"] <= (short?)o["ReorderLevel"] &&
                                             !(bool)o["Discontinued"]).ToList();
            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public void Where_Simple4()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyProduct>()
                       .Table("Products")
                       .Column(o => (decimal)o["UnitPrice"])
                       .Column(o => (bool)o["Discontinued"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };

            //下面这个例子是调用两次where以筛选出UnitPrice大于10且已停产的产品
            var table = db.GetTable<MyProduct>();
            var q = table.Where(o => (decimal)o["UnitPrice"] > 10m);
            var products = q.Where(o => !(bool)o["Discontinued"]).ToList();

            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public void Where_Drilldown()
        {
            //Where - Drilldown
            //This sample prints a list of customers from the state of Washington along with their orders. 
            //A sequence of customers is created by selecting customers where the region is 'WA'. The sample 
            //uses doubly nested foreach statements to print the order numbers for each customer in the sequence. 
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                mapping.Map<MyCustomer>()
                       .Table("Customers")
                       .Column(o => (string)o["Region"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            var table = db.GetTable<MyCustomer>();
            var waCustomers = table.Where(o => (string)o["Region"] == "WA").ToList();
            Assert.IsTrue(waCustomers.Count > 0);

        }
        #endregion

        #region Join Operation
        [TestMethod]
        public void Join_OnToMany()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var mapping = new DataContextMapping<DataContext>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                mapping.Map<MyCustomer>()
                       .Table("Customers")
                       .Column(o => (string)o["City"]);

                mapping.Map<MyOrder>()
                       .Table("Orders")
                       .Column(o => o["OrderID"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };

            var customers = db.GetTable<MyCustomer>();
            var orders = db.GetTable<MyOrder>();
            var q = from c in customers
                    from o in orders
                    where (string)c["City"] == "London"
                    select o;
            var items = q.ToList();
            Assert.IsTrue(items.Count > 0);

        }



        [TestMethod]
        public void Join_TowWay()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                mapping.Map<MyOrder>()
                       .Table("Orders")
                       .Column(o => o["OrderID"])
                       .Column(o => o["CustomerID"]);

                mapping.Map<MyCustomer>()
                       .Table("Customers")
                       .Column(o => o["CustomerID"])
                       .Column(o => o["ContactName"]);

                return mapping;
            });
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            var customers = db.GetTable<MyCustomer>();
            var orders = db.GetTable<MyOrder>();

            var items = (from c in customers
                         join o in orders on c["CustomerID"]
                         equals o["CustomerID"] into q
                         select new
                         {
                             ContactName = c["ContactName"],
                             OrderCount = q.Count()
                         }).ToList();
            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public void Join_ThreeWay()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(ALinq.MySQL.MySqlProvider));

                mapping.Map<MyCustomer>()
                       .Table("Customers")
                       .Column(o => o["CustomerID"])
                       .Column(o => o["City"])
                       .Column(o => o["ContactName"]);

                mapping.Map<MyOrder>()
                       .Table("Orders")
                       .Column(o => o["CustomerID"])
                       .Column(o => o["OrderID"]);

                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => o["City"]);

                return mapping;
            });
            string conn = "server=localhost;User Id=root;Password=test;Persist Security Info=True;database=northwind";
            var db = new DataContext(conn, mappingSource) { Log = Console.Out };
            var customers = db.GetTable<MyCustomer>();
            var orders = db.GetTable<MyOrder>();
            var employees = db.GetTable<MyEmployee>();
            var items = (from c in customers
                         join o in orders on c["CustomerID"]
                             equals o["CustomerID"] into ords
                         join e in employees on c["City"]
                             equals e["City"] into emps
                         select new
                         {
                             ContactName = c["ContactName"],
                             ords = ords.Count(),
                             emps = emps.Count(),
                         }).ToList();

        }


        [TestMethod]
        public void Join_LetAssignment()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(MySqlProvider));
                mapping.Map<MyCustomer>()
                       .Table("Customers")
                       .Column(o => (int)o["CustomerID"])
                       .Column(o => (int)o["OrderID"])
                       .Column(o => (int)o["CustomerID"])
                       .Column(o => (string)o["Country"])
                       .Column(o => (string)o["City"])
                       .Column(o => (string)o["ContactName"]);

                mapping.Map<MyOrder>()
                       .Table("Orders")
                       .Column(o => (int)o["OrderID"])
                       .Column(o => (int)o["CustomerID"]);

                return mapping;
            });
            string conn = "server=localhost;User Id=root;Password=test;Persist Security Info=True;database=northwind";
            var db = new DataContext(conn, mappingSource) { Log = Console.Out };

            var customers = db.GetTable<MyCustomer>();
            var orders = db.GetTable<MyOrder>();
            var q = (from c in customers
                     join o in orders on c["CustomerID"]
                         equals o["CustomerID"] into ords
                     let z = (string)c["City"] + (string)c["Country"]
                     from o in ords
                     select new
                     {
                         ContactName = c["ContactName"],
                         OrderID = o["OrderID"],
                         z
                     }).ToList();
            Assert.IsTrue(q.Count > 0);

        }

        [TestMethod]
        public void Join_CompositeKey()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(MySqlProvider));

                mapping.Map<MyOrder>()
                       .Table("Orders")
                       .Column(o => o["OrderID"]);

                mapping.Map<MyProduct>()
                       .Table("Products")
                       .Column(o => o["ProductID"]);

                mapping.Map<MyOrderDetail>()
                       .Table("OrderDetails")
                       .Column(o => o["OrderID"])
                       .Column(o => o["ProductID"])
                       .Column(o => o["UnitPrice"]);
                return mapping;
            });
            string conn = "server=localhost;User Id=root;Password=test;Persist Security Info=True;database=northwind";
            var db = new DataContext(conn, mappingSource) { Log = Console.Out };
            var orders = db.GetTable<MyOrder>();
            var products = db.GetTable<MyProduct>();
            var orderDetails = db.GetTable<MyOrderDetail>();
            (from o in orders
             from p in products
             join d in orderDetails
                 on new
                 {
                     OrderID = o["OrderID"],
                     ProductID = p["ProductID"],
                 }
                 equals new
                            {
                                OrderID = d["OrderID"],
                                ProductID = d["ProductID"]
                            } into details
             from d in details
             select new
             {
                 OrderID = o["OrderID"],
                 ProductID = p["ProductID"],
                 UnitPrice = d["UnitPrice"]

             }).ToList();

        }

        [TestMethod]
        public void Join_NullableNonnullableKeyRelationship()
        {
            var mappingSource = new FluentMappingSource(delegate(Type contextType)
            {
                var mapping = new DataContextMapping(contextType);
                mapping.Provider = new ProviderAttribute(typeof(MySqlProvider));
                mapping.Map<MyOrder>()
                       .Table("Orders")
                       .Column(o => o["OrderID"])
                       .Column(o => o["EmployeeID"]);

                mapping.Map<MyEmployee>()
                       .Table("Employees")
                       .Column(o => o["EmployeeID"])
                       .Column(o => o["FirstName"]);

                return mapping;
            });
            string conn = "server=localhost;User Id=root;Password=test;Persist Security Info=True;database=northwind";
            var db = new DataContext(conn, mappingSource) { Log = Console.Out };
            var orders = db.GetTable<MyOrder>();
            var employees = db.GetTable<MyEmployee>();
            var q = (from o in orders
                     join e in employees
                         on o["EmployeeID"] equals
                         e["EmployeeID"] into emps
                     from e in emps
                     select new
                     {
                         OrderID = o["OrderID"],
                         FirstName = e["FirstName"]
                     }).ToList();
            Assert.IsTrue(q.Count > 0);


        }


        #endregion
    }
}
