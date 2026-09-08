using System;
using System.IO;
using System.Threading.Tasks;
using ShakyFruits.Core.Interfaces;

namespace ShakyFruits.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        public async Task<string> SaveFileAsync(Stream stream, string fileName, string folderName)
        {
            if (stream == null || stream.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream);
            }

            return filePath;
        }
    }
}
