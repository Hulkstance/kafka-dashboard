using System;
using System.Threading;
using System.Threading.Tasks;
using Dashboard.Core.Services;

namespace Dashboard.Core.UseCases
{
    public sealed record SubscribeKafkaTopicsAndPublishItCommand(string[] Topics);


    public sealed class SubscribeKafkaTopicsAndPublishItUseCase
    {
        private readonly IKafkaTopicSubscriber _kafkaTopicSubscriber;
        private readonly IMessagePublisher _messagePublisher;

        public SubscribeKafkaTopicsAndPublishItUseCase(IKafkaTopicSubscriber kafkaTopicSubscriber,
            IMessagePublisher messagePublisher)
        {
            _kafkaTopicSubscriber = kafkaTopicSubscriber;
            _messagePublisher = messagePublisher;
        }

        public async Task Execute(SubscribeKafkaTopicsAndPublishItCommand command,
            CancellationToken cancellationToken = default)
        {
            if (command.Topics.Length == 0)
            {
                return;
            }
            
            await foreach (var msg in _kafkaTopicSubscriber.Subscribe(command.Topics, cancellationToken))
            {
                await _messagePublisher.Publish(msg, cancellationToken);
            }
        }
    }
}