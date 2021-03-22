using System;

namespace DemoApi.Domain
{
    public abstract class Event
    {
        public Guid AggregateId { get; }

        protected Event(Guid aggregateId)
        {
            AggregateId = aggregateId;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
    }
}