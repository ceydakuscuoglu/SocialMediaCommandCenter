using Microsoft.AspNetCore.Mvc;
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Constants;
using ShakyFruits.Core.Helpers;
using ShakyFruits.Services;

namespace ShakyFruits.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotTestController : ControllerBase
    {
        private readonly KlingAiBotService _botService;

        public BotTestController(KlingAiBotService botService)
        {
            _botService = botService;
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
                    if (request.ReferenceVideo == null) return BadRequest("Referans video zorunludur!");
                    savedVideoPath = await SaveFileAsync(request.ReferenceVideo, "Videos");
                }

                // --- YENİ ROKET MİMARİSİ ---

                // 2. Videonun süresini oku (Recreate ise eski bir videonun süresini varsayabilir veya DB'den çekebilirsin)
                double videoDuration = request.IsRecreate ? 5.0 : KlingCostCalculator.GetVideoDurationInSeconds(savedVideoPath);

                // 3. Güvenlik Koruma Kalkanı
                if (string.IsNullOrWhiteSpace(request.TargetModel) || request.TargetModel == "string") request.TargetModel = "VIDEO 2.6";
                if (string.IsNullOrWhiteSpace(request.TargetResolution) || request.TargetResolution == "string") request.TargetResolution = "720p";

                // 4. Maliyeti kendi C# sunucumuzda hesapla! (Bot açılmıyor, kilitlenme yok)
                int calculatedCost = KlingCostCalculator.CalculateCost(request.TargetModel, request.TargetResolution, videoDuration);

                // İLERİDE BURAYA EKLENECEK: Bu istek veritabanına "Onay Bekliyor" statüsüyle kaydedilecek ve bir Guid (ID) dönecek.

                return Ok(new
                {
                    Message = "Dosyalar yüklendi ve maliyet anında hesaplandı. Onayınız bekleniyor.",
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
        public async Task<IActionResult> Confirm()
        {
            try
            {
                await _botService.ConfirmAndGenerateAsync();
                return Ok("Onay verildi! Üretim simüle edildi ve sekme kapatıldı.");
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