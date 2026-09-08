using System.Threading.Tasks;
using ShakyFruits.Core.Enums;

namespace ShakyFruits.Core.Interfaces
{
    public interface IPublishService
    {
        Task<object?> PublishVideoAsync(int videoGenerationId, string? videoPath, string caption, SocialPlatform platform);
    }
}
