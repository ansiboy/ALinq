## 直接执行 SQL 查询

ALINQ 将您编写的查询转换成参数化 SQL 查询（以文本形式），然后将它们发送至 SQL 服务器进行处理。

**示例一**

在下面的示例中，假定 Customer 类的数据分布在两个表（customer1 和 customer2）中。  此查询将返回 Customer 对象的序列。  

```cs
var db = new Northwnd(@"c:\Northwind.db3");
IEnumerable<Customer> results = db.ExecuteQuery<Customer>
(@"SELECT c1.custid as CustomerID, c2.custName as ContactName
    FROM Customer1 as c1, customer2 as c2
    WHERE c1.custid = c2.custid"
);
```

**示例二**

ExecuteQuery 方法也允许带有参数。  请使用类似如下内容的代码来执行参数化查询。  

```cs
var db = new Northwind(@"c:\Northwind.db3");
IEnumerable<Customer> results = db.ExecuteQuery<Customer>
    ("SELECT contactname FROM customers WHERE city = {0}",
    "London");
```