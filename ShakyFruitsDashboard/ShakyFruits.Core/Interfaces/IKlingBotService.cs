using System.IO;
using System.Threading.Tasks;

namespace ShakyFruits.Core.Interfaces
{
    public interface IKlingBotService
    {
        Task<object> PrepareAsync(
            int? existingFruitAssetId,
            Stream? fruitImageStream,
            string? fruitImageFileName,
            int? existingReferenceVideoId,
            Stream? referenceVideoStream,
            string? referenceVideoFileName,
            bool isRecreate,
            string targetModel,
            string targetResolution,
            bool isMultipleFruits,
            string? targetUrl,
            string? fruitTitle,
            string? danceStyle,
            string? title = null);

        Task<object?> ConfirmAsync(string sessionId);

        Task<object> GetCurrentCreditsAsync();
    }
}
