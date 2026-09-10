namespace ShakyFruits.Core.DTOs
{
    public class TempPrepareSessionDto
    {
        public int? ExistingFruitAssetId { get; set; }
        public int? ExistingReferenceVideoId { get; set; }
        public string? TempImagePath { get; set; }
        public string? TempVideoPath { get; set; }
        public string FinalImagePathToUse { get; set; } = string.Empty;
        public string? FinalVideoPathToUse { get; set; }
        public string? Title { get; set; }
        public string? FruitTitle { get; set; }
        public string? DanceStyle { get; set; }
        public bool IsMultipleFruits { get; set; }
        public string TargetModel { get; set; } = "VIDEO 2.6";
        public string TargetResolution { get; set; } = "720p";
        public string? TargetUrl { get; set; }
        public bool IsRecreate { get; set; }
    }
}
