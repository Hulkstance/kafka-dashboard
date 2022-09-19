using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dashboard.Core.Model;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Web.Hubs
{
    public record ConsumedKafkaMessage(Guid Id, string Topic, string Body, DateTime ConsumedAt);
    
    public class KafkaMessagesHub : Hub
    {
        private readonly Channel<KafkaMessage> _channel;

        public KafkaMessagesHub(Channel<KafkaMessage> channel)
        {
            _channel = channel;
        }
        
        public async Task PublishKafkaMessage(ConsumedKafkaMessage msg)
        {
            await Clients.All.SendAsync("ReceivedKafkaMessage", msg);
        } 

        public async IAsyncEnumerable<ConsumedKafkaMessage> KafkaMessagesStream()
        {
            await foreach (var (guid, topic, body) in _channel.Reader.ReadAllAsync())
            {
                yield return new ConsumedKafkaMessage(guid, topic, body, DateTime.UtcNow);
            }
        }
    }
}