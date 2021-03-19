namespace DemoApi.Domain
{
    public class PhoneInfoChanged : Event
    {
        public readonly string PhoneInfo;

        public PhoneInfoChanged(string phoneInfo)
        {
            PhoneInfo = phoneInfo;
        }   
    }
}