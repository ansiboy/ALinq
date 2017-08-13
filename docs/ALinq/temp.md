
```cs
[Provider(typeof(ALinq.SQLite.SQLiteProvider))]
public class Northwind : DataContext
{
    static string dbPath = "Northwind.db3";

    public Northwind() : base(dbPath)
    {
    }

    public Table<Category> Categories
    {
        get
        {
            return this.GetTable<Category>();
        }
    }
}
```