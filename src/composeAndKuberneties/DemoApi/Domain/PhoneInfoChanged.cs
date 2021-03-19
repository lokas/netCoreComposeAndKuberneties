namespace DemoApi.Domain
{
    public class PhoneInfoChanged : Event
    {
        public PhoneInfoChanged(string phoneInfo)
        {
            PhoneInfo = phoneInfo;
        }

        public string PhoneInfo { get; private set; }
    }
}