## 排序操作

### 升序排序

使用 OrderBy 运算符可按一个或多个键对序列进行排序。

**示例一**

下面的示例按雇佣日期对 Employees 进行排序。

```cs
var hireQuery = db.Employees.OrderBy(o => o.HireDate);
```

**示例二**

下面的示例使用 where 按运费对运往 London 的 Orders 进行排序。

```cs
var freightQuery = db.Orders.Where(o => o.ShipCity == "London").OrderBy(o => o.Freight);
```

### 降序排序

使用 OrderByDescending 运算符可按一个或多个键对序列进行排序。

**示例**

下面的示例按运往国家/地区对来自 EmployeeID 1 的订单进行排序，然后按运费从高到低进行排序。

```cs
var ordQuery = db.Orders.Where(o => o.EmployeeID == 1).OrderBy(o => o.ShipCountry).OrderByDescending(o => o.Freight);
```
