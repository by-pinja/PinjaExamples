namespace Pinja.Examples.Events
{
    [Topic("User")]
    public class UserEvent : Event
    {
        public string UserId { get; set; }
    }
}
