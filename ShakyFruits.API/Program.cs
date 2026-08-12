using Microsoft.EntityFrameworkCore;
using ShakyFruits.Data;
using ShakyFruits.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Dinamik Dosya Yolları (Options Pattern)
builder.Services.Configure<ShakyFruits.Core.Settings.AssetPathOptions>(
    builder.Configuration.GetSection("AssetPaths"));

builder.Services.AddSingleton<KlingAiBotService>();
//builder.Services.AddScoped<ShakyFruits.Services.KlingAiBotService>();
// 3. Controller Sınıflarını ve Swagger Arayüzünü Sisteme Tanıtma
builder.Services.AddControllers(); // FruitsController'ı bulmasını sağlar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Görsel arayüz altyapısı

var app = builder.Build();

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