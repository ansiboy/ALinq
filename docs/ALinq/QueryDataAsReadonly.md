## 将信息作为只读信息检索

当您不打算更改数据时，您可以通过设法产生只读结果来提高查询性能。

通过将 ObjectTrackingEnabled 设置为 false 可实现只读处理。

**示例**

```cs
var db = new Northwind(@"c:\Northwind.db3");

db.ObjectTrackingEnabled = false;
IOrderedQueryable<Employee> hireQuery =
    from emp in db.Employees
    orderby emp.HireDate
    select emp;

foreach (Employee empObj in hireQuery)
{
    Console.WriteLine("EmpID = {0}, Date Hired = {1}",
        empObj.EmployeeID, empObj.HireDate);
}
```