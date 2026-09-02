using System.Collections.Generic;
using System.Threading.Tasks;
using ShakyFruits.Core.Enums;

namespace ShakyFruits.Core.Interfaces
{
    public interface IAiCaptionService
    {
        Task<string> GenerateCaptionAsync(List<string> fruits, string danceStyle, SocialPlatform platform);
    }
}