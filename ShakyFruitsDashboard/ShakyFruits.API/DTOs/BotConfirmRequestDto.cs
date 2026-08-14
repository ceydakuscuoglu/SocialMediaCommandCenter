namespace ShakyFruits.API.DTOs
{
    public class BotConfirmRequestDto
    {
        public int FruitAssetId { get; set; }
        public int? ReferenceVideoId { get; set; }
        public bool IsRecreate { get; set; }
        public string? TargetUrl { get; set; }
        public bool IsMultipleFruits { get; set; }
        public string TargetModel { get; set; } = "VIDEO 2.6";
        public string TargetResolution { get; set; } = "720p";
    }
}