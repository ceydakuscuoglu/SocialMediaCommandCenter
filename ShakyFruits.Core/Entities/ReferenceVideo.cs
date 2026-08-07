using System;
using System.Collections.Generic;
using System.Text;

namespace ShakyFruits.Core.Entities
{
    public enum ReferenceSourceType
    {
        LocalUpload = 0,   // Kendi bilgisayarından yüklediğin mp4
        KlingRecreate = 1  // Kling AI üzerinden recreate edilecek hazır video
    }

    public class ReferenceVideo : BaseEntity
    {
        // Orijinal özellik: Dansın türü (Dashboard'da filtreleme yaparken çok işe yarayacak)
        public string DanceStyle { get; set; }

        // Yeni özellik: Videonun kaynağı (Local mi, Kling mi?)
        public ReferenceSourceType SourceType { get; set; }

        // Orijinal özellik (Güncellendi): Eğer SourceType == LocalUpload ise bu alan dolu olur.
        // Kling ise boş (null) kalabilmesi için soru işareti ekleyerek nullable yaptık.
        public string? VideoPath { get; set; }

        // Yeni özellik: Eğer SourceType == KlingRecreate ise Kling'deki videonun ID'si veya Linki
        public string? KlingSourceUrlOrId { get; set; }

        // Orijinal özellik: Entity Framework Core bire-çok ilişkisi (Buna dokunmuyoruz)
        public virtual ICollection<VideoGeneration>? Generations { get; set; }
    }
}