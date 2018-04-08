## 检测和解决提交冲突

**示例**

ALinq 提供了许多资源，用于检测和解决因多个用户更改数据库而产生的冲突。

**示例**

下面的示例显示了捕获 ChangeConflictException 异常的 try/catch 块。 每个冲突的实体和成员信息会显示在控制台窗口中。

```cs
var db = new SQLiteNorthwind("Northwind.db3");

Customer newCust = new Customer();
newCust.City = "Auburn";
newCust.CustomerID = "AUBUR";
newCust.CompanyName = "AubCo";
db.Customers.InsertOnSubmit(newCust);

try
{
    db.SubmitChanges(ConflictMode.ContinueOnConflict);
}
catch (ChangeConflictException e)
{
    Console.WriteLine("Optimistic concurrency error.");
    Console.WriteLine(e.Message);
    Console.ReadLine();
    foreach (ObjectChangeConflict occ in db.ChangeConflicts)
    {
        MetaTable metatable = db.Mapping.GetTable(occ.Object.GetType());
        Customer entityInConflict = (Customer)occ.Object;
        Console.WriteLine("Table name: {0}", metatable.TableName);
        Console.Write("Customer ID: ");
        Console.WriteLine(entityInConflict.CustomerID);
        foreach (MemberChangeConflict mcc in occ.MemberConflicts)
        {
            object currVal = mcc.CurrentValue;
            object origVal = mcc.OriginalValue;
            object databaseVal = mcc.DatabaseValue;
            MemberInfo mi = mcc.Member;
            Console.WriteLine("Member: {0}", mi.Name);
            Console.WriteLine("current value: {0}", currVal);
            Console.WriteLine("original value: {0}", origVal);
            Console.WriteLine("database value: {0}", databaseVal);
        }
    }
}
catch (Exception ee)
{
    // Catch other exceptions.
    Console.WriteLine(ee.Message);
}
finally
{
    Console.WriteLine("TryCatch block has finished.");
}
```

### 指定并发异常的引发时间

在 ALinq 中，当因出现开放式并发冲突而导致对象不能更新时，会引发 ChangeConflictException 异常。

在向数据库提交您所做的更改前，您可以指定应何时引发并发异常：

* 第一次失败时引发异常 (FailOnFirstConflict)。
* 完成所有更新尝试，积累所有失败，然后在异常中报告积累的失败 (ContinueOnConflict)。

引发 ChangeConflictException 异常时，该异常会提供对 ChangeConflictCollection 集合的访问。 此集合提供了有关每个冲突（映射到单个失败的更新尝试）的详细信息，包括对 MemberConflicts 集合的访问。 每个成员冲突映射到未通过并发检查的更新中的单个成员。

**示例**

下面的代码显示了这两个值的示例。

```cs
var db = new Northwind("...");

// Create, update, delete code.

db.SubmitChanges(ConflictMode.FailOnFirstConflict);
// or
db.SubmitChanges(ConflictMode.ContinueOnConflict);
```

### 指定测试哪些成员是否发生并发冲突

通过将三个枚举之一应用于 ColumnAttribute 特性 的 UpdateCheck 属性 (Property)，可指定将哪些成员包含在用于检测开放式并发冲突的更新检查范围内。

UpdateCheck 属性 与 ALinq 中的运行时并发功能一起使用。

**注意**

只要未将任何成员指定为 IsVersion=true，就会将原始成员值与当前数据库状态进行比较。

* 始终使用此成员检测冲突

    将 UpdateCheck 属性值设置为 Always。

* 永不使用此成员检测冲突

    将 UpdateCheck 属性值设置为 Never。

* 仅在应用程序已更改此成员的值时才使用此成员检测冲突

    将 UpdateCheck 属性值设置为 WhenChanged。


**示例**

下面的示例指定在更新检查期间永远都不应该测试 HomePage 对象

```cs
[Column(Storage="_HomePage", DbType="NText", UpdateCheck=UpdateCheck.Never)]
public string HomePage
{
    get
    {
        return this._HomePage;
    }
    set
    {
        this._HomePage = value;
    }
}
    ```