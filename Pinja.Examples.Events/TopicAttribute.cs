using System;

namespace Pinja.Examples.Events
{
    /// <summary>
    /// An attribute to specify a topic token for an event class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TopicAttribute : Attribute
    {
        /// <summary>
        /// The topic token.
        /// </summary>
        public string TopicToken { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="TopicAttribute"/>.
        /// </summary>
        /// <param name="topicToken">The topic token.</param>
        public TopicAttribute(string topicToken)
        {
            TopicToken = topicToken;
        }
    }
}
