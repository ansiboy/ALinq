# ALinq 动态查询

## I.前言

ALinq Dynamic 为 ALinq 提供了一个 Entiy SQL 的查询接口，使得它们能够应用 Entity SQL 进行数据的查询。它的原理是将 Entiy SQL 解释为 Linq 表达式，再执行生成的 Linq 表达式。

## II.概述

### 一. 使用

#### 1. 程序集的引用以及命名空间

使用 ALinq Dynamic，你需要引用ALinq.Dynamic.dll，在使用时，还需要引入ALinq.Dynamic命名空间。当然，使用前你还需要完成建模的工作，本文假设你已经会了，否则请参考Linq to SQL或ALinq教程。

示例一

下面的示例，由于使用到dynamic关键字，必须运行在.NET Framework4或以上。

``` cs
using System;
using ALinq.Dynamic;
using NorthwindDemo;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new NorthwindDataContext();
            var q = db.CreateQuery("select e.FirstName, e.LastName from Employees as e");
            foreach (var item in q)
            {
                var e = item as dynamic;
                Console.WriteLine("{0} {1}", e.FirstName, e.LastName);
            }
        }
    }
}
```

示例二

下面的示例可以运行在.NET Framework 3.5（为了便于阅读以及节省编幅，仅给出关键部份代码）。

``` cs
var db = new NorthwindDataContext();
var q = db.CreateQuery<IDataRecord>("select e.FirstName, e.LastName from Employees as e");
foreach (var e in q)
    Console.WriteLine("{0} {1}", e["FirstName"], e["LastName"]);
```

#### from 关键字

在 from 关键子后，紧跟的是实体类的类名或者强类型 DataContext 的成员名称。例如：

语句一

```sql
select e.FirstName, e.LastName from Employees as e
``` 

Employees 是 NorthwindDataContext 对象的属性成员。 

语句二

```sql
select e.FirstName, e.LastName from Employee as e
``` 

Employees 是 Employee 实体类的名称。 

### 二. 大小写敏感
#### 1．关键词不区分大小写
对关键词是不区分大小写的，例如下面的这两条ESQL语句是相等的。
语句一

``` sql
select p from Product as p
```

语句二

``` sql
SELECT p FROM Product AS p
```

#### 2．命名空间区分大小写

对于命名空间，是区分大小写的，例如下面的两条ESQL语句是不等价的。

语句一

``` sql
using NorthwindDemo 
select p from Product as p
```

语句二

```sql
using NORTHWINDDEMO
SELECT p FROM Product AS p
```

这是因为命名空间是区分大小区的，NorthwindDemo和NORTHWINDDEMO是不等价的。

#### 3. 实体类名称区分大小写

对于实体类的名称，是区分大小写的，例如下面的例句是不等价的

语句一

```sql
select p from Product as p
```

语句二

```sql
select p from PRODUCT as p
```

这是因为，实体类的名称，Product和PRODUCT不等价。

#### 4. 属性名称区分大小写

对属性是区分大小写的，例如下面的例句是不等价的

语句一

```sql
select p.ProductId, p.ProductName from Product as p
```

语句二

```sql
select p.ProductID, p.ProductNAME from Product as p
```

#### 5. 函数名称区分大小写 

对于函数名称，有点特殊，如果是系统自带的函数，则不区分大小写，如果是用户自定义函数，则区分大小写。

示例

```sql
select value GetFullName(e.FirstName, e.LastName) from Employees as e
```

其中GetFullName是区分大小写的。


### 三. 命名空间

#### 1. using 关键字
通过使用 using 关键字，可以引入命名空间，避免使用全局标识。例如：

```cs
var esql = @"using NorthwindDemo;
             select e.FirstName, e.LastName 
             from Employee as e";
var q1 = db.CreateQuery(esql);
```

其等效于：

```cs
var esql = @"select e.FirstName, e.LastName 
             from NorthwindDemo.Employee as e";
var q1 = db.CreateQuery(esql);
```

#### 2. 默认引入的命名空间

在使用 CreateQuery 方法进行查询的时候，ALinq Dynamic 会自动引入 DataContext 所在的命名空间。例如：
```cs
var db = new NorthwindDemo.NorthwindDataContext();
var esql = "select e.FirstName, e.LastName from Employee as e";
```

由于 DataContext ，即 db 变量，对应的实体类命名空间为 NorthwindDemo， esql 语句会自动引入 NothwindDemo 命名空间，即等效于下面的语句：

```cs
var db = new NorthwindDemo.NorthwindDataContext();
var esql = @"using NorthwindDemo;
             select e.FirstName, e.LastName from Employee as e";
```

### 四. 标识符

#### 1. 简单标识符

标识符由字母，数字，和下划线组成，并且第一个字符必须是字母（a-z或A-Z）。

#### 2. 带引号的标识符 

带引号的标识符是括在方括号（[]）中的任意字符序列。使用带引号的标识符可以指定含有在标识符中无效的字符的标识符。方括号中的所有字符都是标识符的一部分，包括所有空格。
但是，带引号的标识符，第一个字符不能是数字，并且不能包含以下字符：

* 换行符
* 回车符
* 制表符
* Backspace

额外的方括号（即括起标识符的方括号中的方括号）。

单引号（即：'）

双引号（即："）

使用带引号的标识符可以创建在标识符中无效的属性名称字符，如下面的示例所示： 

```cs
var esql = "select c.ContactName as [Contact Name] from Customers as c";
var q = db.CreateQuery(esql).Execute();
```

使用带引号的标识符，还可以指定关键字作为标识符。例如，如果类型 Email 具有名为“From”的属性，则可以使用方括号来消除与保留关键字“FROM”的歧义，如下所示： 

```cs
var esql = "select e.[From] from Emails AS e";
```

但是下面的例子是非法的

标识符中带引号 

```cs
var esql = "select c.ContactName as ['Contact Name'] from Customers as c";
```

标识符以数字开始 

```cs
var esql = "select c.ContactName as [0ContactName] from Customers as c";
```

#### 3. 别名规则

如果需要，建议在查询中指定别名，别名可以应用于：

行构造函数

* 查询语句的 FROM 子句

* 查询语句的 SELECT 子句

* 查询语句的 GROUP BY 子句

有效的别名

* 有效的别外包括简单标识符或者带引号的标识符


### 五. 参数

参数是查询语句之外定义的变量，参数名在查询语句中定义，并以(@)符号作为前缀。这可以将它们与属性名或查询中定义的其它名称区分开来。参数为两类，命名参数和顺序参数，对于命名参数，必须指名称，参数的传递不要按照顺序。顺序参数不需要指定名称，在查询语句中使用序号（序号从0开始）作为参数名，参数必须按顺序传递。

命名参数
```cs
var esql = @"select e from Employees as e
         where e.FirstName = @f and e.LastName = @l";
var q = db.CreateQuery(esql, new ObjectParameter("l", "mak"), new ObjectParameter("f", "mike"));
```

顺序参数
```cs
var esql = @"select e from Employees as e
         where e.FirstName = @0 and e.LastName = @1";
var q = db.CreateQuery(esql, "mike", "mak");
```

### 六. 变量

变量表达式是对当前作用域中定义的命名表达式的引用。变量引用必须是有效的标识符。 
以下示例演示如何在查询语句中使用变量。FROM 子句中的 c 是变量定义。在 SELECT 子句中使用的 c 表示变量引用。 
```cs
var esql = "select c.ContactName from Customers as c";
```

### 七. 文字

* 空值

使用 null 表示，空文字用于表示对任何类型均为空的值。

* 布尔值

布尔值文字由关键字 true 和 false 表示。 

* 整型

    整型文字可以为类型 Int32，UInt32，Int64，UInt64。

    Int32 文字是一系列数字字符。例如：123 。

    UInt32 文字是一系列数字字符，后跟一个 U。例如：123u 。

    Int64 文字是一系列数字字符，后跟一个 L。例如：123l 。

    UInt64 文字是一系列数字字符，后跟一个 UL。 例如：123ul 。

* 小数

    固定点数字（小数）是一系列数字字符、一个圆点 (.) 和另一系列数字字符，后跟一个字母“m”。
    
    例如： 123m

* 双精度小数

    双精度浮点数字是一系列数字字符、一个圆点 (.) 和另一系列数字字符，后面可能跟一个指数。例如：123.67，123.67E+3

* 单精度小数

    单精度浮点数字（或浮点数）是双精度浮点数字语法后跟小写 f。 例如：123.67f，123.67F

* 字符串

    字符串是包含在引号内的一系列字符。引号可以同时为单引号 (') 或同时为双引号 (")。例如：'hello'，"This is a string!" 。

* 日期时间

    日期时间文字独立于区域设置并由日期部分和时间部分组成，其中日期部分是必需的。使用 # 包括日期时间字符串。日期部分必须采用格式：YYYY-MM-DD，其中，YYYY 是介于0001 与 9999 之间的四位年度值，MM 是介于 1 与 12 之间的月份，而 DD 是对给定月份 MM 有效的日期值。 
时间部分的格式必须为：HH:MM[:SS]，默认值为 00:00:00，其中 HH 为介于 0 至 23 之间的小时值，MM 为介于 0 至 59 之间的分钟值，SS 为介于 0 至 59 之间的秒值。

    例如：#2017-5-2#，#2018-5-2 10:44:20#

* 二进制

    二进制字符串文字是由单引号分隔的一系列十六进制数字，位于关键字“binary”或快捷方式符号 X 或 x 之后。快捷方式符号 X 是不区分大小写的。例如：Binary '00ffaabb'

* Guid

    GUID 文本表示全局唯一标识符。该文本是一个序列，由关键字 GUID 后跟十六进制数字构成，采用称为“注册表”格式的格式：包含在单引号内的 8-4-4-4-12。十六进制数字区分大小写。
在 GUID 符号与文字负载之间可以存在任意数目的空格，但是不能存在新行。例如：Guid'CD582A70-863B-4E4B-9530-174A83EEB0C0'，GUID 'CD582A70-863B-4E4B-9530-174A83EEB0C0' 。 

### 八. 运算符优先级

### 九. 成员访问

使用点运算符（.）可以访问实例的属性、字段、方法。如果成员是元素的集合，还使用方括号（[]）可以访问集合中的元素。

**属性访问**

```sql
select o.OrderId, o.OrderDate from Orders as o
```

```sql
select o.OrderDate.Value.Year from Orders as o where o.OrderDate is not null
```

**方法访问**

将 OrderDate 转为 'yyyy-MM-dd' 格式的字符串。

```sql
select o.OrderDate.Value.ToString('yyyy-MM-dd') as Date 
from Orders as o where o.OrderDate is not null
```

**访问集合中的元素**

下面的例句中，获取 OrderDetails 属性中的第一个 OrderDetail 。

```cs
var esql = "select o.OrderId, o.OrderDetails[0] as FirstOrderDetail from Orders as o";
db.CreateQuery(esql).Execute();
```

**静态方法的访问**

下面的例句中，调用System.Guid类的NewGuid方法

```cs
var esql = "select System.Guid.NewGuid() as Guid, e.FirstName from Employees as e";
var q = db.CreateQuery<IDataRecord>(esql);

foreach (var item in q)
    Console.WriteLine("{0} {1}", item[0], item[1]);
```

**静态属性的访问**

下面的例句中，调用System.Math类的PI属性

```cs
var esql = "System.Math.PI";
var pi = db.CreateQuery<float>(esql).Single();
```

**DataContext方法访问**

下面的例名中的GetFullName是db实例的一个方法。
```cs
var esql = "select value GetFullName(e.FirstName, e.LastName) from Employees as e";
q = db.CreateQuery<string>(esql);
```

**DataContext属性访问**

下面的例句演示DataContext属性的访问，其中的Version是db实例的一个属性。
```cs
var esql = "select Version, e.FirstName from Employees as e";
var q = db.CreateQuery<IDataRecord>(esql);
```


### 十. 分页

.可以使用 SKIP 和 LIMIT 关键字来执行物理分面，如果只需要限制返回的记录数，也可以使用 TOP 关键字。但TOP 和 SKIP/LIMIT/TAKE 是互斥的，不能一起使用。

**TOP 慨述**

在 SELECT 关键字之后，可以使用 TOP 关键字，限制返回的记录数。注意：是 TOP (NUMBER) 而不是 TOP NUMBER。正确的例句：

```cs
var esql = "select top(1) p from Products as p";
db.CreateQuery(esql).Execute();
```

**SKIP 和 LIMIT/TAKE 慨述**

SKIP 和 LIMIT/TAKE 是 SELECT 子句的组成部份，SKIP 和 LIMIT/TAKE 不必一起出现，可以单独使用 SKIP 和 LIMIT/TAKE，但必须出现 SELECT 子语句的最后，另外，LIMIT 与 TAKE是等效的。

示例一

```cs
var esql = "select p from Products as p skip 10";
var products = db.CreateQuery<Product>(esql).Execute();
```

示例二

```cs
var esql = "select p from Products as p limit 10";
var products = db.CreateQuery<Product>(esql).Execute();
```

或

```cs
var esql = "select p from Products as p take 10";
var products = db.CreateQuery<Product>(esql).Execute();
```

示例三

```cs
var esql = "select p from Products as p skip 10 limit 10";
var products = db.CreateQuery<Product>(esql).Execute();
```

或

```cs
var esql = "select p from Products as p skip 10 take 10";
var products = db.CreateQuery<Product>(esql).Execute();
```

### 十一. 编写查询语句

**嵌套的查询语句**

* 嵌套表达式

嵌套表达式可以放置在任何可接受其返回类型值的位置。例如：

```cs
var esql = @"Row('mike' as FirstName, 'mak' as LastName, 
                    Row('ansiboy@163.com' as Email, 
                        '13434126607' as Phone) as Contact)";
var row = db.CreateQuery<IDataRecord>(esql).Single() as dynamic;
Console.WriteLine("{0} {1}", row.FirstName, row.LastName);
Console.WriteLine("{0} {1}", row.Contact.Email, row.Contact.Phone);
```


* 嵌套查询

嵌套查询可以放在投影子句中。例如： 

```cs
var esql = @"select c.CategoryID, c.CategoryName, 
  (select value count() from c.Products) as ProductCount
             from Categories as c
             limit 5";
var items = db.CreateQuery<dynamic>(esql);
foreach (var item in items)
{
Console.WriteLine("ID:{0}, Name：{1}，Products Count:{2}", item.CategoryID, 
                  item.CategoryName, item.ProductCount);
}
```


在查询语句中，嵌套查询必须括在括号中，例如：
```cs
var esql = @"(select p.ProductID, p.ProductName from Products as p where p.UnitPrice < 100 )
              union
             (select p.ProductID, p.ProductName from Products as p where p.UnitPrice > 200)";
var q = db.CreateQuery<IDataRecord>(esql);
```

* 使用索引器查询 

下面的示例，选取员工的FirstName和LastName，并且过滤掉FirstName为Mike的记录。

```cs
var esql = @"select e.FirstName, e.LastName from Employees as e";
var q = db.CreateQuery<IDataRecord>(esql)
            .Where(o=> (string)o["FirstName"] != "Mike");
```

* 使用接口查询

下面的示例是基于接口的查询示例，值得注意的是，Employee实体类必须继承IEmployee接口，否则会出异常。

```cs
var esql = "select e from Employees as e";
var q = db.CreateQuery<IEmployee>(esql)
            .Where(o => o.FirstName == "F" && o.LastName == "L")
            .Select(o => new { o.FirstName, o.LastName, o.BirthDate });
```

* 参数化数据源

使用参数化的数据源，可以将Linq查询与Entity SQL查询结合起来。下面的示例中，选通过Linq查询获取Country为EN的员工记录，然后在Entity SQL语句中，再过滤掉LastName为Mak的记录。

```cs
var employees = db.Employees.Where(o => o.Country == "EN");
var esql = "select e from @0 as e where e.LastName != 'Mak'";
var q = db.CreateQuery(esql, employees);
```

## III. 参考

### 一. 算术运算符

运算符|说明
---|---
+（加）| 加法
/（除）| 除法
%（取模）| 返回除法运算的余数。
*（乘）| 乘法
-（负号）| 求反
-（减）| 减法

### 二. 规范函数

#### １．聚合函数

运算符             |说明
------------------|-------------------
Avg (exression)   |返回非空的平均值。
BigCount ()       |返回集合的记录数。
Count ()          |返回集合的记录数
Max (expression)  |返回非空值的最大值。
Min ([expression])|返回非空值的最小值。
Sum (expression)  |返回非空值的总和。

* Avg

返回非空的平均值。

参数

exression：数值类型

返回值

expression 的类型。

示例

```sql
select value avg(p.UnitPrice) from Products as p
```

* BigCount 

返回集合的记录数。

示例

```sql
select value bigcount() from products as p
```

* Count 

返回集合的记录数

示例

```sql
select value count() from products as p
```
```sql
select value count(c.Products) from category as c
```
```sql
select c.CategoryId, count(select p from c.Products as p) as ProductsCount from category as c
```

* Max

示例

```sql
select value max(p.UnitPrice) from Products as p
```
```sql
select max(select value p.UnitPrice from c.Products as p) from Categories as c
```
* Min

返回非空值的最小值。

示例

```sql
select value min(p.UnitPrice) from Products as p
```
```sql
select min(select value p.UnitPrice from c.Products as p) from Categories as c
```

* Sum

返回非空值的总和。

示例

```sql 
select sum(o.UnitPrice) from OrderDetails as o
```
```sql
select key, sum(o.Quantity) as SumQuantity  from OrderDetails as o group by o.Product as key
```

**基于集合的聚合**

基于集合的聚合（集合函数）针对集合而运行并返回值。 例如，如果 ORDERS 是所有订单的集合，则可以使用以下表达式计算最早的发货日期：
```sql
select value min(o.ShippedDate) from Orders as o
```

将在当前环境名称解析范围内计算基于集合的聚合内的表达式。

**基本组的聚合**	
基于组的聚合将按照 GROUP BY 子句定义的方式对组进行计算。 对于结果中的每个组，将使用每个组中的元素作为聚合计算的输入来计算单独的聚合。 当在 select 表达式中使用 group-by 子句时，在投影或 order-by 子句中只存在分组表达式名称、聚合或常量表达式。

以下示例计算每种产品的平均订购数量：

```sql
select avg(d.Quantity) from OrderDetails as d
group by d.ProductId
```

在 SELECT 表达式中，可以在没有显式 group-by 子句的情况下使用基于组的聚合。在这种情况下，会将所有元素视为单个组。这等效于基于常量指定分组的情形。例如，请看下面的表达式：
```sql
select avg(d.Quantity) from OrderDetails as d
```

#### 2. 数学函数

函数                         |说明
----------------------------|--------------------------
Abs (value)                 |返回value的绝对值。
Ceiling (value)             |返回不小于value的最小整数。
Floor (value)               |返回不大于value的最大整数。
Power (value, expression)   |返回对指定value求指定的expression幂次所得的结果。
Round (value)               |返回value的整数部份，舍入到最近的整数。
Round (value, digits)       |返回value，舍入到最近的指定的digits。

**Abs (value)**

value：Int16、Int32、Int64、Byte、Single、Double 和 Decimal。

返回值

value的类型。

**Ceiling (value)**

value：Single、Double 和 Decimal

返回值: value的类型。

**Floor**

返回不大于value的最大整数。

value：Single、Double 和 Decimal

返回值: value的类型

**Power**

value：Int32、Int64、Double 或 Decimal。

exponent：Int64、Double 或 Decimal。

返回值: value的类型

**Round**

value：Double 或 Decimal

digits：Int16 或 Int32

返回值: value的类型。

示例

```cs
Round(748.58)
```
```cs
Round(748.58,1)
```

#### 3. 字符串函数

函数                                 |说明
------------------------------------|------------------------
Contact (string1, string2)          |返回包含追加了string1的string2的字符串。
Contains (string, target)           |如果 target 包含在 string 中，则返回 true。
EndsWith (string, target)           |如果 target 以 string 结尾，则返回 true。
IndexOf (target, string)            |返回 target 在 string 中的位置，如果没找到则返回 0。 返回 1 指示 string 的起始位置。 索引号从 1 开始。
Left (string, length)               |返回 string 左侧开始的前 length 个字符。 如果 string 的长度小于 length，则返回整个字符串。
Length (string)                     |返回字符串的 (Int32) 长度，以字符为单位。
LTrim (string)                      |返回没有前导空格的 string。
Replace (string1, string2, string3) |返回 string1，其中所有 string2 都替换为 string3。
Right (string, leng	th)             |返回 string 的后 length 个字符。 如果 string 的长度小于 length，则返回整个字符串。
RTrim (string)                      |返回没有尾随空格的 string。
Substring (string, start, length)   |返回字符串的从 start 位置开始、长度为 length 个字符的子字符串。 start 为 1 指示字符串的第一个字符。 索引号从 1 开始。
StartsWith (string, target)         |如果 string 以 target 开头，则返回 true。
ToLower (string)                    |返回全部大写字符都转换为小写字符的 string。
ToUpper (string)                    |返回全部小写字符都转换为大写字符的 string。
Trim (string)                       |返回没有前导空格和尾随空格的 string。

***Contact(string1,string2)***

返回包含追加了string1的string2的字符串。

**参数**

string1：将string2追加到其后的字符串。
String2：追加到string1之后的字符串。

**返回值**

一个String。

**示例**

```cs
Concat('abc','123')
```

***Contains(string, target)***

如果 target 包含在 string 中，则返回 true。

**参数**

string：在其中进行搜索的字符串。

target：所搜索的目标字符串。

**返回值**

如果 target 包含在 string 中，则为 true；否则为 false。

**示例**

```cs
Contains('abc','bc')
```

***EndsWith***

如果 target 以 string 结尾，则返回 true。

**参数**

string：在其中进行搜索的字符串。

target：在 string 末尾搜索的目标字符串。

**返回值**

如果 string 以 target 结尾，则返回 True；否则返回 false。

**示例**

```sql
-- The following example returns true.
EndsWith('abc', 'bc')
```

***IndexOf***

返回 target 在 string 中的位置，如果没找到则返回 0。 返回 1 指示 string 的起始位置。 索引号从 1 开始。

**参数**

target：要搜索的字符串。

string：在其中进行搜索的字符串。

**返回值**

Int32 

**示例**
```sql
-- The following example returns 4.
IndexOf('xyz', 'abcxyz')
```

***Left***

返回 string 左侧开始的前 length 个字符。 如果 string 的长度小于 length，则返回整个字符串。

**参数**

string：String。

length：Int16、Int32、Int64 或 Byte。 length 不能小于零。

**返回值**

一个 String

**示例**
```sql
-- The following example returns abc.
Left('abcxyz', 3)
```

***Length***

返回字符串的 (Int32) 长度，以字符为单位。

**参数**

string：String

**返回值**

Int32 


**示例**

```sql
-- The following example returns 6.
Legth('abcxyz')
```

***LTrim***

返回没有前导空格的 string。

**参数**

String

**返回值**

一个 String。

**示例**

```sql
-- The following example returns abc.
LTrim(' abc')
```

***Replace***

返回 string1，其中所有 string2 都替换为 string3。

**参数**

一个 String。

**返回值**

一个 String。

**示例**
```sql
-- The following example returns abcxyz.
Concat('abcdef', 'def', 'xyz')
```

**Right**

返回 string 的后 length 个字符。 如果 string 的长度小于 length，则返回整个字符串。

**参数**

string：String。

length：Int16、Int32、Int64 或 Byte。 length 不能小于零。

**返回值**

一个 String。

**示例**

```sql
-- The following example returns xyz.
Right('abcxyz', 3)
```

***RTrim***

返回没有尾随空格的 string。

**参数**

expression：一个 String。

**返回值**

一个 String。

***Substring***

返回字符串的从 start 位置开始、长度为 length 个字符的子字符串。 start 为 1 指示字符串的第一个字符。 索引号从 1 开始。

**参数**

string：String。

start：Int16、Int32、Int64 和 Byte。 start 不能小于一。

length：Int16、Int32、Int64 和 Byte。 length 不能小于零。

**返回值**

一个 String。

**示例**

```sql
-- The following example returns xyz.
Substring('abcxyz', 4, 3)
```

***StartsWith***

如果 string 以 target 开头，则返回 true。

**参数**

string：在其中进行搜索的字符串。

target：在 string 开头搜索的目标字符串。

**返回值**

如果 string 以 target 开头，则返回 True；否则返回 false。

**示例**
```sql
-- The following example returns true.
StartsWith('abc', 'ab')
```

***ToLower***

返回全部大写字符都转换为小写字符的 string。

**参数**

expression：一个 String。

**返回值**

一个 String。

**示例**
```sql
-- The following example returns abc.
ToLower('ABC')
```

***ToUpper***

返回全部小写字符都转换为大写字符的 string。

**参数**

expression：一个 String。

**返回值**

一个 String。

**示例**
```sql
-- The following example returns ABC.
ToUpper('abc')
```

***Trim***

返回没有前导空格和尾随空格的 string。

**参数**

一个 String。

**返回值**

一个 String。

**示例**
```sql
-- The following example returns abc.
Trim(' abc ')
```

#### 4. 日期和时间函数

函数                                  |说明
-------------------------------------|----------------------------------------------
Day (expression)                     |将 expression 的日期部分作为介于 1 到 31 之间的Int32返回。
DayOfYear (expression)               |将 expression 的日部分作为一个介于 1 到 366 之间的Int32返回。其中，对于闰年的最后一天将返回 366。
Minute (expression)                  |将 expression 的分钟部分作为一个介于 0 到 23 之间的Int32返回。
Month (expression)                   |将 expression 的月部分作为一个介于 1 到 12 之间的Int32返回。
Second (expression)                  |将 expression 的秒部分作为一个介于 0 到59之间的Int32返回。
Year (expression)                    |将 expression 的年部分作为Int32返回。

***Day***

将 expression 的日期部分作为介于 1 到 31 之间的Int32返回。

**参数** 

expression：DateTime或String

**返回值**

Int

**示例**

```
Day(#2012-12-9#)
```
```
Day('2012-12-9')
```

***DayOfYear***

将 expression 的日部分作为一个介于 1 到 366 之间的Int32返回。其中，对于闰年的最后一天将返回 366。

**参数** 

expression：DateTime或String

**返回值**

Int32

**示例**

```
DayOfYear(#2012-10-7#)
```
```
DayOfYear('2012-10-7')
```

***Hour***

将 expression 的小时部分作为一个介于 0 到 23 之间的Int32返回。

**参数**

exression：DateTime或String

**返回值**

Int32

**示例**

Hour(#2012-10-7 10:12:32#)

***Minute***

将 expression 的分钟部分作为一个介于 0 到 23 之间的Int32返回。

**参数**

exression：DateTime或String

**返回值**

Int32

**示例**

```
Minute(#2012-10-7 10:12:32#)
```

***Month***

将 expression 的月部分作为一个介于 1 到 12 之间的Int32返回。

**参数**

exression：DateTime或String

**返回值** 

Int32

**示例**
```
Month(#2012-10-7#)
```

***Second*** 

**参数**

exression：DateTime或String

**返回值**

Int

**示例**

```
Second(#2012-10-7 10:12:32#)
```

***Year***

将 expression 的年部分作为Int32返回。

**参数**

DateTime或String

**返回值**

Int32

**示例**

```
Year(#2012-10-7#)
```

#### 5. 按位规范函数
函数                          |说明
------------------------------|--------------------------------
BitWiseAnd (value1, value2)   |按照 value1 和 value2 的类型返回 value1 和 value2 的位与结果。
BitWiseNot (value)            |返回 value 的位求反结果。
BitWiseOr (value1, value2)    |按照 value1 和 value2 的类型返回 value1 和 value2 的位或结果。
BitWiseXor (value1, value2)   |按照 value1 和 value2 的类型返回 value1 和 value2 的位异或结果。

***1） BitWiseAnd***

**参数**

整数类型

**示例**

```
BitWiseAnd(1,3)
```

***2） BitWiseNot***

**参数**

整数类型

**示例**

```
BitWiseNot(1)
```

***3） BitWiseOr***

**参数**

整数类型

***4） BitWiseXor*** 

**参数**

整数类型

**示例**

```
BitWiseXor(1,3)
```

#### 6. 其它函数
函数                         |说明
-----------------------------|------------------------------
NewGuid ()                   |生产一个 Guid 值

***1） NewGuid***
**示例**
```cs
var esql = "select newguid() as Guid, e.FirstName from Employees as e";
var q = db.CreateQuery<IDataRecord>(esql);

foreach (var item in q)
    Console.WriteLine("{0} {1}", item[0], item[1]);
```

### 三. 比较运算符

运算符                        |说明
-----------------------------|-----------------------------
=（等于）                     |比较两个表达式是否相等。
>（大于）                     |比较两个表达式以确定左侧表达式的值是否大于右侧表达式的值。
>=（大于或等于）               |比较两个表达式以确定左侧表达式的值是否大于或等于右侧表达式的值。
IS [NOT] NULL                |确定查询表达式是否为 null。
<（小于）                     |比较两个表达式以确定左侧表达式的值是否小于右侧表达式的值。
<=（小于或等于）               |比较两个表达式以确定左侧表达式的值是否小于或等于右侧表达式的值。
[NOT] BETWEEN                |确定表达式的结果值是否在指定范围内。
!=（不等于）                  |比较两个表达式以确定左侧表达式是否不等于右侧表达式。
[NOT] LIKE                   |确定特定字符串是否与指定的模式匹配。

***1) =（等于）***

比较两个表达式是否相等。

语法

```
expression=expression

or 

expression==expression
```

**参数**

expression

任何有效表达式。 两个表达式都必须具有可隐式转换的数据类型。

**结果类型**

如果左侧表达式等于右侧表达式，则为 true；否则为 false。

**注释**

== 运算符等效于 =。

***2) >（大于）***

比较两个表达式以确定左侧表达式的值是否大于右侧表达式的值。

**语法**
```
expression > expression
```

**参数**

expression

任何有效表达式。 两个表达式都必须具有可隐式转换的数据类型。

**结果类型**

如果左侧表达式的值大于右侧表达式的值，则为 true；否则为 false。

***3) >=（大于或等于）***

比较两个表达式以确定左侧表达式的值是否大于或等于右侧表达式的值。

**语法**
```
expression>=expression
```

**参数**

expression

任何有效表达式。 两个表达式都必须具有可隐式转换的数据类型。

**结果类型**

如果左侧表达式的值大于或等于右侧表达式的值，则为 true；否则为 false。

***4) IS [NOT] NULL***

确定查询表达式是否为 null。

**语法**

```
expression IS [ NOT ] NULL
```

**参数**

expression

任何有效的查询表达式。 不可以是集合，不可含有集合成员，也不可以是具有集合类型属性的记录类型。

*NOT*

对 IS NULL 的 EDM.Boolean 结果取反。

**返回值**

如果 expression 返回 null，则为 true；否则为 false。

**注释**
使用 IS NULL 可确定外部联接的元素是否为 null：
```sql
select c 
from Customers as c 
     left join Orders as o on c.CustomerID = o.CustomerID  
where o is not null
```

使用 IS NULL 可确定成员是否有实际值：
```sql
select p from Products as p where p.Category not is null
```

***5) <（小于）***

比较两个表达式以确定左侧表达式的值是否小于右侧表达式的值。

**语法**
```
expression<expression
```

**参数**

expression

任何有效表达式。 两个表达式都必须具有可隐式转换的数据类型。

**结果类型**

如果左侧表达式的值小于右侧表达式的值，则为 true；否则为 false。

***6) <=（小于或等于）***

比较两个表达式以确定左侧表达式的值是否小于或等于右侧表达式的值。

**语法**
```
expression<=expression
```

**参数**

expression

任何有效表达式。 两个表达式都必须具有可隐式转换的数据类型。

**结果类型**

如果左侧表达式的值小于或等于右侧表达式的值，则为 true；否则为 false。

**7) [NOT] BETWEEN**

确定表达式的结果值是否在指定范围内。

**语法**
```
expression [ NOT ] BETWEEN begin_expression AND end_expression
```

**参数**

expression：要测试是否在 begin_expression 和 end_expression 所定义的范围内的任何有效表达式。begin_expression：任何有效表达式。 

end_expression：任何有效表达式。 

*NOT*：指定对 BETWEEN 的结果取反。

*AND*：用作一个占位符，指示 expression 应该处于由 begin_expression 和 end_expression 指定的范围内。

**返回值**

如果 expression 处于由 begin_expression 和 end_expression 指定的范围内，则为 true；否则为 false。 	

**注释**

若要指定某个排除范围，请使用大于 (>) 和小于 (<) 运算符而不要使用 BETWEEN。

**示例**
```sql
select p from Products as p where p.ProductId Between 10 and 100
```
```sql
select p from Products as p where p.ProductId not between 10 and 100
```

***8) !=（不等于）***

比较两个表达式以确定左侧表达式是否不等于右侧表达式。 !=（不等于）运算符在功能上等效于 <> 运算符。

**语法**
```
expression!=expression
or
expression <> expression
```

**参数**

expression：任何有效表达式。 两个表达式都必须具有可隐式转换的数据类型。

**结果类型**

如果左侧表达式不等于右侧表达式，则为 true；否则为 false。

***9) [NOT] LIKE***

确定特定字符 String 是否与指定模式相匹配。

**语法**
```
match [NOT] LIKE pattern [ESCAPE escape]
```

**参数**

match：计算结果为 String 的 实体 SQL 表达式。

pattern：要与指定 String 匹配的模式。

escape：一个转义符。

*NOT*：指定对 LIKE 的结果取反。

**返回值**

如果 string 与模式相匹配，则为 true；否则为 false。


### 四. 逻辑和 Case 表达式运算符

运算符                 |说明
----------------------|-----------------------
&&                    |逻辑与。
！                    |逻辑非。
||                    |逻辑或。
CASE                  |求出一组布尔表达式的值以确定结果。
THEN                  |当 WHEN 子句取值为 true 时的结果。

***1) &&(AND)***

如果两个表达式均为 true，则返回 true；否则，返回 false 或 NULL。

**语法**
```
boolean_expression AND boolean_expression
or
boolean_expression && boolean_expression 
```

**参数**

boolean_expression: 布尔值的任何有效表达式。

**注意**

符号“&&”与 AND 运算符具有相同的功能。

***2) ！(NOT)***

对 Boolean 表达式求反。

**语法**
```
NOT boolean_expression
or
! boolean_expressionboolean_expression
```

**参数**

布尔值的任何有效表达式。

**注意**

符号“!”与NOT运算符具有相同的功能。

***3) ||(OR)***

组合两个 Boolean 表达式。

**语法**
```
boolean_expression OR boolean_expression
or 
boolean_expression || boolean_expression
```

**参数**
boolean_expression: 布尔值的任何有效表达式

**注意**

OR 是 实体 SQL 逻辑运算符。它用于组合两个条件。在一个语句中使用多个逻辑运算符时，首先计算 AND 运算符，然后计算 OR 运算符。不过，使用括号可以更改求值的顺序。

双竖线 (||) 与 OR 运算符具有相同的功能。

***4) CASE***

求出一组 Boolean 表达式的值以确定结果。 

**语法**

```
CASE
     WHEN Boolean_expression THEN result_expression 
    [ ...n ] 
     [ 
    ELSE else_result_expression 
     ] 
END
```

**参数**
n：一个占位符，表明可以使用多个 WHEN Boolean_expression THEN result_expression 子句。

THEN result_expression：作为在 Boolean_expression 的计算结果为 true 时返回的表达式。result expression 是任何有效的表达式。

ELSE else_result_expression：比较运算的结果都不为 true 时返回的表达式。如果忽略此参数且比较运算计算的结果不为 true，CASE 将返回空值。else_result_expression 是任何有效的表达式。else_result_expression 及任何 result_expression 的数据类型必须相同或必须是隐式转换的数据类型。

WHEN Boolean_expression：使用 CASE 搜索格式时所计算的 Boolean 表达式。Boolean_expression 是任何有效的 Boolean 表达式。 

**返回值**

从 result_expression 和可选 else_result_expression 的类型集中返回优先级最高的类型。

**示例**

```sql
select value 
  (case 
       		when e.Country = 'Chinese' then 'CN',
             when e.Country = 'English' then 'EN',
             when e.Country = 'HongKong' then 'HK',
             else 'other'
       end)
from Employees as e
```

5) THEN

当 WHEN 子句取值为 true 时的结果。 

**语法**
```
WHEN when_expression THEN then_expression
```

**参数**

when_expression：任何有效的 Boolean 表达式。

then_expression：任何返回集合的有效查询表达式。

**注意**

如果 when_expression 取值为 true，则结果为对应的 then-expression。如果任何 WHEN 条件均未得以满足，将求出 else-expression 的值。然而，如果没有 else-expression，则结果为空值。 


### 五. 查询运算符

### 六. 引用运算符

### 七. 集合运算符

### 八. 类型运算符

### 九. 其他运算符

### 十. 扩展查询方法




