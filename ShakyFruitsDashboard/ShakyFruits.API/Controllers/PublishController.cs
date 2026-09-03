using Microsoft.AspNetCore.Mvc;
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Interfaces;
using ShakyFruits.Data; // Kendi DbContext namespace'ini buraya ekle
using System;
using System.Threading.Tasks;

namespace ShakyFruits.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishController : ControllerBase
    {
        private readonly IVideoUploaderService _uploaderService;
        private readonly ApplicationDbContext _context; // Entity Framework Core DbContext'in

        // DbContext'i Dependency Injection ile içeri alıyoruz
        public PublishController(IVideoUploaderService uploaderService, ApplicationDbContext context)
        {
            _uploaderService = uploaderService;
            _context = context;
        }
        [HttpPost("publish-video")]
        public async Task<IActionResult> PublishVideo([FromBody] PublishRequestDto request)
        {
            // 1. Üretilmiş videoyu (VideoGeneration) buluyoruz
            var generation = await _context.VideoGenerations.FindAsync(request.VideoGenerationId);

            if (generation == null)
            {
                return NotFound(new { message = "Veritabanında böyle bir üretilmiş video bulunamadı." });
            }

            // 2. Playwright ile Yükleme İşlemini Başlat
            bool success = await _uploaderService.UploadVideoAsync(
                request.VideoPath,       // veya doğrudan generation.OutputVideoPath
                request.Caption,         // veya doğrudan generation.AiGeneratedCaption
                request.Platform,
                request.CoverImagePath);

            // 3. Yükleme sonucuna göre Veritabanına "Yayınlanmış Video" ekle
            if (success)
            {
                // Yükleme başarılıysa PublishedVideo tablosuna YENİ kayıt atıyoruz!
                var publishedVideo = new PublishedVideo
                {
                    VideoGenerationId = generation.Id,
                    Platform = request.Platform,
                    PublishedAt = DateTime.UtcNow
                    // PostUrl alanını şimdilik boş bırakabiliriz, videonun tam linkini 
                    // daha sonra analitik botumuz kazıma yaparken bulup doldurabilir.
                };

                _context.PublishedVideos.Add(publishedVideo);

                // İsteğe bağlı: generation.Status = GenerationStatus.Completed; (veya Published) diyebilirsin

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Video platforma başarıyla yüklendi ve Yayınlananlar (PublishedVideo) tablosuna eklendi!",
                    publishedAt = publishedVideo.PublishedAt
                });
            }
            else
            {
                return StatusCode(500, new { message = "Playwright yükleme sırasında bir hata oluştu." });
            }
        }
    }
}