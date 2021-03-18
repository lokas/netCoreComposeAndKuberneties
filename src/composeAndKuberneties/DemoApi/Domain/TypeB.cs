using System;

namespace DemoApi.Domain
{
    public class TypeB
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Last { get; set; }

        public const string StreamName = nameof(TypeB) + "_Aggregate";

        public static TypeB Create
        {
            get
            {
                Guid g = Guid.NewGuid();
                return new TypeB
                {
                    Id = g,
                    Name = $"Name_{g}",
                    Last = $"Last_{g}"
                };
            }
        }
    }
}