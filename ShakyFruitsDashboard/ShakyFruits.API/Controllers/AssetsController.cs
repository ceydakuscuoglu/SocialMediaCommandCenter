using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Settings;
using ShakyFruits.Data;

namespace ShakyFruits.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AssetPathOptions _assetPaths;

        public AssetsController(ApplicationDbContext context, IOptions<AssetPathOptions> assetPathsOptions)
        {
            _context = context;
            _assetPaths = assetPathsOptions.Value;
        }

        [HttpPost("fruits")]
        public async Task<IActionResult> AddFruitAsset([FromBody] CreateFruitAssetRequestDto request)
        {
            var fruitTypes = await _context.FruitTypes
                .Where(f => request.FruitTypeIds.Contains(f.Id))
                .ToListAsync();

            var newAsset = new FruitAsset
            {
                Title = request.Title,
                ImagePath = request.ImagePath,
                IsMultipleFruits = request.IsMultipleFruits,
                FruitsInImage = fruitTypes
            };

            _context.FruitAssets.Add(newAsset);
            await _context.SaveChangesAsync();

            // DÖNGÜ KIRICI: Sadece UI'ın ihtiyacı olan alanları dönüyoruz.
            return Ok(new
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
            });
        }

        [HttpPost("reference-videos")]
        public async Task<IActionResult> AddReferenceVideo([FromBody] CreateReferenceVideoRequestDto request)
        {
            var newReference = new ReferenceVideo
            {
                DanceStyle = request.DanceStyle,
                SourceType = request.SourceType,
                VideoPath = request.SourceType == ReferenceSourceType.LocalUpload ? request.VideoPath : null,
                KlingSourceUrlOrId = request.SourceType == ReferenceSourceType.KlingRecreate ? request.KlingSourceUrlOrId : null
            };

            _context.ReferenceVideos.Add(newReference);
            await _context.SaveChangesAsync();

            // DÖNGÜ KIRICI
            return Ok(new
            {
                id = newReference.Id,
                danceStyle = newReference.DanceStyle,
                sourceType = newReference.SourceType.ToString(),
                videoPath = newReference.VideoPath,
                klingSourceUrlOrId = newReference.KlingSourceUrlOrId
            });
        }

        [HttpPatch("complete-generation")]
        public async Task<IActionResult> CompleteVideoGeneration([FromBody] CompleteGenerationRequestDto request)
        {
            var generation = await _context.VideoGenerations.FindAsync(request.VideoGenerationId);
            if (generation == null) return NotFound();

            generation.OutputVideoPath = request.OutputVideoPath;
            generation.AiGeneratedCaption = request.AiGeneratedCaption;
            generation.Status = GenerationStatus.Completed;

            await _context.SaveChangesAsync();

            // DÖNGÜ KIRICI
            return Ok(new
            {
                id = generation.Id,
                status = generation.Status.ToString(),
                outputVideoPath = generation.OutputVideoPath,
                aiGeneratedCaption = generation.AiGeneratedCaption
            });
        }

        [HttpPost("fruit-types")]
        public async Task<IActionResult> AddFruitType([FromBody] CreateFruitTypeRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Meyve adı boş olamaz.");

            var exists = await _context.FruitTypes.AnyAsync(f => f.Name.ToLower() == request.Name.ToLower());
            if (exists)
                return BadRequest($"'{request.Name}' türü sistemde zaten kayıtlı.");

            var newFruitType = new FruitType
            {
                Name = request.Name
            };

            _context.FruitTypes.Add(newFruitType);
            await _context.SaveChangesAsync();

            // DÖNGÜ KIRICI
            return Ok(new
            {
                id = newFruitType.Id,
                name = newFruitType.Name
            });
        }

        [HttpGet("fruits")]
        public async Task<IActionResult> GetFruitAssets()
        {
            // Include ile bağlı olduğu türleri çekip, Select ile döngüyü kırıyoruz
            var assets = await _context.FruitAssets
                .Include(f => f.FruitsInImage)
                .OrderByDescending(f => f.CreatedAt) // En son eklenen en üstte gelsin
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

            return Ok(assets);
        }

        [HttpGet("reference-videos")]
        public async Task<IActionResult> GetReferenceVideos()
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

            return Ok(videos);
        }

        [HttpGet("fruit-types")]
        public async Task<IActionResult> GetFruitTypes()
        {
            var types = await _context.FruitTypes
                .OrderBy(t => t.Name) // Alfabetik sıralama UI tarafındaki Dropdown'lar için çok daha şıktır
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name
                })
                .ToListAsync();

            return Ok(types);
        }

        // İhtiyaç halinde tekil bir meyveyi ID ile getirmek için (Örn: Düzenleme sayfası)
        [HttpGet("fruits/{id}")]
        public async Task<IActionResult> GetFruitAssetById(int id)
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

            if (asset == null) return NotFound();

            return Ok(asset);
        }
    }
}