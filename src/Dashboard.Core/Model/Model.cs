using System;

namespace Dashboard.Core.Model
{
    public record KafkaMessage(Guid Id, string Topic, string Body);
}