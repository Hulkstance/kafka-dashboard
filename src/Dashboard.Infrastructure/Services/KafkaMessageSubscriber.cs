using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dashboard.Core.Model;
using Dashboard.Core.Services;
using Microsoft.Extensions.Logging;

namespace Dashboard.Infrastructure.Services
{
    public sealed class ConfluentKafkaTopicSubscriber : IKafkaTopicSubscriber, IDisposable
    {
        private readonly IConsumer<Ignore, string> _consumer;

        public ConfluentKafkaTopicSubscriber(IConsumer<Ignore, string> consumer)
        {
            _consumer = consumer;
        }

        public IAsyncEnumerable<KafkaMessage> Subscribe(string[] topics, CancellationToken cancellationToken)
        {
            var ch = Channel.CreateBounded<ConsumeResult<Ignore, string>>(1);
            var task = Consume(topics, ch.Writer, cancellationToken);
            return ch.Reader.ReadAllAsync(cancellationToken)
                .Select(msg => new KafkaMessage(Guid.NewGuid(), msg.Topic, msg.Message.Value));
        }

        private async Task Consume(IEnumerable<string> topics, ChannelWriter<ConsumeResult<Ignore, string>> writer,
            CancellationToken cancellationToken)
        {
            _consumer.Subscribe(topics);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _consumer.Consume(cancellationToken);
                    await writer.WriteAsync(cr, cancellationToken);
                }
                catch (Exception e)
                {
                    writer.Complete(e);
                    break;
                }
            }

            writer.Complete();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _consumer.Close(); // Commit offsets and leave the group cleanly.
                _consumer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}