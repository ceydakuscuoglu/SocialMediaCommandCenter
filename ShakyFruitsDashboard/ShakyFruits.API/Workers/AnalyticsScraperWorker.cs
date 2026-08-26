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

                // Test için 1 dakika (60 sn) ayarlıyoruz. Canlıda bunu 12 veya 24 saat yapabilirsin.
                // Örn: TimeSpan.FromHours(12)
                var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation($"Analitik taraması başladı: {DateTime.Now}");

                    try
                    {
                        // 1. Singleton içinden Scoped servislere erişmek için yeni bir Scope (Kapsam) açıyoruz
                        using var scope = _scopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var scraperService = scope.ServiceProvider.GetRequiredService<SocialMediaScraperService>();

                        // 2. Sistemdeki tüm "Yayınlanmış Videoları" bul (Platformu TikTok olanları al)
                        // İleride IG veya YT gelirse filtreyi genişletiriz
                        var activeVideos = await dbContext.PublishedVideos
                                                          .Where(v => v.Platform == Core.Enums.SocialPlatform.TikTok)
                                                          .ToListAsync(stoppingToken);

                        if (activeVideos.Count == 0)
                        {
                            _logger.LogInformation("Taranacak video bulunamadı. Beklemeye geçiliyor.");
                            continue;
                        }

                        // 3. Her bir videonun URL'sine git ve istatistikleri çek
                        foreach (var video in activeVideos)
                        {
                            if (stoppingToken.IsCancellationRequested) break;

                            _logger.LogInformation($"Kazınıyor: {video.PostUrl}");

                            try
                            {
                                // Kazıyıcı motoru çalıştır
                                var stats = await scraperService.ScrapeTikTokStatsAsync(video.PostUrl);

                                // 4. Gelen veriyi veritabanındaki VideoAnalytics tablosuna kaydet
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

                                _logger.LogInformation($"Başarılı: {video.Id} ID'li video için {stats.Views} izlenme kaydedildi.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Video kazınırken hata oluştu (ID: {video.Id}). Hata: {ex.Message}");
                                // Bir video hata verirse döngüyü kırma, diğer videoya geç
                            }
                        }

                        // 5. Tüm işlemleri topluca veritabanına kaydet
                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Tüm istatistikler başarıyla veritabanına kaydedildi.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Worker genel bir hata yakaladı: {ex.Message}");
                    }
                }
            }
        }
    }