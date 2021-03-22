using System;

namespace DemoApi.Domain
{
    public class PhoneInfoChanged : Event
    {
        public PhoneInfoChanged(string phoneInfo, Guid aggregateId) : base(aggregateId)
        {
            PhoneInfo = phoneInfo;
        }

        public string PhoneInfo { get; private set; }
    }
}