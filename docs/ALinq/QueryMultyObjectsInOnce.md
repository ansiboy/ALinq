## 一次检索许多对象

通过使用 LoadWith，可以在一个查询中检索许多对象。

**示例**

下面的代码使用 LoadWith 方法来同时检索 Customer 和 Order 对象。

```cs
var db = new Northwind(@"Northwind.db3");
DataLoadOptions ds = new DataLoadOptions();
ds.LoadWith<Customer>(c => c.Orders);
ds.LoadWith<Order>(o => o.OrderDetails);
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