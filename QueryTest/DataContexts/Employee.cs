namespace NorthwindDemo
{
    public partial class Employee : IEmployee
    {
        public string Full()
        {
            return this.FirstName + " " + this.LastName;
        }
    }
}