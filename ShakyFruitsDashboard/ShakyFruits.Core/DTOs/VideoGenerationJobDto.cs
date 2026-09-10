using System;

namespace ShakyFruits.Core.DTOs
{
    public class VideoGenerationJobDto
    {
        public string Title { get; set; }
        public Guid JobId { get; set; } = Guid.NewGuid();
        public bool IsRecreate { get; set; }
        public string? TargetUrl { get; set; }
        public string SavedImagePath { get; set; }
        public string? SavedVideoPath { get; set; }
        public string AppliedPrompt { get; set; }
        public string TargetModel { get; set; }
        public string TargetResolution { get; set; }
    }
}
