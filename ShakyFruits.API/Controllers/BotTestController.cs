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

        [HttpPost("run-browser")]
        public async Task<IActionResult> RunBrowser([FromQuery] bool isRecreate, [FromQuery] string? url = null)
        {
            await _botService.RunTestAsync(isRecreate, url);
            return Ok("Bot başarıyla çalıştırıldı!");
        }
    }
}