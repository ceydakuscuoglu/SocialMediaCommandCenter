using Microsoft.AspNetCore.Http;

namespace ShakyFruits.API.DTOs
{
    public class BotPrepareRequestDto
    {
        public bool IsRecreate { get; set; }
        public string? TargetUrl { get; set; }
        public bool IsMultipleFruits { get; set; }
        public string TargetModel { get; set; } = "VIDEO 2.6";
        public string TargetResolution { get; set; } = "720p";

        // === YENİ: VAR OLANLARI SEÇMEK İÇİN ID'LER ===
        public int? ExistingFruitAssetId { get; set; }
        public int? ExistingReferenceVideoId { get; set; }

        // === DOSYALAR (Artık Nullable) ===
        public IFormFile? FruitImage { get; set; }
        public IFormFile? ReferenceVideo { get; set; }

        public string? FruitTitle { get; set; }
        public string? DanceStyle { get; set; }
    }

    public class TempPrepareSession
    {
        // Hangi ID'ler seçildi? (Yeni yüklendiyse bunlar null kalır)
        public int? ExistingFruitAssetId { get; set; }
        public int? ExistingReferenceVideoId { get; set; }

        // Yeni dosya yüklendiyse Temp klasöründeki yolları
        public string? TempImagePath { get; set; }
        public string? TempVideoPath { get; set; }

        // Kling AI'a maliyet hesabı için gönderilecek "Kesin" yollar
        public string FinalImagePathToUse { get; set; }
        public string? FinalVideoPathToUse { get; set; }

        public string? FruitTitle { get; set; }
        public string? DanceStyle { get; set; }
        public bool IsMultipleFruits { get; set; }
        public string TargetModel { get; set; }
        public string TargetResolution { get; set; }
        public string? TargetUrl { get; set; }
        public bool IsRecreate { get; set; }
    }
}