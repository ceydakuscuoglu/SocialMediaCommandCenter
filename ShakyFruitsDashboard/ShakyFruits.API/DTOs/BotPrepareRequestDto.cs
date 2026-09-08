using Microsoft.AspNetCore.Http;
using ShakyFruits.Core.Models;

namespace ShakyFruits.API.DTOs
{
    public class BotPrepareRequestDto
    {
        public bool IsRecreate { get; set; }
        public string? TargetUrl { get; set; }
        public bool IsMultipleFruits { get; set; }
        public string TargetModel { get; set; } = "VIDEO 2.6";
        public string TargetResolution { get; set; } = "720p";

        // === VAR OLANLARI SEÇMEK İÇİN ID'LER ===
        public int? ExistingFruitAssetId { get; set; }
        public int? ExistingReferenceVideoId { get; set; }

        // === DOSYALAR ===
        public IFormFile? FruitImage { get; set; }
        public IFormFile? ReferenceVideo { get; set; }

        public string? FruitTitle { get; set; }
        public string? DanceStyle { get; set; }
    }

    public class TempPrepareSession : ShakyFruits.Core.Models.TempPrepareSession
    {
    }
}