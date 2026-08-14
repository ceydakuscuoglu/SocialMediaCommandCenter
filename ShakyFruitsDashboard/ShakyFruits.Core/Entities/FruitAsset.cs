using ShakyFruits.Core.Constants; // Fabrikayı dahil ettik

namespace ShakyFruits.Core.Entities
{
    public class FruitAsset : BaseEntity
    {
        public string Title { get; set; }
        public string ImagePath { get; set; }
        public bool IsMultipleFruits { get; set; }

        public virtual ICollection<FruitType> FruitsInImage { get; set; }
        public virtual ICollection<VideoGeneration> Generations { get; set; }

        public string GetAppliedFixPrompt()
        {
            return KlingPrompts.GetFixPrompt(this.IsMultipleFruits);
        }
    }
}