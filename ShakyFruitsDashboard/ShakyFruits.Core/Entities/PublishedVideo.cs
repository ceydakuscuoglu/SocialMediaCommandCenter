using ShakyFruits.Core.Enums;
using System;
using System.Collections.Generic;

namespace ShakyFruits.Core.Entities
{
    public class PublishedVideo : BaseEntity
    {
        // Hangi Kling videomuzu yayınladık?
        public int VideoGenerationId { get; set; }
        public virtual VideoGeneration VideoGeneration { get; set; }

        public SocialPlatform Platform { get; set; }

        // Botun kazıma yapmak için gideceği URL (Örn: tiktok.com/@ceyda/video/123)
        public string PostUrl { get; set; }

        public DateTime PublishedAt { get; set; }

        // Bire-Çok İlişki (Bir videonun günlük olarak yüzlerce analitik kaydı olabilir)
        public virtual ICollection<VideoAnalytics> AnalyticsHistory { get; set; }
    }
}