using System;
using System.Collections.Generic;
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
    public class AssetsService : IAssetsService
    {
        private readonly ApplicationDbContext _context;
        private readonly AssetPathOptions _assetPaths;

        public AssetsService(ApplicationDbContext context, IOptions<AssetPathOptions> assetPathOptions)
        {
            _context = context;
            _assetPaths = assetPathOptions.Value;
        }

        public async Task<object> AddFruitAssetAsync(string title, string imagePath, bool isMultipleFruits, List<int> fruitTypeIds)
        {
            var fruitTypes = await _context.FruitTypes
                .Where(f => fruitTypeIds.Contains(f.Id))
                .ToListAsync();

            var resolvedImagePath = AssetPathHelper.ResolvePath(_assetPaths.FruitImages, imagePath);

            var newAsset = new FruitAsset
            {
                Title = title,
                ImagePath = resolvedImagePath,
                IsMultipleFruits = isMultipleFruits,
                FruitsInImage = fruitTypes
            };

            _context.FruitAssets.Add(newAsset);
            await _context.SaveChangesAsync();

            return new
            {
                id = newAsset.Id,
                title = newAsset.Title,
                imagePath = newAsset.ImagePath,
                isMultipleFruits = newAsset.IsMultipleFruits,
                fruits = newAsset.FruitsInImage.Select(f => new
                {
                    id = f.Id,
                    name = f.Name
                }).ToList()
            };
        }

        public async Task<object> GetFruitAssetsAsync()
        {
            var assets = await _context.FruitAssets
                .Include(f => f.FruitsInImage)
                .OrderByDescending(f => f.CreatedAt)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    imagePath = a.ImagePath,
                    isMultipleFruits = a.IsMultipleFruits,
                    fruits = a.FruitsInImage.Select(f => new
                    {
                        id = f.Id,
                        name = f.Name
                    }).ToList()
                })
                .ToListAsync();

            return assets;
        }

        public async Task<object?> GetFruitAssetByIdAsync(int id)
        {
            var asset = await _context.FruitAssets
                .Include(f => f.FruitsInImage)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    imagePath = a.ImagePath,
                    isMultipleFruits = a.IsMultipleFruits,
                    fruits = a.FruitsInImage.Select(f => new
                    {
                        id = f.Id,
                        name = f.Name
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return asset;
        }

        public async Task<object?> UpdateFruitAssetAsync(int id, string title, string imagePath, bool isMultipleFruits, List<int> fruitTypeIds)
        {
            var existingAsset = await _context.FruitAssets
                .Include(f => f.FruitsInImage)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (existingAsset == null) return null;

            var updatedFruitTypes = await _context.FruitTypes
                .Where(f => fruitTypeIds.Contains(f.Id))
                .ToListAsync();

            existingAsset.Title = title;
            existingAsset.ImagePath = AssetPathHelper.ResolvePath(_assetPaths.FruitImages, imagePath);
            existingAsset.IsMultipleFruits = isMultipleFruits;

            existingAsset.FruitsInImage.Clear();
            foreach (var type in updatedFruitTypes)
            {
                existingAsset.FruitsInImage.Add(type);
            }

            await _context.SaveChangesAsync();

            return new
            {
                id = existingAsset.Id,
                title = existingAsset.Title,
                imagePath = existingAsset.ImagePath,
                isMultipleFruits = existingAsset.IsMultipleFruits,
                fruits = existingAsset.FruitsInImage.Select(f => new
                {
                    id = f.Id,
                    name = f.Name
                }).ToList()
            };
        }

        public async Task<object> AddReferenceVideoAsync(string? danceStyle, ReferenceSourceType sourceType, string? videoPath, string? klingSourceUrlOrId)
        {
            var resolvedVideoPath = sourceType == ReferenceSourceType.LocalUpload
                ? AssetPathHelper.ResolvePath(_assetPaths.ReferenceVideos, videoPath)
                : null;

            var newReference = new ReferenceVideo
            {
                DanceStyle = danceStyle,
                SourceType = sourceType,
                VideoPath = resolvedVideoPath,
                KlingSourceUrlOrId = sourceType == ReferenceSourceType.KlingRecreate ? klingSourceUrlOrId : null
            };

            _context.ReferenceVideos.Add(newReference);
            await _context.SaveChangesAsync();

            return new
            {
                id = newReference.Id,
                danceStyle = newReference.DanceStyle,
                sourceType = newReference.SourceType.ToString(),
                videoPath = newReference.VideoPath,
                klingSourceUrlOrId = newReference.KlingSourceUrlOrId
            };
        }

        public async Task<object> GetReferenceVideosAsync()
        {
            var videos = await _context.ReferenceVideos
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new
                {
                    id = v.Id,
                    danceStyle = v.DanceStyle,
                    sourceType = v.SourceType.ToString(),
                    videoPath = v.VideoPath,
                    klingSourceUrlOrId = v.KlingSourceUrlOrId
                })
                .ToListAsync();

            return videos;
        }

        public async Task<object?> UpdateReferenceVideoAsync(int id, string? danceStyle, ReferenceSourceType sourceType, string? videoPath, string? klingSourceUrlOrId)
        {
            var existingVideo = await _context.ReferenceVideos.FindAsync(id);
            if (existingVideo == null) return null;

            var resolvedVideoPath = sourceType == ReferenceSourceType.LocalUpload
                ? AssetPathHelper.ResolvePath(_assetPaths.ReferenceVideos, videoPath)
                : null;

            existingVideo.DanceStyle = danceStyle;
            existingVideo.SourceType = sourceType;
            existingVideo.VideoPath = resolvedVideoPath;
            existingVideo.KlingSourceUrlOrId = sourceType == ReferenceSourceType.KlingRecreate ? klingSourceUrlOrId : null;

            await _context.SaveChangesAsync();

            return new
            {
                id = existingVideo.Id,
                danceStyle = existingVideo.DanceStyle,
                sourceType = existingVideo.SourceType.ToString(),
                videoPath = existingVideo.VideoPath,
                klingSourceUrlOrId = existingVideo.KlingSourceUrlOrId
            };
        }

        public async Task<object> AddFruitTypeAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Meyve adı boş olamaz.");

            var exists = await _context.FruitTypes.AnyAsync(f => f.Name.ToLower() == name.ToLower());
            if (exists)
                throw new InvalidOperationException($"'{name}' türü sistemde zaten kayıtlı.");

            var newFruitType = new FruitType
            {
                Name = name
            };

            _context.FruitTypes.Add(newFruitType);
            await _context.SaveChangesAsync();

            return new
            {
                id = newFruitType.Id,
                name = newFruitType.Name
            };
        }

        public async Task<object> GetFruitTypesAsync()
        {
            var types = await _context.FruitTypes
                .OrderBy(t => t.Name)
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name
                })
                .ToListAsync();

            return types;
        }

        public async Task<object?> UpdateFruitTypeAsync(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Meyve adı boş olamaz.");

            var existingType = await _context.FruitTypes.FindAsync(id);
            if (existingType == null) return null;

            var exists = await _context.FruitTypes.AnyAsync(f => f.Id != id && f.Name.ToLower() == name.ToLower());
            if (exists)
                throw new InvalidOperationException($"'{name}' türü sistemde zaten kayıtlı.");

            existingType.Name = name;
            await _context.SaveChangesAsync();

            return new
            {
                id = existingType.Id,
                name = existingType.Name
            };
        }

        public async Task<object?> CompleteVideoGenerationAsync(int videoGenerationId, string outputVideoPath, string? aiGeneratedCaption, string? title = null)
        {
            var generation = await _context.VideoGenerations.FindAsync(videoGenerationId);
            if (generation == null) return null;

            if (!string.IsNullOrWhiteSpace(title))
            {
                generation.Title = title;
            }

            generation.OutputVideoPath = AssetPathHelper.ResolvePath(_assetPaths.Outputs, outputVideoPath);
            generation.AiGeneratedCaption = aiGeneratedCaption;
            generation.Status = GenerationStatus.Completed;

            await _context.SaveChangesAsync();

            return new
            {
                id = generation.Id,
                title = generation.Title,
                status = generation.Status.ToString(),
                outputVideoPath = generation.OutputVideoPath,
                aiGeneratedCaption = generation.AiGeneratedCaption
            };
        }
    }
}
