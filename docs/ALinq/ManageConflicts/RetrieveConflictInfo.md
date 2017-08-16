## 检索冲突信息

### 检索实体冲突信息

使用 ObjectChangeConflict 类的对象来提供有关 ChangeConflictException 异常指出的冲突的信息。

**示例**

下面的示例循环访问累积冲突的列表。

```cs
var db = new Northwind("...");

try
{
    // 更新 操作

    db.SubmitChanges(ConflictMode.ContinueOnConflict);
}

catch (ChangeConflictException e)
{
    Console.WriteLine("Optimistic concurrency error.");
    Console.WriteLine(e.Message);
    foreach (ObjectChangeConflict occ in db.ChangeConflicts)
    {
        MetaTable metatable = db.Mapping.GetTable(occ.Object.GetType());
        Customer entityInConflict = (Customer)occ.Object;
        Console.WriteLine("Table name: {0}", metatable.TableName);
        Console.Write("Customer ID: ");
        Console.WriteLine(entityInConflict.CustomerID);
        Console.ReadLine();
    }
}
```

### 检索成员冲突信息

使用 MemberChangeConflict 类检索有关发生冲突的各成员的信息。

**示例**

下面的代码循环访问 ObjectChangeConflict 对象。 对于每个对象，它会接着循环访问 MemberChangeConflict 对象。

```cs
var db = new Northwind("...");

try
{
    db.SubmitChanges(ConflictMode.ContinueOnConflict);
}

catch (ChangeConflictException e)
{
    Console.WriteLine("Optimistic concurrency error.");
    Console.WriteLine(e.Message);
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
            Console.ReadLine();
        }
    }
}
```