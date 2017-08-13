## 如何关闭延迟加载

可以通过将 DeferredLoadingEnabled 设置为 false 来关闭延迟加载。 

**示例**

下面的示例演示如何通过将 DeferredLoadingEnabled 设置为 false 来关闭延迟加载

```cs
var db = new Northwind(@"c:\northwnd.db3");
db.DeferredLoadingEnabled = false;

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