## 集合查询

### 返回两个序列之间的差集

使用 Except<TSource> 运算符可返回两个序列之间的差集。

**示例**

此示例使用 Except<TSource> 返回有 Customers 居住但无 Employees 居住的所有国家/地区的序列。

```cs
var infoQuery = db.Customers
    .Select(c => c.Country)
    .Except(db.Employees.Select(e => e.Country));
```

### 返回两个序列的交集

使用 Intersect<TSource> 运算符可返回两个序列的交集。

**示例**

此示例使用 Intersect<TSource> 返回既有 Customers 居住又有 Employees 居住的所有国家/地区的序列。

```cs
var infoQuery = db.Customers
    .Select(c => c.Country)
    .Intersect(db.Employees.Select(e => e.Country));
```

### 返回两个序列的并集

使用 Union<TSource> 运算符可返回两个序列的并集。

**示例**

此示例使用 Union<TSource> 返回有 Customers 或 Employees 的所有国家/地区的序列。

```cs
var infoQuery = db.Customers
    .Select(c => c.Country)
    .Union(db.Employees.Select(e => e.Country));
```