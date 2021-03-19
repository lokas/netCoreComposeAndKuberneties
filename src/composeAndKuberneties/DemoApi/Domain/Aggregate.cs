using System;
using System.Collections.Generic;

namespace DemoApi.Domain
{
    public class Aggregate
    {
        private readonly AggregateState _state;

        private List<Event> _events = new List<Event>();

        public Aggregate(Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            _state = new AggregateState
            {
                PhoneInfo = String.Empty,
                Name = string.Empty,
                LastName = string.Empty
            };
        }

        //TODO: how to pass version for expected version

        public Aggregate(Guid id, AggregateState state)
        {
            Id = id;
            _state = state;
        }

        public Guid Id { get; }

        public void AddPhoneInfo(string phoneInfo)
        {
            if (phoneInfo.Equals(_state.PhoneInfo))
                return;

            if (string.IsNullOrEmpty(_state.PhoneInfo))
                _events.Add(new PhoneInfoCreated(phoneInfo));
            else
                _events.Add(new PhoneInfoChanged(phoneInfo));

            _state.PhoneInfo = phoneInfo;
        }

        public void AddPersonalDetails(string name, string lastName)
        {
            if (_state.LastName.Equals(name) && _state.Name.Equals(name))
                return;

            if (string.IsNullOrEmpty(_state.Name))
                _events.Add(new PersonalDetailsCreated(name, lastName));
            else
                _events.Add(new PersonalDetailsChanged(name, lastName));

            _state.LastName = lastName;
            _state.Name = name;
        }

        public void Internals(Action<IReadOnlyList<Event>> getEvents)
        {
            getEvents(_events);
        }
    }
}
