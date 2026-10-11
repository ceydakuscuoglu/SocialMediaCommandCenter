using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShakyFruits.Core.Settings;
using ShakyFruits.Services;

namespace ShakyFruits.API.Controllers
{
    /// <summary>
    /// Yeni bir bilgisayarda ilk kurulum adımları için kullanılır.
    /// 
    /// KULLANIM SIRASI:
    ///   1. appsettings.json → BrowserSettings → Headless = false yapın
    ///   2. Uygulamayı başlatın ve Swagger'ı açın (https://localhost:PORT/swagger)
    ///   3. Aşağıdaki endpoint'leri sırayla çağırın:
    ///      POST /api/setup/login/tiktok         → TikTok oturumu (BrowserData)
    ///      POST /api/setup/login/tiktok-studio  → TikTok Studio analytics oturumu (BrowserData)
    ///      POST /api/setup/login/tiktok-ads     → TikTok Ads (ads.tiktok.com) trend scraping oturumu (BrowserData)
    ///      POST /api/setup/login/instagram      → Instagram oturumu (BrowserData)
    ///      POST /api/setup/login/meta           → Meta Business Suite oturumu (BrowserData)
    ///      POST /api/setup/login/kling          → Kling AI video üretim oturumu (KlingSession)
    ///   4. Her birinde açılan Chrome penceresinde oturum açın
    ///   5. Tüm login'ler bittikten sonra Headless = true yapın
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Tags("🔧 İlk Kurulum - Login Setup")]
    public class SetupController : ControllerBase
    {
        private readonly SocialMediaScraperService _scraperService;
        private readonly KlingAiBotService _klingBotService;
        private readonly BrowserSettings _browserSettings;
        private readonly ILogger<SetupController> _logger;

        public SetupController(
            SocialMediaScraperService scraperService,
            KlingAiBotService klingBotService,
            IOptions<BrowserSettings> browserOptions,
            ILogger<SetupController> logger)
        {
            _scraperService = scraperService;
            _klingBotService = klingBotService;
            _browserSettings = browserOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Güncel tarayıcı ayarlarını ve her iki oturum klasörünün durumunu gösterir.
        /// Headless=true ise üretim modundasınız, false ise login modundasınız.
        /// </summary>
        [HttpGet("browser-status")]
        public IActionResult GetBrowserStatus()
        {
            string browserDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");
            string klingSessionDir = Path.Combine(AppContext.BaseDirectory, "KlingSession");

            bool browserDataExists = Directory.Exists(browserDataDir) && Directory.GetFiles(browserDataDir, "*", SearchOption.AllDirectories).Length > 0;
            bool klingSessionExists = Directory.Exists(klingSessionDir) && Directory.GetFiles(klingSessionDir, "*", SearchOption.AllDirectories).Length > 0;

            return Ok(new
            {
                Message = "Tarayıcı yapılandırması",
                BrowserSettings = new
                {
                    Headless = _browserSettings.Headless,
                    LoginWaitSeconds = _browserSettings.LoginWaitSeconds,
                    MetaLoginWaitSeconds = _browserSettings.MetaLoginWaitSeconds,
                    ScrapingWaitMs = _browserSettings.ScrapingWaitMs,
                    Mode = _browserSettings.Headless ? "🟢 Üretim Modu (Headless)" : "🟡 Kurulum Modu (Görünür)"
                },
                SessionStatus = new
                {
                    BrowserData = new
                    {
                        Path = browserDataDir,
                        Exists = browserDataExists,
                        UsedBy = new[] { "TikTok Scraper", "TikTok Studio Analytics", "TikTok Ads / Creative Center (Trend)", "Instagram Scraper", "Meta Business Suite", "Kling AI (kredi sorgulama)" },
                        Status = browserDataExists
                            ? "✅ Oturum verisi mevcut"
                            : "⚠️ Oturum verisi yok — /login/tiktok, /login/tiktok-studio, /login/instagram, /login/meta endpoint'lerini çağırın"
                    },
                    KlingSession = new
                    {
                        Path = klingSessionDir,
                        Exists = klingSessionExists,
                        UsedBy = new[] { "Kling AI (video üretim botu)" },
                        Status = klingSessionExists
                            ? "✅ Oturum verisi mevcut"
                            : "⚠️ Oturum verisi yok — /login/kling endpoint'ini çağırın"
                    }
                },
                SetupInstructions = _browserSettings.Headless
                    ? new string[]
                    {
                        "✅ Sistem üretim modunda.",
                        "Eğer yeni bilgisayarda ilk kurulum yapıyorsanız:",
                        "1. appsettings.json → BrowserSettings → Headless = false yapın",
                        "2. Uygulamayı yeniden başlatın",
                        "3. login endpoint'lerini çağırın (tiktok, tiktok-studio, instagram, meta, kling)",
                        "4. Oturum açın ve Headless = true'ya geri alın"
                    }
                    : new string[]
                    {
                        "🟡 Sistem kurulum modunda (Headless=false).",
                        "Aşağıdaki endpoint'leri sırayla çağırın:",
                        "1. POST /api/setup/login/tiktok",
                        "2. POST /api/setup/login/tiktok-studio",
                        "3. POST /api/setup/login/tiktok-ads",
                        "4. POST /api/setup/login/instagram",
                        "5. POST /api/setup/login/meta",
                        "6. POST /api/setup/login/kling",
                        "7. Tüm login'ler tamamlandıktan sonra Headless = true yapın"
                    }
            });
        }

        /// <summary>
        /// TikTok oturumu açar (genel scraping için).
        /// Chrome açılır → QR kod okutun veya kullanıcı adı/şifre girin → süre dolunca kapanır.
        /// Çerezler: BrowserData klasörü
        /// Süre: appsettings.json → LoginWaitSeconds (varsayılan: 120 sn)
        /// </summary>
        [HttpPost("login/tiktok")]
        public async Task<IActionResult> SetupTikTokLogin()
        {
            _logger.LogInformation("=== TikTok Login Setup başlatıldı ===");
            try
            {
                await _scraperService.SetupTikTokLoginAsync();
                return Ok(new
                {
                    Message = "✅ TikTok login tamamlandı!",
                    Detail = "Çerezler BrowserData klasörüne kaydedildi.",
                    NextStep = "Devam: POST /api/setup/login/tiktok-studio"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TikTok login setup sırasında hata oluştu.");
                return StatusCode(500, new { Message = "❌ TikTok login başarısız.", Error = ex.Message, Tip = "Chrome yüklü ve Headless = false olduğundan emin olun." });
            }
        }

        /// <summary>
        /// TikTok Studio oturumu açar (analytics scraping için).
        /// Doğrudan tiktok.com/tiktokstudio'ya gider — analytics verilerini çekmek için oturum gerekir.
        /// Çerezler: BrowserData klasörü (TikTok ile aynı)
        /// Süre: appsettings.json → LoginWaitSeconds (varsayılan: 120 sn)
        /// </summary>
        [HttpPost("login/tiktok-studio")]
        public async Task<IActionResult> SetupTikTokStudioLogin()
        {
            _logger.LogInformation("=== TikTok Studio Login Setup başlatıldı ===");
            try
            {
                await _klingBotService.SetupTikTokStudioLoginAsync(_browserSettings.LoginWaitSeconds);
                return Ok(new
                {
                    Message = "✅ TikTok Studio login tamamlandı!",
                    Detail = "Çerezler BrowserData klasörüne kaydedildi. Artık TikTok Studio analytics scraping çalışabilir.",
                    NextStep = "Devam: POST /api/setup/login/instagram"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TikTok Studio login setup sırasında hata oluştu.");
                return StatusCode(500, new { Message = "❌ TikTok Studio login başarısız.", Error = ex.Message, Tip = "Chrome yüklü ve Headless = false olduğundan emin olun." });
            }
        }

        /// <summary>
        /// TikTok Ads / Creative Center oturumu açar (trend hashtag scraping için).
        /// ads.tiktok.com ayrı bir oturum gerektiriyor — TikTok Studio'dan farklıdır!
        /// Chrome açılır → TikTok hesabınızla giriş yapın → süre dolunca kapanır.
        /// Çerezler: BrowserData klasörü
        /// Süre: appsettings.json → LoginWaitSeconds (varsayılan: 120 sn)
        /// </summary>
        [HttpPost("login/tiktok-ads")]
        public async Task<IActionResult> SetupTikTokAdsLogin()
        {
            _logger.LogInformation("=== TikTok Ads Login Setup başlatıldı ===");
            try
            {
                await _scraperService.SetupTikTokAdsLoginAsync();
                return Ok(new
                {
                    Message = "✅ TikTok Ads login tamamlandı!",
                    Detail = "Çerezler BrowserData klasörüne kaydedildi. Artık TikTok trend scraping (ads.tiktok.com) çalışabilir.",
                    NextStep = "Devam: POST /api/setup/login/instagram"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TikTok Ads login setup sırasında hata oluştu.");
                return StatusCode(500, new { Message = "❌ TikTok Ads login başarısız.", Error = ex.Message, Tip = "Chrome yüklü ve Headless = false olduğundan emin olun." });
            }
        }

        /// <summary>
        /// Instagram oturumu açar.
        /// Chrome açılır → kullanıcı adı ve şifre girin → süre dolunca kapanır.
        /// Çerezler: BrowserData klasörü
        /// Süre: appsettings.json → LoginWaitSeconds (varsayılan: 120 sn)
        /// </summary>
        [HttpPost("login/instagram")]
        public async Task<IActionResult> SetupInstagramLogin()
        {
            _logger.LogInformation("=== Instagram Login Setup başlatıldı ===");
            try
            {
                await _scraperService.SetupInstagramLoginAsync();
                return Ok(new
                {
                    Message = "✅ Instagram login tamamlandı!",
                    Detail = "Çerezler BrowserData klasörüne kaydedildi.",
                    NextStep = "Devam: POST /api/setup/login/meta"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Instagram login setup sırasında hata oluştu.");
                return StatusCode(500, new { Message = "❌ Instagram login başarısız.", Error = ex.Message, Tip = "Chrome yüklü ve Headless = false olduğundan emin olun." });
            }
        }

        /// <summary>
        /// Meta Business Suite oturumu açar (Instagram Analytics için).
        /// Facebook veya Instagram ile giriş yapın, gerekirse 2FA kodunu girin.
        /// Çerezler: BrowserData klasörü
        /// Süre: appsettings.json → MetaLoginWaitSeconds (varsayılan: 180 sn)
        /// </summary>
        [HttpPost("login/meta")]
        public async Task<IActionResult> SetupMetaLogin()
        {
            _logger.LogInformation("=== Meta Business Suite Login Setup başlatıldı ===");
            try
            {
                await _scraperService.SetupMetaBusinessSuiteLoginAsync();
                return Ok(new
                {
                    Message = "✅ Meta Business Suite login tamamlandı!",
                    Detail = "Çerezler BrowserData klasörüne kaydedildi.",
                    NextStep = "Devam: POST /api/setup/login/kling"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meta Business Suite login setup sırasında hata oluştu.");
                return StatusCode(500, new { Message = "❌ Meta Business Suite login başarısız.", Error = ex.Message, Tip = "Chrome yüklü ve Headless = false olduğundan emin olun." });
            }
        }

        /// <summary>
        /// Kling AI oturumu açar (video üretim botu için).
        /// ⚠️ BrowserData'dan FARKLI bir klasör kullanır: KlingSession
        /// Chrome açılır → Kling AI'a giriş yapın → süre dolunca kapanır.
        /// Çerezler: KlingSession klasörü (AppContext.BaseDirectory altında)
        /// Süre: appsettings.json → LoginWaitSeconds (varsayılan: 120 sn)
        /// </summary>
        [HttpPost("login/kling")]
        public async Task<IActionResult> SetupKlingLogin()
        {
            _logger.LogInformation("=== Kling AI Login Setup başlatıldı ===");
            try
            {
                await _klingBotService.SetupKlingLoginAsync(_browserSettings.LoginWaitSeconds);
                return Ok(new
                {
                    Message = "✅ Kling AI login tamamlandı!",
                    Detail = "Çerezler KlingSession klasörüne kaydedildi. Artık Kling AI video üretim botu çalışabilir.",
                    NextStep = "Tüm login'ler tamamlandı! appsettings.json → Headless = true yaparak üretim moduna geçin."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kling AI login setup sırasında hata oluştu.");
                return StatusCode(500, new { Message = "❌ Kling AI login başarısız.", Error = ex.Message, Tip = "Playwright tarayıcısının yüklü olduğundan emin olun (playwright install chromium). Headless = false olmalı." });
            }
        }

        /// <summary>
        /// Tüm platformlara sırayla login olur:
        /// TikTok → TikTok Studio → TikTok Ads → Instagram → Meta Business Suite → Kling AI
        /// ⚠️ Toplam süre: LoginWaitSeconds × 5 + MetaLoginWaitSeconds (varsayılan ~12 dakika)
        /// </summary>
        [HttpPost("login/all")]
        public async Task<IActionResult> SetupAllLogins()
        {
            _logger.LogInformation("=== Tüm Platform Login Setup başlatıldı ===");

            var results = new List<object>();

            // 1. TikTok
            try
            {
                _logger.LogInformation("--- TikTok login başlıyor ---");
                await _scraperService.SetupTikTokLoginAsync();
                results.Add(new { Platform = "TikTok", Status = "✅ Başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TikTok login başarısız.");
                results.Add(new { Platform = "TikTok", Status = "❌ Başarısız", Error = ex.Message });
            }

            // 2. TikTok Studio
            try
            {
                _logger.LogInformation("--- TikTok Studio login başlıyor ---");
                await _klingBotService.SetupTikTokStudioLoginAsync(_browserSettings.LoginWaitSeconds);
                results.Add(new { Platform = "TikTok Studio", Status = "✅ Başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TikTok Studio login başarısız.");
                results.Add(new { Platform = "TikTok Studio", Status = "❌ Başarısız", Error = ex.Message });
            }

            // 3. TikTok Ads
            try
            {
                _logger.LogInformation("--- TikTok Ads login başlıyor ---");
                await _scraperService.SetupTikTokAdsLoginAsync();
                results.Add(new { Platform = "TikTok Ads", Status = "✅ Başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TikTok Ads login başarısız.");
                results.Add(new { Platform = "TikTok Ads", Status = "❌ Başarısız", Error = ex.Message });
            }

            // 4. Instagram
            try
            {
                _logger.LogInformation("--- Instagram login başlıyor ---");
                await _scraperService.SetupInstagramLoginAsync();
                results.Add(new { Platform = "Instagram", Status = "✅ Başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Instagram login başarısız.");
                results.Add(new { Platform = "Instagram", Status = "❌ Başarısız", Error = ex.Message });
            }

            // 4. Meta Business Suite
            try
            {
                _logger.LogInformation("--- Meta Business Suite login başlıyor ---");
                await _scraperService.SetupMetaBusinessSuiteLoginAsync();
                results.Add(new { Platform = "Meta Business Suite", Status = "✅ Başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meta Business Suite login başarısız.");
                results.Add(new { Platform = "Meta Business Suite", Status = "❌ Başarısız", Error = ex.Message });
            }

            // 5. Kling AI
            try
            {
                _logger.LogInformation("--- Kling AI login başlıyor ---");
                await _klingBotService.SetupKlingLoginAsync(_browserSettings.LoginWaitSeconds);
                results.Add(new { Platform = "Kling AI", Status = "✅ Başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kling AI login başarısız.");
                results.Add(new { Platform = "Kling AI", Status = "❌ Başarısız", Error = ex.Message });
            }

            bool allSuccessful = results.All(r => r.ToString()!.Contains("Başarılı"));

            return Ok(new
            {
                Message = allSuccessful
                    ? "✅ Tüm platform login'leri tamamlandı!"
                    : "⚠️ Bazı login'ler başarısız oldu. Detayları inceleyin.",
                Results = results,
                NextStep = allSuccessful
                    ? "appsettings.json → BrowserSettings → Headless = true yaparak üretim moduna geçin."
                    : "Başarısız olan platformlar için ayrı endpoint'leri tekrar deneyin."
            });
        }
    }
}
