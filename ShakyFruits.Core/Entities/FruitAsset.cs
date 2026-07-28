using System;
using System.Collections.Generic;
using System.Text;

namespace ShakyFruits.Core.Entities
{
    public class FruitAsset : BaseEntity
    {
        public string Title { get; set; } // Örn: "Yalnız Çilek", "Muz ve Kivi Çetesi"
        public string ImagePath { get; set; } // \assets\fruit_images\muz_kivi.jpg

        // Arayüzden fotoğrafı yüklerken seçeceğin "Bu fotoğrafta birden fazla meyve var" bayrağı
        public bool IsMultipleFruits { get; set; }

        // Görselin içindeki meyveler listesi (Many-to-Many İlişki)
        public virtual ICollection<FruitType> FruitsInImage { get; set; }

        public virtual ICollection<VideoGeneration> Generations { get; set; }

        // DİKKAT: Bu bir veritabanı kolonu (sütunu) DEĞİLDİR. (Getter property)
        // Kodu çağırdığında IsMultipleFruits durumuna bakıp sana doğru fix promptu döndürür.
        public string GetAppliedFixPrompt()
        {
            if (IsMultipleFruits)
            {
                return "Buraya ÇOKLU meyveler için kullandığın fix prompt metnini yaz (Örn: preserve the shapes of all fruits...)";
            }

            return "Buraya TEK meyve için kullandığın fix prompt metnini yaz (Örn: preserve the original fruit texture...)";
        }
    }
}
