using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Confluent.Kafka;
using Dashboard.Core.Model;
using Dashboard.Core.Services;
using Dashboard.Core.UseCases;
using Dashboard.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Infrastructure.AspNetCore
{
    public static class Extensions
    {
        private static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
        {
            var conf = configuration.GetSection("Kafka").GetSection("ConsumerSettings").Get<ConsumerConfig>();
            services.AddSingleton<ConsumerConfig>(conf);
            services.AddSingleton<List<Topic>>(_ =>
                configuration.GetSection("Kafka").GetSection("Topics").Get<List<string>>().Select(x => new Topic(x))
                    .ToList());
            services.AddSingleton<ConsumerBuilder<Ignore, string>>(sp =>
            {
                var c = sp.GetService<ConsumerConfig>();
                return new ConsumerBuilder<Ignore, string>(c);
            });
            services.AddSingleton<IConsumer<Ignore, string>>(sp =>
            {
                var builder = sp.GetService<ConsumerBuilder<Ignore, string>>();
                return builder.Build();
            });
            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddTransient<IKafkaTopicSubscriber, ConfluentKafkaTopicSubscriber>();
            services.AddTransient<IMessagePublisher, SignalRMessagePublisher>();
            services.AddSingleton<Channel<KafkaMessage>>(
                Channel.CreateBounded<KafkaMessage>(new BoundedChannelOptions(10)));
            services.AddTransient<SubscribeKafkaTopicsAndPublishItUseCase>();
            services.AddKafka(configuration);
            return services;
        }
    }
}