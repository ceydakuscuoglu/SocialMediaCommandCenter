using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Constants;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Helpers;
using ShakyFruits.Core.Models;
using ShakyFruits.Core.Services;
using ShakyFruits.Services;
using ShakyFruits.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ShakyFruits.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KlingAIBotController : ControllerBase
    {
        private readonly KlingAiBotService _botService;
        private readonly VideoQueueManager _queueManager; // KUYRUK YÖNETİCİSİ EKLENDİ
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public KlingAIBotController(
            KlingAiBotService botService,
            VideoQueueManager queueManager,
            ApplicationDbContext context,
            IMemoryCache cache) // <-- YENİ PARAMETRE
        {
            _botService = botService;
            _queueManager = queueManager;
            _context = context; // <-- ATAMA YAPILIYOR
            _cache = cache;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            // Projenin çalıştığı dizinde "Uploads/Images" veya "Uploads/Videos" klasörleri oluşturur
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Dosya isminin çakışmaması için benzersiz (Guid) bir isim veriyoruz
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return filePath; // Botun kullanacağı fiziksel dosya yolunu (Örn: C:\...\Uploads\Images\abc.png) döndürür
        }


        // 1. AŞAMA: Hazırlık ve Fiyat Alma
        [HttpPost("prepare")]
        public async Task<IActionResult> Prepare([FromForm] BotPrepareRequestDto request)
        {
            try
            {
                // 1. Dosyaları kalıcı klasöre değil, "Temp" (Geçici) klasörüne kaydet
                string savedImagePath = await SaveFileAsync(request.FruitImage, "Temp");
                if (string.IsNullOrEmpty(savedImagePath)) return BadRequest("Meyve fotoğrafı zorunludur!");

                string savedVideoPath = string.Empty;
                if (!request.IsRecreate)
                {
                    if (request.ReferenceVideo == null) return BadRequest("Sıfırdan üretim için referans video zorunludur!");
                    savedVideoPath = await SaveFileAsync(request.ReferenceVideo, "Temp");
                }

                // --- VERİTABANI KAYIT İŞLEMİ BURADAN TAMAMEN SİLİNDİ ---

                if (string.IsNullOrWhiteSpace(request.TargetModel) || request.TargetModel == "string") request.TargetModel = "VIDEO 2.6";
                if (string.IsNullOrWhiteSpace(request.TargetResolution) || request.TargetResolution == "string") request.TargetResolution = "720p";

                string appliedPrompt = KlingPrompts.GetFixPrompt(request.IsMultipleFruits);

                // 2. Maliyet Hesaplama
                int calculatedCost = 0;
                double videoDuration = 0;

                if (request.IsRecreate)
                {
                    calculatedCost = await _botService.PrepareAndGetCostAsync(
                        request.IsRecreate, savedImagePath, savedVideoPath, appliedPrompt, request.TargetUrl, request.TargetModel, request.TargetResolution);
                }
                else
                {
                    videoDuration = KlingCostCalculator.GetVideoDurationInSeconds(savedVideoPath);
                    calculatedCost = KlingCostCalculator.CalculateCost(request.TargetModel, request.TargetResolution, videoDuration);
                }

                // 3. Verileri RAM'e (Cache) Kaydet
                var sessionId = Guid.NewGuid().ToString(); // Benzersiz bir bilet oluşturuyoruz
                var sessionData = new TempPrepareSession
                {
                    TempImagePath = savedImagePath,
                    TempVideoPath = savedVideoPath,
                    FruitTitle = !string.IsNullOrWhiteSpace(request.FruitTitle) ? request.FruitTitle : request.FruitImage.FileName,
                    DanceStyle = !string.IsNullOrWhiteSpace(request.DanceStyle) ? request.DanceStyle : "Özel Yükleme",
                    IsMultipleFruits = request.IsMultipleFruits,
                    TargetModel = request.TargetModel,
                    TargetResolution = request.TargetResolution,
                    TargetUrl = request.TargetUrl,
                    IsRecreate = request.IsRecreate
                };

                // Bu bilet 30 dakika boyunca geçerli olacak
                _cache.Set(sessionId, sessionData, TimeSpan.FromMinutes(30));

                // 4. REACT'e DB ID'leri yerine SessionId döndür
                return Ok(new
                {
                    Message = "Hazırlık tamamlandı. Onay bekleniyor.",
                    CalculatedCredits = calculatedCost,
                    VideoDurationSeconds = videoDuration,
                    SessionId = sessionId // <-- ARTIK REACT BU ID'Yİ SAKLAYACAK
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Hata: {ex.Message}");
            }
        }

        // 2. AŞAMA: Onay ve Üretim
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] BotConfirmRequestDto request)
        {
            try
            {
                // 1. RAM'den (Cache) bilet numaramıza göre verileri çekiyoruz
                if (!_cache.TryGetValue(request.SessionId, out TempPrepareSession sessionData))
                {
                    return BadRequest("Oturum süresi dolmuş veya geçersiz. Lütfen sayfayı yenileyip tekrar hazırlık yapın.");
                }

                // 2. VERİTABANINA KAYIT (Prepare'den sildiğimiz işlemi buraya aldık)
                var fruitAsset = new FruitAsset
                {
                    ImagePath = sessionData.TempImagePath,
                    Title = sessionData.FruitTitle,
                    IsMultipleFruits = sessionData.IsMultipleFruits
                };
                _context.FruitAssets.Add(fruitAsset);

                ReferenceVideo? refVideo = null;
                if (!string.IsNullOrEmpty(sessionData.TempVideoPath))
                {
                    refVideo = new ReferenceVideo
                    {
                        VideoPath = sessionData.TempVideoPath,
                        DanceStyle = sessionData.DanceStyle,
                        SourceType = ReferenceSourceType.LocalUpload
                    };
                    _context.ReferenceVideos.Add(refVideo);
                }

                // Değişiklikleri kaydet ki Fruit ve Video için gerçek ID'ler oluşsun
                await _context.SaveChangesAsync();

                // 3. Asıl işi (VideoGeneration) oluştur ve yeni oluşan ID'leri bağla
                var newGeneration = new VideoGeneration
                {
                    FruitAssetId = fruitAsset.Id, // <-- Artık 0 değil, taptaze oluşan ID!
                    ReferenceVideoId = refVideo?.Id,
                    IsRecreate = sessionData.IsRecreate,
                    TargetUrl = sessionData.TargetUrl,
                    AppliedPrompt = KlingPrompts.GetFixPrompt(sessionData.IsMultipleFruits),
                    TargetModel = sessionData.TargetModel,
                    TargetResolution = sessionData.TargetResolution,
                    Status = GenerationStatus.Pending
                };

                _context.VideoGenerations.Add(newGeneration);
                await _context.SaveChangesAsync();

                // 4. Sadece oluşan işin ID'sini Worker'ın dinlediği kuyruğa gönder
                await _queueManager.QueueJobAsync(newGeneration.Id);

                // 5. Temizlik: İşlem bittiğine göre RAM'i meşgul etmemesi için Cache'i siliyoruz
                _cache.Remove(request.SessionId);

                return Ok(new
                {
                    Message = "Videonuz başarıyla sıraya alındı!",
                    JobId = newGeneration.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Onaylama sırasında hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("generations")]
        public async Task<IActionResult> GetGenerations()
        {
            try
            {
                // Veritabanından en son eklenen işleri en üstte olacak şekilde (OrderByDescending) çekiyoruz
                var generations = await _context.VideoGenerations
                    .Include(x => x.FruitAsset)
                    .Include(x => x.ReferenceVideo)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new VideoGenerationListDto
                    {
                        Id = x.Id,
                        FruitImagePath = x.FruitAsset.ImagePath,
                        // Null koruması: Eğer Recreate ise video yoktur, null döner
                        ReferenceVideoPath = x.ReferenceVideo != null ? x.ReferenceVideo.VideoPath : null,
                        IsRecreate = x.IsRecreate,
                        AppliedPrompt = x.AppliedPrompt,
                        Status = x.Status.ToString(), // Enum'u doğrudan metne çeviriyoruz ("Pending", "Completed" vs.)
                        ErrorMessage = x.ErrorMessage,
                        OutputVideoPath = x.OutputVideoPath,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                return Ok(generations);
            }
            catch (Exception ex)
            {
                return BadRequest($"Listeleme sırasında hata oluştu: {ex.Message}");
            }
        }

        [HttpDelete("generations/{id}")]
        public async Task<IActionResult> DeleteGeneration(int id)
        {
            try
            {
                // 1. Veritabanında ilgili işi bul
                var generation = await _context.VideoGenerations.FindAsync(id);

                if (generation == null)
                {
                    return NotFound(new { Message = "Silinmek istenen kayıt bulunamadı." });
                }

                // 2. Fiziksel Dosya Temizliği (Opsiyonel ama önerilir)
                // Eğer bu iş tamamlanmışsa ve diskte bir çıktı videosu varsa, onu da temizleyelim
                if (!string.IsNullOrEmpty(generation.OutputVideoPath) && System.IO.File.Exists(generation.OutputVideoPath))
                {
                    System.IO.File.Delete(generation.OutputVideoPath);
                }

                // 3. Veritabanından kaydı sil
                _context.VideoGenerations.Remove(generation);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "İşlem başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Silme işlemi sırasında hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("credits")]
        public async Task<IActionResult> GetCredits()
        {
            try
            {
                // _botService artık KlingCreditsModel dönüyor, bu da doğrudan ön yüze gidiyor
                var credits = await _botService.GetCreditsAsync();
                return Ok(credits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Krediler çekilirken bir hata oluştu.", Error = ex.Message });
            }
        }
    }
}