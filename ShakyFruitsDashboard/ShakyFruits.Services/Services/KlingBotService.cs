using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using ShakyFruits.Core.Constants;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Helpers;
using ShakyFruits.Core.Interfaces;
using ShakyFruits.Core.DTOs;
using ShakyFruits.Data;

namespace ShakyFruits.Services
{
    public class KlingBotService : IKlingBotService
    {
        private readonly KlingAiBotService _botService;
        private readonly IVideoQueueManager _queueManager;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IFileStorageService _fileStorageService;

        public KlingBotService(
            KlingAiBotService botService,
            IVideoQueueManager queueManager,
            ApplicationDbContext context,
            IMemoryCache cache,
            IFileStorageService fileStorageService)
        {
            _botService = botService;
            _queueManager = queueManager;
            _context = context;
            _cache = cache;
            _fileStorageService = fileStorageService;
        }

        public async Task<object> PrepareAsync(
            int? existingFruitAssetId,
            Stream? fruitImageStream,
            string? fruitImageFileName,
            int? existingReferenceVideoId,
            Stream? referenceVideoStream,
            string? referenceVideoFileName,
            bool isRecreate,
            string targetModel,
            string targetResolution,
            bool isMultipleFruits,
            string? targetUrl,
            string? fruitTitle,
            string? danceStyle,
            string? title = null)
        {
            string finalImagePath = string.Empty;
            string? tempImagePath = null;

            if (existingFruitAssetId.HasValue)
            {
                var existingFruit = await _context.FruitAssets.FindAsync(existingFruitAssetId.Value);
                if (existingFruit == null)
                    throw new ArgumentException("Seçilen meyve fotoğrafı bulunamadı!");

                finalImagePath = existingFruit.ImagePath;
            }
            else if (fruitImageStream != null && !string.IsNullOrWhiteSpace(fruitImageFileName))
            {
                tempImagePath = await _fileStorageService.SaveFileAsync(fruitImageStream, fruitImageFileName, "Temp");
                finalImagePath = tempImagePath;
            }
            else
            {
                throw new ArgumentException("Lütfen ya var olan bir meyveyi seçin ya da yeni bir fotoğraf yükleyin!");
            }

            string? finalVideoPath = null;
            string? tempVideoPath = null;

            if (!isRecreate)
            {
                if (existingReferenceVideoId.HasValue)
                {
                    var existingVideo = await _context.ReferenceVideos.FindAsync(existingReferenceVideoId.Value);
                    if (existingVideo == null)
                        throw new ArgumentException("Seçilen referans video bulunamadı!");

                    finalVideoPath = existingVideo.VideoPath;
                }
                else if (referenceVideoStream != null && !string.IsNullOrWhiteSpace(referenceVideoFileName))
                {
                    tempVideoPath = await _fileStorageService.SaveFileAsync(referenceVideoStream, referenceVideoFileName, "Temp");
                    finalVideoPath = tempVideoPath;
                }
                else
                {
                    throw new ArgumentException("Sıfırdan üretim için var olan bir videoyu seçmeli veya yeni bir video yüklemelisiniz!");
                }
            }

            if (string.IsNullOrWhiteSpace(targetModel) || targetModel == "string") targetModel = "VIDEO 2.6";
            if (string.IsNullOrWhiteSpace(targetResolution) || targetResolution == "string") targetResolution = "720p";

            string appliedPrompt = KlingPrompts.GetFixPrompt(isMultipleFruits);

            int calculatedCost = 0;
            double videoDuration = 0;

            if (isRecreate)
            {
                calculatedCost = await _botService.PrepareAndGetCostAsync(
                    isRecreate, finalImagePath, finalVideoPath, appliedPrompt, targetUrl, targetModel, targetResolution);
            }
            else
            {
                videoDuration = KlingCostCalculator.GetVideoDurationInSeconds(finalVideoPath);
                calculatedCost = KlingCostCalculator.CalculateCost(targetModel, targetResolution, videoDuration);
            }

            var sessionId = Guid.NewGuid().ToString();
            var resolvedTitle = !string.IsNullOrWhiteSpace(title)
                ? title
                : (!string.IsNullOrWhiteSpace(fruitTitle) ? $"{fruitTitle} - {danceStyle ?? "Video"}" : (fruitImageFileName ?? "Kayıtlı Meyve Video"));

            var sessionData = new TempPrepareSessionDto
            {
                ExistingFruitAssetId = existingFruitAssetId,
                ExistingReferenceVideoId = existingReferenceVideoId,
                TempImagePath = tempImagePath,
                TempVideoPath = tempVideoPath,
                FinalImagePathToUse = finalImagePath,
                FinalVideoPathToUse = finalVideoPath,
                Title = resolvedTitle,
                FruitTitle = !string.IsNullOrWhiteSpace(fruitTitle) ? fruitTitle : (fruitImageFileName ?? "Kayıtlı Meyve"),
                DanceStyle = !string.IsNullOrWhiteSpace(danceStyle) ? danceStyle : "Kayıtlı Stil",
                IsMultipleFruits = isMultipleFruits,
                TargetModel = targetModel,
                TargetResolution = targetResolution,
                TargetUrl = targetUrl,
                IsRecreate = isRecreate
            };

            _cache.Set(sessionId, sessionData, TimeSpan.FromMinutes(30));

            return new
            {
                Message = "Hazırlık tamamlandı. Onay bekleniyor.",
                CalculatedCredits = calculatedCost,
                VideoDurationSeconds = videoDuration,
                SessionId = sessionId
            };
        }

        public async Task<object?> ConfirmAsync(string sessionId)
        {
            if (!_cache.TryGetValue(sessionId, out TempPrepareSessionDto? sessionData) || sessionData == null)
            {
                return null;
            }

            int finalFruitAssetId = 0;
            int? finalReferenceVideoId = null;

            if (sessionData.ExistingFruitAssetId.HasValue)
            {
                finalFruitAssetId = sessionData.ExistingFruitAssetId.Value;
            }
            else
            {
                var fruitAsset = new FruitAsset
                {
                    ImagePath = sessionData.TempImagePath ?? string.Empty,
                    Title = sessionData.FruitTitle ?? "Kayıtlı Meyve",
                    IsMultipleFruits = sessionData.IsMultipleFruits
                };
                _context.FruitAssets.Add(fruitAsset);
                await _context.SaveChangesAsync();
                finalFruitAssetId = fruitAsset.Id;
            }

            if (sessionData.ExistingReferenceVideoId.HasValue)
            {
                finalReferenceVideoId = sessionData.ExistingReferenceVideoId.Value;
            }
            else if (!string.IsNullOrEmpty(sessionData.TempVideoPath))
            {
                var refVideo = new ReferenceVideo
                {
                    VideoPath = sessionData.TempVideoPath,
                    DanceStyle = sessionData.DanceStyle ?? "Kayıtlı Stil",
                    SourceType = ReferenceSourceType.LocalUpload
                };
                _context.ReferenceVideos.Add(refVideo);
                await _context.SaveChangesAsync();
                finalReferenceVideoId = refVideo.Id;
            }

            var newGeneration = new VideoGeneration
            {
                Title = !string.IsNullOrWhiteSpace(sessionData.Title)
                    ? sessionData.Title
                    : (!string.IsNullOrWhiteSpace(sessionData.FruitTitle) ? $"{sessionData.FruitTitle} Video" : "Kling Video"),
                FruitAssetId = finalFruitAssetId,
                ReferenceVideoId = finalReferenceVideoId,
                IsRecreate = sessionData.IsRecreate,
                TargetUrl = sessionData.TargetUrl,
                AppliedPrompt = KlingPrompts.GetFixPrompt(sessionData.IsMultipleFruits),
                TargetModel = sessionData.TargetModel,
                TargetResolution = sessionData.TargetResolution,
                Status = GenerationStatus.Pending
            };

            _context.VideoGenerations.Add(newGeneration);
            await _context.SaveChangesAsync();

            await _queueManager.QueueJobAsync(newGeneration.Id);

            _cache.Remove(sessionId);

            return new
            {
                Message = "Videonuz başarıyla sıraya alındı!",
                JobId = newGeneration.Id
            };
        }

        public async Task<object> GetCurrentCreditsAsync()
        {
            string cacheFilePath = Path.Combine(Directory.GetCurrentDirectory(), "credits_cache.json");
            var today = DateTime.UtcNow.Date;

            if (File.Exists(cacheFilePath))
            {
                string jsonContent = await File.ReadAllTextAsync(cacheFilePath);
                var cachedData = JsonSerializer.Deserialize<CreditCacheModel>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (cachedData != null && cachedData.LastScrapedAt.Date == today)
                {
                    return new
                    {
                        remainingCredits = cachedData.RemainingCredits,
                        membershipCredits = cachedData.MembershipCredits,
                        topUpCredits = cachedData.TopUpCredits,
                        bonusCredits = cachedData.BonusCredits,
                        source = "Local JSON File"
                    };
                }
            }

            var scrapedCredits = await _botService.GetCreditsAsync();

            var newCacheData = new CreditCacheModel
            {
                RemainingCredits = scrapedCredits.RemainingCredits,
                MembershipCredits = scrapedCredits.MembershipCredits,
                TopUpCredits = scrapedCredits.TopUpCredits,
                BonusCredits = scrapedCredits.BonusCredits,
                LastScrapedAt = DateTime.UtcNow
            };

            string newJsonContent = JsonSerializer.Serialize(newCacheData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(cacheFilePath, newJsonContent);

            return new
            {
                remainingCredits = newCacheData.RemainingCredits,
                membershipCredits = newCacheData.MembershipCredits,
                topUpCredits = newCacheData.TopUpCredits,
                bonusCredits = newCacheData.BonusCredits,
                source = "Live Scrape"
            };
        }

        private class CreditCacheModel
        {
            public double RemainingCredits { get; set; }
            public double MembershipCredits { get; set; }
            public double TopUpCredits { get; set; }
            public double BonusCredits { get; set; }
            public DateTime LastScrapedAt { get; set; }
        }
    }
}
