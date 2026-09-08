using System;
using System.Threading.Tasks;
using ShakyFruits.Core.Enums;

namespace ShakyFruits.Core.Interfaces
{
    public interface IGenerationService
    {
        Task<object?> GenerateAiCaptionAsync(int id, SocialPlatform platform);
        Task<object> AddHistoricalVideoAsync(int fruitAssetId, int? referenceVideoId, bool isRecreate, string? targetUrl, string? outputVideoPath, string? aiGeneratedCaption, bool isPublished, SocialPlatform platform, string? postUrl, DateTime? publishedAt);
        Task<object?> UpdateHistoricalVideoAsync(int id, int fruitAssetId, int? referenceVideoId, bool isRecreate, string? targetUrl, string? outputVideoPath, string? aiGeneratedCaption, bool isPublished, SocialPlatform platform, string? postUrl, DateTime? publishedAt);
        Task<object> GetGenerationsAsync();
        Task<bool> DeleteGenerationAsync(int id);
    }
}
