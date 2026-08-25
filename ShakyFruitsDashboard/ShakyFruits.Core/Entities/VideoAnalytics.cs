using System;

namespace ShakyFruits.Core.Entities
{
    public class VideoAnalytics : BaseEntity
    {
        public int PublishedVideoId { get; set; }
        public virtual PublishedVideo PublishedVideo { get; set; }

        public int Views { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int Shares { get; set; }

        // Verinin bottan okunup veritabanına yazıldığı an (Snapshot zamanı)
        public DateTime RecordedAt { get; set; }
    }
}