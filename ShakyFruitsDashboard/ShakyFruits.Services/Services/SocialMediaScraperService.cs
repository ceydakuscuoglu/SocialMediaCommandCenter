using Microsoft.Playwright;
using ShakyFruits.Core.Entities;
using System;
using System.Threading.Tasks;

namespace ShakyFruits.Services
{
    public class SocialMediaScraperService
    {
        public async Task SetupTikTokLoginAsync()
        {
            // Kling ile aynı klasörü kullanıyoruz, böylece tüm çerezler tek yerde toplanıyor
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = false, // Ekranı mutlaka görmeliyiz
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

        public async Task<VideoAnalytics> ScrapeTikTokStatsAsync(string videoUrl)
        {
            // Kalıcı oturum klasörümüz (Kling'de kullandığımızın aynısı)
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();

            // YENİ GÜVENLİK ZIRHI: Gerçek Chrome ve Anti-Bot ayarları
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = false, // DİKKAT: İlk seferlik false yapıyoruz ki ekranı görüp puzzle'ı çözelim
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
                Headless = false,
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