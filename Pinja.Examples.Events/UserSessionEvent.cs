using System;

namespace Pinja.Examples.Events
{
    [Topic("Session")]
    public class UserSessionEvent : UserEvent
    {
        public Guid SessionId { get; set; }
    }
}
