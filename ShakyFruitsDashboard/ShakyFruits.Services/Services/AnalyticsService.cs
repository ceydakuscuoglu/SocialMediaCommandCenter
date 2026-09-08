using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Interfaces;
using ShakyFruits.Core.Models;
using ShakyFruits.Data;

namespace ShakyFruits.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly SocialMediaScraperService _scraperService;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            ApplicationDbContext context,
            SocialMediaScraperService scraperService,
            ILogger<AnalyticsService> logger)
        {
            _context = context;
            _scraperService = scraperService;
            _logger = logger;
        }

        public async Task<VideoAnalytics> TestTikTokScraperAsync(string url)
        {
            return await _scraperService.ScrapeTikTokStatsAsync(url);
        }

        public async Task<object> GetInternalStatsAsync()
        {
            var totalVideos = await _context.VideoGenerations.CountAsync();

            var fruitUsage = await _context.VideoGenerations
                .Include(v => v.FruitAsset)
                .Where(v => v.FruitAsset != null)
                .GroupBy(v => v.FruitAsset!.Title)
                .Select(g => new { FruitName = g.Key, UsageCount = g.Count() })
                .OrderByDescending(x => x.UsageCount)
                .ToListAsync();

            var danceStyleUsage = await _context.VideoGenerations
                .Include(v => v.ReferenceVideo)
                .Where(v => v.ReferenceVideo != null && !string.IsNullOrEmpty(v.ReferenceVideo.DanceStyle))
                .GroupBy(v => v.ReferenceVideo!.DanceStyle)
                .Select(g => new { DanceStyle = g.Key, UsageCount = g.Count() })
                .OrderByDescending(x => x.UsageCount)
                .ToListAsync();

            var statusDistribution = await _context.VideoGenerations
                .GroupBy(v => v.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            return new
            {
                Overview = new { TotalGenerations = totalVideos },
                FruitStats = fruitUsage,
                DanceStats = danceStyleUsage,
                SystemHealth = statusDistribution
            };
        }

        public async Task<object> GetPublishedVideosAsync()
        {
            return await _context.PublishedVideos
                .OrderByDescending(p => p.PublishedAt)
                .Select(p => new
                {
                    id = p.Id,
                    videoGenerationId = p.VideoGenerationId,
                    platform = (int)p.Platform,
                    postUrl = p.PostUrl,
                    publishedAt = p.PublishedAt,
                    latestStats = p.AnalyticsHistory
                        .OrderByDescending(a => a.RecordedAt)
                        .Select(a => new
                        {
                            views = a.Views,
                            likes = a.Likes,
                            comments = a.Comments,
                            shares = a.Shares,
                            favorites = a.Favorites,
                            recordedAt = a.RecordedAt
                        }).FirstOrDefault(),
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
        }

        public async Task<PublishedVideo> TrackPublishedVideoAsync(int videoGenerationId, string postUrl)
        {
            var videoExists = await _context.VideoGenerations.AnyAsync(v => v.Id == videoGenerationId);
            if (!videoExists)
            {
                throw new KeyNotFoundException($"Sistemde {videoGenerationId} ID'li bir üretim kaydı bulunamadı.");
            }

            var newPublishedVideo = new PublishedVideo
            {
                VideoGenerationId = videoGenerationId,
                Platform = SocialPlatform.TikTok,
                PostUrl = postUrl,
                PublishedAt = DateTime.UtcNow
            };

            _context.PublishedVideos.Add(newPublishedVideo);
            await _context.SaveChangesAsync();

            return newPublishedVideo;
        }

        public async Task<List<AccountAnalyticsHistory>> GetAccountHistoryAsync(SocialPlatform? platform)
        {
            var query = _context.AccountAnalyticsHistory.AsQueryable();

            if (platform.HasValue)
            {
                query = query.Where(a => a.Platform == platform.Value);
            }

            return await query
                .OrderByDescending(a => a.RecordedAt)
                .Take(30)
                .ToListAsync();
        }

        public async Task<object?> GetLatestAccountStatsAsync(SocialPlatform platform)
        {
            var latestRecord = await _context.AccountAnalyticsHistory
                .Where(a => a.Platform == platform)
                .OrderByDescending(a => a.RecordedAt)
                .FirstOrDefaultAsync();

            if (latestRecord == null) return null;

            var previousRecord = await _context.AccountAnalyticsHistory
                .Where(a => a.Platform == platform && a.RecordedAt < latestRecord.RecordedAt.AddDays(-6))
                .OrderByDescending(a => a.RecordedAt)
                .FirstOrDefaultAsync();

            return new
            {
                totalFollowers = latestRecord.TotalFollowers,
                followerGrowth = previousRecord != null ? latestRecord.TotalFollowers - previousRecord.TotalFollowers : 0,
                totalVideoViews = latestRecord.TotalVideoViews,
                viewsGrowth = previousRecord != null ? latestRecord.TotalVideoViews - previousRecord.TotalVideoViews : 0,
                profileViews = latestRecord.ProfileViews,
                totalLikes = latestRecord.TotalLikes,
                totalComments = latestRecord.TotalComments,
                totalShares = latestRecord.TotalShares,
                recordedAt = latestRecord.RecordedAt
            };
        }

        public async Task<object> GetLatestTikTokStatsAsync()
        {
            var platform = SocialPlatform.TikTok;
            var today = DateTime.UtcNow.Date;

            var latestRecord = await _context.AccountAnalyticsHistory
                .Where(a => a.Platform == platform)
                .OrderByDescending(a => a.RecordedAt)
                .FirstOrDefaultAsync();

            if (latestRecord != null && latestRecord.RecordedAt.Date == today)
            {
                return new
                {
                    source = "Database Cache",
                    data = new
                    {
                        id = latestRecord.Id,
                        platform = latestRecord.Platform.ToString(),
                        lifetimeLikes = latestRecord.LifetimeLikes,
                        totalFollowers = latestRecord.TotalFollowers,
                        followingCount = latestRecord.FollowingCount,
                        totalVideoViews = latestRecord.TotalVideoViews,
                        profileViews = latestRecord.ProfileViews,
                        totalLikes = latestRecord.TotalLikes,
                        totalComments = latestRecord.TotalComments,
                        totalShares = latestRecord.TotalShares,
                        estimatedRewards = latestRecord.EstimatedRewards,
                        recordedAt = latestRecord.RecordedAt
                    }
                };
            }

            try
            {
                var scrapedStats = await _scraperService.ScrapeAccountAnalyticsAsync();
                if (scrapedStats != null)
                {
                    scrapedStats.Platform = platform;
                    _context.AccountAnalyticsHistory.Add(scrapedStats);
                    await _context.SaveChangesAsync();
                    return new
                    {
                        source = "Live Scrape",
                        data = new
                        {
                            id = scrapedStats.Id,
                            platform = scrapedStats.Platform.ToString(),
                            lifetimeLikes = scrapedStats.LifetimeLikes,
                            totalFollowers = scrapedStats.TotalFollowers,
                            followingCount = scrapedStats.FollowingCount,
                            totalVideoViews = scrapedStats.TotalVideoViews,
                            profileViews = scrapedStats.ProfileViews,
                            totalLikes = scrapedStats.TotalLikes,
                            totalComments = scrapedStats.TotalComments,
                            totalShares = scrapedStats.TotalShares,
                            estimatedRewards = scrapedStats.EstimatedRewards,
                            recordedAt = scrapedStats.RecordedAt
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TikTok live scraping failed, falling back to database cache.");
            }

            if (latestRecord != null)
            {
                return new
                {
                    source = "Database Cache",
                    data = new
                    {
                        id = latestRecord.Id,
                        platform = latestRecord.Platform.ToString(),
                        lifetimeLikes = latestRecord.LifetimeLikes,
                        totalFollowers = latestRecord.TotalFollowers,
                        followingCount = latestRecord.FollowingCount,
                        totalVideoViews = latestRecord.TotalVideoViews,
                        profileViews = latestRecord.ProfileViews,
                        totalLikes = latestRecord.TotalLikes,
                        totalComments = latestRecord.TotalComments,
                        totalShares = latestRecord.TotalShares,
                        estimatedRewards = latestRecord.EstimatedRewards,
                        recordedAt = latestRecord.RecordedAt
                    }
                };
            }

            return new
            {
                source = "Initial",
                data = new
                {
                    id = 0,
                    platform = "TikTok",
                    lifetimeLikes = 0,
                    totalFollowers = 0,
                    followingCount = 0,
                    totalVideoViews = 0,
                    profileViews = 0,
                    totalLikes = 0,
                    totalComments = 0,
                    totalShares = 0,
                    estimatedRewards = 0,
                    recordedAt = DateTime.UtcNow
                }
            };
        }

        public async Task<object> GetLatestInstagramStatsAsync()
        {
            var platform = SocialPlatform.Instagram;
            var today = DateTime.UtcNow.Date;

            var latestRecord = await _context.AccountAnalyticsHistory
                .Where(a => a.Platform == platform)
                .OrderByDescending(a => a.RecordedAt)
                .FirstOrDefaultAsync();

            if (latestRecord != null && latestRecord.RecordedAt.Date == today)
            {
                return new
                {
                    source = "Database Cache",
                    data = new
                    {
                        id = latestRecord.Id,
                        platform = latestRecord.Platform.ToString(),
                        totalFollowers = latestRecord.TotalFollowers,
                        totalReach = latestRecord.TotalVideoViews,
                        contentInteractions = latestRecord.TotalLikes,
                        profileVisits = latestRecord.ProfileViews,
                        recordedAt = latestRecord.RecordedAt
                    }
                };
            }

            try
            {
                var scrapedStats = await _scraperService.ScrapeMetaBusinessSuiteAnalyticsAsync();
                if (scrapedStats != null)
                {
                    scrapedStats.Platform = platform;
                    _context.AccountAnalyticsHistory.Add(scrapedStats);
                    await _context.SaveChangesAsync();
                    return new
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
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Instagram live scraping failed, falling back to database cache.");
            }

            if (latestRecord != null)
            {
                return new
                {
                    source = "Database Cache",
                    data = new
                    {
                        id = latestRecord.Id,
                        platform = latestRecord.Platform.ToString(),
                        totalFollowers = latestRecord.TotalFollowers,
                        totalReach = latestRecord.TotalVideoViews,
                        contentInteractions = latestRecord.TotalLikes,
                        profileVisits = latestRecord.ProfileViews,
                        recordedAt = latestRecord.RecordedAt
                    }
                };
            }

            return new
            {
                source = "Initial",
                data = new
                {
                    id = 0,
                    platform = "Instagram",
                    totalFollowers = 0,
                    totalReach = 0,
                    contentInteractions = 0,
                    profileVisits = 0,
                    recordedAt = DateTime.UtcNow
                }
            };
        }

        public async Task ForceRefreshAccountStatsAsync()
        {
            _logger.LogInformation("Manuel genel hesap kazıması tetiklendi...");
            var tiktokHistory = await _scraperService.ScrapeAccountAnalyticsAsync();
            if (tiktokHistory != null)
            {
                _context.AccountAnalyticsHistory.Add(tiktokHistory);
            }

            var instagramHistory = await _scraperService.ScrapeMetaBusinessSuiteAnalyticsAsync();
            if (instagramHistory != null)
            {
                _context.AccountAnalyticsHistory.Add(instagramHistory);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<object> GetAllVideosLatestStatsAsync()
        {
            var videos = await _context.PublishedVideos
                .Include(v => v.VideoGeneration)
                    .ThenInclude(vg => vg.FruitAsset)
                .Include(v => v.VideoGeneration)
                    .ThenInclude(vg => vg.ReferenceVideo)
                .Include(v => v.AnalyticsHistory)
                .ToListAsync();

            return videos.Select(v =>
            {
                var latestRecord = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).FirstOrDefault();
                return new
                {
                    id = v.Id,
                    videoId = v.Id,
                    videoGenerationId = v.VideoGenerationId,
                    platform = v.Platform.ToString(),
                    postUrl = v.PostUrl,
                    publishedAt = v.PublishedAt,
                    fruitTitle = v.VideoGeneration?.FruitAsset?.Title ?? "Bilinmiyor",
                    danceStyle = v.VideoGeneration?.ReferenceVideo?.DanceStyle ?? "Bilinmiyor",
                    latestStats = latestRecord != null ? new
                    {
                        views = latestRecord.Views,
                        likes = latestRecord.Likes,
                        comments = latestRecord.Comments,
                        shares = latestRecord.Shares,
                        favorites = latestRecord.Favorites,
                        recordedAt = latestRecord.RecordedAt
                    } : null,
                    views = latestRecord?.Views ?? 0,
                    likes = latestRecord?.Likes ?? 0,
                    comments = latestRecord?.Comments ?? 0,
                    shares = latestRecord?.Shares ?? 0,
                    favorites = latestRecord?.Favorites ?? 0,
                    lastScrapedAt = latestRecord?.RecordedAt
                };
            }).ToList();
        }

        public async Task<object> GetVideoSmartStatsAsync(int videoId)
        {
            var video = await _context.PublishedVideos
                .Include(v => v.AnalyticsHistory)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null) throw new KeyNotFoundException("Video bulunamadı.");

            var today = DateTime.UtcNow.Date;
            var latestRecord = video.AnalyticsHistory
                .Where(a => a.RecordedAt.Date == today)
                .OrderByDescending(a => a.RecordedAt)
                .FirstOrDefault();

            if (latestRecord != null)
            {
                return new
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
                };
            }

            var scrapedStats = await _scraperService.ScrapeTikTokStatsAsync(video.PostUrl);
            scrapedStats.PublishedVideoId = videoId;
            _context.VideoAnalytics.Add(scrapedStats);
            await _context.SaveChangesAsync();

            return new
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
            };
        }

        public async Task<object> ForceRefreshVideoStatsAsync(int videoId)
        {
            var video = await _context.PublishedVideos.FindAsync(videoId);
            if (video == null) throw new KeyNotFoundException("Video bulunamadı.");

            var scrapedStats = await _scraperService.ScrapeTikTokStatsAsync(video.PostUrl);
            scrapedStats.PublishedVideoId = videoId;

            _context.VideoAnalytics.Add(scrapedStats);
            await _context.SaveChangesAsync();

            return new
            {
                Message = "Video istatistikleri zorla yenilendi.",
                source = "Forced Live Scrape",
                stats = scrapedStats
            };
        }

        public async Task<object> GetLeaderboardsAsync()
        {
            var videosWithStats = await _context.PublishedVideos
                .Where(v => v.AnalyticsHistory.Any())
                .Select(v => new
                {
                    Fruits = v.VideoGeneration.FruitAsset.FruitsInImage.Select(f => f.Name).ToList(),
                    DanceStyle = v.VideoGeneration.ReferenceVideo != null ? v.VideoGeneration.ReferenceVideo.DanceStyle : "Bilinmiyor",
                    LatestViews = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).Select(a => (double?)a.Views).FirstOrDefault() ?? 0,
                    LatestLikes = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).Select(a => (double?)a.Likes).FirstOrDefault() ?? 0
                })
                .ToListAsync();

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
                .Take(10)
                .ToList();

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
                .Take(10)
                .ToList();

            return new
            {
                fruitLeaderboard,
                danceLeaderboard
            };
        }

        public async Task<object> GetSoloVsGroupAnalyticsAsync()
        {
            var stats = await _context.PublishedVideos
                .Where(v => v.AnalyticsHistory.Any())
                .Select(v => new
                {
                    IsMultiple = v.VideoGeneration.FruitAsset.IsMultipleFruits,
                    LatestViews = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).Select(a => (double?)a.Views).FirstOrDefault() ?? 0,
                    LatestLikes = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).Select(a => (double?)a.Likes).FirstOrDefault() ?? 0,
                    LatestShares = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).Select(a => (double?)a.Shares).FirstOrDefault() ?? 0
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

            return comparison;
        }

        public async Task<object> GetEngagementMetricsAsync()
        {
            var videos = await _context.PublishedVideos
                .Where(v => v.AnalyticsHistory.Any())
                .Include(v => v.VideoGeneration).ThenInclude(vg => vg.FruitAsset)
                .Include(v => v.AnalyticsHistory)
                .ToListAsync();

            var engagingVideos = videos.Select(v =>
            {
                var latest = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).First();
                double views = latest.Views > 0 ? latest.Views : 1;
                double viralFactor = Math.Round(((double)latest.Shares / views) * 100, 2);
                double commentRatio = Math.Round(((double)latest.Comments / views) * 100, 2);
                double likeRatio = Math.Round(((double)latest.Likes / views) * 100, 2);

                return new
                {
                    videoId = v.Id,
                    postUrl = v.PostUrl,
                    title = v.VideoGeneration?.FruitAsset?.Title ?? "Bilinmiyor",
                    viralFactor,
                    commentRatio,
                    likeRatio,
                    views = latest.Views,
                    shares = latest.Shares,
                    comments = latest.Comments
                };
            })
            .OrderByDescending(x => x.viralFactor)
            .Take(10)
            .ToList();

            return new { topEngagingVideos = engagingVideos };
        }

        public async Task<object> GetFruitCombinationsAsync()
        {
            var videos = await _context.PublishedVideos
                .Where(v => v.AnalyticsHistory.Any())
                .Include(v => v.VideoGeneration).ThenInclude(vg => vg.FruitAsset).ThenInclude(fa => fa.FruitsInImage)
                .Include(v => v.AnalyticsHistory)
                .ToListAsync();

            var combinationStats = videos
                .Select(v =>
                {
                    var fruits = v.VideoGeneration?.FruitAsset?.FruitsInImage?.Select(f => f.Name).OrderBy(n => n).ToList() ?? new List<string>();
                    var combinationName = fruits.Count > 0 ? string.Join(" + ", fruits) : "Unknown";
                    var latest = v.AnalyticsHistory.OrderByDescending(a => a.RecordedAt).First();
                    double views = latest.Views > 0 ? latest.Views : 1;
                    double viralFactor = Math.Round(((double)latest.Shares / views) * 100, 2);

                    return new
                    {
                        combinationName,
                        views = latest.Views,
                        viralFactor
                    };
                })
                .GroupBy(x => x.combinationName)
                .Select(g => new
                {
                    combination = g.Key,
                    videoCount = g.Count(),
                    averageViews = Math.Round(g.Average(x => x.views), 0),
                    averageViralFactor = Math.Round(g.Average(x => x.viralFactor), 2)
                })
                .OrderByDescending(x => x.averageViews)
                .ToList();

            return combinationStats;
        }

        public async Task<object> GetLifecycleInsightsAsync()
        {
            var videos = await _context.PublishedVideos
                .Where(v => v.AnalyticsHistory.Count >= 2)
                .Include(v => v.VideoGeneration).ThenInclude(vg => vg.FruitAsset)
                .Include(v => v.AnalyticsHistory)
                .ToListAsync();

            var lateBloomers = new List<object>();

            foreach (var video in videos)
            {
                var history = video.AnalyticsHistory.OrderBy(a => a.RecordedAt).ToList();
                var firstRecord = history.First();
                var latestRecord = history.Last();

                if (firstRecord.Views > 0)
                {
                    double multiplier = Math.Round((double)latestRecord.Views / firstRecord.Views, 1);
                    if (multiplier >= 2.0)
                    {
                        lateBloomers.Add(new
                        {
                            videoId = video.Id,
                            title = video.VideoGeneration?.FruitAsset?.Title ?? "Bilinmiyor",
                            postUrl = video.PostUrl,
                            firstRecordedViews = firstRecord.Views,
                            latestViews = latestRecord.Views,
                            momentumMultiplier = multiplier
                        });
                    }
                }
            }

            return new
            {
                lateBloomers = lateBloomers.OrderByDescending(x => ((dynamic)x).momentumMultiplier).Take(5).ToList()
            };
        }

        public async Task<object> GenerateGoldenHoursHeatmapAsync(Stream csvStream)
        {
            var records = new List<FollowerActivityRecord>();

            using (var stream = new StreamReader(csvStream))
            {
                await stream.ReadLineAsync(); // Header

                string? line;
                // CA2024 uyumlu async satır okuma
                while ((line = await stream.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var columns = line.Split(',');
                    if (columns.Length >= 3)
                    {
                        records.Add(new FollowerActivityRecord
                        {
                            Date = columns[0].Trim(),
                            Hour = columns[1].Trim(),
                            ActiveFollowers = int.TryParse(columns[2].Replace("\"", "").Trim(), out int active) ? active : 0
                        });
                    }
                }
            }

            if (records.Count == 0)
                throw new InvalidDataException("CSV dosyası okunamadı veya içi boş.");

            var heatmapData = records
                .GroupBy(r => r.Hour.Replace("\"", "").Trim())
                .Select(g => new
                {
                    hour = g.Key,
                    averageActiveFollowers = Math.Round(g.Average(r => r.ActiveFollowers), 0),
                    totalActiveInPeriod = g.Sum(r => r.ActiveFollowers)
                })
                .OrderByDescending(x => x.averageActiveFollowers)
                .ToList();

            var peakHourData = heatmapData.FirstOrDefault();
            int peakHour = 0;
            int recommendedHour = 0;

            if (peakHourData != null && int.TryParse(peakHourData.hour, out peakHour))
            {
                recommendedHour = (peakHour - 2 + 24) % 24;
            }

            return new
            {
                message = "Isı haritası ve sörf stratejisi başarıyla oluşturuldu.",
                goldenHour = new
                {
                    peakTime = $"{peakHour}:00",
                    recommendedPostingTime = $"{recommendedHour}:00",
                    expectedAudienceAtPeak = peakHourData?.averageActiveFollowers ?? 0,
                    recommendation = $"Takipçileriniz en yoğun saat {peakHour}:00 civarında aktif. Algoritmanın videonuzu işleyip For You akışına sürmesi için en ideal paylaşım saati: {recommendedHour}:00."
                },
                heatmap = heatmapData
            };
        }

        public async Task<object?> GetInstagramLiveDemographicsAsync()
        {
            return await _scraperService.GetLiveDemographicsAsync();
        }

        public async Task<object> GetDailyTrendsAsync(TrendPlatform? platform)
        {
            if (platform == TrendPlatform.TikTok || !platform.HasValue)
            {
                return await _scraperService.GetTikTokDailyTrendsAsync();
            }

            var trends = await _context.DailyTrends
                .Where(t => t.Platform == platform.Value)
                .OrderByDescending(t => t.ViewCount)
                .Take(20)
                .ToListAsync();

            return trends;
        }

        private class FollowerActivityRecord
        {
            public string Date { get; set; } = string.Empty;
            public string Hour { get; set; } = string.Empty;
            public int ActiveFollowers { get; set; }
        }
    }
}
