using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShakyFruits.Data; // DbContext'in olduğu namespace'i eklemeyi unutma
using ShakyFruits.Services; // Senin servis namespace'ine göre ayarla
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Entities;

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

        // 3. GET: UI için Hesap Geneli Tarihsel Verileri Getir
        [HttpGet("account-history")]
        public async Task<IActionResult> GetAccountHistory()
        {
            try
            {
                // React'in grafikleri çizebilmesi için eskiden yeniye doğru sıralı gönderiyoruz
                var history = await _context.AccountAnalyticsHistory
                 .OrderBy(a => a.RecordedAt)
                 .Select(a => new
                 {
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

        // 2. DASHBOARD KARTLARI İÇİN: Akıllı Cache Mantığı (YENİ)
        [HttpGet("latest-account-stats")]
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
    }
}