using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Models;

namespace ShakyFruits.Core.Interfaces
{
    public interface IAnalyticsService
    {
        Task<VideoAnalytics> TestTikTokScraperAsync(string url);
        Task<object> GetInternalStatsAsync();
        Task<object> GetPublishedVideosAsync();
        Task<PublishedVideo> TrackPublishedVideoAsync(int videoGenerationId, string postUrl);
        Task<List<AccountAnalyticsHistory>> GetAccountHistoryAsync(SocialPlatform? platform);
        Task<object?> GetLatestAccountStatsAsync(SocialPlatform platform);
        Task<object> GetLatestTikTokStatsAsync();
        Task<object> GetLatestInstagramStatsAsync();
        Task ForceRefreshAccountStatsAsync();
        Task<object> GetAllVideosLatestStatsAsync();
        Task<object> GetVideoSmartStatsAsync(int videoId);
        Task<object> ForceRefreshVideoStatsAsync(int videoId);
        Task<object> ForceRefreshAllVideosStatsAsync();
        Task<object> GetLeaderboardsAsync();
        Task<object> GetSoloVsGroupAnalyticsAsync();
        Task<object> GetEngagementMetricsAsync();
        Task<object> GetFruitCombinationsAsync();
        Task<object> GetLifecycleInsightsAsync();
        Task<object> GenerateGoldenHoursHeatmapAsync(Stream csvStream);
        Task<object?> GetInstagramLiveDemographicsAsync();
        Task<object> GetDailyTrendsAsync(TrendPlatform? platform);
    }
}
