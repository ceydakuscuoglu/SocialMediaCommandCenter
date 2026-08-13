namespace ShakyFruits.API.DTOs
{
    public class BotConfirmRequestDto
    {
        // Prepare metodunda sunucuya kaydettiğimiz ve ön yüze döndüğümüz dosya yolları
        public string SavedImagePath { get; set; }
        public string? SavedVideoPath { get; set; }

        public bool IsRecreate { get; set; }
        public string? TargetUrl { get; set; }
        public bool IsMultipleFruits { get; set; }
        public string TargetModel { get; set; } = "VIDEO 2.6";
        public string TargetResolution { get; set; } = "720p";
    }
}