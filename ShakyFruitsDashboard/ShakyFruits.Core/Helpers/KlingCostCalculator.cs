using System;
using System.Diagnostics;

namespace ShakyFruits.Core.Helpers
{
    public static class KlingCostCalculator
    {
        // 1. Videonun süresini okuyan şimşek hızında metot
        public static double GetVideoDurationInSeconds(string filePath)
        {
            try
            {
                using var tfile = TagLib.File.Create(filePath);
                return tfile.Properties.Duration.TotalSeconds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Video süresi okunamadı: {ex.Message}");
                return 5.0; // Hata olursa varsayılan bir süre dön (isteğe bağlı)
            }
        }

        // 2. Kling AI tablolarına birebir uyan matematik motoru
        public static int CalculateCost(string model, string resolution, double durationInSeconds)
        {
            int roundedSeconds = (int)Math.Round(durationInSeconds, MidpointRounding.AwayFromZero);
            if (roundedSeconds < 1) roundedSeconds = 1;

            // Güvenlik Zırhı: Gelen değerleri null ihtimaline karşı koru, boşlukları sil ve küçük harfe çevir
            string safeModel = (model ?? "").Trim().ToLower();
            string safeResolution = (resolution ?? "").Trim().ToLower();

            int multiplier = 5; // Varsayılan en düşük model çarpanı (2.6 / 720p)

            // Artık sadece "3.0" ve "1080" kelimelerini arıyoruz.
            // Artık sadece "3" rakamını arıyoruz, ".0" kısmına takılmıyoruz.
            if (safeModel.Contains("3"))
            {
                // 1080 kelimesi geçiyorsa 12, geçmiyorsa 9
                multiplier = safeResolution.Contains("1080") ? 12 : 9;
            }
            else if (safeModel.Contains("2.6"))
            {
                // 1080 kelimesi geçiyorsa 8, geçmiyorsa 5
                multiplier = safeResolution.Contains("1080") ? 8 : 5;
            }

            return roundedSeconds * multiplier;
        }
    }
}