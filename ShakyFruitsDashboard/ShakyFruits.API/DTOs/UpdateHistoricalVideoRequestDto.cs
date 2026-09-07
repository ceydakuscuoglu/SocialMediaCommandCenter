using ShakyFruits.Core.Enums;

namespace ShakyFruits.API.DTOs
{
    public class UpdateHistoricalVideoRequestDto
    {
        public int FruitAssetId { get; set; }
        public int? ReferenceVideoId { get; set; }

        public bool IsRecreate { get; set; }

        // YENİ: String alanların sonuna '?' eklendi ki Frontend null gönderdiğinde C# çökmesin
        public string? TargetUrl { get; set; }
        public string? OutputVideoPath { get; set; }
        public string? AiGeneratedCaption { get; set; }

        public bool IsPublished { get; set; }
        public SocialPlatform Platform { get; set; }

        public string? PostUrl { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}
