using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ShakyFruits.Core.Interfaces;

namespace ShakyFruits.Services
{
    public class VideoQueueManager : IVideoQueueManager
    {
        private readonly Channel<int> _queue;

        public VideoQueueManager()
        {
            var options = new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            };

            _queue = Channel.CreateUnbounded<int>(options);
        }

        public async ValueTask QueueJobAsync(int generationId, CancellationToken cancellationToken = default)
        {
            await _queue.Writer.WriteAsync(generationId, cancellationToken);
        }

        public IAsyncEnumerable<int> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return _queue.Reader.ReadAllAsync(cancellationToken);
        }
    }
}