using Microsoft.AspNetCore.Mvc;
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace ShakyFruits.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerationController : ControllerBase
    {
        private readonly IGenerationService _generationService;

        public GenerationController(IGenerationService generationService)
        {
            _generationService = generationService;
        }

        [HttpPost("{id}/generate-caption")]
        public async Task<IActionResult> GenerateAiCaption(int id, [FromQuery] SocialPlatform platform = SocialPlatform.TikTok)
        {
            try
            {
                var result = await _generationService.GenerateAiCaptionAsync(id, platform);
                if (result == null)
                    return NotFound("Video üretimi bulunamadı.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Yapay zeka servisiyle iletişim kurulamadı.", Error = ex.Message });
            }
        }

        [HttpPost("historical-videos")]
        public async Task<IActionResult> AddHistoricalVideo([FromBody] AddHistoricalVideoRequestDto request)
        {
            try
            {
                var result = await _generationService.AddHistoricalVideoAsync(
                    request.FruitAssetId,
                    request.ReferenceVideoId,
                    request.IsRecreate,
                    request.TargetUrl,
                    request.OutputVideoPath,
                    request.AiGeneratedCaption,
                    request.IsPublished,
                    request.Platform,
                    request.PostUrl,
                    request.PublishedAt);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Geçmiş video eklenirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpPut("historical-videos/{id}")]
        public async Task<IActionResult> UpdateHistoricalVideo(int id, [FromBody] UpdateHistoricalVideoRequestDto request)
        {
            try
            {
                var result = await _generationService.UpdateHistoricalVideoAsync(
                    id,
                    request.FruitAssetId,
                    request.ReferenceVideoId,
                    request.IsRecreate,
                    request.TargetUrl,
                    request.OutputVideoPath,
                    request.AiGeneratedCaption,
                    request.IsPublished,
                    request.Platform,
                    request.PostUrl,
                    request.PublishedAt);

                if (result == null)
                    return NotFound(new { message = "Güncellenmek istenen video kaydı bulunamadı." });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Video kaydı güncellenirken hata oluştu.", error = ex.Message });
            }
        }

        [HttpGet]
        [HttpGet("generations")]
        public async Task<IActionResult> GetGenerations()
        {
            try
            {
                var history = await _generationService.GetGenerationsAsync();
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest($"Listeleme sırasında hata oluştu: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [HttpDelete("generations/{id}")]
        public async Task<IActionResult> DeleteGeneration(int id)
        {
            try
            {
                var success = await _generationService.DeleteGenerationAsync(id);
                if (!success)
                {
                    return NotFound(new { Message = "Silinmek istenen kayıt bulunamadı." });
                }

                return Ok(new { Message = "İşlem başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Silme işlemi sırasında hata oluştu: {ex.Message}");
            }
        }
    }
}