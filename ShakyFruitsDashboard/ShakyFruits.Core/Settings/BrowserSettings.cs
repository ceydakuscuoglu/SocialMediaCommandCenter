namespace ShakyFruits.Core.Settings
{
    /// <summary>
    /// appsettings.json'daki "BrowserSettings" bölümüne karşılık gelir.
    /// Yeni bir bilgisayarda çalıştırırken:
    ///   1) Headless = false yapın
    ///   2) Swagger'dan /api/setup/login/tiktok, /instagram, /meta endpoint'lerini çağırın
    ///   3) Açılan tarayıcıda oturum açın — çerezler BrowserData klasörüne kaydedilir
    ///   4) Headless = true yapın, uygulama artık gizli modda çalışır
    /// </summary>
    public class BrowserSettings
    {
        /// <summary>
        /// true → Tarayıcı arka planda çalışır (üretim modu)
        /// false → Tarayıcı ekranda görünür (ilk kurulum / login modu)
        /// </summary>
        public bool Headless { get; set; } = true;

        /// <summary>
        /// TikTok ve Instagram login bekleme süresi (saniye).
        /// QR kod veya şifre girişi için yeterince uzun tutun.
        /// </summary>
        public int LoginWaitSeconds { get; set; } = 120;

        /// <summary>
        /// Meta Business Suite login bekleme süresi (saniye).
        /// 2FA ve hesap seçimi için daha uzun tutulur.
        /// </summary>
        public int MetaLoginWaitSeconds { get; set; } = 180;

        /// <summary>
        /// Scraping işlemlerinde sayfalardaki içeriklerin yüklenmesi için bekleme süresi (ms).
        /// </summary>
        public int ScrapingWaitMs { get; set; } = 4000;
    }
}
