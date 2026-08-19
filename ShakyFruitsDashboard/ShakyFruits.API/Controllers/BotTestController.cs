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
                // 1. Dosyaları sunucuya (diske) kaydet
                string savedImagePath = await SaveFileAsync(request.FruitImage, "Images");
                if (string.IsNullOrEmpty(savedImagePath)) return BadRequest("Meyve fotoğrafı zorunludur!");

                string savedVideoPath = string.Empty;
                if (!request.IsRecreate)
                {
                    if (request.ReferenceVideo == null) return BadRequest("Sıfırdan üretim için referans video zorunludur!");
                    savedVideoPath = await SaveFileAsync(request.ReferenceVideo, "Videos");
                }

                // 2. VERİTABANINA KAYIT (Eksik olan kritik parça burasıydı)

                // 1. Meyveyi veritabanına kaydet (Dinamik Title ile)
                var fruitAsset = new FruitAsset
                {
                    ImagePath = savedImagePath,
                    // Kullanıcı bir başlık girdiyse onu kullan, girmediyse dosya adını (örn: muz.png) kullan
                    Title = !string.IsNullOrWhiteSpace(request.FruitTitle)
                            ? request.FruitTitle
                            : request.FruitImage.FileName,

                    IsMultipleFruits = request.IsMultipleFruits
                };
                _context.FruitAssets.Add(fruitAsset);

                // 2. Varsa referans videoyu veritabanına kaydet
                ReferenceVideo? refVideo = null;
                if (!string.IsNullOrEmpty(savedVideoPath))
                {
                    refVideo = new ReferenceVideo
                    {
                        VideoPath = savedVideoPath,

                        // Kullanıcı bir dans stili/başlık girdiyse onu kullan, girmediyse varsayılan bir isim ver
                        DanceStyle = !string.IsNullOrWhiteSpace(request.DanceStyle)
                                     ? request.DanceStyle
                                     : "Özel Yükleme (Bilinmeyen Dans)",

                        // Biz şu an fiziksel dosya yüklediğimiz için kaynak tipi kesinlikle LocalUpload'dur
                        SourceType = ReferenceSourceType.LocalUpload
                    };
                    _context.ReferenceVideos.Add(refVideo);
                }

                // 3. Değişiklikleri kaydet ki ID'ler (Identity) oluşsun
                await _context.SaveChangesAsync();


                // 3. Güvenlik ve Prompt Ayarları
                if (string.IsNullOrWhiteSpace(request.TargetModel) || request.TargetModel == "string") request.TargetModel = "VIDEO 2.6";
                if (string.IsNullOrWhiteSpace(request.TargetResolution) || request.TargetResolution == "string") request.TargetResolution = "720p";

                string appliedPrompt = KlingPrompts.GetFixPrompt(request.IsMultipleFruits);

                // 4. Maliyet Hesaplama Mimari
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

                // 5. REACT'E ID'LERİ DÖNDÜR (Artık UI bu ID'leri Confirm'e gönderebilecek)
                return Ok(new
                {
                    Message = request.IsRecreate ? "Bot URL'den maliyeti okudu ve onay bekliyor." : "Dosyalar yüklendi ve maliyet anında hesaplandı. Onayınız bekleniyor.",
                    CalculatedCredits = calculatedCost,
                    VideoDurationSeconds = videoDuration,
                    FruitAssetId = fruitAsset.Id, // <-- YENİ
                    ReferenceVideoId = refVideo?.Id // <-- YENİ (Nullable)
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
    }
}