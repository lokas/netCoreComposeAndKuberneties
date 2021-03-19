namespace DemoApi.Domain
{
    public class PersonalDetailsChanged : Event
    {
        public PersonalDetailsChanged(string name, string lastName)
        {
            Name = name;
            Last = lastName;
        }
        public readonly string Name;
        public readonly string Last;
    }
}