using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShakyFruits.Core.Models;
using ShakyFruits.Core.Services;
using ShakyFruits.Services;

namespace ShakyFruits.API.Workers
{
    // BackgroundService, uygulamanın yaşam döngüsü boyunca arka planda çalışan bir .NET yapısıdır.
    public class KlingWorkerService : BackgroundService
    {
        private readonly VideoQueueManager _queueManager;
        private readonly KlingAiBotService _botService;
        private readonly ILogger<KlingWorkerService> _logger;

        public KlingWorkerService(
            VideoQueueManager queueManager,
            KlingAiBotService botService,
            ILogger<KlingWorkerService> logger)
        {
            _queueManager = queueManager;
            _botService = botService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kling Video Üretim İşçisi (Worker) arka planda çalışmaya başladı...");

            try
            {
                // Kuyruğa yeni bir iş geldikçe bu döngü tetiklenir
                await foreach (var job in _queueManager.ReadAllAsync(stoppingToken))
                {
                    _logger.LogInformation($"Yeni iş alındı. JobId: {job.JobId}");

                    try
                    {
                        // TODO: İleride burada veritabanındaki (Eyotek tarzı bir mimariyle) 
                        // işin statüsünü "İşleniyor (Processing)" olarak güncelleyeceğiz.

                        // Playwright Botunu Tetikle!
                        await _botService.PrepareAndGetCostAsync(
                            job.IsRecreate,
                            job.SavedImagePath,
                            job.SavedVideoPath,
                            job.AppliedPrompt,
                            job.TargetUrl,
                            job.TargetModel,
                            job.TargetResolution
                        );

                        // İşlemleri bitir ve Generate butonuna bas (Bu metodu bot servisine ekleyeceğiz)
                        await _botService.ConfirmAndGenerateAsync();

                        _logger.LogInformation($"İş başarıyla tamamlandı. JobId: {job.JobId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Bot çalışırken hata oluştu. JobId: {job.JobId}, Hata: {ex.Message}");
                        // TODO: Hata durumunda veritabanında statüyü "Hata (Failed)" olarak güncelle.
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Uygulama kapanırken (Ctrl+C) fırlatılır, güvenli çıkış sağlanır.
                _logger.LogInformation("Worker servisi durduruluyor...");
            }
        }
    }
}