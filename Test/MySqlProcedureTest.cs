using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ALinq;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.DataAccess.Types;

namespace Test
{
    [TestClass]
    public class MySqlProcedureTest
    {
        [TestMethod]
        public void CreateDatabase()
        {
            var db = new ProcedureDatabase();
            if (db.DatabaseExists())
                db.DeleteDatabase();
            db.CreateDatabase();
        }

        [TestMethod]
        public void CreateProcedure1()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP PROCEDURE IF EXISTS ADD_CATEGORY;");
            sb.AppendLine("CREATE PROCEDURE ADD_CATEGORY(IN CATEGORY_NAME VARCHAR(30), IN CATEGORY_DESCRIPTION TEXT)");
            sb.AppendLine("BEGIN");
            sb.AppendLine("INSERT INTO CATEGORIES (CategoryName, Description)");
            sb.AppendLine("Values (category_name, category_description);");
            sb.AppendLine("END;");
            sb.AppendLine();

            var db = new ProcedureDatabase();
            db.ExecuteCommand(sb.ToString());
        }

        [TestMethod]
        public void CreateProcedure2()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP PROCEDURE IF EXISTS GET_CATEGORIES_COUNT;");
            sb.AppendLine("CREATE PROCEDURE GET_CATEGORIES_COUNT(OUT RETURN_VALUE INT)");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT COUNT(*) INTO RETURN_VALUE");
            sb.AppendLine("FROM CATEGORIES;");
            sb.AppendLine("END;");
            sb.AppendLine();

            new ProcedureDatabase().ExecuteCommand(sb.ToString());
        }

        [TestMethod]
        public void CreateProcedure3()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP PROCEDURE IF EXISTS GET_CATEGORY_NAME;");
            sb.AppendLine("CREATE PROCEDURE GET_CATEGORY_NAME(CATEGORY_ID INT, OUT RETURN_VALUE VARCHAR(30))");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT CATEGORYNAME INTO RETURN_VALUE");
            sb.AppendLine("FROM CATEGORIES");
            sb.AppendLine("WHERE CATEGORYID = CATEGORY_ID;");
            sb.AppendLine("END;");
            sb.AppendLine();

            new ProcedureDatabase().ExecuteCommand(sb.ToString());
        }

        [TestMethod]
        public void CreateProcedure4()
        {
            var sb = new StringBuilder();

            sb.AppendLine("DROP PROCEDURE IF EXISTS GET_ALL_CATEGORIES;");
            sb.AppendLine("CREATE PROCEDURE GET_ALL_CATEGORIES()");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT CATEGORYID, CATEGORYNAME, DESCRIPTION");
            sb.AppendLine("FROM CATEGORIES;");
            sb.AppendLine("END;");

            new ProcedureDatabase().ExecuteCommand(sb.ToString());
        }

        [TestMethod]
        public void CreateProcedure5()
        {
            var sb = new StringBuilder();

            sb.AppendLine("DROP PROCEDURE IF EXISTS GET_CATEGORIES_AND_PRODUCTS;");
            sb.AppendLine("CREATE PROCEDURE GET_CATEGORIES_AND_PRODUCTS()");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT CATEGORYID, CATEGORYNAME, DESCRIPTION");
            sb.AppendLine("FROM CATEGORIES;");
            sb.AppendLine("SELECT ProductID, ProductNAME");
            sb.AppendLine("FROM PRODUCTS;");
            sb.AppendLine("END;");

            new ProcedureDatabase().ExecuteCommand(sb.ToString());
        }

        [TestMethod]
        public void CreateProcedure6()
        {
            var sb = new StringBuilder();

            sb.AppendLine("DROP PROCEDURE IF EXISTS GET_CATEGORY_PICTURE;");
            sb.AppendLine("CREATE PROCEDURE GET_CATEGORY_PICTURE(IN CATEGORY_ID INT, OUT CATEGORY_PICTURE BLOB)");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT PICTURE INTO CATEGORY_PICTURE");
            sb.AppendLine("FROM CATEGORIES");
            sb.AppendLine("WHERE CATEGORYID = CATEGORY_ID;");
            sb.AppendLine("END;");

            new ProcedureDatabase().ExecuteCommand(sb.ToString());
        }

        [TestMethod]
        public void CreateFunction1()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP FUNCTION IF EXISTS GET_CATEGORY_NAME;");
            sb.AppendLine("CREATE FUNCTION GET_CATEGORY_NAME( category_id int ) RETURNS varchar(20)");
            sb.AppendLine("BEGIN");
            sb.AppendLine("DECLARE category_name varchar(30);");
            sb.AppendLine("Select CategoryName into category_name");
            sb.AppendLine("from categories");
            sb.AppendLine("where CategoryID = category_id;");
            sb.AppendLine("RETURN category_name;");
            sb.AppendLine("END");
        }



        [TestMethod]
        public void AddCategory()
        {
            var db = new ProcedureDatabase();
            var id = db.Categories.Max(o => o.CategoryID) + 1;
            db.AddCategory("Name", "Descrition");
        }

        [TestMethod]
        public void AddProduct()
        {
            var db = new ProcedureDatabase();
            var id = db.Products.Max(o => o.ProductID) + 1;
            db.Products.Insert(new Product
            {
                ProductID = id,
                ProductName = "Name"
            });
        }

        [TestMethod]
        public void GetCategoriesCount()
        {
            var db = new ProcedureDatabase();
            int count;
            db.GetCategoriesCount(out count);
            Assert.AreEqual(db.Categories.Count(), count);
        }

        [TestMethod]
        public void GetPictures()
        {
            var db = new ProcedureDatabase();
            var ids = db.Categories.Select(o => o.CategoryID).ToArray();
            //ids.Select(o => db.GetCategoryPicture(o, out pic)).ToArray();
            foreach (var id in ids)
            {
                byte[] pic;
                db.GetCategoryPicture(id, out pic);
            }
        }

        [TestMethod]
        public void GetCategoryName1()
        {
            var db = new ProcedureDatabase();
            var id = db.Categories.Max(o => o.CategoryID);
            string name;
            db.GetCategoryName(id, out name);
            Assert.AreEqual(db.Categories.Where(o => o.CategoryID == id).Select(o => o.CategoryName).Single(), name);
        }

        [TestMethod]
        public void GetCategoryName2()
        {
            var db = new ProcedureDatabase();
            var id = db.Categories.Max(o => o.CategoryID);
            var name = db.Categories.Where(o => o.CategoryID == id).Select(o => GetCategoryName2(o.CategoryID)).Single();
            Assert.AreEqual(db.Categories.Where(o => o.CategoryID == id).Select(o => o.CategoryName).Single(), name);
        }

        [Function(Name = "get_category_name_fun")]
        public static string GetCategoryName2(int id)
        {
            throw new NotSupportedException();
        }

        [TestMethod]
        public void GetAllCategores()
        {
            var db = new ProcedureDatabase();
            var items = db.GetCategories().ToArray();
            Assert.AreEqual(db.Categories.Count(), items.Count());
        }

        [TestMethod]
        public void GetCategoresAndProducts()
        {
            var db = new ProcedureDatabase();
            var result = db.GetCategoriesAndProducts();
            var categories = result.GetResult<Category>().ToArray();
            var products = result.GetResult<Product>().ToArray();

            foreach (var category in categories)
            {
                Console.WriteLine("{0} {1} {2}", category.CategoryID, category.CategoryName, category.Description);
            }

            Assert.AreEqual(db.Categories.Count(), categories.Count());
            Assert.AreEqual(db.Products.Count(), products.Count());
        }

        [Provider(typeof(ALinq.MySQL.MySqlProvider))]
        public class ProcedureDatabase : DataContext
        {
            public ProcedureDatabase()
                : base("server=vpc1;user id=root;password=test;database=proceduredatabase1;")
            {
                Log = Console.Out;
            }

            public Table<Category> Categories
            {
                get { return GetTable<Category>(); }
            }

            public Table<Product> Products
            {
                get { return GetTable<Product>(); }
            }

            [Function(Name = "add_category")]
            public void AddCategory(
                [Parameter(Name = "CATEGORY_NAME")]
                string name,
                [Parameter(Name = "CATEGORY_DESCRIPTION")]
                string description)
            {
                ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), name, description);
            }

            [Function(Name = "GET_CATEGORIES_COUNT")]
            public void GetCategoriesCount(
                [Parameter(Name = "RETURN_VALUE")]
                out int count)
            {
                count = 0;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), count);
                count = (int)result.GetParameterValue(0);
            }

            [Function(Name = "GET_CATEGORY_NAME")]
            public void GetCategoryName(
                [Parameter(Name = "CATEGORY_ID")]
                int id,
                [Parameter(Name = "RETURN_VALUE")]
                out string name)
            {
                name = null;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id, name);
                name = (string)result.GetParameterValue(1);
            }

            [Function(Name = "GET_ALL_CATEGORIES")]
            public ISingleResult<Category> GetCategories()
            {
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
                return (ISingleResult<Category>)result.ReturnValue;
            }

            [Function(Name = "GET_CATEGORIES_AND_PRODUCTS")]
            [ResultType(typeof(Category))]
            [ResultType(typeof(Product))]
            public IMultipleResults GetCategoriesAndProducts()
            {
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
                return (IMultipleResults)result.ReturnValue;
            }

            [Function(Name = "GET_CATEGORY_PICTURE")]
            public void GetCategoryPicture(
                [Parameter(Name = "CATEGORY_ID")] int categoryID,
                [Parameter(Name = "CATEGORY_PICTURE")] out Byte[] value)
            {
                value = null;
                ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), categoryID, value);
            }
        }

        [Table(Name = "CATEGORIES")]
        public class Category
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
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

            [Column(DbType = "Text")]
            public string Description
            {
                get;
                set;
            }

            [Column(DbType = "Binary")]
            public ALinq.Binary Picture
            {
                get;
                set;
            }
        }

        [Table(Name = "PRODUCTS")]
        public class Product
        {
            [Column(IsPrimaryKey = true)]
            public int ProductID { get; set; }

            [Column]
            public string ProductName { get; set; }
        }
    }


}
