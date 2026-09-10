using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Helpers;
using ShakyFruits.Core.Interfaces;
using ShakyFruits.Core.Settings;
using ShakyFruits.Data;

namespace ShakyFruits.Services
{
    public class GenerationService : IGenerationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiCaptionService _aiCaptionService;
        private readonly AssetPathOptions _assetPaths;

        public GenerationService(
            ApplicationDbContext context,
            IAiCaptionService aiCaptionService,
            IOptions<AssetPathOptions> assetPathOptions)
        {
            _context = context;
            _aiCaptionService = aiCaptionService;
            _assetPaths = assetPathOptions.Value;
        }

        public async Task<object?> GenerateAiCaptionAsync(int id, SocialPlatform platform)
        {
            var generation = await _context.VideoGenerations
                .Include(vg => vg.FruitAsset)
                    .ThenInclude(fa => fa.FruitsInImage)
                .Include(vg => vg.ReferenceVideo)
                .FirstOrDefaultAsync(vg => vg.Id == id);

            if (generation == null) return null;

            var fruits = generation.FruitAsset.FruitsInImage.Select(f => f.Name).ToList();
            var danceStyle = generation.ReferenceVideo?.DanceStyle ?? "eğlenceli trend";

            var generatedText = await _aiCaptionService.GenerateCaptionAsync(fruits, danceStyle, platform);

            generation.AiGeneratedCaption = generatedText;
            await _context.SaveChangesAsync();

            return new
            {
                message = "Açıklama başarıyla üretildi.",
                platform = platform.ToString(),
                caption = generatedText
            };
        }

        public async Task<object> AddHistoricalVideoAsync(
            string? title,
            int fruitAssetId,
            int? referenceVideoId,
            bool isRecreate,
            string? targetUrl,
            string? outputVideoPath,
            string? aiGeneratedCaption,
            bool isPublished,
            SocialPlatform platform,
            string? postUrl,
            DateTime? publishedAt)
        {
            var fruitAsset = await _context.FruitAssets.FindAsync(fruitAssetId);
            if (fruitAsset == null)
                throw new ArgumentException("Geçersiz FruitAssetId. Önce meyveyi sisteme eklemelisiniz.");

            var newGeneration = new VideoGeneration
            {
                Title = !string.IsNullOrWhiteSpace(title) ? title : fruitAsset.Title,
                FruitAssetId = fruitAssetId,
                ReferenceVideoId = referenceVideoId,
                IsRecreate = isRecreate,
                TargetUrl = targetUrl,
                AppliedPrompt = fruitAsset.GetAppliedFixPrompt(),
                TargetModel = "Bilinmiyor (Geçmiş Veri)",
                TargetResolution = "Bilinmiyor",
                Status = GenerationStatus.Completed,
                OutputVideoPath = AssetPathHelper.ResolvePath(_assetPaths.Outputs, outputVideoPath),
                AiGeneratedCaption = aiGeneratedCaption
            };

            _context.VideoGenerations.Add(newGeneration);
            await _context.SaveChangesAsync();

            int? publishedVideoId = null;

            if (isPublished && !string.IsNullOrWhiteSpace(postUrl))
            {
                var newPublished = new PublishedVideo
                {
                    VideoGenerationId = newGeneration.Id,
                    Platform = platform,
                    PostUrl = postUrl,
                    PublishedAt = publishedAt ?? DateTime.UtcNow
                };

                _context.PublishedVideos.Add(newPublished);
                await _context.SaveChangesAsync();
                publishedVideoId = newPublished.Id;
            }

            return new
            {
                message = "Geçmiş video sisteme başarıyla eklendi.",
                videoGenerationId = newGeneration.Id,
                title = newGeneration.Title,
                publishedVideoId = publishedVideoId,
                outputVideoPath = newGeneration.OutputVideoPath,
                appliedPrompt = newGeneration.AppliedPrompt,
                isTrackedByScraper = publishedVideoId.HasValue
            };
        }

        public async Task<object?> UpdateHistoricalVideoAsync(
            int id,
            string? title,
            int fruitAssetId,
            int? referenceVideoId,
            bool isRecreate,
            string? targetUrl,
            string? outputVideoPath,
            string? aiGeneratedCaption,
            bool isPublished,
            SocialPlatform platform,
            string? postUrl,
            DateTime? publishedAt)
        {
            var generation = await _context.VideoGenerations
                .Include(v => v.PublishedVideo)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (generation == null) return null;

            if (!string.IsNullOrWhiteSpace(title))
            {
                generation.Title = title;
            }

            if (generation.FruitAssetId != fruitAssetId)
            {
                var fruitAsset = await _context.FruitAssets.FindAsync(fruitAssetId);
                if (fruitAsset == null)
                    throw new ArgumentException("Geçersiz FruitAssetId. Güncellenmek istenen meyve sistemde yok.");

                generation.AppliedPrompt = fruitAsset.GetAppliedFixPrompt();
            }

            if (referenceVideoId.HasValue && generation.ReferenceVideoId != referenceVideoId)
            {
                var refVideoExists = await _context.ReferenceVideos.AnyAsync(r => r.Id == referenceVideoId.Value);
                if (!refVideoExists)
                    throw new ArgumentException("Geçersiz ReferenceVideoId. Sistemde böyle bir referans video yok.");
            }

            generation.FruitAssetId = fruitAssetId;
            generation.ReferenceVideoId = referenceVideoId;
            generation.IsRecreate = isRecreate;
            generation.TargetUrl = targetUrl;
            generation.OutputVideoPath = AssetPathHelper.ResolvePath(_assetPaths.Outputs, outputVideoPath);
            generation.AiGeneratedCaption = aiGeneratedCaption;

            if (isPublished)
            {
                if (generation.PublishedVideo != null)
                {
                    generation.PublishedVideo.Platform = platform;
                    generation.PublishedVideo.PostUrl = postUrl;
                    if (publishedAt.HasValue)
                        generation.PublishedVideo.PublishedAt = publishedAt.Value;
                }
                else
                {
                    generation.PublishedVideo = new PublishedVideo
                    {
                        Platform = platform,
                        PostUrl = postUrl,
                        PublishedAt = publishedAt ?? DateTime.UtcNow
                    };
                }
            }
            else
            {
                if (generation.PublishedVideo != null)
                {
                    _context.PublishedVideos.Remove(generation.PublishedVideo);
                }
            }

            await _context.SaveChangesAsync();

            return new
            {
                message = "Geçmiş video kaydı başarıyla güncellendi.",
                videoGenerationId = generation.Id,
                title = generation.Title,
                outputVideoPath = generation.OutputVideoPath,
                appliedPrompt = generation.AppliedPrompt,
                isPublished = isPublished,
                platform = platform,
                postUrl = postUrl
            };
        }

        public async Task<object> GetGenerationsAsync()
        {
            var history = await _context.VideoGenerations
                .Include(v => v.FruitAsset)
                .Include(v => v.ReferenceVideo)
                .Include(v => v.PublishedVideo)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new
                {
                    id = v.Id,
                    title = v.Title,
                    fruitImagePath = v.FruitAsset.ImagePath,
                    referenceVideoPath = v.ReferenceVideo != null ? v.ReferenceVideo.VideoPath : null,
                    isRecreate = v.IsRecreate,
                    appliedPrompt = v.AppliedPrompt,
                    status = v.Status.ToString(),
                    errorMessage = v.ErrorMessage,
                    outputVideoPath = v.OutputVideoPath,
                    createdAt = v.CreatedAt,
                    fruitAssetId = v.FruitAssetId,
                    referenceVideoId = v.ReferenceVideoId,
                    aiGeneratedCaption = v.AiGeneratedCaption,
                    isPublished = v.PublishedVideo != null,
                    platform = v.PublishedVideo != null ? (int)v.PublishedVideo.Platform : 0,
                    postUrl = v.PublishedVideo != null ? v.PublishedVideo.PostUrl : ""
                })
                .ToListAsync();

            return history;
        }

        public async Task<bool> DeleteGenerationAsync(int id)
        {
            var generation = await _context.VideoGenerations.FindAsync(id);
            if (generation == null) return false;

            if (!string.IsNullOrEmpty(generation.OutputVideoPath) && File.Exists(generation.OutputVideoPath))
            {
                try
                {
                    File.Delete(generation.OutputVideoPath);
                }
                catch
                {
                    // Ignore physical deletion failure if file is locked or missing
                }
            }

            _context.VideoGenerations.Remove(generation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
