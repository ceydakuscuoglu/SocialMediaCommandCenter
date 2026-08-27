namespace ShakyFruits.API.DTOs
{
    public class CompleteGenerationRequestDto
    {
        public int VideoGenerationId { get; set; }
        public string OutputVideoPath { get; set; }
        public string? AiGeneratedCaption { get; set; }
    }
}
