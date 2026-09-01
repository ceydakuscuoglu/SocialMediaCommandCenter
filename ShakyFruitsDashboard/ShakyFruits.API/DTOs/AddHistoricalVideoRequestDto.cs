using System;
using ShakyFruits.Core.Enums;

namespace ShakyFruits.API.DTOs
{
    public class AddHistoricalVideoRequestDto
    {
        // 1. Temel Varlık İlişkileri
        public int FruitAssetId { get; set; } // Hangi meyve? (Zorunlu)
        public int? ReferenceVideoId { get; set; } // Referans video ID'si (Opsiyonel)

        // 2. Video Dosyası ve Detayları
        public string OutputVideoPath { get; set; } // Videonun bilgisayarındaki yolu
        public string? AiGeneratedCaption { get; set; }

        // 3. Yayınlanma Durumu (TikTok'ta var mı?)
        public bool IsPublished { get; set; }
        public string? PostUrl { get; set; } // TikTok Linki (Kazıyıcı buraya gidecek)
        public DateTime? PublishedAt { get; set; }
        public SocialPlatform Platform { get; set; } = SocialPlatform.TikTok;
    }
}