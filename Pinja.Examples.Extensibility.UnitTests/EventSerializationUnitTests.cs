using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Pinja.Examples.Events;

namespace Pinja.Examples.Extensibility.UnitTests
{
    [TestFixture]
    public class EventSerializationUnitTests
    {
        private JsonSerializerOptions _jsonSerializerOptions;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.RegisterExtensibleTypes(new EvenTypeDescriptorPolicy(), new Dictionary<Type, TypeDescriptorPolicy>(), new[] { typeof(Event).Assembly });
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _jsonSerializerOptions = serviceProvider.GetService<IOptions<JsonSerializerOptions>>().Value;
            _jsonSerializerOptions.Converters.Add(new ConstantInvariantTimeSpanJsonConverter());
        }

        [Test]
        public void Serialize_DeserializeAsRootType_TypeInformationIsPreserved()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;
            var e = new UserSessionTimeoutEvent
            {
                Timeout = TimeSpan.FromMinutes(15),
                UserId = "The magnificent user",
                Timestamp = timestamp,
                SessionId = sessionId
            };

            var expectedEvent = new UserSessionTimeoutEvent
            {
                Timeout = TimeSpan.FromMinutes(15),
                UserId = "The magnificent user",
                Timestamp = timestamp,
                SessionId = sessionId,
                ExtensionData = new ExtensionData() // Nothing unknown
            };

            // Pre-asert topic
            e.Topic.Should().Be("User.Session.Timeout", "topic should be automatically assigned from Topic attributes");

            // Act
            var serialized = JsonSerializer.Serialize(e, _jsonSerializerOptions);

            var deserialized = JsonSerializer.Deserialize<Event>(serialized, _jsonSerializerOptions);

            // Assert
            deserialized.Should().BeEquivalentTo(expectedEvent, options => options.RespectingRuntimeTypes(), "type information should be preserved when deserializing as root type");
        }

        [Test]
        public void Serialize_DeserializeAsRootType_UnknownSubTopicAndMember_TypeInformationIsPreserved_ExtensionDataIsPopulated()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;
            var serialized =
                $"{{\"Topic\":\"User.Session.Timeout.Monitored\",\"Monitor\":\"MonitoringSystem\",\"Timeout\":\"00:15:00\",\"SessionId\":\"{sessionId}\",\"UserId\":\"The magnificent user\",\"Timestamp\":\"{timestamp:o}\"}}";

            var expectedEvent = new UserSessionTimeoutEvent
            {
                Topic = "User.Session.Timeout.Monitored",
                Timeout = TimeSpan.FromMinutes(15),
                UserId = "The magnificent user",
                Timestamp = timestamp,
                SessionId = sessionId,
                ExtensionData = new ExtensionData
                {
                    TypeDescriptor = null, // Represented by Topic
                    ExtensionMembers =
                    {
                        [1] = new ExtensionMember
                        {
                            Index = 1,
                            Name = "Monitor",
                            Value = "MonitoringSystem"
                        }
                    }
                }
            };

            // Act
            var deserialized = JsonSerializer.Deserialize<Event>(serialized, _jsonSerializerOptions);

            var reSerialized = JsonSerializer.Serialize(deserialized, _jsonSerializerOptions);

            // Assert
            deserialized.Should().BeEquivalentTo(expectedEvent, options => options.RespectingRuntimeTypes(), "type information should be preserved when deserializing as root type");
            reSerialized.Should().BeEquivalentTo(serialized, "serialized data should be preserved when re-serializing");
        }

        [Test]
        public void Serialize_DeserializeAsClosestKnownType_UnknownSubTopicAndMember_TypeInformationIsPreserved_ExtensionDataIsPopulated()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var timestamp = new DateTime(2020,
                1,
                13,
                14,
                44,
                27,
                987,
                DateTimeKind.Utc);
            var serialized =
                $"{{\"Topic\":\"User.Session.Timeout.Monitored\",\"Monitor\":\"MonitoringSystem\",\"Timeout\":\"00:15:00\",\"SessionId\":\"{sessionId}\",\"UserId\":\"The magnificent user\",\"Timestamp\":\"{timestamp:yyyy-MM-ddTHH:mm:ss.FFFFFFFK}\"}}";

            var expectedEvent = new UserSessionTimeoutEvent
            {
                Topic = "User.Session.Timeout.Monitored",
                Timeout = TimeSpan.FromMinutes(15),
                UserId = "The magnificent user",
                Timestamp = timestamp,
                SessionId = sessionId,
                ExtensionData = new ExtensionData
                {
                    TypeDescriptor = null, // Represented by Topic
                    ExtensionMembers =
                    {
                        [1] = new ExtensionMember
                        {
                            Index = 1,
                            Name = "Monitor",
                            Value = "MonitoringSystem"
                        }
                    }
                }
            };

            // Act
            var deserialized = JsonSerializer.Deserialize<UserSessionTimeoutEvent>(serialized, _jsonSerializerOptions);

            var reSerialized = JsonSerializer.Serialize(deserialized, _jsonSerializerOptions);

            // Assert
            deserialized.Should().BeEquivalentTo(expectedEvent, options => options.RespectingRuntimeTypes(), "type information should be preserved when deserializing as root type");
            reSerialized.Should().BeEquivalentTo(serialized, "serialized data should be preserved when re-serializing");
        }
    }
}
