using System.Collections.Generic;

namespace ShakyFruits.API.DTOs
{
    public class TikTokTrendDashboardDto
    {
        public List<TrendItemDto> TrendingHashtags { get; set; } = new();
        public List<TrendSongDto> TrendingSongs { get; set; } = new();
        public List<TrendVideoDto> TrendingVideos { get; set; } = new();
    }

    public class TrendItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
    }

    public class TrendSongDto
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
    }

    public class TrendVideoDto
    {
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Likes { get; set; } = string.Empty;
    }
}
