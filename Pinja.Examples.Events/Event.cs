using System;
using Pinja.Examples.Extensibility;

namespace Pinja.Examples.Events
{
    /// <summary>
    /// Base class for all events
    /// </summary>
    [Serializable]
    public class Event : IExtensible
    {
        private string _topic;

        /// <inheritdoc />
        public ExtensionData ExtensionData { get; set; }

        /// <summary>
        /// The topic of the event.
        /// </summary>
        public string Topic
        {
            get => _topic ??= AppendTopic(TopicMappings.GetTopic(GetType()));
            set => _topic = value;
        }

        /// <summary>
        /// The time when the event occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Appends leaf level token(s) to the topic.
        /// </summary>
        /// <param name="baseTopic">The base topic of the event.</param>
        /// <returns>The appended topic.</returns>
        /// <remarks>Only leaf level topics are allowed to append the topic.</remarks>
        protected virtual string AppendTopic(string baseTopic)
        {
            return baseTopic;
        }
    }
}
