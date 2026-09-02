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

        [HttpPost("historical-videos")]
        public async Task<IActionResult> AddHistoricalVideo([FromBody] AddHistoricalVideoRequestDto request)
        {
            try
            {
                // 1. Meyveyi nesne olarak veritabanından çek
                var fruitAsset = await _context.FruitAssets.FindAsync(request.FruitAssetId);
                if (fruitAsset == null)
                    return BadRequest("Geçersiz FruitAssetId. Önce meyveyi sisteme eklemelisiniz.");

                // 2. VideoGeneration kaydını oluştur
                var newGeneration = new VideoGeneration
                {
                    FruitAssetId = request.FruitAssetId,
                    ReferenceVideoId = request.ReferenceVideoId,

                    IsRecreate = request.IsRecreate, // <-- Artık sabit false değil, UI'dan geliyor
                    TargetUrl = request.TargetUrl,   // <-- UI'dan geliyor

                    AppliedPrompt = fruitAsset.GetAppliedFixPrompt(),
                    TargetModel = "Bilinmiyor (Geçmiş Veri)",
                    TargetResolution = "Bilinmiyor",
                    Status = GenerationStatus.Completed,
                    OutputVideoPath = request.OutputVideoPath,
                    AiGeneratedCaption = request.AiGeneratedCaption
                };

                _context.VideoGenerations.Add(newGeneration);
                await _context.SaveChangesAsync();

                int? publishedVideoId = null;

                // 3. Eğer video TikTok'ta yayınlandıysa PublishedVideo tablosuna da ekle
                if (request.IsPublished && !string.IsNullOrWhiteSpace(request.PostUrl))
                {
                    var newPublished = new PublishedVideo
                    {
                        VideoGenerationId = newGeneration.Id,
                        Platform = request.Platform,
                        PostUrl = request.PostUrl,
                        PublishedAt = request.PublishedAt ?? DateTime.UtcNow
                    };

                    _context.PublishedVideos.Add(newPublished);
                    await _context.SaveChangesAsync();
                    publishedVideoId = newPublished.Id;
                }

                return Ok(new
                {
                    message = "Geçmiş video sisteme başarıyla eklendi.",
                    videoGenerationId = newGeneration.Id,
                    publishedVideoId = publishedVideoId,
                    outputVideoPath = newGeneration.OutputVideoPath,
                    appliedPrompt = newGeneration.AppliedPrompt, // UI'da doğru atandığını görmek için
                    isTrackedByScraper = publishedVideoId.HasValue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Geçmiş video eklenirken hata oluştu.", Error = ex.Message });
            }
        }

        // 1. MEYVE GÖRSELİNİ GÜNCELLE
        [HttpPut("fruits/{id}")]
        public async Task<IActionResult> UpdateFruitAsset(int id, [FromBody] UpdateFruitAssetRequestDto request)
        {
            // Include ile var olan meyve türlerini de çekiyoruz ki güncelleyebilelim
            var existingAsset = await _context.FruitAssets
                .Include(f => f.FruitsInImage)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (existingAsset == null)
                return NotFound(new { Message = "Güncellenmek istenen meyve kaydı bulunamadı." });

            // Yeni seçilen meyve türlerini bul
            var updatedFruitTypes = await _context.FruitTypes
                .Where(f => request.FruitTypeIds.Contains(f.Id))
                .ToListAsync();

            existingAsset.Title = request.Title;
            existingAsset.ImagePath = request.ImagePath;
            existingAsset.IsMultipleFruits = request.IsMultipleFruits;

            // Çoka-çok ilişkiyi güncelle (Eskileri temizle, yenileri ekle)
            existingAsset.FruitsInImage.Clear();
            foreach (var type in updatedFruitTypes)
            {
                existingAsset.FruitsInImage.Add(type);
            }

            await _context.SaveChangesAsync();

            // Döngü kırıcı formatta geri dön
            return Ok(new
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
            });
        }

        // 2. REFERANS VİDEOYU GÜNCELLE
        [HttpPut("reference-videos/{id}")]
        public async Task<IActionResult> UpdateReferenceVideo(int id, [FromBody] UpdateReferenceVideoRequestDto request)
        {
            var existingVideo = await _context.ReferenceVideos.FindAsync(id);
            if (existingVideo == null)
                return NotFound(new { Message = "Güncellenmek istenen referans video bulunamadı." });

            existingVideo.DanceStyle = request.DanceStyle;
            existingVideo.SourceType = request.SourceType;
            existingVideo.VideoPath = request.SourceType == ReferenceSourceType.LocalUpload ? request.VideoPath : null;
            existingVideo.KlingSourceUrlOrId = request.SourceType == ReferenceSourceType.KlingRecreate ? request.KlingSourceUrlOrId : null;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = existingVideo.Id,
                danceStyle = existingVideo.DanceStyle,
                sourceType = existingVideo.SourceType.ToString(),
                videoPath = existingVideo.VideoPath,
                klingSourceUrlOrId = existingVideo.KlingSourceUrlOrId
            });
        }

        // 3. MEYVE TÜRÜNÜ (SÖZLÜK) GÜNCELLE
        [HttpPut("fruit-types/{id}")]
        public async Task<IActionResult> UpdateFruitType(int id, [FromBody] UpdateFruitTypeRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Meyve adı boş olamaz.");

            var existingType = await _context.FruitTypes.FindAsync(id);
            if (existingType == null)
                return NotFound(new { Message = "Güncellenmek istenen meyve türü bulunamadı." });

            // İsim değiştirilirken, aynı isimde BAŞKA bir kayıt var mı diye kontrol et
            var exists = await _context.FruitTypes.AnyAsync(f => f.Id != id && f.Name.ToLower() == request.Name.ToLower());
            if (exists)
                return BadRequest($"'{request.Name}' türü sistemde zaten kayıtlı.");

            existingType.Name = request.Name;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = existingType.Id,
                name = existingType.Name
            });
        }
    }
}