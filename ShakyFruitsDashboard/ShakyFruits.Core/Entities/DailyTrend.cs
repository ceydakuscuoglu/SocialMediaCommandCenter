using System;
using System.Collections.Generic;
using System.Text;

using ShakyFruits.Core.Enums;

namespace ShakyFruits.Core.Entities
{
    public class DailyTrend : BaseEntity
    {
        public string Name { get; set; } // Şarkı adı veya Hashtag metni
        public string TrendType { get; set; } // "Song", "Hashtag", "Effect"
        public TrendPlatform Platform { get; set; }
        public long ViewCount { get; set; } // İzlenme/Kullanım sayısı
        public DateTime TrendDate { get; set; } = DateTime.Today; // Hangi günün trendi olduğu
    }
}