## 调试

### 显示生成的 SQL

您可以通过使用 Log 属性查看为查询生成的 SQL 代码和更改处理方式。 此方法对了解 ALinq 功能和调试特定的问题可能很有用。

**示例**

下面的示例使用 Log 属性在 SQL 代码执行前在控制台窗口中显示此代码。 您可以将此属性与查询、插入、更新和删除命令一起使用。

```sql
SELECT [t0].[CustomerID], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Address], [t0].[City], [t0].[Region], [t0].[PostalCode], [t0].[Country], [t0].[Phone], [t0].[Fax]
FROM [Customers] AS [t0]
WHERE [t0].[City] = @p0
-- CommandType: Text
-- @p0: Input String (Size = 0; Prec = 0; Scale = 0) [London]
-- Context: SQLiteProvider Model: SQLite AttributedMetaModel Build: 3.1.20.35
```

```
AROUT
BSBEV
CONSH
EASTC
NORTS
SEVES
```

```cs
db.Log = Console.Out;
IQueryable<Customer> custQuery =
    from cust in db.Customers
    where cust.City == "London"
    select cust;

foreach(Customer custObj in custQuery)
{
    Console.WriteLine(custObj.CustomerID);
}
```

### 显示变更集

可以通过使用 GetChangeSet 来查看由 DataContext 跟踪的更改。

**示例**

下面的示例检索所在城市为伦敦的客户，将其所在城市更改为巴黎，然后将所做的更改提交回数据库。

```cs
var db = new SQLiteNorthwind("Northwind.db3");
var custQuery = db.Customers.Where(c => c.City == "London");

foreach (Customer custObj in custQuery)
{
    Console.WriteLine("CustomerID: {0}", custObj.CustomerID);
    Console.WriteLine("\tOriginal value: {0}", custObj.City);
    custObj.City = "Paris";
    Console.WriteLine("\tUpdated value: {0}", custObj.City);
}

ChangeSet cs = db.GetChangeSet();
Console.Write("Total changes: {0}", cs);
// Freeze the console window.
Console.ReadLine();

db.SubmitChanges();
```

执行此代码所得到的输出与如下内容类似。 请注意，结尾处的摘要显示做了六项更改。

```
CustomerID: AROUT
	Original value: London
	Updated value: Paris
CustomerID: BSBEV
	Original value: London
	Updated value: Paris
CustomerID: CONSH
	Original value: London
	Updated value: Paris
CustomerID: EASTC
	Original value: London
	Updated value: Paris
CustomerID: NORTS
	Original value: London
	Updated value: Paris
CustomerID: SEVES
	Original value: London
	Updated value: Paris
Total changes: {Inserts: 0, Deletes: 0, Updates: 6}
```

### 显示 ALinq 命令

使用 GetCommand 可显示 SQL 命令及其他信息

**示例**

在下面的示例中，控制台窗口会显示执行查询所产生的输出，接着显示所生成的 SQL 命令、命令的类型和连接的类型。

```cs
var db = new SQLiteNorthwind("Northwind.db3");
var custQuery = db.Customers.Where(c => c.City == "London");

Console.WriteLine("Customers from London:");
foreach (var z in custQuery)
{
    Console.WriteLine("\t {0}", z.ContactName);
}

var dc = db.GetCommand(custQuery);
Console.WriteLine("\nCommand Text: \n{0}", dc.CommandText);
Console.WriteLine("\nCommand Type: {0}", dc.CommandType);
Console.WriteLine("\nConnection: {0}", dc.Connection);
```

输出形式如下：

```
Customers from London:
	 Thomas Hardy
	 Victoria Ashworth
	 Elizabeth Brown
	 Ann Devon
	 Simon Crowther
	 Hari Kumar

Command Text: 
SELECT [t0].[CustomerID], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Address], [t0].[City], [t0].[Region], [t0].[PostalCode], [t0].[Country], [t0].[Phone], [t0].[Fax]
FROM [Customers] AS [t0]
WHERE [t0].[City] = @p0

Command Type: Text

Connection: System.Data.SQLite.SQLiteConnection
```

