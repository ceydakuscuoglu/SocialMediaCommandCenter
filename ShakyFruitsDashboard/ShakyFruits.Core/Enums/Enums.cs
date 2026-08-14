using System;
using System.Collections.Generic;
using System.Text;

namespace ShakyFruits.Core.Enums
{
    public enum GenerationStatus
    {
        Pending = 0,    // Bekliyor (Bot henüz almadı)
        Processing = 1, // Üretiliyor (Bot Kling AI'da çalışıyor)
        Completed = 2,  // Tamamlandı (Video diske indi)
        Failed = 3      // Hata Aldı
    }

    public enum TrendPlatform
    {
        TikTok = 1,
        Instagram = 2
    }
}