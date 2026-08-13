using Microsoft.EntityFrameworkCore; // Include() fonksiyonu için gerekli
using Microsoft.Extensions.DependencyInjection; // IServiceScopeFactory için gerekli
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Services;
using ShakyFruits.Services;
using ShakyFruits.Data;
// DbContext'inin bulunduğu namespace'i (Örn: ShakyFruits.Data) eklemeyi unutma

namespace ShakyFruits.API.Workers
{
    public class KlingWorkerService : BackgroundService
    {
        private readonly VideoQueueManager _queueManager;
        private readonly KlingAiBotService _botService;
        private readonly IServiceScopeFactory _scopeFactory; // EF Core bağlantısı için
        private readonly ILogger<KlingWorkerService> _logger;

        public KlingWorkerService(
            VideoQueueManager queueManager,
            KlingAiBotService botService,
            IServiceScopeFactory scopeFactory,
            ILogger<KlingWorkerService> logger)
        {
            _queueManager = queueManager;
            _botService = botService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var jobId in _queueManager.ReadAllAsync(stoppingToken))
            {
                // Her iş için yeni, temiz bir veritabanı bağlantısı açıyoruz
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // 1. İşi, ilişkili olduğu dosya yollarıyla (FruitAsset ve ReferenceVideo) birlikte çekiyoruz
                    var job = await dbContext.VideoGenerations
                        .Include(x => x.FruitAsset)
                        .Include(x => x.ReferenceVideo)
                        .FirstOrDefaultAsync(x => x.Id == jobId, stoppingToken);

                    if (job == null) continue;

                    try
                    {
                        // 2. Statüyü "İşleniyor" yap
                        job.Status = GenerationStatus.Processing;
                        await dbContext.SaveChangesAsync(stoppingToken);

                        // 3. İlişkili tablolardan gerçek dosya yollarını oku (Null korumalı)
                        string imagePath = job.FruitAsset.ImagePath;
                        string videoPath = job.ReferenceVideo?.VideoPath ?? string.Empty;

                        // 4. Playwright Botunu Tetikle
                        await _botService.PrepareAndGetCostAsync(
                            job.IsRecreate,
                            imagePath,
                            videoPath,
                            job.AppliedPrompt,
                            job.TargetUrl,
                            job.TargetModel,
                            job.TargetResolution
                        );

                        await _botService.ConfirmAndGenerateAsync();

                        // 5. Başarılı olursa statüyü güncelle
                        job.Status = GenerationStatus.Completed;
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        // 6. Hata olursa sebebiyle birlikte veritabanına yaz
                        job.Status = GenerationStatus.Failed;
                        job.ErrorMessage = ex.Message;
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogError($"Bot hatası (JobId: {jobId}): {ex.Message}");
                    }
                }
            }
        }
    }
}