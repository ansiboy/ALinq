using System;
using System.Linq;
using ALinq;
using ALinq.Access;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;
using System.Collections.Generic;

namespace Test
{
    [TestClass]
    public class FluentMappingTest
    {
        public class EmployeeMapping : EntityMapping<Employee>
        {
            public EmployeeMapping()
            {
                this.TableAttribute = new TableAttribute { Name = "Employees" };
                this.Column(o => o.EmployeeID, new ColumnAttribute { UpdateCheck = UpdateCheck.Never });
            }
        }

        class MyEmployee
        {
            public int EmployeeID { get; set; }
            public string FirstName { get; set; }
            public string Address { get; set; }

            private Dictionary<string, object> values = new Dictionary<string, object>();
            //[Column]
            public object this[string name]
            {
                get
                {
                    object item;
                    if (values.TryGetValue(name, out item))
                        return item;

                    return null;
                }
                set
                {
                    values[name] = value;
                }
            }
        }

        public class NorthwindMapping : DataContextMapping<NorthwindDatabase>
        {
            public NorthwindMapping()
            {
                this.Provider = new ProviderAttribute(typeof(AccessDbProvider));
                this.Database = new DatabaseAttribute();

                this.Map<Category>()
                    .Table(new TableAttribute { Name = "Categories" })
                    .Column(o => o.CategoryID, new ColumnAttribute { UpdateCheck = UpdateCheck.Never })
                    .Column(o => o.CategoryName, new ColumnAttribute { UpdateCheck = UpdateCheck.Never })
                    .Column(o => o.Description, new ColumnAttribute { UpdateCheck = UpdateCheck.Never })
                    .Association(o => o.Products, new AssociationAttribute { OtherKey = "CategoryID" });

                this.Map<Product>()
                    .Table(new TableAttribute { Name = "Products" })
                    .Column(o => o.ProductID)
                    .Column(o => o.ProductName)
                    .Association(o => o.Category, new AssociationAttribute { OtherKey = "CategoryID" });

                this.Map<Contact>()
                    .Table("Contacts")
                    .Inheritance("Unknown", typeof(Contact), true)
                    .Inheritance("Full", typeof(FullContact))
                    .Inheritance(new InheritanceMappingAttribute { Code = "EmployeeContact", Type = typeof(EmployeeContact) })
                    .Inheritance(new InheritanceMappingAttribute { Code = "Supplier", Type = typeof(SupplierContact) })
                    .Inheritance(new InheritanceMappingAttribute { Code = "Customer", Type = typeof(CustomerContact) })
                    .Inheritance(new InheritanceMappingAttribute { Code = "Shipper", Type = typeof(ShipperContact) })
                    .Column(o => o.ContactID, new ColumnAttribute { Storage = "_ContactID", AutoSync = AutoSync.OnInsert, CanBeNull = false, IsPrimaryKey = true, IsDbGenerated = true, UpdateCheck = UpdateCheck.Never })
                    .Column(o => o.ContactType, new ColumnAttribute { Storage = "_ContactType", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never, IsDiscriminator = true })
                    .Column(o => o.CompanyName, new ColumnAttribute { Storage = "_CompanyName", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never })
                    .Column(o => o.Phone, new ColumnAttribute { Storage = "_Phone", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never })
                    .Column(o => o.GUID, new ColumnAttribute { Storage = "_Phone", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never });

                this.Map<FullContact>()
                    .Column(o => o.ContactName, o => o.NeverUpdateCheck())
                    .Column(o => o.ContactTitle, o => o.NeverUpdateCheck())
                    .Column(o => o.Address, o => o.NeverUpdateCheck())
                    .Column(o => o.City, o => o.NeverUpdateCheck())
                    .Column(o => o.Region, o => o.NeverUpdateCheck())
                    .Column(o => o.PostalCode, o => o.NeverUpdateCheck())
                    .Column(o => o.Country, o => o.NeverUpdateCheck())
                    .Column(o => o.Fax, o => o.NeverUpdateCheck());

                this.Map<MyEmployee>()
                    .Table("Employees")
                    .Column(o => o.FirstName)
                    .Column(o => o.EmployeeID)
                    .Column(o => o.Address);

                this.Function(o => o.Max(0), new FunctionAttribute { IsComposable = true });
                this.Function(o => o.FindUser(null));
                this.Function(o => o.Now(), true);

                var employeeMapping = this.GetEntityMapping<MyEmployee>();
                employeeMapping.Column(o => o["Region"]);

            }
        }

        [TestMethod]
        public void FunctionMappingTest()
        {
            var mappingSource = new FluentMappingSource(o => new NorthwindMapping());
            var db = new DataContext("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            var item = db.GetTable<MyEmployee>().Select(o => new { F = o["FirstName"] }).First();
            //Assert.IsTrue(item.L == null);
        }

        [TestMethod]
        public void NorthwindMappingTest()
        {
            var mappingSource = new FluentMappingSource(o => new NorthwindMapping());
            var db = new AccessNorthwind("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            db.Contacts.OfType<FullContact>().ToArray();
        }

        [TestMethod]
        public void Test2()
        {
            var mappingSource = new FluentMappingSource(o => new NorthwindMapping());
            var db = new AccessNorthwind("C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            db.Employees.ToArray();
        }

        [TestMethod]
        public void Test1()
        {
            Exception myexc = null;
            try
            {
                var mapping = new DataContextMapping<AccessNorthwind>();
                mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

                var categoryMapping = new EntityMapping<Category>();
                categoryMapping.Column(o => o.CategoryID);
                categoryMapping.Column(o => o.CategoryName);
                categoryMapping.Column(o => o.Products);
                mapping.Add(categoryMapping);

                var mappingSource = new FluentMappingSource(o => mapping);
                //mappingSource.Add(mapping);

                var db = new AccessNorthwind(@"C:/Northwind.mdb", mappingSource);
                db.Categories.Select(o => new { o.CategoryID, o.CategoryName, Products = o.Products.ToArray() }).ToArray();
            }
            catch (Exception exc)
            {
                myexc = exc;
            }
            Assert.IsNotNull(myexc);
            Console.WriteLine(myexc.Message);
        }

        [TestMethod]
        public void Test3()
        {
            var mappingSource = new FluentMappingSource(delegate
            {
                var contextMapping = new DataContextMapping<DataContext>();
                contextMapping.Provider = new ProviderAttribute((typeof(AccessDbProvider)));

                var employeeMapping = new EntityMapping<MyEmployee>();
                employeeMapping.Table("Employees")
                               .Column(o => (string)o["City"])
                               .Column(o => (string)o["FirstName"])
                               .Column(o => (string)o["LastName"]);

                contextMapping.Add(employeeMapping);
                return contextMapping;
            });
            var dc = new DataContext("C:/Northwind.mdb", mappingSource);
            var metaTable = dc.Mapping.GetTable(typeof(MyEmployee));
            Assert.IsNotNull(metaTable);
        }

        [TestMethod]
        public void ColumnMapping()
        {
            var mapping = new DataContextMapping<AccessNorthwind>();
            mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

            var categoryMapping = new EntityMapping<Category>();
            categoryMapping.TableAttribute = new TableAttribute { Name = "Categories" };
            categoryMapping.Column(o => o.CategoryID);
            categoryMapping.Column(o => o.CategoryName);
            mapping.Add(categoryMapping);

            var mappingSource = new FluentMappingSource(o => mapping);
            //mappingSource.Add(mapping);

            var db = new AccessNorthwind(@"C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            db.Categories.Select(o => new { o.CategoryID, o.CategoryName }).ToArray();

        }

        [TestMethod]
        public void AssociationMapping()
        {
            var mapping = new DataContextMapping<AccessNorthwind>();
            mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

            var categoryMapping = new EntityMapping<Category>();
            categoryMapping.TableAttribute = new TableAttribute { Name = "Categories" };
            categoryMapping.Column(o => o.CategoryID, new ColumnAttribute { IsPrimaryKey = true });
            categoryMapping.Column(o => o.CategoryName);
            categoryMapping.Association(o => o.Products, new AssociationAttribute { OtherKey = "CategoryID" });
            mapping.Add(categoryMapping);

            var productMapping = new EntityMapping<Product>();
            productMapping.TableAttribute = new TableAttribute { Name = "Products" };
            productMapping.Column(o => o.ProductID, o => o.PrimaryKey());
            productMapping.Column(o => o.ProductName, o => o.NeverUpdateCheck());
            productMapping.Column(o => o.CategoryID, o => o.NeverUpdateCheck());
            productMapping.Association(o => o.Category, o => o.Keys("CategoryID", "CategoryID").Storage("_Category"));
            mapping.Add(productMapping);

            var mappingSource = new FluentMappingSource(o => mapping);
            //mappingSource.Add(mapping);

            var db = new AccessNorthwind(@"C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            db.Categories.Select(o => new { o.CategoryID, o.CategoryName, o.Products }).ToArray();

            db.Categories.ToArray();
            db.Products.ToArray();

            var products = db.Products.ToList();
            products.ForEach(o => Console.WriteLine(o.Category));
        }

        [TestMethod]
        public void AssociationMapping1()
        {
            var mapping = new DataContextMapping<AccessNorthwind>();
            mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

            var categoryMapping = new EntityMapping<Category>();
            categoryMapping.TableAttribute = new TableAttribute { Name = "Categories" };
            categoryMapping.Column(o => o.CategoryID, o => o.PrimaryKey());
            categoryMapping.Column(o => o.CategoryName, o => o.NeverUpdateCheck());
            categoryMapping.Association(o => o.Products, o => o.Keys("CategoryID", "CategoryID"));
            mapping.Add(categoryMapping);

            var productMapping = new EntityMapping<Product>();
            productMapping.TableAttribute = new TableAttribute { Name = "Products" };
            productMapping.Column(o => o.ProductID, o => o.PrimaryKey());
            productMapping.Column(o => o.ProductName);
            productMapping.Column(o => o.CategoryID);
            productMapping.Association(o => o.Category, o => o.Storage("_Category").Keys("CategoryID", "CategoryID"));
            mapping.Add(productMapping);

            var mappingSource = new FluentMappingSource(o => mapping);
            //mappingSource.Add(mapping);

            var db = new AccessNorthwind(@"C:/Northwind.mdb", mappingSource) { Log = Console.Out };
            db.Categories.Select(o => new { o.CategoryID, o.CategoryName, o.Products }).ToArray();

            db.Categories.ToArray();
            db.Products.ToArray();

            var products = db.Products.ToList();
            products.ForEach(o => Console.WriteLine(o.Category));
        }

        [TestMethod]
        public void GetColumn()
        {
            var mapping = new DataContextMapping<AccessNorthwind>();
            mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

            var categoryMapping = new EntityMapping<Category>();
            categoryMapping.TableAttribute = new TableAttribute { Name = "Categories" };
            categoryMapping.Column(o => o.CategoryID, o => o.PrimaryKey());
            categoryMapping.Column(o => o.CategoryName, o => o.NeverUpdateCheck());
            categoryMapping.Association(o => o.Products, o => o.Keys("CategoryID", "CategoryID"));
            mapping.Add(categoryMapping);

            var mappingSource = new FluentMappingSource(o => mapping);
            //mappingSource.Add(mapping);

            var column = categoryMapping.GetColumn(o => o.CategoryID);
            Assert.IsTrue(column.IsPrimaryKey);


        }

        [TestMethod]
        public void GetDataContextMapping()
        {
            var mapping = new DataContextMapping<AccessNorthwind>();
            mapping.Provider = new ProviderAttribute(typeof(AccessDbProvider));

            var categoryMapping = new EntityMapping<Category>();
            categoryMapping.TableAttribute = new TableAttribute { Name = "Categories" };
            categoryMapping.Column(o => o.CategoryID, o => o.PrimaryKey());
            categoryMapping.Column(o => o.CategoryName, o => o.NeverUpdateCheck());
            categoryMapping.Association(o => o.Products, o => o.Keys("CategoryID", "CategoryID"));
            mapping.Add(categoryMapping);

            var mappingSource = new FluentMappingSource(o => mapping);
            //mappingSource.Add(mapping);

            //mapping = mappingSource.GetDataContextMapping<AccessNorthwind>();
            //Assert.IsNotNull(mapping);

            //var mapping1 = mappingSource.GetDataContextMapping<DataContext>();
            //Assert.IsNull(mapping1);
        }


        class T
        {
            [Column(IsPrimaryKey = true)]
            public int ID { get; set; }
        }
    }
}
