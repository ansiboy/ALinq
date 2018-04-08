## 解决冲突

### 通过保留数据库值解决冲突

若要先协调预期数据库值与实际数据库值之间的差异，再尝试重新提交更改，则可以使用 OverwriteCurrentValues 保留在数据库中找到的值。 然后会覆盖对象模型中的当前值。 

**注意**

在所有情况下，都会先通过从数据库中检索更新后的数据来刷新客户端上的记录。 此操作确保了下一次更新尝试将通过相同的并发检查。

**示例**

在本方案中，当 User1 尝试提交更改时将引发 ChangeConflictException 异常，原因是 User2 同时已更改了 Assistant 和 Department 列。 下表说明了这种情况。

	
&nbsp;                                      | Manager |  Assistant|Department
--------------------------------------------|---------|-----------|-----------
原始数据库在被 User1 和 User2 查询时的状态。     |Alfreds  |Maria      |销售额
User1 准备提交这些更改。                       |Alfred   |           |Marketing
User2 已经提交了这些更改。                     |         |  Mary      |服务

User1 决定通过用更新的数据库值覆盖对象模型中的当前值来解决此冲突。

User1 通过使用 OverwriteCurrentValues 解决了此冲突后，数据库中的结果将如下表中所示：

	
&nbsp;              |       Manager    |  Assistant        | Department
--------------------|------------------|-------------------|-----------
解决冲突后的新状态。   |Alfreds（原始）     |  Mary（来自 User2）|服务（来自 User2）

下面的示例代码演示了如何用数据库值覆盖对象模型中的当前值。 （未对各个成员冲突进行检查或自定义处理。）

```cs
var db = new Northwind("...");
try
{
    db.SubmitChanges(ConflictMode.ContinueOnConflict);
}

catch (ChangeConflictException e)
{
    Console.WriteLine(e.Message);
    foreach (ObjectChangeConflict occ in db.ChangeConflicts)
    {
        // All database values overwrite current values.
        occ.Resolve(RefreshMode.OverwriteCurrentValues);
    }
}
```

### 通过覆盖数据库值解决冲突

若要先协调预期数据库值与实际数据库值之间的差异，再尝试重新提交更改，则可以使用 KeepCurrentValues 覆盖数据库值。

**注意**

在所有情况下，都会先通过从数据库中检索更新后的数据来刷新客户端上的记录。 此操作确保了下一次更新尝试将通过相同的并发检查。

**示例**

在本方案中，当 User1 尝试提交更改时将引发 ChangeConflictException 异常，原因是 User2 同时已更改了 Assistant 和 Department 列。 下表说明了这种情况。

&nbsp;                                      | Manager |  Assistant|Department
--------------------------------------------|---------|-----------|-----------
原始数据库在被 User1 和 User2 查询时的状态。     |Alfreds  |Maria      |销售额
User1 准备提交这些更改。                       |Alfred   |           |Marketing
User2 已经提交了这些更改。                     |         |Mary        |服务

User1 决定通过用当前客户端成员值覆盖数据库值来解决此冲突。

User1 通过使用 KeepCurrentValues 解决了此冲突后，数据库中的结果将如下表中所示：

&nbsp;           | Manager             |  Assistant    |  Department
-----------------|---------------------|---------------|------------------------
解决冲突后的新状态。|  Alfred（来自 User1） |  Maria（原始） |  Marketing（来自 User1）

下面的示例代码演示了如何用当前客户端成员值覆盖数据库值。 （未对各个成员冲突进行检查或自定义处理。）

```cs
try
{
    db.SubmitChanges(ConflictMode.ContinueOnConflict);
}

catch (ChangeConflictException e)
{
    Console.WriteLine(e.Message);
    foreach (ObjectChangeConflict occ in db.ChangeConflicts)
    {
        //No database values are merged into current.
        occ.Resolve(RefreshMode.KeepCurrentValues);
    }
}
```

### 通过与数据库值合并解决并发冲突

若要先协调预期数据库值与实际数据库值之间的差异，再尝试重新提交更改，则可以使用 KeepChanges 将数据库值与当前客户端成员值合并。

**注意**

注意在所有情况下，都会先通过从数据库中检索更新后的数据来刷新客户端上的记录。 此操作确保了下一次更新尝试将通过相同的并发检查。

**示例**

在本方案中，当 User1 尝试提交更改时将引发 ChangeConflictException 异常，原因是 User2 同时已更改了 Assistant 和 Department 列。 下表说明了这种情况。

&nbsp;                                      | Manager |  Assistant|Department
--------------------------------------------|---------|-----------|-----------
原始数据库在被 User1 和 User2 查询时的状态。     |Alfreds  |Maria      |销售额
User1 准备提交这些更改。                       |Alfred   |           |Marketing
User2 已经提交了这些更改。                     |         |Mary        |服务

User1 决定通过将数据库值与当前客户端成员值合并来解决此冲突。 结果将是，数据库值仅在当前变更集也修改了该值时才会被覆盖。

User1 通过使用 KeepChanges 解决了此冲突后，数据库中的结果将如下表所示：

&nbsp;           | Manager             |  Assistant         |  Department
-----------------|---------------------|--------------------|------------------------
解决冲突后的新状态。|  Alfred（来自 User1） |  Mary（来自 User2） |  Marketing（来自 User1）

```cs
try
{
    db.SubmitChanges(ConflictMode.ContinueOnConflict);
}

catch (ChangeConflictException e)
{
    Console.WriteLine(e.Message);
    // Automerge database values for members that client
    // has not modified.
    foreach (ObjectChangeConflict occ in db.ChangeConflicts)
    {
        occ.Resolve(RefreshMode.KeepChanges);
    }
}

// Submit succeeds on second try.
db.SubmitChanges(ConflictMode.FailOnFirstConflict);
```

