using System.Threading.Tasks;
using ShakyFruits.Core.Enums;

namespace ShakyFruits.Core.Interfaces
{
    public interface IVideoUploaderService
    {
        // Kapak fotoğrafı (coverImagePath) opsiyonel olarak eklendi
        Task<bool> UploadVideoAsync(string videoFilePath, string caption, SocialPlatform platform, string coverImagePath = null);
    }
}