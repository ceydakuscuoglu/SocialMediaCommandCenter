using System;
using System.ComponentModel.DataAnnotations.Schema; // Bunu eklemeyi unutma!

namespace ShakyFruits.Core.Entities
{
    public class AccountAnalyticsHistory : BaseEntity
    {
        public int TotalVideoViews { get; set; }
        public int ProfileViews { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalShares { get; set; }

        // EF Core'a para birimi formatını tam olarak söylüyoruz
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedRewards { get; set; }

        public DateTime RecordedAt { get; set; }
    }
}