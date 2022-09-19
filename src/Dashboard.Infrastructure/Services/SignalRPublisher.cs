using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dashboard.Core.Model;
using Dashboard.Core.Services;

namespace Dashboard.Infrastructure.Services
{
    public class SignalRMessagePublisher : IMessagePublisher
    {
        private readonly Channel<KafkaMessage> _channel;
        public SignalRMessagePublisher(Channel<KafkaMessage> channel)
        {
            _channel = channel;
        }

        public async Task Publish(KafkaMessage message, CancellationToken cancellationToken)
        {
            await _channel.Writer.WriteAsync(message, cancellationToken);
        }
    }
}