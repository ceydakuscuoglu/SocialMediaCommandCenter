using Microsoft.Extensions.Configuration;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Interfaces;
using ShakyFruits.Core.Utilities;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShakyFruits.Services
{
    public class GeminiCaptionService : IAiCaptionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiCaptionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // API anahtarını güvenli bir şekilde appsettings.json'dan çekiyoruz
            _apiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<string> GenerateCaptionAsync(List<string> fruits, string danceStyle, SocialPlatform platform)
        {
            var prompt = PromptBuilder.BuildSocialMediaPrompt(fruits, danceStyle, platform);

            // Gemini API v1beta Endpoint (Flash modeli metin üretimi için en hızlısıdır)
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            // Gemini'nin beklediği JSON veri yapısı
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7, // Yaratıcılık oranı (0.0 ile 1.0 arası, 0.7 sosyal medya için ideal)
                    //maxOutputTokens = 300 // Yanıtın çok uzayıp TikTok sınırlarını aşmaması için
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Yanıtı parse edip sadece üretilen metni alıyoruz
            using var jsonDocument = JsonDocument.Parse(responseString);
            var generatedText = jsonDocument.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return generatedText?.Trim() ?? "Açıklama üretilemedi.";
        }
    }
}