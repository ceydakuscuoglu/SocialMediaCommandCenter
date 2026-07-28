using System;
using System.Collections.Generic;
using System.Text;

namespace ShakyFruits.Core.Entities
{
    public class ReferenceVideo : BaseEntity
    {
        public string DanceStyle { get; set; } // Örn: Salsa, Breakdance, HipHop
        public string VideoPath { get; set; } // Lokal dosya yolu: \assets\reference_videos\salsa1.mp4

        public virtual ICollection<VideoGeneration> Generations { get; set; }
    }
}