using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShakyFruits.Data;
using ShakyFruits.Services; // Senin servis namespace'ine göre ayarla
using ShakyFruits.Data; // DbContext'in olduğu namespace'i eklemeyi unutma

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
    }
}