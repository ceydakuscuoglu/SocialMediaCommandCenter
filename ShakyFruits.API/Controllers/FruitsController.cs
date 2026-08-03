using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Settings;
using ShakyFruits.Data;

namespace ShakyFruits.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FruitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AssetPathOptions _assetPaths;

        // Constructor Injection: Veritabanını ve Ayarları buraya çağırıyoruz
        public FruitsController(ApplicationDbContext context, IOptions<AssetPathOptions> assetPathsOptions)
        {
            _context = context;
            _assetPaths = assetPathsOptions.Value;
        }

        [HttpPost("add-dummy-fruit")]
        public async Task<IActionResult> AddDummyFruit()
        {
            // 1. Dinamik dosya yolumuzu kullanarak yeni bir meyve görseli nesnesi oluşturuyoruz
            var newFruit = new FruitAsset
            {
                Title = "Test Çileği",
                IsMultipleFruits = false,
                // Dinamik klasör yolunun sonuna dosya adını ekliyoruz
                ImagePath = Path.Combine(_assetPaths.FruitImages, "test_cilek.png")
            };

            // 2. Veritabanına ekle
            _context.FruitAssets.Add(newFruit);
            await _context.SaveChangesAsync(); // Tarihler (CreatedAt) otomatik atılacak!

            // 3. Ekrana başarılı mesajı ve kaydedilen yolu döndür
            return Ok(new
            {
                Message = "Veritabanına başarıyla kaydedildi!",
                SavedPath = newFruit.ImagePath,
                CreatedDate = newFruit.CreatedAt
            });
        }
    }
}