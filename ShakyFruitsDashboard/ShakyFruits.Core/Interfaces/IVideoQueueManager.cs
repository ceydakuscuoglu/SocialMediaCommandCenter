using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShakyFruits.Core.Interfaces
{
    public interface IVideoQueueManager
    {
        ValueTask QueueJobAsync(int generationId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<int> ReadAllAsync(CancellationToken cancellationToken = default);
    }
}
