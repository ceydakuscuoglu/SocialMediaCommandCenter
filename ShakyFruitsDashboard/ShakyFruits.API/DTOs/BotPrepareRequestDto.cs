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

        // YENİ: Kullanıcı isterse meyveye özel isim verebilecek
        public string? FruitTitle { get; set; }

        // YENİ: Kullanıcı isterse videodaki dansın türünü yazabilecek
        public string? DanceStyle { get; set; }
    }

    public class TempPrepareSession
    {
        public string TempImagePath { get; set; }
        public string? TempVideoPath { get; set; }
        public string FruitTitle { get; set; }
        public string DanceStyle { get; set; }
        public bool IsMultipleFruits { get; set; }
        public string TargetModel { get; set; }
        public string TargetResolution { get; set; }
        public string? TargetUrl { get; set; }
        public bool IsRecreate { get; set; }
    }
}