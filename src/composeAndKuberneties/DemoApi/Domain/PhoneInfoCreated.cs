using Newtonsoft.Json;

namespace DemoApi.Domain
{
    public class PhoneInfoCreated : Event
    {
        public PhoneInfoCreated(string phoneInfo)  
        {
            PhoneInfo = phoneInfo;
        }

        public string PhoneInfo { get; private set; }
     }
}