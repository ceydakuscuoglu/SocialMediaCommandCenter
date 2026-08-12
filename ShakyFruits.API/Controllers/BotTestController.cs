using Microsoft.AspNetCore.Mvc;
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

        // 1. AŞAMA: Hazırlık ve Fiyat Alma
        [HttpPost("prepare")]
        public async Task<IActionResult> Prepare([FromQuery] bool isRecreate, [FromQuery] string? url = null)
        {
            try
            {
                // Botu hazırlığa gönderiyoruz. (Model ve çözünürlük şimdilik varsayılan değerleri kullanacak)
                int cost = await _botService.PrepareAndGetCostAsync(isRecreate, url);

                return Ok(new
                {
                    Message = "Bot dosyaları yükledi ve onayınızı bekliyor!",
                    RequiredCredits = cost
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