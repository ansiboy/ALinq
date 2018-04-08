## 如何：连接到数据库

DataContext 是用来连接到数据库、从中检索对象以及将更改提交回数据库的主要渠道。

1、使用数据库连接字符串或者文件名连接数据库，使用文件名仅适用与单机数据库，Access 和 SQLite 数据库

**示例**

```cs
// DataContext takes a connection string. 
DataContext db = new DataContext(@"c:\Northwind.db3",typeof(SQLiteProvider));

// Get a typed table to run queries.
Table<Customer> Customers = db.GetTable<Customer>();

// Query for customers from London.
var query =
    from cust in Customers
    where cust.City == "London"
    select cust;

foreach (var cust in query)
    Console.WriteLine("id = {0}, City = {1}", cust.CustomerID, cust.City);
```

2、使用 System.Data.IDbConnection 对象连接数据库

**示例**

```cs
var conn = new System.Data.SQLite.SQLiteConnection("Data Source=c:\Northwind.db3");
var db = new DataContext(conn,typeof(SQLiteProvider));

```

**注意**

对于 Access 或者 SQLite 数据库，使用文件名连接数据库，并且文件扩展名是 mdb、accdb（Access 数据库）或者 db,db3（SQLite 数据库）， 可以省略 Provider 类型参数，ALinq 可以根据文件扩展名推断出数据库类型。

**连接 Access 数据库**

```cs
var db = new DataContext(@"c:\Northwind.mdb");
```

*等价于*

```cs
var db = new DataContext(@"c:\Northwind.mdb",typeof(AccessDbProvider));
```

**连接 SQLite 数据库**

```c
var db = new DataContext(@"c:\Northwind.db3");
```

*等价于*

```cs
var db = new DataContext(@"c:\Northwind.db3",typeof(SQLiteProvider));
```

