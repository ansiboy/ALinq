## 连接查询

下面的示例演示如何组合来自多个表的结果。

**示例一**

下面的示例中使用外键导航来选择位于伦敦的客户所下的所有订单。

```cs
var infoQuery = db.Orders.Where(c => c.Customer.City == "London");
```

**示例二**

下面的示例中使用外键导航来筛选出 Supplier 位于美国的脱销 Products。

```cs
var infoQuery = db.Products.Where(prod => prod.Supplier.Country == "" && prod.UnitsInStock == 0);
```

**示例三**

下面的示例显式联接两个表并投影来自这两个表的结果。

```cs
var q = db.Customers.Join(db.Orders, c => c.CustomerID, o => o.CustomerID, (c, o) => new { c.ContactName, o.OrderDate });
```

**示例四**

下面的示例显式联接三个表并投影来自其中各个表的结果。

```cs
var q = db.Customers
            .Join(db.Orders, c => c.CustomerID, o => o.CustomerID, (c, o) => new 
            { 
                Customer = c, 
                Order = o 
            })
            .Join(db.Employees, o => o.Customer.City, e => e.City, (o, e) => new
            {
                o.Customer.ContactName,
                o.Order.OrderDate,
                e.FirstName,
                e.LastName
            });
```

**示例五**

下面的示例演示如何通过使用 DefaultIfEmpty() 实现 LEFT OUTER JOIN。 如果对应的 Employee 没有 Order，则 DefaultIfEmpty() 方法将返回 null。

```cs
var q = db.Employees.Join(db.Orders.DefaultIfEmpty(), e => e, o => o.Employee,
                            (e, o) => new
                            {
                                e.FirstName,
                                e.LastName,
                                Order = o
                            });
```
