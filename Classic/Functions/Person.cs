namespace Functions
{
    public class Person
    {
        public Name Name { get; set; }
        public Address Address { get; set; }
    }

    public class Name
    {
        public string First { get; set; }
        public string Last { get; set; }
        public string Title { get; set; }
    }

    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }
}
