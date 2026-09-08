using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShakyFruits.Core.Entities;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Interfaces;
using ShakyFruits.Data;

namespace ShakyFruits.Services
{
    public class PublishService : IPublishService
    {
        private readonly ApplicationDbContext _context;
        private readonly IVideoUploaderService _uploaderService;

        public PublishService(ApplicationDbContext context, IVideoUploaderService uploaderService)
        {
            _context = context;
            _uploaderService = uploaderService;
        }

        public async Task<object?> PublishVideoAsync(int videoGenerationId, string? videoPath, string caption, SocialPlatform platform)
        {
            var generation = await _context.VideoGenerations
                .Include(v => v.FruitAsset)
                .FirstOrDefaultAsync(v => v.Id == videoGenerationId);

            if (generation == null)
            {
                return null;
            }

            string autoCoverPath = generation.FruitAsset?.ImagePath ?? string.Empty;
            string videoToUpload = videoPath ?? generation.OutputVideoPath ?? string.Empty;

            bool success = await _uploaderService.UploadVideoAsync(
                videoToUpload,
                caption,
                platform,
                autoCoverPath);

            if (!success)
            {
                throw new Exception("Playwright yükleme sırasında bir hata oluştu.");
            }

            var publishedVideo = new PublishedVideo
            {
                VideoGenerationId = generation.Id,
                Platform = platform,
                PublishedAt = DateTime.UtcNow
            };

            _context.PublishedVideos.Add(publishedVideo);
            await _context.SaveChangesAsync();

            return new
            {
                message = "Video platforma başarıyla yüklendi ve Yayınlananlar (PublishedVideo) tablosuna eklendi!",
                publishedAt = publishedVideo.PublishedAt
            };
        }
    }
}
