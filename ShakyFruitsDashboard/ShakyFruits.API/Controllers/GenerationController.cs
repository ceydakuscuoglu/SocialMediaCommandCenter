using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShakyFruits.Core.Enums;
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
    }
}