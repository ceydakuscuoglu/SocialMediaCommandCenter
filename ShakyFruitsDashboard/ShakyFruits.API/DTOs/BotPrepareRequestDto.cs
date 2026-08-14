using Microsoft.AspNetCore.Http;

namespace ShakyFruits.API.DTOs
{
    public class BotPrepareRequestDto
    {
        // === İŞLEM TİPİ ===
        public bool IsRecreate { get; set; }

        // Recreate true ise zorunlu, değilse boş gelebilir
        public string? TargetUrl { get; set; }

        // === PROMPT MANTIĞI ===
        // Artık uzun metinler yerine sadece bu bayrağı (checkbox) alıyoruz
        public bool IsMultipleFruits { get; set; }

        // === KLING AI AYARLARI ===
        public string TargetModel { get; set; } = "VIDEO 2.6";
        public string TargetResolution { get; set; } = "720p";

        // === DOSYALAR ===
        // Swagger üzerinden veya form-data ile dosya yüklemek için IFormFile kullanıyoruz
        public IFormFile FruitImage { get; set; }

        // Local Upload (Sıfırdan yükleme) senaryosu için gerekli, Recreate için boş gelebilir
        public IFormFile? ReferenceVideo { get; set; }
    }
}