## 快速上手

本示例代码可以[**点击这里**](https://github.com/ansiboy/alinq-quick-start)下载。

或者使用 git 命令下载

```
git clone https://github.com/ansiboy/alinq-quick-start
```

该项目已经为你建好了示例里的实体类，以及数据库，为了方便使用的是 SQLite 数据库。

注意：本演练使用的开发平台是 64位，数据库是 SQLite 。如果你的开发平台是 32位，需要引用 32位版本的 System.Data.SQLite.dll 。

**使用 ALinq 的常见步骤**

1. **创建项目**

    创建控制台项目。

1. **程序集的引用**

    使用不同的数据库，需要引用不同的程序集。对于 Access 数据库，需要引用 ALinq.dll 和 ALinq.Access.dll ，对于 SQLite 数据库，需要引用 ALinq.SQLite.dll ，其他数据库依次类推。值得注意的是 Oracle 数据库，如果使用 .NET 自带的 Oracle 数据库驱动，需要引用 ALinq.Oracle.dll ，如果使用 Oracle 提供的数据库驱动，需要引用 ALinq.OdpOracle 。

    另外，还要引用数据库的 ADO.NET 驱动。

    本示例中，使用的是 SQLite 数据库。因此引用了 ALinq.dll，ALinq.SQLite.dll, System.Data.SQLite.dll。

1. **命名空间引用**
    
    一般来说，需要引用下面三个命名空间

    ```cs
    using System.Linq;
    using ALinq;
    using ALinq.Mapping;
    ```

1. **创建实体类**

    ```cs
    [Table(Name = "Categories")]
    public class Category
    {
        [Column(Name = "CategoryID", IsPrimaryKey = true)]
        public int CategoryID { get; set; }

        [Column(Name = "CategoryName")]
        public string CategoryName { get; set; }

        [Column(Name = "Description")]
        public string Description { get; set; }

        [Column(Name = "Picture")]
        public Binary Picture { get; set; }
    }
    ```
    
1. **创建强类型数据上下文**
    ```cs
    [Provider(typeof(ALinq.SQLite.SQLiteProvider))]
    public class Northwind : DataContext
    {
        static string dbPath = "Northwind.db3";

        public Northwind() : base(dbPath)
        {
        }

        public Table<Category> Categories
        {
            get
            {
                return this.GetTable<Category>();
            }
        }
    }
    ```

1. **查询数据**

    ```cs
    var dc = new Northwind();
    var categories = dc.Categories.Where(c => c.CategoryID > 1);
    foreach (var c in categories)
    {
        Console.WriteLine("{0} {1} {2}", c.CategoryID, c.CategoryName, c.Description);
    }
    ```

1. **数据的更新**

    ```cs
    var dc = new Northwind();
    var category = dc.Categories.Single(o => o.CategoryID == 2);
    category.CategoryName = "Computer";
    dc.SubmitChanges();
    ```

1. **数据的删除**

    ```cs
    var dc = new Northwind();
    var category = dc.Categories.Single(o => o.CategoryID == 2);
    dc.Categories.DeleteOnSubmit(category);
    dc.SubmitChanges();
    ```
