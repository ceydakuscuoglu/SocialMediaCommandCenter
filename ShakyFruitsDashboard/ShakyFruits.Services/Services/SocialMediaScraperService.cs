using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.DTOs;
using System.Text.Json;

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

        public async Task<AccountAnalyticsHistory> ScrapeMetaBusinessSuiteAnalyticsAsync()
        {
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = false, // Hataları izlemek için false, her şey çalışınca true yaparsın
                Channel = "chrome",
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });

            var page = await browserContext.NewPageAsync();

            // Verileri toplayacağımız ana nesnemiz
            var analytics = new AccountAnalyticsHistory
            {
                Platform = SocialPlatform.Instagram,
                RecordedAt = DateTime.UtcNow
            };

            try
            {
                // 1. Görev: Results (Sonuçlar) sayfasını kazıyan metoda git
                await ScrapeResultsPageAsync(page, analytics);

                // 2. Görev: Audience (Hedef Kitle) sayfasını kazıyan metoda git
                await ScrapeAudiencePageAsync(page, analytics);

                return analytics;
            }
            catch (Exception ex)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "meta_insights_hata.png" });
                throw new Exception("Meta Business Suite kazıması başarısız. Hata: " + ex.Message);
            }
            finally
            {
                // Önce sekmeyi, sonra tüm tarayıcı altyapısını kesin olarak kapatıyoruz
                if (page != null) await page.CloseAsync();
                if (browserContext != null) await browserContext.CloseAsync();
            }
        }
        // ======================================================================
        // 1. AYRI METOT: SADECE "RESULTS" SAYFASINI KAZIR
        // ======================================================================
        private async Task ScrapeResultsPageAsync(IPage page, AccountAnalyticsHistory analytics)
        {
            _logger.LogInformation("Meta Business Suite 'Results' sayfasına gidiliyor...");
            await page.GotoAsync("https://business.facebook.com/latest/insights/results",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });

            await Task.Delay(5000); // Grafiklerin panele yerleşmesi için kısa bir es

            // JS kullanmadan doğrudan C# ile değerleri ekrandan çekiyoruz
            string reachText = await ExtractMetaValueAsync(page, new[] { "Instagram reach", "Reach", "Erişim" });
            string interactionsText = await ExtractMetaValueAsync(page, new[] { "Content interactions", "Interactions", "Etkileşimler" });
            string visitsText = await ExtractMetaValueAsync(page, new[] { "Instagram profile visits", "Profile visits", "Visits", "Profil ziyaretleri" });

            _logger.LogInformation($"Okunan Değerler -> Erişim: {reachText}, Etkileşim: {interactionsText}, Ziyaret: {visitsText}");

            analytics.TotalVideoViews = ParseSocialNumber(reachText);
            analytics.TotalLikes = ParseSocialNumber(interactionsText);
            analytics.ProfileViews = ParseSocialNumber(visitsText);
        }

        // ======================================================================
        // 2. AYRI METOT: SADECE "AUDIENCE" SAYFASINI KAZIR
        // ======================================================================
        private async Task ScrapeAudiencePageAsync(IPage page, AccountAnalyticsHistory analytics)
        {
            _logger.LogInformation("Meta Business Suite 'Audience' sayfasına geçiliyor...");
            await page.GotoAsync("https://business.facebook.com/latest/insights/people",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });

            await Task.Delay(5000);

            // Takipçi rakamını C# ile çek
            string followersText = await ExtractMetaValueAsync(page, new[] { "Followers", "Total followers", "Takipçiler" });

            _logger.LogInformation($"Okunan Değerler -> Takipçiler: {followersText}");

            analytics.TotalFollowers = ParseSocialNumber(followersText);
        }
        // ======================================================================
        // YARDIMCI METOT: PLAYWRIGHT NATIVE SELECTOR (GÜNCELLENDİ)
        // ======================================================================
        private async Task<string> ExtractMetaValueAsync(IPage page, string[] possibleLabels)
        {
            foreach (var label in possibleLabels)
            {
                try
                {
                    // MÜKEMMEL FİLTRE:
                    // 1. İçinde aradığımız kelime (Örn: "Content interactions") geçen data-pagelet kutularını bul.
                    // 2. Aynı zamanda içinde '.x10d9sdx' class'lı bir rakam barındırmak ZORUNDA olsun.
                    // 3. Last komutuyla: Bu kutulardan EN İÇTEKİNİ (en spesifik olan mini kartı) seç.
                    var container = page.Locator("div[data-pagelet]")
                                      .Filter(new LocatorFilterOptions { HasText = label })
                                      .Filter(new LocatorFilterOptions { Has = page.Locator(".x10d9sdx") })
                                      .Last;

                    // Bulduğumuz o spesifik mini kartın içindeki rakamı yakala
                    var valueLocator = container.Locator(".x10d9sdx").First;

                    // Sadece 2 saniye bekle, çünkü zaten sayfa yüklendi
                    await valueLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 2000 });

                    string value = await valueLocator.InnerTextAsync();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value.Trim(); // Eğer bulursa değeri döndür ve metottan çık
                    }
                }
                catch
                {
                    // Bu kelimeyi bulamazsa (veya timeout olursa) dizideki diğer kelimeye geç
                }
            }
            return "0"; // Hiçbir kombinasyon çalışmazsa 0 dön
        }

        public async Task<object?> GetLiveDemographicsAsync()
        {
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = true, // Arka planda gizlice çalışır
                Channel = "chrome",
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });

            var page = await browserContext.NewPageAsync();

            try
            {
                _logger.LogInformation("Canlı demografi kazıması için Audience sayfasına gidiliyor...");
                await page.GotoAsync("https://business.facebook.com/latest/insights/people",
                    new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });

                // Tabloların ve pasta grafiklerin render olması için süre tanıyoruz
                await Task.Delay(5000);

                // EvaluateAsync<string> olarak değiştirildi ve JS kodu JSON.stringify döndürecek şekilde ayarlandı
                var jsonResult = await page.EvaluateAsync<string>(@"() => {
            let result = {
                womenPercentage: '0%', menPercentage: '0%',
                topCountries: [], topCities: []
            };
            
            // 1. KADIN / ERKEK YÜZDELERİNİ BUL
            let spans = Array.from(document.querySelectorAll('span, div'));
            
            // Parantez () içine alınarak JS koşul hataları giderildi
            let womenEl = spans.find(s => s.innerText && (s.innerText.trim() === 'Women' || s.innerText.trim() === 'Kadınlar'));
            if(womenEl && womenEl.nextElementSibling) result.womenPercentage = womenEl.nextElementSibling.innerText.trim();
            
            let menEl = spans.find(s => s.innerText && (s.innerText.trim() === 'Men' || s.innerText.trim() === 'Erkekler'));
            if(menEl && menEl.nextElementSibling) result.menPercentage = menEl.nextElementSibling.innerText.trim();
            
            // 2. LİSTELERİ ÇEKEN YARDIMCI FONKSİYON (Ülke ve Şehirler için)
            function getListValues(headerTextEn, headerTextTr) {
                let list = [];
                let header = spans.find(s => s.innerText && (s.innerText.trim() === headerTextEn || s.innerText.trim() === headerTextTr));
                
                if (header) {
                    // Başlığın ait olduğu asıl listeyi/kartı bul
                    let container = header.closest('div[data-pagelet]') || header.parentElement.parentElement.parentElement.parentElement;
                    if (container) {
                        // Sıralı listeyi (ol) ve içindeki elemanları (li) yakala
                        let items = container.querySelectorAll('ol > li');
                        items.forEach(li => {
                            // Satırın içindeki tüm metinleri böl (Örn: 'Turkey \n 19.8%')
                            let parts = li.innerText.split('\n').map(t => t.trim()).filter(t => t !== '');
                            if(parts.length >= 2) {
                                // İlk parça isim, son parça yüzdedir
                                list.push({ name: parts[0], percentage: parts[parts.length - 1] });
                            }
                        });
                    }
                }
                return list;
            }
            
            // 3. ÜLKE VE ŞEHİRLERİ LİSTEYE DOLDUR
            result.topCountries = getListValues('Top countries', 'Başlıca ülkeler');
            result.topCities = getListValues('Top cities', 'Başlıca şehirler');
            
            // Nesneyi JSON string olarak C# tarafına fırlat
            return JSON.stringify(result);
        }");

                // System.Text.Json ile string'i güvenle C# modeline dönüştürüyoruz
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var demographics = System.Text.Json.JsonSerializer.Deserialize<object>(jsonResult, options);

                return demographics;
            }
            catch (Exception ex)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "meta_demographics_hata.png" });
                throw new Exception("Canlı demografi kazıması başarısız. Hata: " + ex.Message);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async Task<TikTokTrendDashboardDto> GetTikTokDailyTrendsAsync()
        {
            string userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "BrowserData");

            using var playwright = await Playwright.CreateAsync();
            await using var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = true, // Artık giriş yaptığın için burayı true (gizli) yapabilirsin!
                Channel = "chrome",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });

            var page = await browserContext.NewPageAsync();
            var dashboardData = new TikTokTrendDashboardDto();

            try
            {
                _logger.LogInformation("TikTok Trend sayfasına gidiliyor...");
                await page.GotoAsync("https://ads.tiktok.com/creative/creativeCenter/trends/hashtag?region=US&period=7",
                    new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });

                // Tablonun ilk yüklenmesi için biraz bekle
                await Task.Delay(5000);

                _logger.LogInformation("TikTok Trend Hashtag'leri toplanıyor (Sanal Liste Hilesi)...");

                // BÜYÜK HİLE: Toplama ve kaydırma işlemini tarayıcının içinde (JS) anlık yapıyoruz.
                // Böylece yukarıda kalıp silinen elementleri kaçırmıyoruz!
                var hashtagsJson = await page.EvaluateAsync<string>(@"() => {
            return new Promise((resolve) => {
                let collectedData = new Map(); // Tekrarları önlemek için Map kullanıyoruz
                let scrollAttempts = 0;
                const maxAttempts = 15; // Sonsuz döngüyü engellemek için

                let interval = setInterval(() => {
                    // O an ekranda olanları yakala ve sepete (Map) at
                    let rows = document.querySelectorAll('div[data-index]');
                    rows.forEach(row => {
                        let rankEl = row.querySelector('.w-\\[30px\\]');
                        let rank = rankEl ? rankEl.innerText.trim() : '';

                        let titleEl = row.querySelector('.truncate.text-\\[18px\\]');
                        let name = titleEl ? titleEl.innerText.trim() : '';

                        let stats = Array.from(row.querySelectorAll('.text-text-medium-cc'));
                        let postCount = stats.length > 0 ? stats[0].innerText.trim() + ' Posts' : '';
                        let viewCount = stats.length > 1 ? stats[1].innerText.trim() + ' Views' : '';

                        if (name && rank) {
                            collectedData.set(rank, {
                                Name: name,
                                Rank: rank,
                                Stats: postCount + (postCount && viewCount ? ' / ' : '') + viewCount
                            });
                        }
                    });

                    // Sayfayı biraz aşağı kaydır
                    window.scrollBy(0, 800);
                    scrollAttempts++;

                    // 50 tane topladıysak veya çok fazla kaydırdıysak bitir
                    if (collectedData.size >= 50 || scrollAttempts >= maxAttempts) {
                        clearInterval(interval);
                        
                        // Sepetteki verileri diziye çevir, sıraya diz ve ilk 50'sini al
                        let resultList = Array.from(collectedData.values())
                            .sort((a, b) => parseInt(a.Rank) - parseInt(b.Rank))
                            .slice(0, 50);
                            
                        resolve(JSON.stringify(resultList));
                    }
                }, 800); // Her kaydırma arası 800ms bekle
            });
        }");

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Çekilen json dizisini DTO listemize dönüştürüp atıyoruz
                dashboardData.TrendingHashtags = System.Text.Json.JsonSerializer.Deserialize<List<TrendItemDto>>(hashtagsJson, options) ?? new();

                return dashboardData;
            }
            catch (Exception ex)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "tiktok_trends_hata.png" });
                throw new Exception("TikTok trendleri kazılamadı. Hata: " + ex.Message);
            }
            finally
            {
                if (page != null) await page.CloseAsync();
                if (browserContext != null) await browserContext.CloseAsync();
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