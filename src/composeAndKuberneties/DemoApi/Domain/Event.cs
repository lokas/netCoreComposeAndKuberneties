using System;

namespace DemoApi.Domain
{
    public abstract class Event
    {
        protected Event()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
    }
}