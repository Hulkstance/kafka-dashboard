using System.Collections.Generic;
using System.Threading;
using Dashboard.Core.Model;

namespace Dashboard.Core.Services
{
    public interface IKafkaTopicSubscriber
    {
        IAsyncEnumerable<KafkaMessage> Subscribe(string[] topics, CancellationToken cancellationToken);
    }
}