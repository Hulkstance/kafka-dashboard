using System.Threading;
using System.Threading.Tasks;
using Dashboard.Core.Model;

namespace Dashboard.Core.Services
{
    public interface IMessagePublisher
    {
        Task Publish(KafkaMessage message, CancellationToken cancellationToken);
    }
}