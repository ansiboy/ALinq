## 投影操作

下面的示例演示如何将 Select 语句与其他功能结合使用以构建查询投影。

**示例一**

下面的示例使用 Select 方法返回由 Customers 的联系人姓名组成的序列。

```cs
var nameQuery = db.Customers.Select(cust => cust.ContactName);
```

**示例二**

下面的示例使用 Select 方法和匿名类型返回由 Customers 的联系人姓名和电话号码组成的序列。

```cs
var infoQuery = db.Customers.Select(cust => new { cust.ContactName, cust.Phone });
```

**示例三**

下面的示例使用 Select 方法和匿名类型返回由雇员的姓名和电话号码组成的序列。 在产生的序列中，FirstName 和 LastName 字段组合成单个字段 (Name)，HomePhone 字段重命名为 Phone。

```cs
var info2Query = db.Employees
                   .Select(e => new { Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone });
```

**示例四**

下面的示例使用 Select 方法和匿名类型返回由所有 ProductID 和名为 HalfPrice 的计算所得的值组成的序列。 此值设置为 UnitPrice 的 1/2。

```cs
var specialQuery = db.Products.Select(p => new { p.ProductID, HalfPrice = p.UnitPrice / 2 });
```

**示例五**

下面的示例使用 Select 方法和一个条件语句返回由产品名和产品可用性组成的序列。

```cs
var prodQuery = db.Products.Select(p => new
{
    p.ProductName,
    Availability = p.UnitsInStock - p.UnitsOnOrder < 0 ? "Out Of Stock" : "In Stock"
});
```

**示例六**

下面的示例使用 Select 方法和一个已知类型 (Name) 返回由雇员的姓名组成的序列。

```cs
public class Name
{
    public string FirstName = "";
    public string LastName = "";
}

void empMethod()
{
    var db = new SQLiteNorthwind(@"c:\Northwnd.db3");
    var empQuery = db.Employees
                        .Select(e => new Name
                        {
                            FirstName = e.FirstName,
                            LastName = e.LastName
                        });
}
```

**示例七**

下面的示例使用嵌套查询返回以下结果：

    * 由所有订单及其对应的 OrderID 组成的序列。
    * 由订单中具有折扣的项组成的子序列。
    * 不含运费时节省的资金数额。

```cs
var ordQuery = db.Orders.Select(o => new
{
    o.OrderID,
    DiscountedProducts = o.OrderDetails.Where(od => od.Discount > 0.0),
    FreeShippingDiscount = o.Freight
});
```



