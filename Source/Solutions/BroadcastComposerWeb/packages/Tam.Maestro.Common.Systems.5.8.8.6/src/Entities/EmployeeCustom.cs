namespace Tam.Maestro.Data.Entities
{
    public partial class Employee
    {
        public string FullName
        {
            get { return this.Firstname + " " + this.Lastname; }
        }
    }
}
