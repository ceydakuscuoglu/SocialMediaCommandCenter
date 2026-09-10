namespace ShakyFruits.Core.DTOs
{
    public class CompleteGenerationRequestDto
    {
        public int VideoGenerationId { get; set; }
        public string? Title { get; set; }
        public string OutputVideoPath { get; set; } = string.Empty;
        public string? AiGeneratedCaption { get; set; }
    }
}

