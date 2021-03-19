namespace DemoApi.Domain
{
    public class PersonalDetailsChanged : Event
    {
        public string Name { get; }
        public string Last { get; }

        public PersonalDetailsChanged(string name, string last)
        {
            Name = name;
            Last = last;
        }
    }
}