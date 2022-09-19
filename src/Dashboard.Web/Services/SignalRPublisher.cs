using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dashboard.Core.Model;
using Dashboard.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Services
{
    public class SignalRPublisher : BackgroundService
    {
        private readonly Channel<ConsumedKafkaMessage> _channel;
        private readonly Channel<KafkaMessage> _kafkaChannel;
        private readonly ILogger<SignalRPublisher> _logger;
        private readonly IHubContext<KafkaMessagesHub> _hubContext;

        public SignalRPublisher(Channel<ConsumedKafkaMessage> channel, IHubContext<KafkaMessagesHub> hubContext,
            ILogger<SignalRPublisher> logger, Channel<KafkaMessage> kafkaChannel)
        {
            _channel = channel;
            _logger = logger;
            _kafkaChannel = kafkaChannel;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var msg in _kafkaChannel.Reader.ReadAllAsync(stoppingToken))
            {
                _logger.LogDebug("Message received, {Msg}", msg);
                await _hubContext.Clients.All.SendAsync("ReceivedKafkaMessage", new ConsumedKafkaMessage(msg.Id, msg.Topic, msg.Body, DateTime.UtcNow), cancellationToken: stoppingToken);
            }
        }
    }
}