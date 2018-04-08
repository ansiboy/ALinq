<!-- ## [ALinq 中文文档](http://ansiboy.github.io/ALinq/?ALinq/index)
## [ALinq Dynamic 文档](http://ansiboy.github.io/ALinq/?ALinqDynamic) -->

# ALinq 

## ALinq 是什么

ALinq 是一个支持 Linq 的 ORM ，不但完整实现了 Linq to SQL  的全部功能和 API，注意，是完整实现！！！也就是说，Linq to SQL 中的功能和函数，你都可以在 ALinq 中找到，使用得你的 Linq to SQL 知识与技能，可以轻易地转移到 ALinq 中来。而且，还在 Linq to SQL 的基础上进行了一系列的改进。包括支持更多的数据库，批量的增删改。

ALinq 支持的数据库有：

Access，SQLite，MS SQL Server,　MySQL，Oracle，Firebird，PostgreSQL，DB2 等主流数据库。

## ALinq 为谁设计？

1. 熟悉喜欢 Linq to SQL　的朋友， 希望 Linq to SQL 能应用其实数据库中去。
1. 希望将 Linq to SQL 代码移植到其它数据库中去，例如：Oracle 或者 Access 。

## ALinq 品质如何？是否稳定？

ALinq 已经从 2008 年发布至今，已经经过了大量用户的验证，使用稳定可靠。

## ALinq 的使用

ALinq 非常易于使用，如果你熟悉 Linq to SQL，几分钟即可上手。使用的差异主要有两点。

1. DLL 的引用
1. 命名空间

其他的使用和 Linq to SQL 相同。

### 示例

* 使用 ALinq 连接 Access 数据库

    需要引用 ALinq.dll， ALinq.AccessDB.dll

    ```cs
    //Use connection string initialize,and specify the sql provider.
    var context = new ALinq.DataContext("C:/Northwind.mdb",
                                        typeof(ALinq.Access.AccessDbProvider));

    //or use file name initialize the datacontext, 
    //the datacontext will specify the sql provider by file extension name.
    context = new ALinq.DataContext("C:/Northwind.mdb");

    //Use connection initialize.
    var builder = new OleDbConnectionStringBuilder
    {
        DataSource = "C:/Northwind.mdb",
        Provider = "Microsoft.Jet.OLEDB.4.0"
    };
    var conn = new OleDbConnection(builder.ConnectionString);
    context = new ALinq.DataContext(conn, typeof(ALinq.Access.AccessDbProvider));
    ```

* 使用 ALinq 连接 SQLite 数据库

    需要引用 ALinq.dll， ALinq.SQLite.dll

    ```cs
    //Use connection string initialize.
    var context = new ALinq.DataContext("C:/Northwind.db",
                                        typeof(ALinq.SQLite.SQLiteProvider));

    //or use file name initialize the datacontext, 
    //the datacontext will specify the sql provider by file extension name.
    context = new ALinq.DataContext("C:/Northwind.db");

    //Use connection initialize.
    var builder = new SQLiteConnectionStringBuilder
    {
        DataSource = "C:/Northwind.db"
    };
    var connection = new SQLiteConnection(builder.ToString());
    context = new ALinq.DataContext(connection,
                                    typeof(ALinq.SQLite.SQLiteProvider));
    ```

* 使用 ALinq 连接 MySQL 数据库

    需要引用 ALinq.dll， ALinq.MySQL.dll

    ```cs
    var builder = new MySqlConnectionStringBuilder()
    {
        Server = "localhost",
        Port = 3306,
        UserID = "root",
        Password = "test",
        Database = "Northwind"
    };
    var conn = new MySqlConnection(builder.ToString());
    var context = new ALinq.DataContext(conn,
                                typeof(ALinq.MySQL.MySqlProvider));
    ```

更详细的使用，请参考 Linq to SQL


