using ShakyFruits.Core.Entities;

namespace ShakyFruits.Core.DTOs
{
    public class CreateReferenceVideoRequestDto
    {
        public string DanceStyle { get; set; }
        public ReferenceSourceType SourceType { get; set; }
        public string? VideoPath { get; set; }
        public string? KlingSourceUrlOrId { get; set; }
    }
}

