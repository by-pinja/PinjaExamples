using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Pinja.Examples.Messages;

namespace Pinja.Examples.Extensibility.UnitTests
{
    [TestFixture]
    public class MessageSerializationUnitTests
    {
        private JsonSerializerOptions _allKnownSerializerOptions;
        private JsonSerializerOptions _limitedSerializerOptions;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _allKnownSerializerOptions = new JsonSerializerOptions();
            _allKnownSerializerOptions.Converters.Add(new ConstantInvariantTimeSpanJsonConverter());
            _allKnownSerializerOptions.Converters.Add(new ExtensibleJsonConverterFactory(new ITypeMappings[]
                {
                    new DefaultTypeMappings<Message>(new[] { typeof(Message).Assembly }, TypeDescriptorPolicy.Default),
                    new DefaultTypeMappings<VoteEntry>(new[] { typeof(VoteEntry).Assembly, typeof(DetailedVoteEntry).Assembly }, TypeDescriptorPolicy.Default)
                },
                TypeDescriptorPolicy.Default));

            _limitedSerializerOptions = new JsonSerializerOptions();
            _limitedSerializerOptions.Converters.Add(new ConstantInvariantTimeSpanJsonConverter());
            _limitedSerializerOptions.Converters.Add(new ExtensibleJsonConverterFactory(new ITypeMappings[]
                {
                    new DefaultTypeMappings<Message>(new[] { typeof(Message).Assembly }, TypeDescriptorPolicy.Default),
                    new DefaultTypeMappings<VoteEntry>(new[] { typeof(VoteEntry).Assembly }, TypeDescriptorPolicy.Default)
                },
                TypeDescriptorPolicy.Default));
        }

        [Test]
        public void Serialize_Deserialize_MessagePassesViaLimitedOptions_DetailedInformationIsPreserved()
        {
            // Arrange
            var vote = new Vote
            {
                Subject = "Vote Trump out of office",
                RequiredVotes = 3,
                VoteId = 1
            };

            // Act & Assert
            var serializedVote = JsonSerializer.Serialize(vote, _allKnownSerializerOptions);

            var deserializedAsMessage = JsonSerializer.Deserialize<Message>(serializedVote, _allKnownSerializerOptions);

            deserializedAsMessage.Should()
                                 .BeEquivalentTo(new Vote
                                     {
                                         Subject = "Vote Trump out of office",
                                         RequiredVotes = 3,
                                         VoteId = 1,
                                         ExtensionData = new ExtensionData
                                         {
                                             TypeDescriptor = $"{typeof(Message).FullName}:{typeof(Vote).FullName}"
                                         }
                                     },
                                     options => options.RespectingRuntimeTypes(),
                                     "type information should be preserved");

            var deserializedVote = (Vote)deserializedAsMessage;

            deserializedVote.VoteEntries.Add(new DetailedVoteEntry
            {
                Result = "Yes",
                Voter = "Detailed voter",
                Reason = "You don't even need a reason"
            });

            var reSerialized = JsonSerializer.Serialize(deserializedAsMessage, _allKnownSerializerOptions);

            // Pass via limited options to add a vote entry

            var deserializedLimited = JsonSerializer.Deserialize<Vote>(reSerialized, _limitedSerializerOptions);

            deserializedLimited.Should()
                               .BeEquivalentTo(new Vote
                                   {
                                       Subject = "Vote Trump out of office",
                                       RequiredVotes = 3,
                                       VoteId = 1,
                                       VoteEntries =
                                       {
                                           new VoteEntry
                                           {
                                               Result = "Yes",
                                               Voter = "Detailed voter",
                                               ExtensionData = new ExtensionData
                                               {
                                                   TypeDescriptor = $"{typeof(VoteEntry).FullName}:{typeof(DetailedVoteEntry).FullName}",
                                                   ExtensionMembers =
                                                   {
                                                       [0] = new ExtensionMember
                                                       {
                                                           Index = 0,
                                                           Name = "Reason",
                                                           Value = "You don't even need a reason"
                                                       }
                                                   }
                                               }
                                           }
                                       },
                                       ExtensionData = new ExtensionData
                                       {
                                           TypeDescriptor = $"{typeof(Message).FullName}:{typeof(Vote).FullName}"
                                       }
                                   },
                                   options => options.RespectingRuntimeTypes(),
                                   "type information should be preserved");

            deserializedLimited.VoteEntries.Add(new VoteEntry
            {
                Result = "Hell yes!",
                Voter = "Enthusiastic voter"
            });

            var reSerializedLimited = JsonSerializer.Serialize(deserializedLimited, _limitedSerializerOptions);

            // Finally deserialize with full knowledge

            var finalDeserialized = JsonSerializer.Deserialize<Vote>(reSerializedLimited, _allKnownSerializerOptions);

            finalDeserialized.Should()
                             .BeEquivalentTo(new Vote
                                 {
                                     Subject = "Vote Trump out of office",
                                     RequiredVotes = 3,
                                     VoteId = 1,
                                     VoteEntries =
                                     {
                                         new DetailedVoteEntry
                                         {
                                             Result = "Yes",
                                             Voter = "Detailed voter",
                                             Reason = "You don't even need a reason",
                                             ExtensionData = new ExtensionData
                                             {
                                                 TypeDescriptor = $"{typeof(VoteEntry).FullName}:{typeof(DetailedVoteEntry).FullName}"
                                             }
                                         },
                                         new VoteEntry
                                         {
                                             Result = "Hell yes!",
                                             Voter = "Enthusiastic voter",
                                             ExtensionData = new ExtensionData
                                             {
                                                 TypeDescriptor = $"{typeof(VoteEntry).FullName}"
                                             }
                                         }
                                     },
                                     ExtensionData = new ExtensionData
                                     {
                                         TypeDescriptor = $"{typeof(Message).FullName}:{typeof(Vote).FullName}"
                                     }
                                 },
                                 options => options.RespectingRuntimeTypes(),
                                 "type information should be preserved");
        }
    }
}
