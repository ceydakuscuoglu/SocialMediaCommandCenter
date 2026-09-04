using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShakyFruits.API.DTOs;
using ShakyFruits.API.DTOs.ShakyFruits.API.DTOs;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Data; // DbContext'in olduğu namespace'i eklemeyi unutma
using ShakyFruits.Services; // Senin servis namespace'ine göre ayarla

namespace ShakyFruits.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SocialMediaScraperService _scraperService;

        public AnalyticsController(ApplicationDbContext context, SocialMediaScraperService scraperService)
        {
            _context = context;
            _scraperService = scraperService;
        }

        // 1. DIŞ ANALİTİK: TikTok Kazıma Test Uç Noktası
        [HttpGet("test-tiktok-scraper")]
        public async Task<IActionResult> TestTikTokScraper([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Lütfen test etmek için bir TikTok video linki (url) parametresi gönderin.");

            try
            {
                var stats = await _scraperService.ScrapeTikTokStatsAsync(url);

                return Ok(new
                {
                    Message = "Görev Başarılı! Veriler kazındı.",
                    ScrapedUrl = url,
                    Stats = new
                    {
                        stats.Views,
                        stats.Likes,
                        stats.Comments,
                        stats.Shares,
                        stats.Favorites, // Swagger'da görmek için eklendi
                        stats.RecordedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Kazıma motoru çöktü!", Error = ex.Message });
            }
        }

        // 2. İÇ ANALİTİK: Veritabanı İçgörüleri (Meyve ve Dans İstatistikleri)
        [HttpGet("internal-stats")]
        public async Task<IActionResult> GetInternalStats()
        {
            try
            {
                // Toplam üretilen video sayısı
                var totalVideos = await _context.VideoGenerations.CountAsync();

                // Hangi meyveden kaç video üretilmiş? (En çok kullanılana göre sıralı)
                var fruitUsage = await _context.VideoGenerations
                    .Include(v => v.FruitAsset)
                    .Where(v => v.FruitAsset != null)
                    .GroupBy(v => v.FruitAsset.Title)
                    .Select(g => new { FruitName = g.Key, UsageCount = g.Count() })
                    .OrderByDescending(x => x.UsageCount)
                    .ToListAsync();

                // En çok kullanılan dans/referans stilleri (Örn: Waka Waka, Salsa)
                var danceStyleUsage = await _context.VideoGenerations
                    .Include(v => v.ReferenceVideo)
                    .Where(v => v.ReferenceVideo != null && !string.IsNullOrEmpty(v.ReferenceVideo.DanceStyle))
                    .GroupBy(v => v.ReferenceVideo.DanceStyle)
                    .Select(g => new { DanceStyle = g.Key, UsageCount = g.Count() })
                    .OrderByDescending(x => x.UsageCount)
                    .ToListAsync();

                // Başarı oranı (Kaçı Completed, kaçı Failed veya Pending?)
                var statusDistribution = await _context.VideoGenerations
                    .GroupBy(v => v.Status)
                    .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                    .ToListAsync();

                return Ok(new
                {
                    Overview = new { TotalGenerations = totalVideos },
                    FruitStats = fruitUsage,
                    DanceStats = danceStyleUsage,
                    SystemHealth = statusDistribution
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "İstatistikler çekilirken hata oluştu.", Error = ex.Message });
            }
        }
        // 1. GET: Grafikler ve Tablo için Yayınlanmış Videoları Getir
        [HttpGet("published-videos")]
        public async Task<IActionResult> GetPublishedVideos()
        {
            try
            {
                // Select ile veriyi şekillendiriyoruz. 
                // Bu sayede hem tam UI'ın istediği JSON çıkıyor hem de EF Core döngüsel referans hatası vermiyor.
                var videos = await _context.PublishedVideos
                    .OrderByDescending(p => p.PublishedAt) // En son eklenen video en üstte gelsin
                    .Select(p => new
                    {
                        id = p.Id,
                        videoGenerationId = p.VideoGenerationId,
                        platform = (int)p.Platform,
                        postUrl = p.PostUrl,
                        publishedAt = p.PublishedAt,
                        // UI tarafındaki Recharts için verileri eski tarihten yeniye (.OrderBy) sıralıyoruz
                        analyticsHistory = p.AnalyticsHistory
                            .OrderBy(a => a.RecordedAt)
                            .Select(a => new
                            {
                                views = a.Views,
                                likes = a.Likes,
                                comments = a.Comments,
                                shares = a.Shares,
                                favorites = a.Favorites,
                                recordedAt = a.RecordedAt
                            }).ToList()
                    })
                    .ToListAsync();

                return Ok(videos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Yayınlanan videolar getirilirken hata oluştu.", Error = ex.Message });
            }
        }

        // 2. POST: Yeni Video Ekleyip Takibe Alma (Modal'dan gelecek istek)
        [HttpPost("publish-video")]
        public async Task<IActionResult> PublishVideo([FromBody] PublishVideoRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PostUrl))
                return BadRequest("Geçersiz veri gönderildi. Lütfen bir TikTok Studio URL'si girin.");

            try
            {
                // Güvenlik kontrolü: Kling ile üretilmiş böyle bir video gerçekten var mı?
                var videoExists = await _context.VideoGenerations.AnyAsync(v => v.Id == request.VideoGenerationId);
                if (!videoExists)
                    return NotFound($"Sistemde {request.VideoGenerationId} ID'li bir üretim (Generation) kaydı bulunamadı.");

                var newPublishedVideo = new PublishedVideo
                {
                    VideoGenerationId = request.VideoGenerationId,
                    Platform = ShakyFruits.Core.Enums.SocialPlatform.TikTok, // Varsayılan olarak TikTok atandı
                    PostUrl = request.PostUrl,
                    PublishedAt = DateTime.UtcNow
                };

                _context.PublishedVideos.Add(newPublishedVideo);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Video takip listesine başarıyla eklendi. Playwright botumuz sıradaki turda verileri kazıyacaktır.",
                    Data = new
                    {
                        newPublishedVideo.Id,
                        newPublishedVideo.VideoGenerationId,
                        Platform = (int)newPublishedVideo.Platform,
                        newPublishedVideo.PostUrl,
                        newPublishedVideo.PublishedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Video kaydedilirken veritabanı hatası oluştu.", Error = ex.Message });
            }
        }

        // 3. GET: UI için Hesap Geneli Tarihsel Verileri Getir (Platform Filtreli)
        [HttpGet("account-history")]
        public async Task<IActionResult> GetAccountHistory([FromQuery] SocialPlatform? platform)
        {
            try
            {
                // 1. Sorguyu hazırlıyoruz ama veritabanına henüz gitmiyoruz
                var query = _context.AccountAnalyticsHistory.AsQueryable();

                // 2. Eğer dışarıdan bir platform (Örn: ?platform=Instagram) gönderildiyse filtrele
                if (platform.HasValue)
                {
                    query = query.Where(a => a.Platform == platform.Value);
                }

                // 3. React'in grafikleri çizebilmesi için eskiden yeniye sırala ve JSON'u formatla
                var history = await query
                    .OrderBy(a => a.RecordedAt)
                    .Select(a => new
                    {
                        platform = a.Platform.ToString(), // Frontend'in verileri ayırabilmesi için platform adını da ekliyoruz
                        lifetimeLikes = a.LifetimeLikes,
                        totalFollowers = a.TotalFollowers,
                        followingCount = a.FollowingCount,
                        totalVideoViews = a.TotalVideoViews,
                        profileViews = a.ProfileViews,
                        totalLikes = a.TotalLikes,
                        totalComments = a.TotalComments,
                        totalShares = a.TotalShares,
                        estimatedRewards = a.EstimatedRewards,
                        recordedAt = a.RecordedAt
                    })
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Hesap geneli veriler getirilirken hata oluştu.", Error = ex.Message });
            }
        }

        // ================= TIKTOK ANALİTİKLERİ =================
        [HttpGet("tiktok/latest-account-stats")]
        public async Task<IActionResult> GetTikTokAccountStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var platform = SocialPlatform.TikTok;

                var latestRecord = await _context.AccountAnalyticsHistory
                    .Where(a => a.Platform == platform)
                    .OrderByDescending(a => a.RecordedAt)
                    .FirstOrDefaultAsync();

                if (latestRecord != null && latestRecord.RecordedAt.Date == today)
                    return Ok(new { source = "Database Cache", data = latestRecord });

                // TikTok Studio kazıyıcısını tetikle
                var scrapedStats = await _scraperService.ScrapeAccountAnalyticsAsync();
                scrapedStats.Platform = platform;

                _context.AccountAnalyticsHistory.Add(scrapedStats);
                await _context.SaveChangesAsync();

                return Ok(new { source = "Live Scrape", data = scrapedStats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "TikTok istatistikleri alınamadı.", Error = ex.Message });
            }
        }

        // ================= INSTAGRAM (META) ANALİTİKLERİ =================
        [HttpGet("instagram/latest-account-stats")]
        public async Task<IActionResult> GetInstagramAccountStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var platform = SocialPlatform.Instagram;

                var latestRecord = await _context.AccountAnalyticsHistory
                    .Where(a => a.Platform == platform)
                    .OrderByDescending(a => a.RecordedAt)
                    .FirstOrDefaultAsync();

                // 1. CACHE'DEN DÖNÜŞ (Gereksiz alanlar temizlendi)
                if (latestRecord != null && latestRecord.RecordedAt.Date == today)
                {
                    return Ok(new
                    {
                        source = "Database Cache",
                        data = new
                        {
                            id = latestRecord.Id,
                            platform = latestRecord.Platform.ToString(),
                            totalFollowers = latestRecord.TotalFollowers,
                            totalReach = latestRecord.TotalVideoViews,    // Reach değerini taşıyor
                            contentInteractions = latestRecord.TotalLikes, // Etkileşim değerini taşıyor
                            profileVisits = latestRecord.ProfileViews,     // Profil ziyareti
                            recordedAt = latestRecord.RecordedAt
                        }
                    });
                }

                // 2. CANLI KAZIMA VE KAYIT
                var scrapedStats = await _scraperService.ScrapeMetaBusinessSuiteAnalyticsAsync();
                scrapedStats.Platform = platform;

                _context.AccountAnalyticsHistory.Add(scrapedStats);
                await _context.SaveChangesAsync();

                // 3. YENİ KAZINAN VERİNİN DÖNÜŞÜ (Gereksiz alanlar temizlendi)
                return Ok(new
                {
                    source = "Live Scrape",
                    data = new
                    {
                        id = scrapedStats.Id,
                        platform = scrapedStats.Platform.ToString(),
                        totalFollowers = scrapedStats.TotalFollowers,
                        totalReach = scrapedStats.TotalVideoViews,
                        contentInteractions = scrapedStats.TotalLikes,
                        profileVisits = scrapedStats.ProfileViews,
                        recordedAt = scrapedStats.RecordedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Instagram (Meta) istatistikleri alınamadı.", Error = ex.Message });
            }
        }


        // 3. UI TARAFINDA "ŞİMDİ YENİLE" BUTONU İÇİN (Opsiyonel)
        [HttpPost("force-refresh")]
        public async Task<IActionResult> ForceRefreshAccountStats()
        {
            try
            {
                // DB'deki tarihi umursamadan doğrudan taze veri çeker
                var scrapedStats = await _scraperService.ScrapeAccountAnalyticsAsync();

                _context.AccountAnalyticsHistory.Add(scrapedStats);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "İstatistikler zorla yenilendi ve veritabanına yazıldı.", Data = scrapedStats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Zorla yenileme başarısız.", Error = ex.Message });
            }
        }
        // 1. DASHBOARD TABLOSU İÇİN: Tüm Videoların En Güncel Durumunu Anında Getir (Tarayıcı açmaz)
        [HttpGet("videos/latest-stats")]
        public async Task<IActionResult> GetAllVideosLatestStats()
        {
            try
            {
                // Tüm videoları ve onların SADECE EN SON kaydedilmiş analitik verisini çekiyoruz
                var videos = await _context.PublishedVideos
                    .Select(v => new
                    {
                        videoId = v.Id,
                        platform = v.Platform.ToString(),
                        postUrl = v.PostUrl,
                        publishedAt = v.PublishedAt,
                        // Tarihe göre tersten sırala ve sadece en üsttekini (en yenisini) al
                        latestStats = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(videos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Video listesi getirilirken hata oluştu.", Error = ex.Message });
            }
        }

        // 2. TEKİL VİDEO İÇİN AKILLI UÇ NOKTA (Bugün güncellenmişse DB'den, yoksa Playwright ile kazır)
        [HttpGet("videos/{videoId}/smart-stats")]
        public async Task<IActionResult> GetSmartVideoStats(int videoId)
        {
            try
            {
                var video = await _context.PublishedVideos.FindAsync(videoId);
                if (video == null) return NotFound(new { Message = "Video bulunamadı." });

                var today = DateTime.UtcNow.Date;

                // İlgili videonun DB'deki en son analitik kaydını bul
                var latestRecord = await _context.VideoAnalytics
                    .Where(va => va.PublishedVideoId == videoId)
                    .OrderByDescending(va => va.RecordedAt)
                    .FirstOrDefaultAsync();

                // 1. CACHE KONTROLÜ: Eğer DB'de kayıt varsa ve "Bugün" çekildiyse, beklemeden direkt dön
                if (latestRecord != null && latestRecord.RecordedAt.Date == today)
                {
                    return Ok(new
                    {
                        source = "Database Cache",
                        stats = new
                        {
                            views = latestRecord.Views,
                            likes = latestRecord.Likes,
                            comments = latestRecord.Comments,
                            shares = latestRecord.Shares,
                            favorites = latestRecord.Favorites,
                            recordedAt = latestRecord.RecordedAt
                        }
                    });
                }

                // 2. CANLI KAZIMA: Bugün hiç kayıt yoksa Playwright'ı sadece bu video için tetikle
                var scrapedStats = await _scraperService.ScrapeTikTokStatsAsync(video.PostUrl);

                // Entity ilişkisini (Foreign Key) manuel bağlamamız gerekiyor
                scrapedStats.PublishedVideoId = videoId;

                // DB'ye yeni taze kaydı ekle
                _context.VideoAnalytics.Add(scrapedStats);
                await _context.SaveChangesAsync();

                // 3. Yeni veriyi UI'a dön
                return Ok(new
                {
                    source = "Live Scrape",
                    stats = new
                    {
                        views = scrapedStats.Views,
                        likes = scrapedStats.Likes,
                        comments = scrapedStats.Comments,
                        shares = scrapedStats.Shares,
                        favorites = scrapedStats.Favorites,
                        recordedAt = scrapedStats.RecordedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Videonun güncel istatistikleri alınamadı.", Error = ex.Message });
            }
        }

        // 3. UI TARAFINDA "BU VİDEOYU ŞİMDİ YENİLE" BUTONU İÇİN ZORLA YENİLEME (Opsiyonel)
        [HttpPost("videos/{videoId}/force-refresh")]
        public async Task<IActionResult> ForceRefreshVideoStats(int videoId)
        {
            try
            {
                var video = await _context.PublishedVideos.FindAsync(videoId);
                if (video == null) return NotFound(new { Message = "Video bulunamadı." });

                // DB'deki tarihi umursamadan doğrudan taze veri çeker
                var scrapedStats = await _scraperService.ScrapeTikTokStatsAsync(video.PostUrl);
                scrapedStats.PublishedVideoId = videoId;

                _context.VideoAnalytics.Add(scrapedStats);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Video istatistikleri zorla yenilendi.", source = "Forced Live Scrape", stats = scrapedStats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Zorla yenileme başarısız.", Error = ex.Message });
            }
        }

        [HttpGet("leaderboards")]
        public async Task<IActionResult> GetLeaderboards()
        {
            // 1. Önce sadece ihtiyacımız olan güncel metrikleri ve etiketleri çekerek RAM'i koruyoruz
            var videosWithStats = await _context.PublishedVideos
                .Where(v => v.AnalyticsHistory.Any())
                .Select(v => new
                {
                    Fruits = v.VideoGeneration.FruitAsset.FruitsInImage.Select(f => f.Name).ToList(),
                    DanceStyle = v.VideoGeneration.ReferenceVideo != null ? v.VideoGeneration.ReferenceVideo.DanceStyle : "Bilinmiyor",
                    LatestViews = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).FirstOrDefault().Views,
                    LatestLikes = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).FirstOrDefault().Likes
                })
                .ToListAsync();

            // 2. MEYVE ŞAMPİYONLAR LİGİ (SelectMany ile çoka-çok ilişkiyi düzleştirip grupluyoruz)
            var fruitLeaderboard = videosWithStats
                .SelectMany(v => v.Fruits, (video, fruitName) => new { fruitName, video.LatestViews, video.LatestLikes })
                .GroupBy(x => x.fruitName)
                .Select(g => new
                {
                    name = g.Key,
                    videoCount = g.Count(),
                    averageViews = Math.Round(g.Average(x => x.LatestViews), 0),
                    averageLikes = Math.Round(g.Average(x => x.LatestLikes), 0)
                })
                .OrderByDescending(x => x.averageViews)
                .Take(10); // Sadece Top 10

            // 3. DANS ŞAMPİYONLAR LİGİ
            var danceLeaderboard = videosWithStats
                .Where(v => !string.IsNullOrWhiteSpace(v.DanceStyle) && v.DanceStyle != "Bilinmiyor")
                .GroupBy(v => v.DanceStyle)
                .Select(g => new
                {
                    name = g.Key,
                    videoCount = g.Count(),
                    averageViews = Math.Round(g.Average(v => v.LatestViews), 0),
                    averageLikes = Math.Round(g.Average(v => v.LatestLikes), 0)
                })
                .OrderByDescending(x => x.averageViews)
                .Take(10);

            return Ok(new
            {
                fruitLeaderboard,
                danceLeaderboard
            });
        }

        [HttpGet("solo-vs-group")]
        public async Task<IActionResult> GetSoloVsGroupAnalytics()
        {
            var stats = await _context.PublishedVideos
                .Where(v => v.AnalyticsHistory.Any())
                .Select(v => new
                {
                    IsMultiple = v.VideoGeneration.FruitAsset.IsMultipleFruits,
                    LatestViews = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).FirstOrDefault().Views,
                    LatestLikes = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).FirstOrDefault().Likes,
                    LatestShares = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).FirstOrDefault().Shares
                })
                .ToListAsync();

            var comparison = stats
                .GroupBy(x => x.IsMultiple)
                .Select(g => new
                {
                    type = g.Key ? "Multiple (Plural Fruit)" : "Single (Singular Fruit)",
                    videoCount = g.Count(),
                    totalViews = g.Sum(x => x.LatestViews),
                    averageViews = Math.Round(g.Average(x => x.LatestViews), 0),
                    averageLikes = Math.Round(g.Average(x => x.LatestLikes), 0),
                    averageShares = Math.Round(g.Average(x => x.LatestShares), 0)
                })
                .OrderByDescending(x => x.averageViews)
                .ToList();

            return Ok(comparison);
        }

        [HttpGet("engagement-metrics")]
        public async Task<IActionResult> GetEngagementMetrics()
        {
            // 1. Videoların sadece gerekli bilgilerini ve en güncel istatistiğini çekiyoruz
            var videosWithStats = await _context.PublishedVideos
                .Where(v => v.AnalyticsHistory.Any())
                .Select(v => new
                {
                    videoId = v.Id,
                    postUrl = v.PostUrl,
                    // UI'da videoyu tanımak için meyve isimlerini birleştiriyoruz
                    title = string.Join(" + ", v.VideoGeneration.FruitAsset.FruitsInImage.Select(f => f.Name)),
                    latestStat = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).FirstOrDefault()
                })
                .Where(v => v.latestStat.Views > 0) // Sıfıra bölme hatasını (DivideByZero) önlemek için
                .ToListAsync();

            // 2. Etkileşim ve Viralite Formüllerini Uyguluyoruz
            var engagementData = videosWithStats.Select(v => new
            {
                videoId = v.videoId,
                title = string.IsNullOrWhiteSpace(v.title) ? "Tekil İçerik" : v.title,
                url = v.postUrl,
                views = v.latestStat.Views,

                // Gerçek Etkileşim Oranı (ER) = (Beğeni + Yorum + Paylaşım + Favori) / İzlenme * 100
                engagementRate = Math.Round((double)(v.latestStat.Likes + v.latestStat.Comments + v.latestStat.Shares + v.latestStat.Favorites) / v.latestStat.Views * 100, 2),

                // Viral Katsayısı (K-Factor) = Paylaşım / İzlenme * 100 (TikTok algoritmasının en sevdiği metrik)
                viralFactor = Math.Round((double)v.latestStat.Shares / v.latestStat.Views * 100, 2)
            })
            .OrderByDescending(v => v.engagementRate)
            .ToList();

            // 3. Hesap Geneli Ortalamaları (Benchmark için)
            var accountAverageER = engagementData.Any() ? Math.Round(engagementData.Average(v => v.engagementRate), 2) : 0;
            var accountAverageViral = engagementData.Any() ? Math.Round(engagementData.Average(v => v.viralFactor), 2) : 0;

            return Ok(new
            {
                accountAverages = new
                {
                    averageEngagementRate = accountAverageER,
                    averageViralFactor = accountAverageViral
                },
                // Sadece etkileşim oranı en yüksek 10 videoyu döndürüyoruz
                topEngagingVideos = engagementData.Take(10)
            });
        }

        [HttpGet("fruit-combinations")]
        public async Task<IActionResult> GetFruitCombinationsAnalytics()
        {
            try
            {
                // 1. Yalnızca çoklu meyve içeren (IsMultipleFruits = true) ve analitiği olan videoları çekiyoruz
                var videos = await _context.PublishedVideos
                    .Where(v => v.VideoGeneration.FruitAsset.IsMultipleFruits && v.AnalyticsHistory.Any())
                    .Select(v => new
                    {
                        Fruits = v.VideoGeneration.FruitAsset.FruitsInImage.Select(f => f.Name).ToList(),
                        LatestStats = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).FirstOrDefault()
                    })
                    .ToListAsync();

                // 2. Kombinasyonları oluşturup grupluyoruz
                var combinationStats = videos
                    .Select(v => new
                    {
                        // Meyve isimlerini alfabetik sıraya dizip birleştiriyoruz ki 
                        // "Elma + Armut" ile "Armut + Elma" farklı kombinasyonlar sayılmasın
                        CombinationKey = string.Join(" + ", v.Fruits.OrderBy(name => name)),
                        v.LatestStats
                    })
                    .Where(v => v.LatestStats != null)
                    .GroupBy(x => x.CombinationKey)
                    .Select(g => new
                    {
                        combination = g.Key,
                        videoCount = g.Count(),

                        totalViews = g.Sum(x => x.LatestStats.Views),
                        averageViews = Math.Round(g.Average(x => x.LatestStats.Views), 0),
                        averageLikes = Math.Round(g.Average(x => x.LatestStats.Likes), 0),
                        averageShares = Math.Round(g.Average(x => x.LatestStats.Shares), 0),

                        // Kombinasyonun ortalama K-Factor (Viralite) değeri
                        averageViralFactor = Math.Round(g.Average(x => x.LatestStats.Views > 0
                            ? (double)x.LatestStats.Shares / x.LatestStats.Views * 100
                            : 0), 2)
                    })
                    .OrderByDescending(x => x.averageViews) // En çok izlenen kombinasyonlar üstte
                    .ToList();

                return Ok(combinationStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Kombinasyon analizi getirilirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("lifecycle-insights")]
        public async Task<IActionResult> GetLifecycleInsights()
        {
            // 1. En az 3 farklı güne ait analitik kaydı olan videoları çekiyoruz ki trend (eğilim) ölçebilelim
            var videos = await _context.PublishedVideos
                .Include(v => v.AnalyticsHistory)
                .Include(v => v.VideoGeneration)
                    .ThenInclude(vg => vg.FruitAsset)
                        .ThenInclude(fa => fa.FruitsInImage)
                .Where(v => v.AnalyticsHistory.Count >= 3)
                .ToListAsync();

            var lateBloomers = new List<object>();
            var lifespanData = new List<object>();

            foreach (var video in videos)
            {
                // Tarihe göre eskiden yeniye sıralı analitik geçmişi
                var history = video.AnalyticsHistory.OrderBy(a => a.RecordedAt).ToList();
                var title = string.Join(" + ", video.VideoGeneration.FruitAsset.FruitsInImage.Select(f => f.Name));
                if (string.IsNullOrWhiteSpace(title)) title = "İsimsiz İçerik";

                // --- 1. VİDEO RAF ÖMRÜ (LIFESPAN) HESAPLAMA ---
                // Videonun "aktif" sayılması için iki ölçüm arasında en az 100 izlenme artışı olmasını şart koşalım
                DateTime lastActiveDate = video.PublishedAt;

                for (int i = 1; i < history.Count; i++)
                {
                    var dailyViewIncrease = history[i].Views - history[i - 1].Views;
                    if (dailyViewIncrease > 100) // 100 izlenmeden fazla artış varsa video hala yaşıyordur
                    {
                        lastActiveDate = history[i].RecordedAt;
                    }
                }

                // Yayınlanma tarihinden, son aktif olduğu tarihe kadar geçen "Aktif Gün Sayısı"
                var activeDays = (lastActiveDate.Date - video.PublishedAt.Date).TotalDays;

                lifespanData.Add(new
                {
                    videoId = video.Id,
                    title = title,
                    publishedAt = video.PublishedAt,
                    lastActiveDate = lastActiveDate,
                    activeLifespanDays = activeDays > 0 ? activeDays : 1 // En az 1 gün
                });

                // --- 2. ALGORİTMA DİRİLİŞİ (LATE BLOOMER) TESPİTİ ---
                // Mantık: İlk 3 günün ortalama izlenme artışı ile son 3 günün ortalama izlenme artışını kıyasla.
                // Eğer aradan zaman geçmesine rağmen son günlerdeki ivme, ilk günlerden yüksekse video algoritma dirilişi yaşıyordur!

                if (history.Count >= 5) // Bu hesap için biraz daha uzun bir geçmiş lazım
                {
                    var firstDaysViews = history[2].Views - history[0].Views; // İlk ölçümden 3. ölçüme kadar olan artış

                    var latestIndex = history.Count - 1;
                    var recentDaysViews = history[latestIndex].Views - history[latestIndex - 2].Views; // Son 3 ölçümdeki artış

                    // Eğer son 3 günkü artış, ilk 3 günkü artıştan %50 daha fazlaysa (1.5 katı) ve anlamlı bir rakamsa
                    if (recentDaysViews > (firstDaysViews * 1.5) && recentDaysViews > 1000)
                    {
                        lateBloomers.Add(new
                        {
                            videoId = video.Id,
                            title = title,
                            url = video.PostUrl,
                            firstDaysIncrease = firstDaysViews,
                            recentDaysIncrease = recentDaysViews,
                            momentumMultiplier = Math.Round((double)recentDaysViews / (firstDaysViews == 0 ? 1 : firstDaysViews), 1)
                        });
                    }
                }
            }

            return Ok(new
            {
                // En uzun süre hayatta kalan (izlenmeye devam eden) videoları üstte ver
                lifespanLeaderboard = lifespanData.OrderByDescending(x => (double)x.GetType().GetProperty("activeLifespanDays").GetValue(x, null)).Take(10),

                // Aniden patlayan, "Geç Açan Çiçekleri" listele
                lateBloomers = lateBloomers.OrderByDescending(x => (double)x.GetType().GetProperty("momentumMultiplier").GetValue(x, null)).ToList()
            });
        }
        [HttpPost("golden-hours-heatmap")]
        public async Task<IActionResult> GenerateGoldenHoursHeatmap(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Lütfen geçerli bir CSV dosyası yükleyin.");

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Sadece .csv uzantılı TikTok Activity dosyaları desteklenir.");

            var records = new List<FollowerActivityRecord>();

            try
            {
                // 1. Dosyayı diske kaydetmeden direkt RAM (Stream) üzerinden okuyoruz
                using (var stream = new StreamReader(file.OpenReadStream()))
                {
                    // İlk satırı (Başlıkları - Header) okuyup atlıyoruz
                    await stream.ReadLineAsync();

                    while (!stream.EndOfStream)
                    {
                        var line = await stream.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // CSV sütunlarını virgülle ayır (Format genelde: Tarih, Saat, Aktif Takipçi)
                        var columns = line.Split(',');

                        if (columns.Length >= 3)
                        {
                            records.Add(new FollowerActivityRecord
                            {
                                Date = columns[0].Trim(),
                                Hour = columns[1].Trim(),
                                // Eğer sayı içinde tırnak varsa temizleyip int'e çeviriyoruz
                                ActiveFollowers = int.TryParse(columns[2].Replace("\"", "").Trim(), out int active) ? active : 0
                            });
                        }
                    }
                }

                if (records.Count == 0)
                    return BadRequest("CSV dosyası okunamadı veya içi boş.");

                // 2. ISI HARİTASI (HEATMAP) ANALİZİ
                // Aynı saat dilimlerini (Örn: 15:00) tüm günler için gruplayıp "Ortalama" aktif kişi sayısını buluyoruz
                // 2. ISI HARİTASI (HEATMAP) ANALİZİ
                var heatmapData = records
                    // Gelen saat verisindeki (Örn: "\"16\"") gereksiz tırnakları temizliyoruz
                    .GroupBy(r => r.Hour.Replace("\"", "").Trim())
                    .Select(g => new
                    {
                        hour = g.Key,
                        averageActiveFollowers = Math.Round(g.Average(r => r.ActiveFollowers), 0),
                        totalActiveInPeriod = g.Sum(r => r.ActiveFollowers)
                    })
                    // En aktif (kalabalık) saatleri en üste dizecek şekilde sıralıyoruz
                    .OrderByDescending(x => x.averageActiveFollowers)
                    .ToList();

                // 3. SÖRF STRATEJİSİ (Altın Saat Hesaplaması)
                var peakHourData = heatmapData.FirstOrDefault();

                int peakHour = 0;
                int recommendedHour = 0;

                if (peakHourData != null && int.TryParse(peakHourData.hour, out peakHour))
                {
                    // Zirveden 2 saat öncesini hesapla (Gece 00:00 geçişleri için +24 ve %24 kullanıyoruz)
                    recommendedHour = (peakHour - 2 + 24) % 24;
                }

                return Ok(new
                {
                    message = "Isı haritası ve sörf stratejisi başarıyla oluşturuldu.",
                    goldenHour = new
                    {
                        peakTime = $"{peakHour}:00",
                        recommendedPostingTime = $"{recommendedHour}:00",
                        expectedAudienceAtPeak = peakHourData?.averageActiveFollowers,
                        recommendation = $"Zirve saatiniz {peakHour}:00. Ancak TikTok algoritmasının videonuzu test edip ana kitleye (For You) ulaştırması için dalga kabarmadan önce, yani {recommendedHour}:00 civarında paylaşım yapmanız maksimum ivmeyi (Sörf Etkisi) getirecektir."
                    },
                    heatmap = heatmapData // Tablo/Grafik çizimi için tam liste
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "CSV işlenirken bir hata oluştu.", Error = ex.Message });
            }
        }
        [HttpGet("instagram/live-demographics")]
        public async Task<IActionResult> GetLiveInstagramDemographics()
        {
            try
            {
                // Direkt olarak servisteki otonom metodu çağırıyoruz. DB'ye yazmak yok.
                var demographics = await _scraperService.GetLiveDemographicsAsync();

                return Ok(new
                {
                    message = "Canlı demografik veriler başarıyla çekildi.",
                    data = demographics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Demografik veriler alınırken hata oluştu.", error = ex.Message });
            }
        }


        // 2. DASHBOARD KARTLARI İÇİN: Akıllı Cache Mantığı (Platform Destekli)
        /*[HttpGet("latest-account-stats")]
        public async Task<IActionResult> GetLatestAccountStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                // DB'deki en son kaydı bul
                var latestRecord = await _context.AccountAnalyticsHistory
                    .OrderByDescending(a => a.RecordedAt)
                    .FirstOrDefaultAsync();

                // Eğer DB'de kayıt varsa ve "Bugün" çekildiyse, tarayıcıyı açmadan direkt dön
                if (latestRecord != null && latestRecord.RecordedAt.Date == today)
                {
                    return Ok(new
                    {
                        lifetimeLikes = latestRecord.LifetimeLikes,
                        totalFollowers = latestRecord.TotalFollowers,
                        followingCount = latestRecord.FollowingCount,
                        totalVideoViews = latestRecord.TotalVideoViews,
                        profileViews = latestRecord.ProfileViews,
                        totalLikes = latestRecord.TotalLikes,
                        totalComments = latestRecord.TotalComments,
                        totalShares = latestRecord.TotalShares,
                        estimatedRewards = latestRecord.EstimatedRewards,
                        recordedAt = latestRecord.RecordedAt,
                        source = "Database Cache" // UI'da verinin beklemeden geldiğini görebilirsin
                    });
                }

                // Eğer bugün hiç kayıt yoksa (veya DB tamamen boşsa) Playwright'ı tetikle
                var scrapedStats = await _scraperService.ScrapeAccountAnalyticsAsync();

                // Gelen Entity nesnesini direkt veritabanına kaydet
                _context.AccountAnalyticsHistory.Add(scrapedStats);
                await _context.SaveChangesAsync();

                // Yeni kaydedilen veriyi ön yüze dön
                return Ok(new
                {
                    lifetimeLikes = scrapedStats.LifetimeLikes,
                    totalFollowers = scrapedStats.TotalFollowers,
                    followingCount = scrapedStats.FollowingCount,
                    totalVideoViews = scrapedStats.TotalVideoViews,
                    profileViews = scrapedStats.ProfileViews,
                    totalLikes = scrapedStats.TotalLikes,
                    totalComments = scrapedStats.TotalComments,
                    totalShares = scrapedStats.TotalShares,
                    estimatedRewards = scrapedStats.EstimatedRewards,
                    recordedAt = scrapedStats.RecordedAt,
                    source = "Live Scrape" // Yeni çekildi
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Güncel istatistikler alınamadı.", Error = ex.Message });
            }
        }*/
    }
}