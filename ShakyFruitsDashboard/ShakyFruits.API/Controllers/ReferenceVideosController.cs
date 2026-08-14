using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShakyFruits.Core.Entities;
using ShakyFruits.Data;

namespace ShakyFruits.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferenceVideosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Veritabanı bağlantımızı (Dependency Injection) alıyoruz
        public ReferenceVideosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ReferenceVideos
        // Sistemdeki tüm referans videoları listeler
        [HttpGet]
        public async Task<IActionResult> GetAllVideos()
        {
            var videos = await _context.ReferenceVideos.ToListAsync();
            return Ok(videos);
        }

        // GET: api/ReferenceVideos/by-style?style=Salsa
        // Belirli bir dans stiline (örneğin Salsa veya HipHop) göre videoları getirir
        [HttpGet("by-style")]
        public async Task<IActionResult> GetVideosByStyle([FromQuery] string style)
        {
            if (string.IsNullOrWhiteSpace(style))
            {
                return BadRequest("Lütfen bir dans stili belirtin.");
            }

            var filteredVideos = await _context.ReferenceVideos
                .Where(v => v.DanceStyle.ToLower() == style.ToLower())
                .ToListAsync();

            return Ok(filteredVideos);
        }

        // POST: api/ReferenceVideos
        // Dışarıdan gelen JSON verisiyle yeni bir referans video kaydı oluşturur
        [HttpPost]
        public async Task<IActionResult> AddVideo([FromBody] ReferenceVideo newVideo)
        {
            // Yeni modelimize uygun küçük bir güvenlik/mantık kontrolü
            if (newVideo.SourceType == ReferenceSourceType.KlingRecreate && string.IsNullOrWhiteSpace(newVideo.KlingSourceUrlOrId))
            {
                return BadRequest("Kling AI recreate seçeneği işaretlendiğinde KlingSourceUrlOrId alanı boş bırakılamaz.");
            }

            if (newVideo.SourceType == ReferenceSourceType.LocalUpload && string.IsNullOrWhiteSpace(newVideo.VideoPath))
            {
                return BadRequest("Lokal yükleme seçeneği işaretlendiğinde VideoPath (dosya yolu) boş bırakılamaz.");
            }

            _context.ReferenceVideos.Add(newVideo);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Referans video başarıyla veritabanına eklendi!",
                VideoId = newVideo.Id,
                SourceType = newVideo.SourceType.ToString(),
                CreatedDate = newVideo.CreatedAt
            });
        }
    }
}