using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Helpers;
using ShakyFruits.Core.Interfaces;
using ShakyFruits.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShakyFruits.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiCaptionService _aiCaptionService;

        public GenerationController(ApplicationDbContext context, IAiCaptionService aiCaptionService)
        {
            _context = context;
            _aiCaptionService = aiCaptionService;
        }

        [HttpPost("{id}/generate-caption")]
        public async Task<IActionResult> GenerateAiCaption(int id, [FromQuery] SocialPlatform platform = SocialPlatform.TikTok)
        {
            // 1. Veritabanından videonun meyve ve dans detaylarını çek
            var generation = await _context.VideoGenerations
                .Include(vg => vg.FruitAsset)
                    .ThenInclude(fa => fa.FruitsInImage)
                .Include(vg => vg.ReferenceVideo)
                .FirstOrDefaultAsync(vg => vg.Id == id);

            if (generation == null)
                return NotFound("Video üretimi bulunamadı.");

            var fruits = generation.FruitAsset.FruitsInImage.Select(f => f.Name).ToList();

            // Dans stili boşsa default bir metin atıyoruz
            var danceStyle = generation.ReferenceVideo?.DanceStyle ?? "eğlenceli trend";

            try
            {
                // 2. AI Servisine gönder ve açıklamayı al
                var generatedText = await _aiCaptionService.GenerateCaptionAsync(fruits, danceStyle, platform);

                // 3. Veritabanına kaydet
                generation.AiGeneratedCaption = generatedText;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Açıklama başarıyla üretildi.",
                    platform = platform.ToString(),
                    caption = generatedText
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Yapay zeka servisiyle iletişim kurulamadı.", Error = ex.Message });
            }
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
                    IsRecreate = request.IsRecreate,
                    TargetUrl = request.TargetUrl,
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
                    appliedPrompt = newGeneration.AppliedPrompt,
                    isTrackedByScraper = publishedVideoId.HasValue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Geçmiş video eklenirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpPut("historical-videos/{id}")]
        public async Task<IActionResult> UpdateHistoricalVideo(int id, [FromBody] UpdateHistoricalVideoRequestDto request)
        {
            try
            {
                var generation = await _context.VideoGenerations
                    .Include(v => v.PublishedVideo)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (generation == null)
                    return NotFound(new { message = "Güncellenmek istenen video kaydı bulunamadı." });

                if (generation.FruitAssetId != request.FruitAssetId)
                {
                    var fruitAsset = await _context.FruitAssets.FindAsync(request.FruitAssetId);
                    if (fruitAsset == null)
                        return BadRequest("Geçersiz FruitAssetId. Güncellenmek istenen meyve sistemde yok.");

                    generation.AppliedPrompt = fruitAsset.GetAppliedFixPrompt();
                }

                if (request.ReferenceVideoId.HasValue && generation.ReferenceVideoId != request.ReferenceVideoId)
                {
                    var refVideoExists = await _context.ReferenceVideos.AnyAsync(r => r.Id == request.ReferenceVideoId.Value);
                    if (!refVideoExists)
                        return BadRequest("Geçersiz ReferenceVideoId. Sistemde böyle bir referans video yok.");
                }

                generation.FruitAssetId = request.FruitAssetId;
                generation.ReferenceVideoId = request.ReferenceVideoId;
                generation.IsRecreate = request.IsRecreate;
                generation.TargetUrl = request.TargetUrl;
                generation.OutputVideoPath = request.OutputVideoPath;
                generation.AiGeneratedCaption = request.AiGeneratedCaption;

                if (request.IsPublished)
                {
                    if (generation.PublishedVideo != null)
                    {
                        generation.PublishedVideo.Platform = request.Platform;
                        generation.PublishedVideo.PostUrl = request.PostUrl;
                        if (request.PublishedAt.HasValue)
                            generation.PublishedVideo.PublishedAt = request.PublishedAt.Value;
                    }
                    else
                    {
                        generation.PublishedVideo = new PublishedVideo
                        {
                            Platform = request.Platform,
                            PostUrl = request.PostUrl,
                            PublishedAt = request.PublishedAt ?? DateTime.UtcNow
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

                return Ok(new
                {
                    message = "Geçmiş video kaydı başarıyla güncellendi.",
                    videoGenerationId = generation.Id,
                    outputVideoPath = generation.OutputVideoPath,
                    appliedPrompt = generation.AppliedPrompt,
                    isPublished = request.IsPublished,
                    platform = request.Platform,
                    postUrl = request.PostUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Video kaydı güncellenirken hata oluştu.", error = ex.Message });
            }
        }

        [HttpGet]
        [HttpGet("generations")]
        public async Task<IActionResult> GetGenerations()
        {
            try
            {
                var history = await _context.VideoGenerations
                  .Include(v => v.FruitAsset)
                  .Include(v => v.ReferenceVideo)
                  .Include(v => v.PublishedVideo)
                  .OrderByDescending(v => v.CreatedAt)
                  .Select(v => new VideoGenerationListDto
                  {
                      Id = v.Id,
                      FruitImagePath = v.FruitAsset.ImagePath,
                      ReferenceVideoPath = v.ReferenceVideo != null ? v.ReferenceVideo.VideoPath : null,
                      IsRecreate = v.IsRecreate,
                      AppliedPrompt = v.AppliedPrompt,
                      Status = v.Status.ToString(),
                      ErrorMessage = v.ErrorMessage,
                      OutputVideoPath = v.OutputVideoPath,
                      CreatedAt = v.CreatedAt,
                      FruitAssetId = v.FruitAssetId,
                      ReferenceVideoId = v.ReferenceVideoId,
                      AiGeneratedCaption = v.AiGeneratedCaption,
                      IsPublished = v.PublishedVideo != null,
                      Platform = v.PublishedVideo != null ? (int)v.PublishedVideo.Platform : 0,
                      PostUrl = v.PublishedVideo != null ? v.PublishedVideo.PostUrl : ""
                  })
                  .ToListAsync();
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest($"Listeleme sırasında hata oluştu: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [HttpDelete("generations/{id}")]
        public async Task<IActionResult> DeleteGeneration(int id)
        {
            try
            {
                var generation = await _context.VideoGenerations.FindAsync(id);

                if (generation == null)
                {
                    return NotFound(new { Message = "Silinmek istenen kayıt bulunamadı." });
                }

                if (!string.IsNullOrEmpty(generation.OutputVideoPath) && System.IO.File.Exists(generation.OutputVideoPath))
                {
                    System.IO.File.Delete(generation.OutputVideoPath);
                }

                _context.VideoGenerations.Remove(generation);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "İşlem başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Silme işlemi sırasında hata oluştu: {ex.Message}");
            }
        }
    }
}