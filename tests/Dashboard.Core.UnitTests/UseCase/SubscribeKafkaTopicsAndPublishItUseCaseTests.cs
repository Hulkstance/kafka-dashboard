using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dashboard.Core.Model;
using Dashboard.Core.Services;
using Dashboard.Core.UseCases;
using Moq;
using Xunit;

namespace Dashboard.Core.UnitTests.UseCase
{
    public class SubscribeKafkaTopicsAndPublishItUseCaseTests
    {
        [Fact]
        public async Task TestPublishWhenMessageStreamIsEmpty()
        {
            var kafkaTopicSubscriberMock = new Mock<IKafkaTopicSubscriber>();
            kafkaTopicSubscriberMock.Setup(subscriber =>
                    subscriber.Subscribe(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .Returns<string[], CancellationToken>((_, _) => Enumerable.Empty<KafkaMessage>().ToAsyncEnumerable());

            var messagePublisherMock = new Mock<IMessagePublisher>();

            var useCase =
                new SubscribeKafkaTopicsAndPublishItUseCase(kafkaTopicSubscriberMock.Object,
                    messagePublisherMock.Object);

            await useCase.Execute(new SubscribeKafkaTopicsAndPublishItCommand(new[] {"xD"}));
            kafkaTopicSubscriberMock.Verify(x => x.Subscribe(It.IsAny<string[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
            messagePublisherMock.Verify(x => x.Publish(It.IsAny<KafkaMessage>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        
        [Fact]
        public async Task TestPublishWhenListOfTopicsIsEmpty()
        {
            var kafkaTopicSubscriberMock = new Mock<IKafkaTopicSubscriber>();
            kafkaTopicSubscriberMock.Setup(subscriber =>
                    subscriber.Subscribe(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .Returns<string[], CancellationToken>((_, _) => Enumerable.Empty<KafkaMessage>().ToAsyncEnumerable());

            var messagePublisherMock = new Mock<IMessagePublisher>();

            var useCase =
                new SubscribeKafkaTopicsAndPublishItUseCase(kafkaTopicSubscriberMock.Object,
                    messagePublisherMock.Object);

            await useCase.Execute(new SubscribeKafkaTopicsAndPublishItCommand(Array.Empty<string>()));
            
            kafkaTopicSubscriberMock.Verify(x => x.Subscribe(It.IsAny<string[]>(), It.IsAny<CancellationToken>()),
                Times.Never);
            messagePublisherMock.Verify(x => x.Publish(It.IsAny<KafkaMessage>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestPublishWhenMessageStreamHasElements()
        {
            const int elements = 10;
            var kafkaTopicSubscriberMock = new Mock<IKafkaTopicSubscriber>();
            kafkaTopicSubscriberMock.Setup(subscriber =>
                    subscriber.Subscribe(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .Returns<string[], CancellationToken>((topics, _) =>
                    Enumerable.Range(0, elements).Select(x => new KafkaMessage(Guid.NewGuid(), topics[0], "xD"))
                        .ToAsyncEnumerable());

            var messagePublisherMock = new Mock<IMessagePublisher>();

            var useCase =
                new SubscribeKafkaTopicsAndPublishItUseCase(kafkaTopicSubscriberMock.Object,
                    messagePublisherMock.Object);

            await useCase.Execute(new SubscribeKafkaTopicsAndPublishItCommand(new[] {"xD"}));
            
            kafkaTopicSubscriberMock.Verify(x => x.Subscribe(It.IsAny<string[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
            
            messagePublisherMock.Verify(x => x.Publish(It.IsAny<KafkaMessage>(), It.IsAny<CancellationToken>()),
                Times.Exactly(elements));
        }
        
        [Fact]
        public async Task TestPublishWhenSubscriberThrowsException()
        {
            var kafkaTopicSubscriberMock = new Mock<IKafkaTopicSubscriber>();
            kafkaTopicSubscriberMock.Setup(subscriber =>
                    subscriber.Subscribe(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .Returns<string[], CancellationToken>((topics, _) => throw new Exception("xD"));

            var messagePublisherMock = new Mock<IMessagePublisher>();

            var useCase =
                new SubscribeKafkaTopicsAndPublishItUseCase(kafkaTopicSubscriberMock.Object,
                    messagePublisherMock.Object);

            await Assert.ThrowsAsync<Exception>(async () => await useCase.Execute(new SubscribeKafkaTopicsAndPublishItCommand(new[] {"xD"})));
            
            kafkaTopicSubscriberMock.Verify(x => x.Subscribe(It.IsAny<string[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
            
            messagePublisherMock.Verify(x => x.Publish(It.IsAny<KafkaMessage>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}