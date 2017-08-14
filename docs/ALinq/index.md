## 简介

ALinq 是一个和支持 Linq 查询操作的 ORM，并且兼容 Linq to SQL。目前支持 Access，SQLite，MySQL，Firebird，Oracle，DB2，PostgreSQL 等数据库。

## 入门

* [快速上手](?ALinq/QuickStart)
* 查询示例
    * [聚合查询](?ALinq/QuerySamples/AggregateOperators)
    * [排序查询](?ALinq/QuerySamples/OrderingOperators)
    * [分组查询](?ALinq/QuerySamples/GroupingOperators)
    * [连接查询](?ALinq/QuerySamples/JoinOperators)
    * [投影操作](?ALinq/QuerySamples/ProjectionOperators)
    * [集合查询](?ALinq/QuerySamples/SetOperators)
    * [其它查询](?ALinq/QuerySamples/MiscellaneousOperators)
* [调试](?ALinq/Debug)

## 编程指南

### 与数据库通信
* [连接数据库](?ALinq/ConnectDatabase)
* [直接执行 SQL 命令](?ALinq/ExecuteSQLCommand)
* [重用 ADO.NET 命令何 DataContext 之间的连接](?ALinq/ReuseConnection)

### 查询数据库
* 查询信息
* [将信息作为只读信息](?ALinq/QueryDataAsReadonly)
* [关闭延长加载](?ALinq/CloseDeferredLoading)
* [直接执行 SQL 查询](?ALinq/ExecuteSQLQuery)
* [在查询中处理复合键](?ALinq/QueryWithCompseKey)
* [一次检索多个对象](?ALinq/QueryMultyObjectsInOnce)
* [在 DataContext 级别进行刷选](?ALinq/FilterInDataContext)

### 数据库与对象的映射
* [基于属性的映射](?ALinq/AttributeMapping)
* [外部映射](?ALinq/XMLMapping)
* [Fluent 映射](?ALinq/FluentMapping)

<!-- ### 生成和提交数据更改
* 如何: 向数据库插入行
* 如何: 更新数据库的行
* 如何: 从数据库中删除行
* 如何: 将更改提交到数据库
* 如何: 使用事务封闭数据提交
* 如何: 动态创建数据库
* 如何: 管理更改冲突
    * 如何: 检测何解决提交冲突
    * 如何: 指定并发异常的引发时间
    * 如何: 指定测试哪些成员是否发生并发冲突
    * 如何: 检索实体冲突信息
    * 如何: 检索成员冲突信息
    * 如何: 通过保留数据库值解决冲突
    * 如何: 通过覆盖数据库值解决冲突
    * 如何: 通过与数据库值合并解决冲突 -->
<!-- ### 调试支持
* 如何: 显示生成的 SQL
* 如何: 显示变更集
* 如何: 显示 ALinq 命令 -->
<!-- ### 背景信息
* ADO.NET 与 ALinq
* 自定义插入更新、更新和删除操作
* 使用部分方法添加业务逻辑 -->


     
