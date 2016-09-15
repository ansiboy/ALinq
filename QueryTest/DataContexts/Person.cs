namespace NorthwindDemo
{
    public class Person
    {
        public Person(int employeeId)
        {
            this.EmployeeId = employeeId;
        }

        protected int EmployeeId { get; set; }

        public Person()
        {
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}