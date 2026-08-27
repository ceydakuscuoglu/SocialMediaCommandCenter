using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShakyFruits.Core.Entities
{
    public class AccountAnalyticsHistory : BaseEntity
    {
        // --- YENİ: KANAL (ÖMÜR BOYU) İSTATİSTİKLERİ ---
        public int LifetimeLikes { get; set; } // 41K
        public int TotalFollowers { get; set; } // 7.1K
        public int FollowingCount { get; set; } // 2

        // --- ESKİ: SON 7 GÜNLÜK PERFORMANS İSTATİSTİKLERİ ---
        public int TotalVideoViews { get; set; } // 124.2K
        public int ProfileViews { get; set; }
        public int TotalLikes { get; set; } // 3.9K (Son 7 Gün)
        public int TotalComments { get; set; }
        public int TotalShares { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedRewards { get; set; }

        public DateTime RecordedAt { get; set; }
    }
}