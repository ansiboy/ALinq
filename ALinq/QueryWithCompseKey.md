## 在查询中处理复合键

有些运算符只能带一个参数。 如果您的参数必须包含数据库中的多个列，则您必须创建一个匿名类型来表示这种组合。

**示例**

下面的示例显示了调用 GroupBy 运算符的查询，此运算符只能带一个 key 参数。

```cs
var query = db.Customers.GroupBy(o => new { o.City, o.Region });
foreach (var group in query)
{
    Console.WriteLine("\nLocation Key: {0}", group.Key);
    foreach (var customer in group)
    {
        Console.WriteLine("\t{0} {1}", customer.CompanyName, customer.Address);
    }
}
```