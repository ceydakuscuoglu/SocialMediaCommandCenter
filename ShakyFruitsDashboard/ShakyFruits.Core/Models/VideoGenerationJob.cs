using System;
using System.Collections.Generic;
using System.Text;

namespace ShakyFruits.Core.Models
{
    public class VideoGenerationJob
    {
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
