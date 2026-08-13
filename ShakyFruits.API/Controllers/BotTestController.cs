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

namespace ShakyFruits.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotTestController : ControllerBase
    {
        private readonly KlingAiBotService _botService;
        private readonly VideoQueueManager _queueManager; // KUYRUK YÖNETİCİSİ EKLENDİ
        private readonly ApplicationDbContext _context;

        public BotTestController(
            KlingAiBotService botService,
            VideoQueueManager queueManager,
            ApplicationDbContext context) // <-- YENİ PARAMETRE
        {
            _botService = botService;
            _queueManager = queueManager;
            _context = context; // <-- ATAMA YAPILIYOR
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
                // 1. Dosyaları sunucuya kaydet
                string savedImagePath = await SaveFileAsync(request.FruitImage, "Images");
                if (string.IsNullOrEmpty(savedImagePath)) return BadRequest("Meyve fotoğrafı zorunludur!");

                string savedVideoPath = string.Empty;
                if (!request.IsRecreate)
                {
                    if (request.ReferenceVideo == null) return BadRequest("Sıfırdan üretim için referans video zorunludur!");
                    savedVideoPath = await SaveFileAsync(request.ReferenceVideo, "Videos");
                }

                // 2. Güvenlik ve Prompt Ayarları
                if (string.IsNullOrWhiteSpace(request.TargetModel) || request.TargetModel == "string") request.TargetModel = "VIDEO 2.6";
                if (string.IsNullOrWhiteSpace(request.TargetResolution) || request.TargetResolution == "string") request.TargetResolution = "720p";

                string appliedPrompt = KlingPrompts.GetFixPrompt(request.IsMultipleFruits);

                // 3. HİBRİT MALİYET HESAPLAMA MİMARİSİ
                int calculatedCost = 0;
                double videoDuration = 0;

                if (request.IsRecreate)
                {
                    // DERİN YOL (Playwright): Video bizde değil. Bot URL'ye gider, fotoğrafı yükler ve UI'dan okur.
                    calculatedCost = await _botService.PrepareAndGetCostAsync(
                        request.IsRecreate,
                        savedImagePath,
                        savedVideoPath, // Recreate olduğu için boş gidecek, sorun yok
                        appliedPrompt,
                        request.TargetUrl,
                        request.TargetModel,
                        request.TargetResolution
                    );
                }
                else
                {
                    // HIZLI YOL (C# Matematik Motoru): Video bizde. Botu hiç açmadan anında hesapla!
                    videoDuration = KlingCostCalculator.GetVideoDurationInSeconds(savedVideoPath);
                    calculatedCost = KlingCostCalculator.CalculateCost(request.TargetModel, request.TargetResolution, videoDuration);
                }

                return Ok(new
                {
                    Message = request.IsRecreate ? "Bot URL'den maliyeti okudu ve onay bekliyor." : "Dosyalar yüklendi ve maliyet anında hesaplandı. Onayınız bekleniyor.",
                    CalculatedCredits = calculatedCost,
                    VideoDurationSeconds = videoDuration
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
                // 1. Veritabanı modelini oluştur
                var newGeneration = new VideoGeneration
                {
                    FruitAssetId = request.FruitAssetId,
                    ReferenceVideoId = request.ReferenceVideoId,
                    IsRecreate = request.IsRecreate,
                    TargetUrl = request.TargetUrl,
                    AppliedPrompt = KlingPrompts.GetFixPrompt(request.IsMultipleFruits),
                    TargetModel = request.TargetModel,
                    TargetResolution = request.TargetResolution,
                    Status = GenerationStatus.Pending
                };

                // 2. Veritabanına kaydet (EF Core otomatik olarak bir ID atayacaktır)
                _context.VideoGenerations.Add(newGeneration);
                await _context.SaveChangesAsync();

                // 3. Sadece oluşan ID'yi Worker'ın dinlediği kuyruğa gönder
                await _queueManager.QueueJobAsync(newGeneration.Id);

                return Ok(new
                {
                    Message = "Videonuz sıraya alındı!",
                    JobId = newGeneration.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Hata: {ex.Message}");
            }
        }

        // 3. AŞAMA: İptal
        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel()
        {
            await _botService.CancelGenerationAsync();
            return Ok("İşlem iptal edildi, tarayıcı sekmesi temizlendi.");
        }
    }
}