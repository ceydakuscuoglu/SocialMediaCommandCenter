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
            // Tablo Kuralı 1: Süreyi en yakın tam saniyeye yuvarla
            // AwayFromZero: 3.4 -> 3 saniye, 3.5 ve 3.6 -> 4 saniye yapar
            int roundedSeconds = (int)Math.Round(durationInSeconds, MidpointRounding.AwayFromZero);

            // Güvenlik: Minimum 1 saniye kabul edelim
            if (roundedSeconds < 1) roundedSeconds = 1;

            // Tablo Kuralı 2 & 3: Çarpanları belirle
            int multiplier = 5; // Varsayılan en düşük model çarpanı (2.6 / Standard)

            if (model.Contains("3.0"))
            {
                // Professional (1080p) = 12, Standard (720p) = 9
                multiplier = resolution.Contains("1080p") ? 12 : 9;
            }
            else if (model.Contains("2.6"))
            {
                // Professional (1080p) = 8, Standard (720p) = 5
                multiplier = resolution.Contains("1080p") ? 8 : 5;
            }

            // Toplam maliyet
            return roundedSeconds * multiplier;
        }
    }
}