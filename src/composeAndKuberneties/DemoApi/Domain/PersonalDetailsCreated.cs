using System;

namespace DemoApi.Domain
{
    public class PersonalDetailsCreated : Event
    {
        public string Name { get; }
        public string Last { get; }

        public PersonalDetailsCreated(string name, string last,Guid aggregateId) : base(aggregateId)
        {
            Name = name;
            Last = last;
        }
    }
}