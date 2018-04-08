## 如何：重用 ADO.NET 命令和 DataContext 之间的连接

ALinq 是基于 ADO.NET 的，因此可以重用 ADO.NET 命令和 DataContext 之间的连接

```cs
string connString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=c:\northwind.mdf;
    Integrated Security=True; Connect Timeout=30; User Instance=True";
SqlConnection nwindConn = new SqlConnection(connString);
nwindConn.Open();

Northwnd interop_db = new Northwnd(nwindConn);

SqlTransaction nwindTxn = nwindConn.BeginTransaction();

try
{
    SqlCommand cmd = new SqlCommand(
        "UPDATE Products SET QuantityPerUnit = 'single item' WHERE ProductID = 3");
    cmd.Connection = nwindConn;
    cmd.Transaction = nwindTxn;
    cmd.ExecuteNonQuery();

    interop_db.Transaction = nwindTxn;

    Product prod1 = interop_db.Products
        .First(p => p.ProductID == 4);
    Product prod2 = interop_db.Products
        .First(p => p.ProductID == 5);
    prod1.UnitsInStock -= 3;
    prod2.UnitsInStock -= 5;

    interop_db.SubmitChanges();

    nwindTxn.Commit();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine("Error submitting changes... all changes rolled back.");
}

nwindConn.Close();
```