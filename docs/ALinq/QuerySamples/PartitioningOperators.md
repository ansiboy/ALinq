## 分页操作

## 返回序列中的元素

使用 Take<TSource> 运算符可返回序列中给定数目的元素，然后跳过其余元素。

**示例**

下面的示例使用 Take 选择前五个受雇的 Employees。 请注意，此集合首先按 HireDate 排序。

```cs
var firstHiredQuery = db.Employees.OrderBy(o => o.HireDate).Take(5);
```

## 跳过序列中的元素

**示例**

下面的示例使用 Skip<TSource> 选择 10 种最贵的 Products 以外的所有产品。

```cs
 var lessExpensiveQuery = db.Products.OrderBy(o => o.UnitPrice).Skip(10);
 ```

**示例**

下面的示例结合使用 Skip<TSource> 和 Take<TSource> 方法来跳过前 50 条记录，然后返回下 10 条记录。

```cs
var custQuery2 = db.Customers.OrderBy(o => o.ContactName).Skip(50).Take(10);
```