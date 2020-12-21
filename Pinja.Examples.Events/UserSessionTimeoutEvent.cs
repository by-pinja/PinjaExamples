using System;

namespace Pinja.Examples.Events
{
    [Topic("Timeout")]
    public class UserSessionTimeoutEvent : UserSessionEvent
    {
        public TimeSpan Timeout { get; set; }
    }
}
