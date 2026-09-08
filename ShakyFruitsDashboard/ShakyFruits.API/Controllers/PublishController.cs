using Microsoft.AspNetCore.Mvc;
using ShakyFruits.API.DTOs;
using ShakyFruits.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace ShakyFruits.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishController : ControllerBase
    {
        private readonly IPublishService _publishService;

        public PublishController(IPublishService publishService)
        {
            _publishService = publishService;
        }

        [HttpPost("publish-video")]
        public async Task<IActionResult> PublishVideo([FromBody] PublishRequestDto request)
        {
            try
            {
                var result = await _publishService.PublishVideoAsync(
                    request.VideoGenerationId,
                    request.VideoPath,
                    request.Caption,
                    request.Platform);

                if (result == null)
                {
                    return NotFound(new { message = "Veritabanında böyle bir üretilmiş video bulunamadı." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}