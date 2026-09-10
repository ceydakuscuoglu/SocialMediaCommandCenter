using Microsoft.AspNetCore.Mvc;
using ShakyFruits.Core.DTOs;
using ShakyFruits.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace ShakyFruits.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KlingAIBotController : ControllerBase
    {
        private readonly IKlingBotService _botService;

        public KlingAIBotController(IKlingBotService botService)
        {
            _botService = botService;
        }

        [HttpPost("prepare")]
        public async Task<IActionResult> Prepare([FromForm] BotPrepareRequestDto request)
        {
            try
            {
                var fruitStream = request.FruitImage?.OpenReadStream();
                var refVideoStream = request.ReferenceVideo?.OpenReadStream();

                var result = await _botService.PrepareAsync(
                    request.ExistingFruitAssetId,
                    fruitStream,
                    request.FruitImage?.FileName,
                    request.ExistingReferenceVideoId,
                    refVideoStream,
                    request.ReferenceVideo?.FileName,
                    request.IsRecreate,
                    request.TargetModel,
                    request.TargetResolution,
                    request.IsMultipleFruits,
                    request.TargetUrl,
                    request.FruitTitle,
                    request.DanceStyle,
                    request.Title);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                var result = await _botService.ConfirmAsync(request.SessionId);
                if (result == null)
                {
                    return BadRequest("Oturum süresi dolmuş veya geçersiz. Lütfen sayfayı yenileyip tekrar hazırlık yapın.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Onaylama sırasında hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("credits")]
        public async Task<IActionResult> GetCurrentCredits()
        {
            try
            {
                var credits = await _botService.GetCurrentCreditsAsync();
                return Ok(credits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Krediler getirilirken hata oluştu.", Error = ex.Message });
            }
        }
    }
}