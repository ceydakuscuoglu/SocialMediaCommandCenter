namespace ShakyFruits.API.DTOs
{
    public class CreateFruitAssetRequestDto
    {
        public string Title { get; set; }
        public string ImagePath { get; set; }
        public bool IsMultipleFruits { get; set; }
        public List<int> FruitTypeIds { get; set; } // İçindeki meyvelerin ID'leri
    }
}
