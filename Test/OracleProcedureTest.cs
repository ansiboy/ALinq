using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ALinq;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.DataAccess.Types;
using Test.Properties;

namespace Test
{
    [TestClass]
    public class OdpOracleProcedureTest
    {

        [TestMethod]
        public void CreateDatabase()
        {
            ProcedureDatabase db;
            var str = "DATA SOURCE=vpc1;PERSIST SECURITY INFO=True;USER ID=SYSTEM;PASSWORD=TEST";
            db = CreateDatabaseInstance();
            db.Log = Console.Out;
            db.Connection.ConnectionString = str;
            if (db.DatabaseExists())
                db.DeleteDatabase();

            db.CreateDatabase();

            db = CreateDatabaseInstance();

            #region category instances
            //   bitmap.Save(stream, ImageFormat.Bmp);
            var fun = new Func<Bitmap, ALinq.Binary>(delegate(Bitmap bitmap)
                                                        {
                                                            using (var stream = new MemoryStream())
                                                            {
                                                                bitmap.Save(stream, ImageFormat.Bmp);
                                                                return stream.GetBuffer();
                                                            }
                                                        });
            var categories = new List<Category>
                                 {
                                     new Category
                                         {
                                             CategoryName = "Beverages",
                                             Description = "Soft drinks, coffees, teas, beers, and ales",
                                             Picture = fun(Resources.Picture1)
                                         },
                                     new Category
                                         {
                                             CategoryName = "Condiments",
                                             Description = "Sweet and savory sauces, relishes, spreads, and seasonings",
                                             Picture = fun(Resources.Picture2)
                                         },
                                     new Category
                                         {
                                             CategoryName = "Confections",
                                             Description = "Desserts, candies, and sweet breads",
                                             Picture = fun(Resources.Picture3)
                                         },
                                     new Category
                                         {
                                             CategoryName = "Dairy Products",
                                             Description = "Cheeses",
                                             Picture = fun(Resources.Picture4)
                                         },
                                     new Category
                                         {
                                             CategoryName = "Grains/Cereals",
                                             Description = "Breads, crackers, pasta, and cereal",
                                             Picture = fun(Resources.Picture5)
                                         },
                                     new Category
                                         {
                                             CategoryName = "Meat/Poultry",
                                             Description = "Prepared meats",
                                             Picture = fun(Resources.Picture6)
                                         },
                                     new Category
                                         {
                                             CategoryName = "Produce",
                                             Description = "Dried fruit and bean curd",
                                             Picture = fun(Resources.Picture7)
                                         },
                                     new Category
                                         {
                                             CategoryName = "Seafood",
                                             Description = "Seaweed and fish",
                                             Picture = fun(Resources.Picture8)
                                         },
                                     };
            #endregion

            db.Categories.InsertAllOnSubmit(categories);
            db.SubmitChanges();

            CreateProcedure1();
            CreateProcedure2();
            CreateProcedure3();
            CreateProcedure4();
            CreateProcedure5();
            CreateProcedure6();
            CreateProcedure7();
            CreateProcedure8();
        }

        [TestMethod]
        public void CreateProcedure8()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE PACKAGE PKG3 IS");
            sb.AppendLine("TYPE MYTYPE1 IS REF CURSOR;");
            sb.AppendLine("TYPE MYTYPE2 IS REF CURSOR;");
            sb.AppendLine("PROCEDURE GET_CATEGORIES_AND_PRODUCTS(MYCS1 OUT MYTYPE1, MYCS2 OUT MYTYPE2);");
            sb.AppendLine("END PKG3;");

            CreateDatabaseInstance().ExecuteCommand(sb.ToString());

            sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE PACKAGE BODY PKG3 IS");
            sb.AppendLine("PROCEDURE GET_CATEGORIES_AND_PRODUCTS(MYCS1 OUT MYTYPE1, MYCS2 OUT MYTYPE2) IS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("OPEN MYCS1 FOR SELECT CATEGORYID, CATEGORYNAME, DESCRIPTION");
            sb.AppendLine("FROM CATEGORIES;");
            sb.AppendLine("OPEN MYCS2 FOR SELECT PRODUCTID, PRODUCTNAME");
            sb.AppendLine("FROM PRODUCTS;");
            sb.AppendLine("END GET_CATEGORIES_AND_PRODUCTS;");
            sb.AppendLine("END PKG3;");

            CreateDatabaseInstance().ExecuteCommand(sb.ToString());
        }

        //[TestMethod]
        public void CreateProcedure7()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE PACKAGE PKG2 IS ");
            sb.AppendLine("TYPE MYTYPE IS REF CURSOR; ");
            sb.AppendLine("FUNCTION GET_ALL_CATEGORIES RETURN MYTYPE; ");
            sb.AppendLine("END PKG2;");

            CreateDatabaseInstance().ExecuteCommand(sb.ToString());

            sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE PACKAGE BODY PKG2 IS ");
            sb.AppendLine("FUNCTION GET_ALL_CATEGORIES RETURN MYTYPE IS ");
            sb.AppendLine("MYCS MYTYPE; ");
            sb.AppendLine("BEGIN ");
            sb.AppendLine("OPEN MYCS FOR SELECT CATEGORYID, CATEGORYNAME, DESCRIPTION ");
            sb.AppendLine("FROM CATEGORIES; ");
            sb.AppendLine("RETURN MYCS; ");
            sb.AppendLine("END GET_ALL_CATEGORIES; ");
            sb.AppendLine("END PKG2; ");

            CreateDatabaseInstance().ExecuteCommand(sb.ToString());

        }

        //[TestMethod]
        public void CreateProcedure6()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE FUNCTION GET_CATEGORY_PIC(CATEGORY_ID IN NUMBER) RETURN BLOB AUTHID CURRENT_USER IS");
            sb.AppendLine("varPic BLOB;");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT PICTURE INTO varPic");
            sb.AppendLine("FROM CATEGORIES");
            sb.AppendLine("WHERE CATEGORYID = CATEGORY_ID;");
            sb.AppendLine("RETURN varPic;");
            sb.AppendLine("END;");

            CreateDatabaseInstance().ExecuteCommand(sb.ToString());
        }

        public void CreateProcedure5()
        {
            var sb = new StringBuilder();
            var db = CreateDatabaseInstance();

            sb.AppendLine("CREATE OR REPLACE PACKAGE PKG2 IS");
            sb.AppendLine("TYPE MYTYPE IS REF CURSOR;");
            sb.AppendLine("FUNCTION GET_ALL_CATEGORIES RETURN MYTYPE;");
            sb.AppendLine("END PKG2;");

            db.ExecuteCommand(sb.ToString());

            sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE PACKAGE BODY PKG2 IS");
            sb.AppendLine("FUNCTION GET_ALL_CATEGORIES RETURN MYTYPE IS");
            sb.AppendLine("MYCS MYTYPE;");
            sb.AppendLine("BEGIN");
            sb.AppendLine("OPEN MYCS FOR SELECT CATEGORYID, CATEGORYNAME, DESCRIPTION");
            sb.AppendLine("FROM CATEGORIES;");
            sb.AppendLine("RETURN MYCS;");
            sb.AppendLine("END GET_ALL_CATEGORIES;");
            sb.AppendLine("END PKG2;");

            db.ExecuteCommand(sb.ToString());
        }

        public void CreateProcedure4()
        {
            var sb = new StringBuilder();
            var db = CreateDatabaseInstance();

            sb.AppendLine("CREATE OR REPLACE PACKAGE PKG1 IS");
            sb.AppendLine("TYPE MYTYPE IS REF CURSOR;");
            sb.AppendLine("PROCEDURE GET_ALL_CATEGORIES(MYCS OUT MYTYPE);");
            sb.AppendLine("END PKG1;");
            db.ExecuteCommand(sb.ToString());

            sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE PACKAGE BODY PKG1 IS");
            sb.AppendLine("PROCEDURE GET_ALL_CATEGORIES(MYCS OUT MYTYPE) IS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("OPEN MYCS FOR SELECT CATEGORYID, CATEGORYNAME, DESCRIPTION");
            sb.AppendLine("FROM CATEGORIES;");
            sb.AppendLine("END GET_ALL_CATEGORIES;");
            sb.AppendLine("END PKG1;");

            db.ExecuteCommand(sb.ToString());
        }

        public void CreateProcedure3()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE PROCEDURE GET_CATEGORY_NAME(CATEGORY_ID INT, RETURN_VALUE OUT VARCHAR)");
            sb.AppendLine("IS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT CATEGORYNAME INTO RETURN_VALUE");
            sb.AppendLine("FROM CATEGORIES");
            sb.AppendLine("WHERE CATEGORYID = CATEGORY_ID;");
            sb.AppendLine("END GET_CATEGORY_NAME;");
            sb.AppendLine();

            CreateDatabaseInstance().ExecuteCommand(sb.ToString());
        }

        public void CreateProcedure2()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE PROCEDURE GET_CATEGORIES_COUNT(RETURN_VALUE OUT INT)");
            sb.AppendLine("IS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT COUNT(*) INTO RETURN_VALUE");
            sb.AppendLine("FROM CATEGORIES;");
            sb.AppendLine("END GET_CATEGORIES_COUNT;");
            sb.AppendLine();

            CreateDatabaseInstance().ExecuteCommand(sb.ToString());
        }

        public void CreateProcedure1()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE OR REPLACE PROCEDURE ADD_CATEGORY(CATEGORY_ID IN INT, CATEGORY_NAME IN VARCHAR, CATEGORY_DESCRIPTION IN VARCHAR)");
            sb.AppendLine("IS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("INSERT INTO CATEGORIES (CategoryID,CategoryName, Description)");
            sb.AppendLine("Values (category_id, category_name, category_description);");
            sb.AppendLine("END ADD_CATEGORY;");
            sb.AppendLine();

            var db = CreateDatabaseInstance();
            db.ExecuteCommand(sb.ToString());
        }

        protected virtual ProcedureDatabase CreateDatabaseInstance()
        {
            return new ProcedureDatabase(typeof(ALinq.Oracle.Odp.OracleProvider));
        }

        [TestMethod]
        public void AddCategory()
        {
            var db = CreateDatabaseInstance();
            var id = db.Categories.Max(o => o.CategoryID) + 1;
            db.AddCategory(id, "Name" + id, "Description" + id);
        }

        //[TestMethod]
        public void AddProduct()
        {
            var db = CreateDatabaseInstance();
            for (var i = 0; i < 10; i++)
            {
                db.Products.InsertOnSubmit(new Product
                                               {
                                                   ProductID = i,
                                                   ProductName = "Product" + i
                                               });
            }
        }

        [TestMethod]
        public void GetCategoryName()
        {
            var db = new ProcedureDatabase(typeof(ALinq.Oracle.Odp.OracleProvider));
            var id = db.Categories.Max(o => o.CategoryID);
            string name;
            db.GetCategoryName(id, out name);
            Assert.AreEqual(db.Categories.Where(o => o.CategoryID == id).Single().CategoryName, name);
        }

        [TestMethod]
        public void GetCategoryPicture1()
        {
            var db = new ProcedureDatabase(typeof(ALinq.Oracle.Odp.OracleProvider));
            var id = 2;

            db.Connection.Open();
            var pic = db.GetCategoryPicture(id);
            db.Connection.Close();
            Assert.IsNotNull(pic);
        }

        [TestMethod]
        public void GetCategoryPicture2()
        {
            var db = new ProcedureDatabase(typeof(ALinq.Oracle.Odp.OracleProvider));

            db.Connection.Open();
            var pics = db.Categories.Select(o => GetCategoryPicture(o.CategoryID)).ToArray();
            db.Connection.Close();
        }

        [TestMethod]
        public void GetCategoryPicture3()
        {
            var db = new ProcedureDatabase(typeof(ALinq.Oracle.Odp.OracleProvider));
            //var id = 2;
            db.Connection.Open();
            //var pic = db.DUAL.Select(o => GetCategoryPicture(id)).Single();
            db.Connection.Close();
        }

        [TestMethod]
        public void GetCategoriesCount()
        {
            var db = new ProcedureDatabase(typeof(ALinq.Oracle.Odp.OracleProvider));
            int count;
            db.GetCategoriesCount(out count);
            Assert.AreEqual(db.Categories.Count(), count);
        }

        [Function(Name = "GET_CATEGORY_PIC")]
        static byte[] GetCategoryPicture(int id)
        {
            throw new NotSupportedException("Could not call direct.");
        }

        [TestMethod]
        public void Test()
        {
            ProcedureDatabase db;
            db = new ProcedureDatabase(typeof(ALinq.Oracle.Odp.OracleProvider));
            int count;
            db.GetCategoriesCount(out count);
            Assert.AreEqual(db.Categories.Count(), count);

            var id = db.Categories.Max(o => o.CategoryID);
            string name;
            db.GetCategoryName(id, out name);
            Assert.AreEqual(db.Categories.Where(o => o.CategoryID == id).Single().CategoryName, name);

            OracleRefCursor myrc;
            db.Connection.Open();
            db.GetAllCategories(out myrc);
            var reader = myrc.GetDataReader();
            IEnumerable<Category> categories = db.Translate<Category>(reader).ToArray();
            Assert.AreEqual(db.Categories.Count(), categories.Count());
            db.Connection.Close();

            categories = db.GetAllCategories();
            Assert.AreEqual(db.Categories.Count(), categories.Count());
        }


        [TestMethod]
        public void GetAllCategories1()
        {
            var db = CreateDatabaseInstance();
            OracleRefCursor myrc;
            db.Connection.Open();
            db.GetAllCategories(out myrc);
            var reader = myrc.GetDataReader();
            var items = db.Translate<Category>(reader);
            Assert.AreEqual(db.Categories.Count(), items.Count());
            db.Connection.Close();
        }

        [TestMethod]
        public void GetAllCategories2()
        {
            var db = CreateDatabaseInstance();
            var items = db.GetAllCategories();
            Assert.AreEqual(db.Categories.Count(), items.Count());
        }

        [TestMethod]
        public void GetCategoriesAndProducts()
        {
            var db = CreateDatabaseInstance();
            db.Connection.Open();
            OracleRefCursor cursor1;
            OracleRefCursor cursor2;
            db.GetCategoryAndProducts(out cursor1, out cursor2);
            var categories = db.Translate<Category>(cursor1.GetDataReader());
            var products = db.Translate<Product>(cursor2.GetDataReader());
            Assert.AreEqual(db.Categories.Count(), categories.Count());
            Assert.AreEqual(db.Products.Count(), products.Count());
            db.Connection.Close();
        }

        [Database(Name = "PROCEDUREDATABASE")]
        public class ProcedureDatabase : DataContext
        {
            public ProcedureDatabase(string fileOrServerOrConnection, Type providerType)
                : base(fileOrServerOrConnection, providerType)
            {
                Log = Console.Out;
            }

            public ProcedureDatabase(Type providerType)
                : this("DATA SOURCE=vpc1;PERSIST SECURITY INFO=True;USER ID=PROCEDUREDATABASE;PASSWORD=TEST", providerType)
            {
            }

            public Table<Category> Categories
            {
                get { return GetTable<Category>(); }
            }

            public Table<Product> Products
            {
                get { return GetTable<Product>(); }
            }

            //public Table<Dual> DUAL
            //{
            //    get { return GetTable<Dual>(); }
            //}

            [Function(Name = "ADD_CATEGORY")]
            public void AddCategory(
                [Parameter(Name = "CATEGORY_ID")]
                int id,
                [Parameter(Name = "CATEGORY_NAME")]
                string name,
                [Parameter(Name = "CATEGORY_DESCRIPTION")]
                string description)
            {
                ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id, name, description);
            }

            [Function(Name = "GET_CATEGORIES_COUNT")]
            public void GetCategoriesCount(out int count)
            {
                count = 0;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), count);
                count = (int)result.GetParameterValue(0);
            }

            [Function(Name = "GET_CATEGORY_NAME")]
            public void GetCategoryName(int id, out string name)
            {
                name = null;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id, name);
                name = (string)result.GetParameterValue(1);
            }

            [Function(Name = "PKG1.GET_ALL_CATEGORIES")]
            public virtual void GetAllCategories(out OracleRefCursor myrc)
            {
                myrc = null;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), myrc);
                myrc = (OracleRefCursor)result.GetParameterValue(0);
            }

            [Function(Name = "PKG3.GET_CATEGORIES_AND_PRODUCTS")]
            public void GetCategoryAndProducts(out OracleRefCursor cursor1, out OracleRefCursor cursor2)
            {
                cursor1 = null;
                cursor2 = null;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), cursor1, cursor2);
                cursor1 = (OracleRefCursor)result.GetParameterValue(0);
                cursor2 = (OracleRefCursor)result.GetParameterValue(1);
            }
            //[Function(Name = "PKG1.GET_ALL_CATEGORIES")]
            //public ISingleResult<Category> GetAllCategories1(out OracleRefCursor myrc)
            //{
            //    myrc = null;
            //    var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), myrc);
            //    return (ISingleResult<Category>)result.ReturnValue;
            //}

            [Function(Name = "PKG2.GET_ALL_CATEGORIES")]
            public ISingleResult<Category> GetAllCategories()
            {
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
                return (ISingleResult<Category>)result.ReturnValue;
            }

            [Function(Name = "GET_CATEGORY_PIC")]
            public byte[] GetCategoryPicture(int id)
            {
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id);
                return (byte[])result.ReturnValue;
            }

            //[Function(Name = "GET_CATEGORY_PIC")]
            //public ALinq.Binary GetCategoryPicture(int id)
            //{
            //    var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id);
            //    return (ALinq.Binary)result.ReturnValue;
            //}
        }

        [Table(Name = "CATEGORIES")]
        public class Category
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "NUMBER")]
            public int CategoryID
            {
                get;
                private set;
            }

            [Column(DbType = "VarChar(30)")]
            public string CategoryName
            {
                get;
                set;
            }

            [Column(DbType = "CLOB")]
            public string Description
            {
                get;
                set;
            }

            [Column(DbType = "BLOB")]
            public ALinq.Binary Picture
            {
                get;
                set;
            }
        }

        [Table(Name = "PRODUCTS")]
        public class Product
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "NUMBER")]
            public int ProductID
            {
                get;
                set;
            }

            [Column(DbType = "VarChar(30)")]
            public string ProductName
            {
                get;
                set;
            }
        }

        [Table]
        public class Dual
        {

        }

    }
}
