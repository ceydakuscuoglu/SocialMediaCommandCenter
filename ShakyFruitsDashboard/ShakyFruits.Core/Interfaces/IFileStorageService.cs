using System.IO;
using System.Threading.Tasks;

namespace ShakyFruits.Core.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream stream, string fileName, string folderName);
    }
}
