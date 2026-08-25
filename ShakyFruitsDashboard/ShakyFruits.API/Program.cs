using Microsoft.EntityFrameworkCore;
using ShakyFruits.API.Workers;
using ShakyFruits.Core.Services;
using ShakyFruits.Data;
using ShakyFruits.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();

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

// 1. Veritabanı Bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Dinamik Dosya Yolları (Options Pattern)
builder.Services.Configure<ShakyFruits.Core.Settings.AssetPathOptions>(
    builder.Configuration.GetSection("AssetPaths"));

builder.Services.AddScoped<SocialMediaScraperService>();
builder.Services.AddSingleton<KlingAiBotService>();
//builder.Services.AddScoped<ShakyFruits.Services.KlingAiBotService>();
// 3. Controller Sınıflarını ve Swagger Arayüzünü Sisteme Tanıtma
builder.Services.AddControllers(); // FruitsController'ı bulmasını sağlar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Görsel arayüz altyapısı

// 1. Queue Manager'ı Singleton olarak ekliyoruz (Uygulamada tek bir kuyruk örneği olmalı)
builder.Services.AddSingleton<VideoQueueManager>();

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