using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dashboard.Core.Model;
using Dashboard.Infrastructure.Services;
using Moq;
using Xunit;

namespace Dashboard.Infrastructure.Tests.Services
{
    public class SignalRMessagePublisherTests
    {
        [Fact]
        public async Task TestPublish()
        {
            var msg = new KafkaMessage(Guid.Empty, "", "");
            var ch = Channel.CreateBounded<KafkaMessage>(1);
            var publisher = new SignalRMessagePublisher(ch);
            await publisher.Publish(msg, CancellationToken.None);
            Assert.Equal(1, ch.Reader.Count);
        }
    }
}