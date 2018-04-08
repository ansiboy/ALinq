## 在 DataContext 级别进行筛选

您可以在 DataContext 级别筛选 EntitySets。 此类筛选器适用于用此 DataContext 实例执行的所有查询。

**示例**

在下面的示例中，使用 DataLoadOptions.AssociateWith(LambdaExpression) 来筛选截至 ShippedDate 的客户的预加载订单。

```cs
var db = new Northiwnd(@"Northwnd.db3");
// Preload Orders for Customer.
// One directive per relationship to be preloaded.
DataLoadOptions ds = new DataLoadOptions();
ds.LoadWith<Customer>(c => c.Orders);
ds.AssociateWith<Customer>
    (c => c.Orders.Where(p => p.ShippedDate != DateTime.Today));
db.LoadOptions = ds;

var custQuery =
    from cust in db.Customers
    where cust.City == "London"
    select cust;

foreach (Customer custObj in custQuery)
{
    Console.WriteLine("Customer ID: {0}", custObj.CustomerID);
    foreach (Order ord in custObj.Orders)
    {
        Console.WriteLine("\tOrder ID: {0}", ord.OrderID);
        foreach (OrderDetail detail in ord.OrderDetails)
        {
            Console.WriteLine("\t\tProduct ID: {0}", detail.ProductID);
        }
    }
}
```