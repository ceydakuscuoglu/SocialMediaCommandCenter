using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ShakyFruits.Core.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using ShakyFruits.Data;
using ShakyFruits.Services;

namespace ShakyFruits.API.Workers
{
    public class AnalyticsScraperWorker : BackgroundService
    {
        private readonly ILogger<AnalyticsScraperWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AnalyticsScraperWorker(ILogger<AnalyticsScraperWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Analytics Scraper Worker başlatıldı...");
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation($"--- Analitik Taraması Başladı: {DateTime.Now} ---");

                // 1. ÖNCE HESAP GENELİ (Overview) TARANACAK
                await ProcessAccountOverviewAsync(stoppingToken);

                // 2. SONRA TEKİL VİDEOLAR TARANACAK
                await ProcessIndividualVideosAsync(stoppingToken);

                _logger.LogInformation($"--- Bu Tur Tamamlandı ---");
            }
        }

        // 1. GÖREV: Sadece Tekil Videoları Tarayan Fonksiyon
        private async Task ProcessIndividualVideosAsync(CancellationToken stoppingToken)
        {
            // Bu fonksiyona özel yepyeni bir Scope ve DbContext açıyoruz (Thread-Safe)
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scraperService = scope.ServiceProvider.GetRequiredService<SocialMediaScraperService>();

            try
            {
                var activeVideos = await dbContext.PublishedVideos
                                                  .Where(v => v.Platform == Core.Enums.SocialPlatform.TikTok)
                                                  .ToListAsync(stoppingToken);

                if (activeVideos.Count == 0)
                {
                    _logger.LogInformation("[Video Tarama] Taranacak video bulunamadı.");
                    return; // Fonksiyondan çık
                }

                foreach (var video in activeVideos)
                {
                    if (stoppingToken.IsCancellationRequested) break;
                    _logger.LogInformation($"[Video Tarama] Kazınıyor: {video.PostUrl}");

                    try
                    {
                        var stats = await scraperService.ScrapeTikTokStatsAsync(video.PostUrl);
                        var newAnalyticsRecord = new VideoAnalytics
                        {
                            PublishedVideoId = video.Id,
                            Views = stats.Views,
                            Likes = stats.Likes,
                            Comments = stats.Comments,
                            Shares = stats.Shares,
                            Favorites = stats.Favorites,
                            RecordedAt = DateTime.UtcNow
                        };

                        dbContext.VideoAnalytics.Add(newAnalyticsRecord);
                        _logger.LogInformation($"[Video Tarama] Başarılı: {video.Id} ID'li video için {stats.Views} izlenme.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[Video Tarama] Hata (ID: {video.Id}): {ex.Message}");
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Video Tarama] Genel Hata: {ex.Message}");
            }
        }

        // 2. GÖREV: Sadece Hesap Özetini Tarayan Fonksiyon
        private async Task ProcessAccountOverviewAsync(CancellationToken stoppingToken)
        {
            // Bu fonksiyona özel yepyeni bir Scope ve DbContext açıyoruz
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scraperService = scope.ServiceProvider.GetRequiredService<SocialMediaScraperService>();

            try
            {
                _logger.LogInformation("[Hesap Tarama] TikTok Studio Genel Analizleri kazınıyor...");

                var accountStats = await scraperService.ScrapeAccountAnalyticsAsync();
                var newAccountRecord = new AccountAnalyticsHistory
                {
                    // Yeni eklenenler
                    LifetimeLikes = accountStats.LifetimeLikes,
                    TotalFollowers = accountStats.TotalFollowers,
                    FollowingCount = accountStats.FollowingCount,

                    // Eskiler
                    TotalVideoViews = accountStats.TotalVideoViews,
                    ProfileViews = accountStats.ProfileViews,
                    TotalLikes = accountStats.TotalLikes,
                    TotalComments = accountStats.TotalComments,
                    TotalShares = accountStats.TotalShares,
                    EstimatedRewards = accountStats.EstimatedRewards,
                    RecordedAt = DateTime.UtcNow
                };

                dbContext.AccountAnalyticsHistory.Add(newAccountRecord);
                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation($"[Hesap Tarama] Başarılı. Toplam İzlenme: {accountStats.TotalVideoViews}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Hesap Tarama] Hata: {ex.Message}");
            }
        }
    }
}