using Microsoft.AspNetCore.Mvc;
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace ShakyFruits.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetsService _assetsService;
        private readonly IGenerationService _generationService;

        public AssetsController(IAssetsService assetsService, IGenerationService generationService)
        {
            _assetsService = assetsService;
            _generationService = generationService;
        }

        [HttpPost("fruits")]
        public async Task<IActionResult> AddFruitAsset([FromBody] CreateFruitAssetRequestDto request)
        {
            try
            {
                var result = await _assetsService.AddFruitAssetAsync(
                    request.Title,
                    request.ImagePath,
                    request.IsMultipleFruits,
                    request.FruitTypeIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Meyve görseli eklenirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("fruits")]
        public async Task<IActionResult> GetFruitAssets()
        {
            try
            {
                var assets = await _assetsService.GetFruitAssetsAsync();
                return Ok(assets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Meyve görselleri getirilirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("fruits/{id}")]
        public async Task<IActionResult> GetFruitAssetById(int id)
        {
            try
            {
                var asset = await _assetsService.GetFruitAssetByIdAsync(id);
                if (asset == null) return NotFound();
                return Ok(asset);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Meyve görseli getirilirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpPut("fruits/{id}")]
        public async Task<IActionResult> UpdateFruitAsset(int id, [FromBody] UpdateFruitAssetRequestDto request)
        {
            try
            {
                var result = await _assetsService.UpdateFruitAssetAsync(
                    id,
                    request.Title,
                    request.ImagePath,
                    request.IsMultipleFruits,
                    request.FruitTypeIds);

                if (result == null)
                    return NotFound(new { Message = "Güncellenmek istenen meyve kaydı bulunamadı." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Meyve görseli güncellenirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpPost("reference-videos")]
        public async Task<IActionResult> AddReferenceVideo([FromBody] CreateReferenceVideoRequestDto request)
        {
            try
            {
                var result = await _assetsService.AddReferenceVideoAsync(
                    request.DanceStyle,
                    request.SourceType,
                    request.VideoPath,
                    request.KlingSourceUrlOrId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Referans video eklenirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("reference-videos")]
        public async Task<IActionResult> GetReferenceVideos()
        {
            try
            {
                var videos = await _assetsService.GetReferenceVideosAsync();
                return Ok(videos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Referans videolar getirilirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpPut("reference-videos/{id}")]
        public async Task<IActionResult> UpdateReferenceVideo(int id, [FromBody] UpdateReferenceVideoRequestDto request)
        {
            try
            {
                var result = await _assetsService.UpdateReferenceVideoAsync(
                    id,
                    request.DanceStyle,
                    request.SourceType,
                    request.VideoPath,
                    request.KlingSourceUrlOrId);

                if (result == null)
                    return NotFound(new { Message = "Güncellenmek istenen referans video bulunamadı." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Referans video güncellenirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpPost("fruit-types")]
        public async Task<IActionResult> AddFruitType([FromBody] CreateFruitTypeRequestDto request)
        {
            try
            {
                var result = await _assetsService.AddFruitTypeAsync(request.Name);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Meyve türü eklenirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpGet("fruit-types")]
        public async Task<IActionResult> GetFruitTypes()
        {
            try
            {
                var types = await _assetsService.GetFruitTypesAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Meyve türleri getirilirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpPut("fruit-types/{id}")]
        public async Task<IActionResult> UpdateFruitType(int id, [FromBody] UpdateFruitTypeRequestDto request)
        {
            try
            {
                var result = await _assetsService.UpdateFruitTypeAsync(id, request.Name);
                if (result == null)
                    return NotFound(new { Message = "Güncellenmek istenen meyve türü bulunamadı." });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Meyve türü güncellenirken hata oluştu.", Error = ex.Message });
            }
        }

        [HttpPatch("complete-generation")]
        public async Task<IActionResult> CompleteVideoGeneration([FromBody] CompleteGenerationRequestDto request)
        {
            try
            {
                var result = await _assetsService.CompleteVideoGenerationAsync(
                    request.VideoGenerationId,
                    request.OutputVideoPath,
                    request.AiGeneratedCaption);

                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Video üretimi tamamlanırken hata oluştu.", Error = ex.Message });
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
    }
}