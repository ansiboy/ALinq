## 分组查询

GroupBy 运算符用于对序列中的元素进行分组。

**示例一**

下面的示例按照 CategoryID 对 Products 进行分区。

```cs
 var productQuery = db.Products.GroupBy(o => o.CategoryID);

foreach (var group in productQuery)
{
    Console.WriteLine("\nCategoryID Key = {0}:", group.Key);
    foreach (Product product in group)
    {
        Console.WriteLine("\t{0}", product.ProductName);
    }
}
```

**示例二**

下面的示例使用 Max 来查找每个 CategoryID 的最高单价。

```cs
 var q = db.Products
           .GroupBy(p => p.CategoryID)
           .Select(g => new { CategoryId= g.Key, MaxPrice = g.Max(p => p.UnitPrice) });
```

**示例三**

下面的示例使用 Sum 来查找每个 CategoryID 的总 UnitPrice。

```cs
var priceQuery = db.Products
                    .GroupBy(prod => prod.CategoryID)
                    .Select(grouping => new { grouping.Key, TotalPrice = grouping.Sum(p => p.UnitPrice) });

foreach (var grp in priceQuery)
{
    Console.WriteLine("Category = {0}, Total price = {1}",
        grp.Key, grp.TotalPrice);
}
```

**示例四**

下面的示例使用 Count<TSource> 来查找每个 CategoryID 中已停产 Products 的数量。

```cs
var disconQuery = db.Products
                    .GroupBy(prod => prod.CategoryID)
                    .Select(grouping => new
                    {
                        grouping.Key,
                        NumProducts = grouping.Count(p => p.Discontinued)
                    });

foreach (var prodObj in disconQuery)
{
    Console.WriteLine("CategoryID = {0}, Discontinued# = {1}",
        prodObj.Key, prodObj.NumProducts);
}
```

**示例五**

下面的示例使用后跟的 where 子句来查找至少包含 10 种产品的所有类别。

```cs
var prodCountQuery = db.Products
                        .GroupBy(prod => prod.CategoryID)
                        .Where(grouping => grouping.Count() >= 10)
                        .Select(grouping => new { grouping.Key, ProductCount = grouping.Count() });


foreach (var prodCount in prodCountQuery)
{
    Console.WriteLine("CategoryID = {0}, Product count = {1}",
        prodCount.Key, prodCount.ProductCount);
}
```

**示例六**

下面的示例按 CategoryID 和 SupplierID 对产品进行分组。

```cs
var prodQuery = db.Products
                    .GroupBy(prod => new { prod.CategoryID, prod.SupplierID })
                    .Select(grouping => new { grouping.Key, grouping });

foreach (var grp in prodQuery)
{
    Console.WriteLine("\nCategoryID {0}, SupplierID {1}",
        grp.Key.CategoryID, grp.Key.SupplierID);
    foreach (var listing in grp.grouping)
    {
        Console.WriteLine("\t{0}", listing.ProductName);
    }
}
```

**示例七**

下面的示例返回两个产品序列。 第一个序列包含单价小于或等于 10 的产品。 第二个序列包含单价大于 10 的产品。

```cs
var priceQuery = db.Products.GroupBy(prod => new { Criterion = prod.UnitPrice > 10 });

foreach (var prodObj in priceQuery)
{
    if (prodObj.Key.Criterion == false)
        Console.WriteLine("Prices 10 or less:");
    else
        Console.WriteLine("\nPrices greater than 10");
    foreach (var listing in prodObj)
    {
        Console.WriteLine("{0}, {1}", listing.ProductName,
            listing.UnitPrice);
    }
}
```

**示例八**

GroupBy<TSource, TKey> 运算符只能采用单个键参数。 如果您需要按多个键进行分组，则必须创建匿名类型，如下例所示：

```cs
var custRegionQuery = db.Customers.GroupBy(cust => new { cust.City, cust.Region });

foreach (var grp in custRegionQuery)
{
    Console.WriteLine("\nLocation Key: {0}", grp.Key);
    foreach (var listing in grp)
    {
        Console.WriteLine("\t{0}", listing);
    }
}
```