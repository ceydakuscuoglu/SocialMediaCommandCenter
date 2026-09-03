using ShakyFruits.Core.Enums;

namespace ShakyFruits.API.DTOs
{
    public class PublishRequestDto
    {
        public int VideoGenerationId { get; set; } // İsmimi VideoGenerationId olarak güncelledik
        public string VideoPath { get; set; }
        public string Caption { get; set; }
        public string CoverImagePath { get; set; }
        public SocialPlatform Platform { get; set; }
    }
}
