## Fluent 映射

使用 ALinq Fluent 可以通过编码建立实体类与数据库之间的映射

### 快速开始

我们首先通过一个例子来了解 Fluent 映射

1. 创建映射

    ``` cs
    class NorthwindMappingSource : FluentMappingSource
    {
        public NorthwindMappingSource()
        {
            Map<NorthwindDatabase>(mapping =>
            {
                mapping.ProviderType = typeof(AccessDbProvider);
                mapping.Table(o => o.Categories, CategoryTableName)
                        .PrimaryKey(o => o.CategoryID)
                        .Column(o => o.CategoryName)
                        .Column(o => o.Description)
                        .Column(o => o.Picture)
                        .Association(o => o.Products, o => o.CategoryID, o => o.CategoryID);

                mapping.Table(o => o.Products, ProdcutTableName)
                        .PrimaryKey(o => o.ProductID)
                        .Column(o => o.ProductName)
                        .Column(o => o.CategoryID)
                        .Association(o => o.Category, o => o.CategoryID, o => o.CategoryID);

            });
        }
    }
    ```

1. 使用映射

    ```cs
    var mappingSource = new NorthwindMappingSource();
    var dc = new NorthwindDataContext(mappingSource) { Log = Console.Out };
    var categories = db.Categories.ToArray();
    ```

### 编程指南
    
**实体类的映射**
```cs
var mappingSource = new FluentMappingSource(mapping =>
{
    mapping.Table(o => o.Products, "Products")
            .PrimaryKey(o => o.ProductID)
            .Column(o => o.ProductName);
});
```

**属性或字段的映射**

```cs
var mappingSource = new FluentMappingSource(mapping =>
{
    mapping.Table(o => o.Categories, "Categories")
            .Column(o => o.Products);
});
```

**注意**

* 使用 Table 函数创建实体映射，如果实体类的映射已经存在，则合并修改到原来的映射。

    **例如**

    ```cs
    var mappingSource = new FluentMappingSource();
    mappingSource.Map<NorthwindDatabase>(mapping =>
    {
        mapping.Table(o => o.Products, "Products")
            .PrimaryKey(o => o.ProductID)
            .Column(o => o.ProductName);

        mapping.Table(o => o.Products, "Products")
            .Column(o => o.UnitPrice);
    });
    ```

    等价于

    ```cs
    var mappingSource = new FluentMappingSource();
    mappingSource.Map<NorthwindDatabase>(mapping =>
    {
        mapping.Table(o => o.Products, "Products")
            .PrimaryKey(o => o.ProductID)
            .Column(o => o.ProductName)
            .Column(o => o.UnitPrice);
    });
    ```

* 使用 Column 函数创建属性或字段映射，如果属性或字段的映射已经存在，则合并修改到原来的映射。


    **例如**

    ```cs
    var mappingSource = new FluentMappingSource();
    mappingSource.Map<NorthwindDatabase>(mapping =>
    {
        mapping.Table(o => o.Products, "Products")
            .PrimaryKey(o => o.ProductID)
            .Column(o => o.ProductName,
                    o => {
                        o.Name = "ProductName";
                        o.DbType = "varchar(32)";
                    });

        mapping.Table(o => o.Products, "Products")
            .Column(o => o.ProductName, o => o.Name = "product_name");
    });
    ```

    等价于

    ```cs
    var mappingSource = new FluentMappingSource();
    mappingSource.Map<NorthwindDatabase>(mapping =>
    {
        mapping.Table(o => o.Products, "Products")
            .PrimaryKey(o => o.ProductID)
            .Column(o => o.ProductName, 
                    o => { 
                        o.Name = "product_name"; 
                        o.DbType = "varchar(32)"; 
                    });
    });
    ```

**关系映射**

```cs
    const string CategoryTableName = "Categories";
    const string ProdcutTableName = "Products";

    var mappingSource = new FluentMappingSource();
    mappingSource.Map<NorthwindDatabase>(mapping =>
    {
        mapping.ProviderType = typeof(AccessDbProvider);
        mapping.Table(o => o.Categories, CategoryTableName)
                .PrimaryKey(o => o.CategoryID)
                .Column(o => o.CategoryName)
                .Column(o => o.Description)
                .Column(o => o.Picture)
                .Association(o => o.Products, o => new { o.CategoryID }, o => new { o.CategoryID });

        mapping.Table(o => o.Products, ProdcutTableName)
                .PrimaryKey(o => o.ProductID)
                .Column(o => o.ProductName)
                .Column(o => o.CategoryID)
                .Association(o => o.Category, o => o.CategoryID, o => o.CategoryID);
    });
```

**继承关系映射**
```cs
var mappingSource = new FluentMappingSource();
mappingSource.Map<NorthwindDatabase>(mapping =>
{
        mapping.ProviderType = typeof(AccessDbProvider);
        mapping.Name = typeof(NorthwindDatabase).Name;
        mapping.Table(o => o.Contacts, "Contacts", o => { o.InheritanceCode = "Unknow"; o.IsInheritanceDefault = true; })
                .PrimaryKey(o => o.ContactID, o => { o.Storage = "_ContactID"; })
                .Column(o => o.ContactType, o => o.IsDiscriminator = true)
                .Column(o => o.CompanyName)
                .Column(o => o.Phone)
                .Column(o => o.GUID)
                .Inheritance<FullContact>()
                .Column(o => o.ContactName)
                .Column(o => o.ContactTitle)
                .Column(o => o.Address)
                .Column(o => o.City)
                .Column(o => o.Region)
                .Column(o => o.PostalCode)
                .Column(o => o.Country)
                .Column(o => o.Fax)
                .Inheritance<SupplierContact>()
                .Column(o => o.HomePage)
                .Inheritance<EmployeeContact>()
                .Column(o => o.PhotoPath)
                .Column(o => o.Photo)
                .Column(o => o.Extension)
                .Inheritance<CustomerContact>()
                .Inheritance<ShipperContact>();
});
```










