using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dashboard.Infrastructure.Services;
using Moq;
using Xunit;

namespace Dashboard.Infrastructure.Tests.Services
{
    public class ConfluentKafkaTopicSubscriberTests
    {
        [Fact]
        public async Task TestWhenConsumerReturnData()
        {
            var consumer = new Mock<IConsumer<Ignore, string>>();
            consumer.Setup(x => x.Subscribe(It.IsAny<IEnumerable<string>>()));
            consumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>((cancellationToken) => new ConsumeResult<Ignore, string>()
                    {Topic = "xD", Message = new Message<Ignore, string>() {Value = "xD"}});
            var subscriber = new ConfluentKafkaTopicSubscriber(consumer.Object);
            var source = new CancellationTokenSource();

            var result = await subscriber.Subscribe(new[] {"xD"}, source.Token).Take(10).ToListAsync();

            consumer.Verify(x => x.Subscribe(It.IsAny<IEnumerable<string>>()), Times.Once);
            consumer.Verify(x => x.Consume(It.IsAny<CancellationToken>()), Times.AtLeast(10));
        }

        [Fact]
        public async Task TestWhenConsumerThrows()
        {
            var consumer = new Mock<IConsumer<Ignore, string>>();
            consumer.Setup(x => x.Subscribe(It.IsAny<IEnumerable<string>>()));
            consumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>((cancellationToken) => throw new Exception("xD"));
            var subscriber = new ConfluentKafkaTopicSubscriber(consumer.Object);
            var source = new CancellationTokenSource();

            await Assert.ThrowsAsync<Exception>(async () =>
                await subscriber.Subscribe(new[] {"xD"}, source.Token).Take(10).ToListAsync());

            consumer.Verify(x => x.Subscribe(It.IsAny<IEnumerable<string>>()), Times.Once);
            consumer.Verify(x => x.Consume(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}