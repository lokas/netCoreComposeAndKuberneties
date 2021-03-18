using System;

namespace DemoApi.Domain
{
    public class TypeA
    {
        public Guid Id { get; set; }
        public string PhoneInfo { get; set; }

        public static TypeA Create
        {
            get
            {
                Guid g = Guid.NewGuid();
                return new TypeA
                {
                    Id = g,
                    PhoneInfo = $"PhoneInfo_{g}"
                };
            }
        }

        public const string StreamName = nameof(TypeA) + "_Aggregate";
    }
}