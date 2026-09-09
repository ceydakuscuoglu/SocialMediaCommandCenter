using System.Collections.Generic;

namespace ShakyFruits.Core.Models
{
    // Artık sadece hashtag listesini tutan sade bir kapsayıcı
    public class TikTokTrendDashboardDto
    {
        public List<TrendItemDto> TrendingHashtags { get; set; } = new();
    }

    public class TrendItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public string Stats { get; set; } = string.Empty; // "89.1K Posts / 189.4M Views"
    }
}
