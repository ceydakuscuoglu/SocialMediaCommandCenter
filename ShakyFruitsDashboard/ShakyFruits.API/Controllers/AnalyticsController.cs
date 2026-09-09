using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShakyFruits.Core.DTOs;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Interfaces;

namespace ShakyFruits.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("test-tiktok-scraper")]
        public async Task<IActionResult> TestTikTokScraper([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Lütfen test etmek için bir TikTok video linki (url) parametresi gönderin.");

            try
            {
                var stats = await _analyticsService.TestTikTokScraperAsync(url);
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
                        stats.Favorites,
                        stats.RecordedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Kazıma motoru çöktü!", Error = ex.Message });
            }
        }

        [HttpGet("internal-stats")]
        public async Task<IActionResult> GetInternalStats()
        {
            try
            {
                var stats = await _analyticsService.GetInternalStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "İstatistikler çekilirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("published-videos")]
        public async Task<IActionResult> GetPublishedVideos()
        {
            try
            {
                var videos = await _analyticsService.GetPublishedVideosAsync();
                return Ok(videos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Yayınlanan videolar listelenemedi.", Error = ex.Message });
            }
        }

        [HttpPost("track-video")]
        [HttpPost("publish-video")] // Geriye dönük uyumluluk için alias
        public async Task<IActionResult> TrackPublishedVideo([FromBody] PublishVideoRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PostUrl))
                return BadRequest("Geçersiz veri gönderildi. Lütfen bir TikTok Studio URL'si girin.");

            try
            {
                var video = await _analyticsService.TrackPublishedVideoAsync(request.VideoGenerationId, request.PostUrl);
                return Ok(new
                {
                    Message = "Video takip listesine başarıyla eklendi.",
                    Data = new
                    {
                        video.Id,
                        video.VideoGenerationId,
                        Platform = (int)video.Platform,
                        video.PostUrl,
                        video.PublishedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Video kaydedilirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("account-history")]
        public async Task<IActionResult> GetAccountHistory([FromQuery] SocialPlatform? platform)
        {
            try
            {
                var history = await _analyticsService.GetAccountHistoryAsync(platform);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Hesap geçmişi çekilemedi.", Error = ex.Message });
            }
        }

        [HttpGet("tiktok/latest-account-stats")]
        public async Task<IActionResult> GetLatestTikTokStats()
        {
            try
            {
                var stats = await _analyticsService.GetLatestTikTokStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "En son TikTok verisi çekilemedi.", Error = ex.Message });
            }
        }

        [HttpGet("instagram/latest-account-stats")]
        public async Task<IActionResult> GetLatestInstagramStats()
        {
            try
            {
                var stats = await _analyticsService.GetLatestInstagramStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "En son Instagram verisi çekilemedi.", Error = ex.Message });
            }
        }

        [HttpPost("force-refresh")]
        public async Task<IActionResult> ForceRefreshAccountStats()
        {
            try
            {
                await _analyticsService.ForceRefreshAccountStatsAsync();
                return Ok(new { Message = "Hesap analitiği taraması başarıyla tamamlandı ve veritabanına kaydedildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Manuel tarama sırasında hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("videos/latest-stats")]
        public async Task<IActionResult> GetAllVideosLatestStats()
        {
            try
            {
                var stats = await _analyticsService.GetAllVideosLatestStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Video özet istatistikleri çekilemedi.", Error = ex.Message });
            }
        }

        [HttpGet("videos/{videoId}/smart-stats")]
        public async Task<IActionResult> GetVideoSmartStats(int videoId)
        {
            try
            {
                var stats = await _analyticsService.GetVideoSmartStatsAsync(videoId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Videonun güncel istatistikleri alınamadı.", Error = ex.Message });
            }
        }

        [HttpPost("videos/{videoId}/force-refresh")]
        public async Task<IActionResult> ForceRefreshVideoStats(int videoId)
        {
            try
            {
                var result = await _analyticsService.ForceRefreshVideoStatsAsync(videoId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Zorla yenileme başarısız.", Error = ex.Message });
            }
        }

        [HttpPost("videos/force-refresh-all")]
        public async Task<IActionResult> ForceRefreshAllVideosStats()
        {
            try
            {
                var result = await _analyticsService.ForceRefreshAllVideosStatsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Tüm videoların zorla yenilenmesi sırasında hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("leaderboards")]
        public async Task<IActionResult> GetLeaderboards()
        {
            try
            {
                var leaderboards = await _analyticsService.GetLeaderboardsAsync();
                return Ok(leaderboards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Liderlik tabloları yüklenemedi.", Error = ex.Message });
            }
        }

        [HttpGet("solo-vs-group")]
        public async Task<IActionResult> GetSoloVsGroupAnalytics()
        {
            try
            {
                var analytics = await _analyticsService.GetSoloVsGroupAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Solo vs Grup verisi hesaplanamadı.", Error = ex.Message });
            }
        }

        [HttpGet("engagement-metrics")]
        public async Task<IActionResult> GetEngagementMetrics()
        {
            try
            {
                var metrics = await _analyticsService.GetEngagementMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Etkileşim metrikleri hesaplanamadı.", Error = ex.Message });
            }
        }

        [HttpGet("fruit-combinations")]
        public async Task<IActionResult> GetFruitCombinations()
        {
            try
            {
                var combinations = await _analyticsService.GetFruitCombinationsAsync();
                return Ok(combinations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Kombinasyon analizi yapılamadı.", Error = ex.Message });
            }
        }

        [HttpGet("lifecycle-insights")]
        public async Task<IActionResult> GetLifecycleInsights()
        {
            try
            {
                var insights = await _analyticsService.GetLifecycleInsightsAsync();
                return Ok(insights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Yaşam döngüsü verileri hesaplanamadı.", Error = ex.Message });
            }
        }

        [HttpPost("golden-hours-heatmap")]
        public async Task<IActionResult> GenerateGoldenHoursHeatmap(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Lütfen geçerli bir CSV dosyası yükleyin.");

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Sadece .csv uzantılı TikTok Activity dosyaları desteklenir.");

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _analyticsService.GenerateGoldenHoursHeatmapAsync(stream);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Isı haritası analizi sırasında hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("instagram/live-demographics")]
        public async Task<IActionResult> GetInstagramLiveDemographics()
        {
            try
            {
                var demo = await _analyticsService.GetInstagramLiveDemographicsAsync();
                return demo != null
                    ? Ok(new { Message = "Instagram demografi verileri başarıyla çekildi.", Data = demo })
                    : NotFound(new { Message = "Instagram demografi verileri çekilemedi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Canlı demografi kazıma sırasında hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("tiktok/live-trends")]
        public async Task<IActionResult> GetLiveTrends()
        {
            try
            {
                // Artık platform parametresi göndermene gerek yok
                var result = await _analyticsService.GetTikTokDailyTrendsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Trendler alınırken hata oluştu.", error = ex.Message });
            }
        }
    }
}