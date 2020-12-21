namespace Pinja.Examples.Events
{
    [Topic("LogOn")]
    public class UserSessionLogOnEvent : UserSessionEvent
    {

        public string SourceId { get; set; }
    }
}
