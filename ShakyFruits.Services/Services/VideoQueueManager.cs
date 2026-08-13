using System.Threading.Channels;
using ShakyFruits.Core.Models;

namespace ShakyFruits.Core.Services
{
    public class VideoQueueManager
    {
        // Unbounded channel: Kapasite sınırı yok, belleğin yettiği kadar iş alabilir.
        // Bounded channel da kullanabilirdik (Örn: max 100 iş) ama şimdilik esnek bırakıyoruz.
        private readonly Channel<VideoGenerationJob> _queue;

        public VideoQueueManager()
        {
            var options = new UnboundedChannelOptions
            {
                SingleReader = true, // Sadece tek bir Worker okuyacak (Playwright çakışmasını önler)
                SingleWriter = false // Birden fazla API isteği aynı anda kuyruğa iş atabilir
            };
            _queue = Channel.CreateUnbounded<VideoGenerationJob>(options);
        }

        // API'den (Controller) kuyruğa iş eklemek için kullanılacak metot
        public async ValueTask QueueJobAsync(VideoGenerationJob job, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(job);
            await _queue.Writer.WriteAsync(job, cancellationToken);
        }

        // Arka plan işçisinin (Worker) kuyruktan iş çekmesi için kullanılacak metot
        public IAsyncEnumerable<VideoGenerationJob> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return _queue.Reader.ReadAllAsync(cancellationToken);
        }
    }
}