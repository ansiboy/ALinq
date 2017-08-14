## 其它查询

### 返回序列中的第一个元素

使用 First 运算符可返回序列中的第一个元素。 

使用 FirstOrDefault 运算符可返回序列中的第一个元素或默认值。 

使用 First、FirstOrDefault 的查询是立即执行的。

使用 First 查询，如果查出来的数据为空，则会抛出异常。而使用 FirstOrDefault 查询，则会返回空值。

**示例一**

下面的代码查找表中的第一个 Shipper：

```cs
Shipper shipper = db.Shippers.First();
Console.WriteLine("ID = {0}, Company = {1}", shipper.ShipperID,
    shipper.CompanyName);
```

**示例二**

下面的代码查找具有 CustomerID BONAP 的单个 Customer。

```cs
var custQuery = db.Customers.Where(o => o.CustomerID == "BONAP").First();
```

*等价于*

```cs
var custQuery = db.Customers.First(o => o.CustomerID == "BONAP");
```

### 从序列中消除重复元素

使用 Distinct<TSource> 运算符可从序列中消除重复元素。

**示例**

下面的示例使用 Distinct<TSource> 来选择有客户的唯一城市序列。

```cs
var cityQuery = db.Customers.Select(c => c.City).Distinct();
```

### 确定序列中的元素是否部分或全部满足条件

* 如果序列中的所有元素都满足某一项条件，则 All<TSource> 运算符会返回 true。

* 如果序列中的任意一个元素满足某一项条件，则 Any<TSource> 运算符会返回 true。

**示例**

下面的示例返回由至少下了一个订单的客户组成的序列。 如果给定的 Customer 下了任何 Order，则 Where 方法的计算结果将为 true。

```cs
var OrdersQuery = db.Customers.Where(c => c.Orders.Any());
```

**示例**

下面的示例返回由订单包含以“C”开头的 ShipCity 的客户组成的序列。 返回结果中还包括未下订单的客户。 （按照设计，对于空序列，All<TSource> 运算符返回 true。）通过使用 Count 运算符在控制台输出中消除了未下订单的客户。

```cs
var custEmpQuery = db.Customers
    .Where(c => c.Orders.All(o => o.ShipCity.StartsWith("C")))
    .OrderBy(c => c.CustomerID);

foreach (Customer custObj in custEmpQuery)
{
    if (custObj.Orders.Count > 0)
        Console.WriteLine("CustomerID: {0}", custObj.CustomerID);
    foreach (Order ordObj in custObj.Orders)
    {
        Console.WriteLine("\t OrderID: {0}; ShipCity: {1}",
            ordObj.OrderID, ordObj.ShipCity);
    }
}
```

### 将序列转换为泛型列表

使用 AsEnumerable<TSource> 可返回类型化为泛型 IEnumerable 的参数。

**示例**

在此示例中，ALinq（使用默认泛型 Query）会尝试将查询转换为 SQL 并在服务器上执行。 但 where 子句引用用户定义的客户端方法 (isValidProduct)，此方法无法转换为 SQL。

解决方法是指定 where 的客户端泛型 IEnumerable<T> 实现以替换泛型 IQueryable<T>。 可通过调用 AsEnumerable<TSource> 运算符来执行此操作。

```cs
private bool isValidProduct(Product prod)
{
    return prod.ProductName.LastIndexOf('C') == 0;
}

void ConvertToIEnumerable()
{
    var db = new Northwind(@"c:\Northwind.db3");
    var prodQuery = db.Products.AsEnumerable().Where(prod=>isValidProduct(prod));
}
```


