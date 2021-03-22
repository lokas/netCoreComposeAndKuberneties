using System;

namespace DemoApi.Domain
{
    public class PhoneInfoCreated : Event
    {
        public PhoneInfoCreated(string phoneInfo, Guid aggregateId) : base(aggregateId)
        {
            PhoneInfo = phoneInfo;
        }

        public string PhoneInfo { get; private set; }
     }
}