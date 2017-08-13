## 如何：直接执行 SQL 命令

采用 DataContext 连接时，可以使用 ExecuteCommand 来执行不返回对象的 SQL 命令。

**示例**

```cs
db.ExecuteCommand("UPDATE Products SET UnitPrice = UnitPrice + 1.00");
```