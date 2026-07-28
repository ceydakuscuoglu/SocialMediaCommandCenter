using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using ShakyFruits.Core.Entities;

namespace ShakyFruits.Data
{
    public class ApplicationDbContext : DbContext
    {
        // 1. Constructor (Kurucu Metot): API katmanından gelen veritabanı bağlantı ayarlarını (CEYDAK sunucusu) alır.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 2. DbSets (Veritabanı Tabloları): C# sınıflarımızın SQL'deki karşılıklarıdır.
        public DbSet<FruitType> FruitTypes { get; set; }
        public DbSet<FruitAsset> FruitAssets { get; set; }
        public DbSet<ReferenceVideo> ReferenceVideos { get; set; }
        public DbSet<VideoGeneration> VideoGenerations { get; set; }
        public DbSet<DailyTrend> DailyTrends { get; set; }

        // 3. Fluent API (Kurallar ve İlişkiler): EF Core'un otomatik yapamadığı özel veritabanı ayarlarını burada belirtiriz.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Örnek bir Best Practice: Enum değerlerini veritabanına 0, 1, 2 (int) olarak değil, 
            // okunaklı olması için "Pending", "Completed" (string) olarak kaydeder.
            modelBuilder.Entity<VideoGeneration>()
                .Property(v => v.Status)
                .HasConversion<string>();

            // Not: Many-to-Many (Çoka-Çok) ilişkimizi EF Core otomatik algılayacağı için buraya ekstra kod yazmamıza gerek yok.
        }

        // 4. Otomatik Tarih Yönetimi (Senior Mühendislik Dokunuşu)
        // Veritabanına her kayıt atıldığında CreatedAt ve UpdatedAt tarihlerini senin yerine otomatik doldurur.
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                // Güncelleme yapılıyorsa UpdatedAt tarihini şu anki zamana çek
                entity.UpdatedAt = DateTime.Now;

                // Eğer veri İLK DEFA ekleniyorsa CreatedAt tarihini şu anki zamana çek
                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.Now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}