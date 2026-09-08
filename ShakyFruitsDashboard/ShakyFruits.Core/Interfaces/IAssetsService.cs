using System.Collections.Generic;
using System.Threading.Tasks;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;

namespace ShakyFruits.Core.Interfaces
{
    public interface IAssetsService
    {
        Task<object> AddFruitAssetAsync(string title, string imagePath, bool isMultipleFruits, List<int> fruitTypeIds);
        Task<object> GetFruitAssetsAsync();
        Task<object?> GetFruitAssetByIdAsync(int id);
        Task<object?> UpdateFruitAssetAsync(int id, string title, string imagePath, bool isMultipleFruits, List<int> fruitTypeIds);

        Task<object> AddReferenceVideoAsync(string? danceStyle, ReferenceSourceType sourceType, string? videoPath, string? klingSourceUrlOrId);
        Task<object> GetReferenceVideosAsync();
        Task<object?> UpdateReferenceVideoAsync(int id, string? danceStyle, ReferenceSourceType sourceType, string? videoPath, string? klingSourceUrlOrId);

        Task<object> AddFruitTypeAsync(string name);
        Task<object> GetFruitTypesAsync();
        Task<object?> UpdateFruitTypeAsync(int id, string name);

        Task<object?> CompleteVideoGenerationAsync(int videoGenerationId, string outputVideoPath, string? aiGeneratedCaption);
    }
}
