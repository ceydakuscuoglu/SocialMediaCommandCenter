namespace ShakyFruits.Core.DTOs
{
    public class CreditCacheModelDto
    {
        public double RemainingCredits { get; set; }
        public double MembershipCredits { get; set; }
        public double TopUpCredits { get; set; }
        public double BonusCredits { get; set; }
        public DateTime LastScrapedAt { get; set; } // Verinin çekildiği an
    }
}

