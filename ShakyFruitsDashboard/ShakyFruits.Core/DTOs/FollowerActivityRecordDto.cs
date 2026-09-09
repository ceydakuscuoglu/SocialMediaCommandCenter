namespace ShakyFruits.Core.DTOs
{
    public class FollowerActivityRecordDto 
    {
        public string Date { get; set; } = string.Empty;
        public string Hour { get; set; } = string.Empty;
        public int ActiveFollowers { get; set; }
    }
}

