using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pinja.Examples.Events
{
    /// <summary>
    /// Contains mappings between topics and their implementing classes.
    /// </summary>
    public static class TopicMappings
    {
        private static readonly Type TopicAttributeType = typeof(TopicAttribute);
        private static readonly IReadOnlyDictionary<string, Type> TopicToType;
        private static readonly IReadOnlyDictionary<Type, string> TypeToTopic;

        private static readonly Type RootType = typeof(Event);

        /// <summary>
        /// Initializes static members of <see cref="TopicMappings"/>.
        /// </summary>
        static TopicMappings()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t != RootType && RootType.IsAssignableFrom(t));

            var topicToType = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            var typeToTopic = new Dictionary<Type, string>();

            foreach (var type in types)
            {
                var fullTopic = GetFullTopic(type);
                topicToType[fullTopic] = type;
                typeToTopic[type] = fullTopic;
            }

            TopicToType = topicToType;
            TypeToTopic = typeToTopic;
        }

        /// <summary>
        /// Gets the closest known type for the specified topic.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <returns>The closest known type.</returns>
        /// <exception cref="ArgumentException">Thrown if the topic is null, empty or only whitespaces.</exception>
        public static Type GetClosestType(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("Topic must not be empty.", nameof(topic));
            }

            Type type;
            while (!TopicToType.TryGetValue(topic, out type))
            {
                var separatorIndex = topic.LastIndexOf('.');
                if (separatorIndex == -1)
                {
                    return RootType;
                }

                topic = topic.Substring(0, separatorIndex);
            }

            return type;
        }

        /// <summary>
        /// Gets the base topic of the implementing type.
        /// </summary>
        /// <param name="type">The of the event class.</param>
        /// <returns>The base topic of the type.</returns>
        public static string GetTopic(Type type)
        {
            if (TypeToTopic.TryGetValue(type, out var topic))
            {
                return topic;
            }

            return  null;
        }

        private static string GetFullTopic(Type type)
        {
            var tokens = new List<string>();
            while (type != null)
            {
                var topicAttribute = (TopicAttribute) type.GetCustomAttribute(TopicAttributeType, false);
                if (topicAttribute == null)
                {
                    type = type.BaseType;
                    continue;
                }

                tokens.Insert(0, topicAttribute.TopicToken);
                type = type.BaseType;
            }

            return string.Join('.', tokens);
        }
    }
}
