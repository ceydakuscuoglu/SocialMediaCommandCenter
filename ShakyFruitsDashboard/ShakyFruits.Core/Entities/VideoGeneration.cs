using ShakyFruits.Core.Enums;

namespace ShakyFruits.Core.Entities
{
    public class VideoGeneration : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        // 1. İlişkiler (Dosya yollarını bu tablolardan çekeceğiz)
        public int FruitAssetId { get; set; }
        public virtual FruitAsset FruitAsset { get; set; }

        public int? ReferenceVideoId { get; set; } // Recreate durumunda boş olabilir
        public virtual ReferenceVideo? ReferenceVideo { get; set; }

        // 2. Kling AI Ayarları
        public bool IsRecreate { get; set; }
        public string? TargetUrl { get; set; }
        public string AppliedPrompt { get; set; }
        public string TargetModel { get; set; } = "VIDEO 2.6";
        public string TargetResolution { get; set; } = "720p";

        // 3. Kuyruk ve İzleme
        public GenerationStatus Status { get; set; } = GenerationStatus.Pending;
        public string? ErrorMessage { get; set; }

        // 4. Çıktılar (İleride kullanılacak)
        public string? OutputVideoPath { get; set; }
        public string? AiGeneratedCaption { get; set; }

        // Core/Models/VideoGeneration.cs dosyasının içine ekle:
        public virtual PublishedVideo? PublishedVideo { get; set; }
    }
}