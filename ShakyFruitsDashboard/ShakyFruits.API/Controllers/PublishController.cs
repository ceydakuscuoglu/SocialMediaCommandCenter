using Microsoft.AspNetCore.Mvc;
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Interfaces;
using ShakyFruits.Data; // Kendi DbContext namespace'ini buraya ekle
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
            // 1. Üretilmiş videoyu (VideoGeneration) ve ona bağlı Orijinal Fotoğrafı (FruitAsset) birlikte çekiyoruz
            var generation = await _context.VideoGenerations
                .Include(v => v.FruitAsset) // Entity Framework'e ilişkili tabloyu da getirmesini söylüyoruz
                .FirstOrDefaultAsync(v => v.Id == request.VideoGenerationId);

            if (generation == null)
            {
                return NotFound(new { message = "Veritabanında böyle bir üretilmiş video bulunamadı." });
            }

            // 2. OTOMATİK KAPAK FOTOĞRAFI
            // FruitAsset tablosunda orijinal resmin yolunu tutan özelliğin adı neyse (örneğin ImagePath veya FilePath) onu yaz.
            string autoCoverPath = generation.FruitAsset.ImagePath;

            // İstersen dışarıdan gönderilen DTO'daki video yolunu da generation.OutputVideoPath ile değiştirebilirsin
            string videoToUpload = request.VideoPath ?? generation.OutputVideoPath;

            // 3. Playwright ile Yükleme İşlemini Başlat
            bool success = await _uploaderService.UploadVideoAsync(
                videoToUpload,
                request.Caption,
                request.Platform,
                autoCoverPath); // Dışarıdan geleni değil, doğrudan FruitAsset'ten aldığımız orijinal fotoğrafı veriyoruz
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