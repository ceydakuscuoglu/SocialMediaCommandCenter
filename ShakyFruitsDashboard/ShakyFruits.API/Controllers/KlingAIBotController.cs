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
using System.IO;
using System.Text.Json;
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


        [HttpPost("prepare")]
        public async Task<IActionResult> Prepare([FromForm] BotPrepareRequestDto request)
        {
            try
            {
                string finalImagePath = string.Empty;
                string? tempImagePath = null;

                // 1. MEYVE FOTOĞRAFI MANTIĞI (DB'den mi yoksa yeni yükleme mi?)
                if (request.ExistingFruitAssetId.HasValue)
                {
                    var existingFruit = await _context.FruitAssets.FindAsync(request.ExistingFruitAssetId.Value);
                    if (existingFruit == null) return BadRequest("Seçilen meyve fotoğrafı bulunamadı!");

                    finalImagePath = existingFruit.ImagePath; // DB'deki asıl yol
                }
                else if (request.FruitImage != null)
                {
                    tempImagePath = await SaveFileAsync(request.FruitImage, "Temp");
                    finalImagePath = tempImagePath; // Temp'teki geçici yol
                }
                else
                {
                    return BadRequest("Lütfen ya var olan bir meyveyi seçin ya da yeni bir fotoğraf yükleyin!");
                }

                // 2. REFERANS VİDEO MANTIĞI
                string? finalVideoPath = null;
                string? tempVideoPath = null;

                if (!request.IsRecreate)
                {
                    if (request.ExistingReferenceVideoId.HasValue)
                    {
                        var existingVideo = await _context.ReferenceVideos.FindAsync(request.ExistingReferenceVideoId.Value);
                        if (existingVideo == null) return BadRequest("Seçilen referans video bulunamadı!");

                        finalVideoPath = existingVideo.VideoPath;
                    }
                    else if (request.ReferenceVideo != null)
                    {
                        tempVideoPath = await SaveFileAsync(request.ReferenceVideo, "Temp");
                        finalVideoPath = tempVideoPath;
                    }
                    else
                    {
                        return BadRequest("Sıfırdan üretim için var olan bir videoyu seçmeli veya yeni bir video yüklemelisiniz!");
                    }
                }

                if (string.IsNullOrWhiteSpace(request.TargetModel) || request.TargetModel == "string") request.TargetModel = "VIDEO 2.6";
                if (string.IsNullOrWhiteSpace(request.TargetResolution) || request.TargetResolution == "string") request.TargetResolution = "720p";

                string appliedPrompt = KlingPrompts.GetFixPrompt(request.IsMultipleFruits);

                // 3. MALİYET HESAPLAMA (Artık Final Yolları kullanıyoruz)
                int calculatedCost = 0;
                double videoDuration = 0;

                if (request.IsRecreate)
                {
                    calculatedCost = await _botService.PrepareAndGetCostAsync(
                        request.IsRecreate, finalImagePath, finalVideoPath, appliedPrompt, request.TargetUrl, request.TargetModel, request.TargetResolution);
                }
                else
                {
                    videoDuration = KlingCostCalculator.GetVideoDurationInSeconds(finalVideoPath);
                    calculatedCost = KlingCostCalculator.CalculateCost(request.TargetModel, request.TargetResolution, videoDuration);
                }

                // 4. CACHE (RAM) KAYDI
                var sessionId = Guid.NewGuid().ToString();
                var sessionData = new TempPrepareSession
                {
                    ExistingFruitAssetId = request.ExistingFruitAssetId,
                    ExistingReferenceVideoId = request.ExistingReferenceVideoId,
                    TempImagePath = tempImagePath,
                    TempVideoPath = tempVideoPath,
                    FinalImagePathToUse = finalImagePath,
                    FinalVideoPathToUse = finalVideoPath,
                    FruitTitle = !string.IsNullOrWhiteSpace(request.FruitTitle) ? request.FruitTitle : (request.FruitImage?.FileName ?? "Kayıtlı Meyve"),
                    DanceStyle = !string.IsNullOrWhiteSpace(request.DanceStyle) ? request.DanceStyle : "Kayıtlı Stil",
                    IsMultipleFruits = request.IsMultipleFruits,
                    TargetModel = request.TargetModel,
                    TargetResolution = request.TargetResolution,
                    TargetUrl = request.TargetUrl,
                    IsRecreate = request.IsRecreate
                };

                _cache.Set(sessionId, sessionData, TimeSpan.FromMinutes(30));

                return Ok(new
                {
                    Message = "Hazırlık tamamlandı. Onay bekleniyor.",
                    CalculatedCredits = calculatedCost,
                    VideoDurationSeconds = videoDuration,
                    SessionId = sessionId
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Hata: {ex.Message}");
            }
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] BotConfirmRequestDto request)
        {
            try
            {
                if (!_cache.TryGetValue(request.SessionId, out TempPrepareSession sessionData))
                {
                    return BadRequest("Oturum süresi dolmuş veya geçersiz. Lütfen sayfayı yenileyip tekrar hazırlık yapın.");
                }

                int finalFruitAssetId = 0;
                int? finalReferenceVideoId = null;

                // 1. MEYVE: Var olanı mı kullanalım, yeni mi oluşturalım?
                if (sessionData.ExistingFruitAssetId.HasValue)
                {
                    finalFruitAssetId = sessionData.ExistingFruitAssetId.Value;
                }
                else
                {
                    var fruitAsset = new FruitAsset
                    {
                        ImagePath = sessionData.TempImagePath,
                        Title = sessionData.FruitTitle,
                        IsMultipleFruits = sessionData.IsMultipleFruits
                    };
                    _context.FruitAssets.Add(fruitAsset);
                    await _context.SaveChangesAsync();
                    finalFruitAssetId = fruitAsset.Id;
                }

                // 2. VİDEO: Var olanı mı kullanalım, yeni mi oluşturalım?
                if (sessionData.ExistingReferenceVideoId.HasValue)
                {
                    finalReferenceVideoId = sessionData.ExistingReferenceVideoId.Value;
                }
                else if (!string.IsNullOrEmpty(sessionData.TempVideoPath))
                {
                    var refVideo = new ReferenceVideo
                    {
                        VideoPath = sessionData.TempVideoPath,
                        DanceStyle = sessionData.DanceStyle,
                        SourceType = ReferenceSourceType.LocalUpload
                    };
                    _context.ReferenceVideos.Add(refVideo);
                    await _context.SaveChangesAsync();
                    finalReferenceVideoId = refVideo.Id;
                }

                // 3. ANA KAYIT (Generation) OLUŞTUR
                var newGeneration = new VideoGeneration
                {
                    FruitAssetId = finalFruitAssetId, // Dinamik olarak atandı
                    ReferenceVideoId = finalReferenceVideoId, // Dinamik olarak atandı
                    IsRecreate = sessionData.IsRecreate,
                    TargetUrl = sessionData.TargetUrl,
                    AppliedPrompt = KlingPrompts.GetFixPrompt(sessionData.IsMultipleFruits),
                    TargetModel = sessionData.TargetModel,
                    TargetResolution = sessionData.TargetResolution,
                    Status = GenerationStatus.Pending
                };

                _context.VideoGenerations.Add(newGeneration);
                await _context.SaveChangesAsync();

                // 4. WORKER'I TETİKLE
                await _queueManager.QueueJobAsync(newGeneration.Id);

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

        /*[HttpGet("credits")]
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
        }*/
        [HttpGet("credits")]
        public async Task<IActionResult> GetCurrentCredits()
        {
            try
            {
                // Dosyanın kaydedileceği yol (Proje ana dizininde credits_cache.json)
                string cacheFilePath = Path.Combine(Directory.GetCurrentDirectory(), "credits_cache.json");
                var today = DateTime.UtcNow.Date;

                // 1. JSON DOSYASINI KONTROL ET (Uygulama kapansa bile dosya durur)
                if (System.IO.File.Exists(cacheFilePath))
                {
                    string jsonContent = await System.IO.File.ReadAllTextAsync(cacheFilePath);
                    var cachedData = JsonSerializer.Deserialize<CreditCacheModelDto>(jsonContent);

                    // Eğer dosya varsa ve "Bugün" güncellendiyse direkt dosyadan dön
                    if (cachedData != null && cachedData.LastScrapedAt.Date == today)
                    {
                        return Ok(new
                        {
                            remainingCredits = cachedData.RemainingCredits,
                            membershipCredits = cachedData.MembershipCredits,
                            topUpCredits = cachedData.TopUpCredits,
                            bonusCredits = cachedData.BonusCredits,
                            source = "Local JSON File" // Veritabanı yok, dosyadan geldi
                        });
                    }
                }

                // 2. DOSYA YOKSA VEYA DÜNDEN KALDIYSA PLAYWRIGHT ÇALIŞSIN
                var scrapedCredits = await _botService.GetCreditsAsync();

                // 3. YENİ VERİYİ JSON DOSYASINA YAZ (Bir sonraki başlatmada buradan okuyacak)
                var newCacheData = new CreditCacheModelDto
                {
                    RemainingCredits = scrapedCredits.RemainingCredits,
                    MembershipCredits = scrapedCredits.MembershipCredits,
                    TopUpCredits = scrapedCredits.TopUpCredits,
                    BonusCredits = scrapedCredits.BonusCredits,
                    LastScrapedAt = DateTime.UtcNow
                };

                string newJsonContent = JsonSerializer.Serialize(newCacheData, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(cacheFilePath, newJsonContent);

                // 4. Ön yüze (React) gönder
                return Ok(new
                {
                    remainingCredits = newCacheData.RemainingCredits,
                    membershipCredits = newCacheData.MembershipCredits,
                    topUpCredits = newCacheData.TopUpCredits,
                    bonusCredits = newCacheData.BonusCredits,
                    source = "Live Scrape" // Yeni çekildi ve dosyaya yazıldı
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Krediler getirilirken hata oluştu.", Error = ex.Message });
            }
        }
    }
}