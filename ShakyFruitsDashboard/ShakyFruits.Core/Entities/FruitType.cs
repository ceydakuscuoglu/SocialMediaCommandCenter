using System;
using System.Collections.Generic;
using System.Text;

namespace ShakyFruits.Core.Entities
{
    public class FruitType : BaseEntity
    {
        public string Name { get; set; } // Örn: Çilek, Muz, Elma

        // Çoka-Çok ilişki (Bir meyve türü birçok fotoğrafta yer alabilir)
        public virtual ICollection<FruitAsset> FruitAssets { get; set; }
    }
}
