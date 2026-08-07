using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync() asenkron metodu için eklendi
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

        public FruitsController(ApplicationDbContext context, IOptions<AssetPathOptions> assetPathsOptions)
        {
            _context = context;
            _assetPaths = assetPathsOptions.Value;
        }

        // POST: api/Fruits/add-dummy-fruit (Önceki yazdığımız veri ekleme metodu)
        [HttpPost("add-dummy-fruit")]
        public async Task<IActionResult> AddDummyFruit()
        {
            var newFruit = new FruitAsset
            {
                Title = "Test Çileği",
                IsMultipleFruits = false,
                ImagePath = Path.Combine(_assetPaths.FruitImages, "test_cilek.png")
            };

            _context.FruitAssets.Add(newFruit);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Veritabanına başarıyla kaydedildi!",
                SavedPath = newFruit.ImagePath,
                CreatedDate = newFruit.CreatedAt
            });
        }

        // GET: api/Fruits
        // Veritabanındaki TÜM meyve görsellerini getirir
        [HttpGet]
        public async Task<IActionResult> GetAllFruits()
        {
            var fruits = await _context.FruitAssets.ToListAsync();
            return Ok(fruits);
        }

        // GET: api/Fruits/filter?isMultiple=true
        // Tekli veya Çoklu olma durumuna göre veritabanı seviyesinde filtreleme yapar
        [HttpGet("filter")]
        public async Task<IActionResult> GetFruitsByStatus([FromQuery] bool isMultiple)
        {
            var filteredFruits = await _context.FruitAssets
                .Where(f => f.IsMultipleFruits == isMultiple)
                .ToListAsync();

            return Ok(filteredFruits);
        }
    }
}