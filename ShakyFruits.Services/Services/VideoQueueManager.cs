using System.Threading.Channels;

namespace ShakyFruits.Core.Services
{
    public class VideoQueueManager
    {
        // Artık sadece ID taşıyoruz!
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