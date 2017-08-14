## 聚合操作

### 返回数值序列的平均值

Average 运算符用于计算数值序列的平均值。

**示例一**

```cs
var averageFreight =db.Orders.Select(o => o.Freight).Average();
```

**示例二**

```cs
 var categories = db.Products
                    .GroupBy(p => p.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        AveragePrice = g.Average(p => p.UnitPrice)
                    });
```

### 对序列中的元素进行计数

使用 Count 运算符可计算序列中的元素数目。

**示例一**

下面的示例计算数据库中的 Customers 数目。

```cs
var customerCount = db.Customers.Count();
```

**示例二**

下面的示例计算数据库中尚未停产的产品数目。

```cs
var notDiscontinuedCount = db.Products.Where(o => !o.Discontinued).Count();
```

*等价于*

```cs
var notDiscontinuedCount = db.Products.Count(o => !o.Discontinued);
```

### 查找数值序列中的最大值

使用 Max 运算符可查找数值序列中的最高值。

**示例一**

下面的示例查找任何员工的最近雇佣日期。

```cs
var latestHireDate = db.Employees.Select(e => e.HireDate).Max();
```

*等价于*

```cs
var latestHireDate = db.Employees.Max(e => e.HireDate);
```

**示例二**

下面的示例查找任何产品的最大库存件数。

```cs
 var maxUnitsInStock = db.Products.Select(product => product.UnitsInStock).Max();
```

*等价于*

```cs
 var maxUnitsInStock = db.Products.Max(product => product.UnitsInStock);
 ```

 **示例三**

 下面的示例使用 Max 查找每个类别中单价最高的 Products。 然后，按类别列出输出结果。

 ```cs
 var maxQuery = db.Products.GroupBy(o => o.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    MostExpensiveProducts = g.Key.Products.Where(p => p.UnitPrice == g.Max(p1 => p1.UnitPrice))
                });
 ```

### 查找数值序列中的最小值

使用 Min 运算符可返回数值序列中的最小值。

**示例一**

```cs
var lowestUnitPrice = db.Products.Select(o => o.UnitPrice).Min();
```

*等价于*

```cs
var lowestUnitPrice = db.Products.Min(o => o.UnitPrice);
```

**示例二**

下面的示例查找所有订单的最低运费额。

```cs
 var lowestFreight = db.Orders.Select(o => o.Freight).Min();
 ```

 *等价于*

 ```cs
var lowestFreight = db.Orders.Min(o => o.Freight);
```

**示例三**

下面的示例使用 Min 查找每个类别中单价最低的 Products。 

```cs
var minQuery = db.Products.GroupBy(o => o.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    LeastExpensiveProducts = g.Key.Products.Where(p => p.UnitPrice == g.Min(p1 => p1.UnitPrice))
                });
```

### 计算数值序列中的值之和

使用 Sum 运算符可以计算序列中数值的和。


