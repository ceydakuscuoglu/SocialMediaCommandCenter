using Microsoft.EntityFrameworkCore;
using ShakyFruits.API.Workers;
using ShakyFruits.Core.Interfaces;
using ShakyFruits.Data;
using ShakyFruits.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
// Diğer servis kayıtlarının olduğu yere ekle
builder.Services.AddHostedService<AnalyticsScraperWorker>();

// Ön yüz (Tauri/Vite) için CORS politikasını tanımlıyoruz
builder.Services.AddCors(options =>
{
    options.AddPolicy("TauriCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()  // Burası değişti! (http://localhost:1420 yerine)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Gemini servisinin HTTP istekleri yapabilmesi için HttpClient'ı kaydediyoruz
builder.Services.AddHttpClient<IAiCaptionService, GeminiCaptionService>();

// Controller IAiCaptionService istediğinde GeminiCaptionService sınıfını verecek
builder.Services.AddScoped<IAiCaptionService, GeminiCaptionService>();
builder.Services.AddScoped<IVideoUploaderService, PlaywrightVideoUploaderService>();

// 1. Veritabanı Bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Dinamik Dosya Yolları (Options Pattern)
builder.Services.Configure<ShakyFruits.Core.Settings.AssetPathOptions>(
    builder.Configuration.GetSection("AssetPaths"));

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<SocialMediaScraperService>();
builder.Services.AddSingleton<KlingAiBotService>();
// 3. Controller Sınıflarını ve Swagger Arayüzünü Sisteme Tanıtma
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Görsel arayüz altyapısı

// 1. Queue Manager'ı Singleton olarak ekliyoruz (Uygulamada tek bir kuyruk örneği olmalı)
builder.Services.AddSingleton<IVideoQueueManager, VideoQueueManager>();
builder.Services.AddSingleton<VideoQueueManager>(sp => (VideoQueueManager)sp.GetRequiredService<IVideoQueueManager>());

// 2. Arka plan işçimizi (BackgroundService) sisteme barındırılan servis olarak kaydediyoruz
builder.Services.AddHostedService<KlingWorkerService>();

// (Not: KlingAiBotService zaten Transient veya Singleton olarak eklenmiş olmalı)

var app = builder.Build();

app.UseRouting();
// CORS politikasını uygulamaya dahil ediyoruz
app.UseCors("TauriCorsPolicy");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 4. Swagger Arayüzünü Aktif Etme
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 5. Gelen istekleri Controller'lara yönlendirme
app.MapControllers();

app.Run();