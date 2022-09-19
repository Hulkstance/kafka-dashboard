using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dashboard.Core.Model;
using Dashboard.Core.UseCases;
using Microsoft.Extensions.Hosting;

namespace Dashboard.Web.Services
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly SubscribeKafkaTopicsAndPublishItUseCase _subscribeKafkaTopicsAndPublishItUse;
        private readonly List<Topic> _topics;
        public KafkaConsumer(SubscribeKafkaTopicsAndPublishItUseCase subscribeKafkaTopicsAndPublishItUse, List<Topic> topics)
        {
            _subscribeKafkaTopicsAndPublishItUse = subscribeKafkaTopicsAndPublishItUse;
            _topics = topics;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _subscribeKafkaTopicsAndPublishItUse.Execute(
                new SubscribeKafkaTopicsAndPublishItCommand(_topics.Select(x => x.Value).ToArray()),
                stoppingToken);
        }
    }
}