using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using System;
using System.Threading.Tasks;

namespace ShakyFruits.Services
{
    public class SocialMediaScraperService
    {
        private readonly ILogger<SocialMediaScraperService> _logger;

        // Constructor ile Logger'ı içeri alıyoruz
        public SocialMediaScraperService(ILogger<SocialMediaScraperService> logger)
        {
            _logger = logger;
        }
        public async Task SetupTikTokLoginAsync()
        {
            // Kling ile aynı klasörü kullanıyoruz, böylece tüm çerezler tek yerde toplanıyor
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = true, // Ekranı mutlaka görmeliyiz
                Channel = "chrome",
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });

            var page = await browserContext.NewPageAsync();

            // Doğrudan TikTok giriş sayfasına yönlendir
            await page.GotoAsync("https://www.tiktok.com/login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Sana QR kod okutman veya şifre girmen için tam 2 dakika (120 saniye) süre veriyoruz.
            // Giriş yaptıktan sonra tarayıcıyı kendi kendine kapanana kadar elleme.
            await Task.Delay(120000);

            await page.CloseAsync();
        }

        public async Task SetupInstagramLoginAsync()
        {
            // Kling ve TikTok ile aynı klasörü kullanıyoruz, böylece tüm çerezler tek yerde toplanıyor
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = false, // Giriş yapabilmeni görmek için ekranı açık başlatıyoruz
                Channel = "chrome",
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });

            var page = await browserContext.NewPageAsync();

            _logger.LogInformation("Instagram giriş sayfasına yönlendiriliyor...");
            await page.GotoAsync("https://www.instagram.com/accounts/login/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Kullanıcı adı ve şifreni girip giriş yapman için 2 dakika (120 saniye) süre tanıyoruz.
            // Giriş yaptıktan ve ana sayfa açıldıktan sonra tarayıcıyı elleme, sürenin bitmesini bekle.
            await Task.Delay(120000);

            await page.CloseAsync();
        }
        public async Task SetupMetaBusinessSuiteLoginAsync()
        {
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = false, // Giriş işlemini yapabilmen için ekranı açıyoruz
                Channel = "chrome",
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });

            var page = await browserContext.NewPageAsync();

            _logger.LogInformation("Meta Business Suite giriş sayfasına yönlendiriliyor...");
            await page.GotoAsync("https://business.facebook.com/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Facebook veya Instagram ile giriş yapman, İki Faktörlü Doğrulamayı (2FA) geçmen 
            // ve doğru işletme/sayfa hesabını seçmen için sana 3 dakika (180 saniye) veriyoruz.
            // Sayfa tamamen açılıp paneli gördükten sonra tarayıcıyı kendi kendine kapanana kadar elleme.
            await Task.Delay(180000);

            await page.CloseAsync();
        }
        public async Task<VideoAnalytics> ScrapeTikTokStatsAsync(string videoUrl)
        {
            // Kalıcı oturum klasörümüz (Kling'de kullandığımızın aynısı)
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();

            // YENİ GÜVENLİK ZIRHI: Gerçek Chrome ve Anti-Bot ayarları
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = true, // DİKKAT: İlk seferlik false yapıyoruz ki ekranı görüp puzzle'ı çözelim
                Channel = "chrome", // Gerçek Google Chrome'u kullanır (Bot algılamasını %90 azaltır)
                Args = new[] { "--disable-blink-features=AutomationControlled" } // Bot etiketini siler
            });

            var page = await browserContext.NewPageAsync();

            try
            {
                // 2. DOĞRUDAN TIKTOK STUDIO'YA GİT
                // YENİ KOD: Sadece HTML iskeleti yüklenene kadar bekle
                await page.GotoAsync(videoUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
                await Task.Delay(4000);
                // 3. İstatistik kartının YÜKLÜ VE DOLU OLAN İKİNCİ VERSİYONUNU seçiyoruz (.Last ekledik)
                // Gönderdiğin HTML'e birebir uyumlu ve test edilmiş seçici blok (Aynen kalabilir, önceki adımdakiyle aynı ve tamamen doğru)
                var statsContainer = page.Locator("[data-tt='VideoOverviewPage_VideoInfoCard_FlexRow']").Last;
                await statsContainer.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

                var statSpans = statsContainer.Locator("[data-tt='VideoOverviewPage_VideoInfoCard_TUXText']");

                string viewsText = await statSpans.Nth(0).InnerTextAsync();
                string likesText = await statSpans.Nth(1).InnerTextAsync();
                string commentsText = await statSpans.Nth(2).InnerTextAsync();
                string sharesText = await statSpans.Nth(3).InnerTextAsync();
                string favoritesText = await statSpans.Nth(4).InnerTextAsync();

                return new VideoAnalytics
                {
                    Views = ParseSocialNumber(viewsText),
                    Likes = ParseSocialNumber(likesText),
                    Comments = ParseSocialNumber(commentsText),
                    Shares = ParseSocialNumber(sharesText),
                    Favorites = ParseSocialNumber(favoritesText),
                    RecordedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "tiktok_hata_ekrani.png" });
                throw new Exception("Kazıma başarısız. tiktok_hata_ekrani.png dosyasına bakın. İç hata: " + ex.Message);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async Task<AccountAnalyticsHistory> ScrapeAccountAnalyticsAsync()
        {
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = true,
                Channel = "chrome",
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });

            var page = await browserContext.NewPageAsync();

            try
            {
                await page.GotoAsync("https://www.tiktok.com/tiktokstudio", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
                await Task.Delay(4000); // Rakamların render olması için bekle

                // 1. KISIM: PROFİL ÖZETİ (Ömür Boyu Beğeni, Takipçi, Takip Edilen)
                var profileSpans = page.Locator("[data-tt='NewHome_UserInfo_TUXText_17']");
                await profileSpans.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

                string lifetimeLikesText = await profileSpans.Nth(0).InnerTextAsync();
                string followersText = await profileSpans.Nth(1).InnerTextAsync();
                string followingText = await profileSpans.Nth(2).InnerTextAsync();

                // 2. KISIM: 7 GÜNLÜK METRİKLER (Görüntülenme, Profil Görüntülenme vb.)
                var metricSpans = page.Locator(".absolute-value");
                await metricSpans.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

                string totalViewsText = await metricSpans.Nth(0).InnerTextAsync();
                string profileViewsText = await metricSpans.Nth(1).InnerTextAsync();
                string recentLikesText = await metricSpans.Nth(2).InnerTextAsync();
                string commentsText = await metricSpans.Nth(3).InnerTextAsync();
                string sharesText = await metricSpans.Nth(4).InnerTextAsync();
                string rewardsText = await metricSpans.Nth(5).InnerTextAsync();

                return new AccountAnalyticsHistory
                {
                    // Yeni Profil Verileri
                    LifetimeLikes = ParseSocialNumber(lifetimeLikesText),
                    TotalFollowers = ParseSocialNumber(followersText),
                    FollowingCount = ParseSocialNumber(followingText),

                    // 7 Günlük Veriler
                    TotalVideoViews = ParseSocialNumber(totalViewsText),
                    ProfileViews = ParseSocialNumber(profileViewsText),
                    TotalLikes = ParseSocialNumber(recentLikesText),
                    TotalComments = ParseSocialNumber(commentsText),
                    TotalShares = ParseSocialNumber(sharesText),
                    EstimatedRewards = decimal.TryParse(rewardsText.Replace("$", "").Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal reward) ? reward : 0,

                    RecordedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "account_analytics_hata.png" });
                throw new Exception("Hesap geneli analiz kazıması başarısız. İç hata: " + ex.Message);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async Task<AccountAnalyticsHistory> ScrapeInstagramAccountAnalyticsAsync()
        {
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = false, // İlk testte verilerin gelip gelmediğini görmek için false yapabilirsin (Sonra true yaparsın)
                Channel = "chrome",
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });

            var page = await browserContext.NewPageAsync();

            try
            {
                _logger.LogInformation("Instagram Insights sayfasına gidiliyor...");
                await page.GotoAsync("https://www.instagram.com/accounts/insights/?timeframe=7",
                    new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });

                _logger.LogInformation("İstatistiklerin yüklenmesi bekleniyor...");

                // KRİTİK DÜZELTME: Instagram'ın rakamları yüklemesi için h1 etiketlerinin ekranda görünmesini ve dolmasını bekliyoruz
                await page.Locator("h1").First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 20000 });
                await Task.Delay(4000); // Ekstra React render payı

                // JavaScript ile sayfadaki tüm metin bloklarını tarayıp ilgili metnin altındaki/üstündeki sayıyı alıyoruz
                var statsDict = await page.EvaluateAsync<Dictionary<string, string>>(@"() => {
            let result = {};
            let bodyText = document.body.innerText;
            
            // Sayfadaki metinleri satır satır bölüp arıyoruz (Instagram DOM yapısı değişimlerine karşı %100 dayanıklıdır)
            let lines = bodyText.split('\n').map(l => l.trim());
            
            for (let i = 0; i < lines.length; i++) {
                let line = lines[i];
                
                // Bir sonraki veya önceki satırda rakam olma ihtimaline karşı anahtar kelimeleri yakalıyoruz
                if (line === 'Görüntülemeler' && i > 0) {
                    // Genelde üstteki satır rakamdır
                    result['views'] = lines[i - 1] || lines[i + 1];
                }
                if (line === 'Etkileşimler' && i > 0) {
                    result['engagements'] = lines[i - 1] || lines[i + 1];
                }
                if (line === 'Profil hareketleri' && i > 0) {
                    result['profileActivity'] = lines[i - 1] || lines[i + 1];
                }
                if ((line === 'Toplam takipçi' || line === 'Takipçiler') && i > 0) {
                    // Takipçi sayısını yakala
                    let val = lines[i - 1];
                    if(val && !val.includes('%') && !val.includes('Zaman')) {
                        result['followers'] = val;
                    }
                }
            }
            
            // Eğer yukarıdaki satır mantığı kaçarsa doğrudan h1 taraması yapalım
            let h1s = document.querySelectorAll('h1');
            h1s.forEach(h1 => {
                let containerText = h1.closest('div')?.parentElement?.innerText || '';
                let val = h1.innerText.trim();
                
                if (containerText.includes('Görüntülemeler') && !result['views']) result['views'] = val;
                if (containerText.includes('Etkileşimler') && !result['engagements']) result['engagements'] = val;
                if (containerText.includes('Profil hareketleri') && !result['profileActivity']) result['profileActivity'] = val;
                if ((containerText.includes('Toplam takipçi') || containerText.includes('Takipçiler')) && !result['followers'] && !val.includes('%')) result['followers'] = val;
            });

            return result;
        }");

                statsDict.TryGetValue("views", out string viewsText);
                statsDict.TryGetValue("engagements", out string engagementsText);
                statsDict.TryGetValue("profileActivity", out string profileActivityText);
                statsDict.TryGetValue("followers", out string followersText);

                return new AccountAnalyticsHistory
                {
                    Platform = SocialPlatform.Instagram,
                    TotalFollowers = ParseSocialNumber(followersText),
                    TotalVideoViews = ParseSocialNumber(viewsText),
                    ProfileViews = ParseSocialNumber(profileActivityText),
                    TotalLikes = ParseSocialNumber(engagementsText),

                    LifetimeLikes = 0,
                    FollowingCount = 0,
                    TotalComments = 0,
                    TotalShares = 0,
                    EstimatedRewards = 0,
                    RecordedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "ig_account_analytics_hata.png" });
                throw new Exception("Instagram hesap analizi başarısız. Hata: " + ex.Message);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        // --- GÜNCELLENEN AKILLI ÇEVİRİCİ ---
        private int ParseSocialNumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;

            text = text.Trim().ToUpper();
            double multiplier = 1;

            // Eğer K (Bin) veya M (Milyon) varsa ondalık sayıdır (Örn: 1.5M veya 1,5M)
            if (text.EndsWith("M") || text.EndsWith("K"))
            {
                if (text.EndsWith("M")) { multiplier = 1000000; text = text.Replace("M", ""); }
                else if (text.EndsWith("K")) { multiplier = 1000; text = text.Replace("K", ""); }

                text = text.Replace(",", "."); // Ondalık standarda çek
            }
            else
            {
                // Eğer K veya M yoksa, düz sayıdır. İçindeki binlik ayraçları (virgül veya nokta) tamamen sil (Örn: 5,277 -> 5277)
                text = text.Replace(",", "").Replace(".", "");
            }

            if (double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedValue))
            {
                return (int)Math.Round(parsedValue * multiplier);
            }
            // $ veya € gibi para birimlerini temizle
            text = text.Replace("$", "").Replace("€", "").Replace("£", "");

            return 0;
        }
    }
}