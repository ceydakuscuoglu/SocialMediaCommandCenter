using System;
using System.Collections.Generic;
using System.Text;
using ShakyFruits.Core.Enums;

namespace ShakyFruits.Core.Entities
{
    public class VideoGeneration : BaseEntity
    {
        // Foreign Keys (Yabancı Anahtarlar)
        public int FruitAssetId { get; set; }
        public int ReferenceVideoId { get; set; }

        public GenerationStatus Status { get; set; } = GenerationStatus.Pending;

        // Üretim bittikten sonra dolacak alanlar
        public string? OutputVideoPath { get; set; } // Çıktı video yolu
        public string? AiGeneratedCaption { get; set; } // Otomatik üretilen hashtagli metin

        // Navigation Properties (EF Core'un tabloları bağlaması için 'virtual' anahtar kelimesi önemlidir)
        public virtual FruitAsset FruitAsset { get; set; } // Değişen kısım
        public virtual ReferenceVideo ReferenceVideo { get; set; }
    }
}
