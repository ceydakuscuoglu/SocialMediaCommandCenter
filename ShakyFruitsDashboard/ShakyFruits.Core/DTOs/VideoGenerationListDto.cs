namespace ShakyFruits.Core.DTOs
{
    public class VideoGenerationListDto
    {
        public string Title { get; set; }
        public int Id { get; set; }

        // Ön yüzde küçük bir önizleme (thumbnail) göstermek için resim ve video yolları
        public string FruitImagePath { get; set; }
        public string? ReferenceVideoPath { get; set; }

        public bool IsRecreate { get; set; }
        public string AppliedPrompt { get; set; }

        // Statüyü string olarak dönüyoruz ki ön yüzde "Pending", "Processing" gibi kolayca okunsun
        public string Status { get; set; }

        public string? ErrorMessage { get; set; }
        public string? OutputVideoPath { get; set; }

        // Yeni eklenen alan: Videonun kuyruğa alınma/oluşturulma tarihi
        public DateTime CreatedAt { get; set; }
        // ==========================================
        // YENİ EKLENEN ALANLAR (Ön yüz Modalı için)
        // ==========================================
        public int FruitAssetId { get; set; }
        public int? ReferenceVideoId { get; set; }
        public string? AiGeneratedCaption { get; set; }

        public bool IsPublished { get; set; }
        public int Platform { get; set; }
        public string? PostUrl { get; set; }
    }
}
