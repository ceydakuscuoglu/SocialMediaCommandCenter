using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ShakyFruits.Core.Enums;
using ShakyFruits.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShakyFruits.Services
{
    public class PlaywrightVideoUploaderService : IVideoUploaderService
    {
        readonly ILogger<PlaywrightVideoUploaderService> _logger;

        public PlaywrightVideoUploaderService(ILogger<PlaywrightVideoUploaderService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> UploadVideoAsync(string videoFilePath, string caption, SocialPlatform platform, string coverImagePath = null)
        {
            if (!File.Exists(videoFilePath))
            {
                _logger.LogError("Yüklenecek video dosyası diskte bulunamadı: {Path}", videoFilePath);
                return false;
            }

            try
            {
                using var playwright = await Playwright.CreateAsync();

                // Bilgisayarında tarayıcı verilerinin saklanacağı güvenli bir klasör yolu oluşturuyoruz (Örn: AppData/Local/ShakyFruitsProfile)
                var userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ShakyFruitsProfile");

                _logger.LogInformation("Kalıcı tarayıcı profili yükleniyor: {ProfilePath}", userDataDir);

                // Tarayıcıyı bu kalıcı profille başlatıyoruz
                var context = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
                {
                    Headless = false,
                    Args = new[] { "--start-maximized" },
                    ViewportSize = ViewportSize.NoViewport // Tam ekran çalışması için bu ayar önemlidir
                });

                // Zaten açık bir sekme varsa onu kullan, yoksa yeni sekme aç
                var page = context.Pages.Count > 0 ? context.Pages[0] : await context.NewPageAsync();

                if (platform == SocialPlatform.TikTok)
                {
                    await UploadToTikTokAsync(page, videoFilePath, caption, coverImagePath);
                }
                else if (platform == SocialPlatform.Instagram)
                {
                    await UploadToInstagramAsync(page, videoFilePath, caption, coverImagePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Platform} platformuna video yüklenirken kritik hata oluştu.", platform);
                return false;
            }
        }

        private async Task UploadToTikTokAsync(IPage page, string videoPath, string caption, string coverPath)
        {
            _logger.LogInformation("TikTok Creator Center sayfasına gidiliyor...");
            await page.GotoAsync("https://www.tiktok.com/creator-center/upload");
            await Task.Delay(300000);

            // 1. VİDEO YÜKLEME
            _logger.LogInformation("Video dosyası seçiliyor...");
            // DOM'dan aldığımız gizli (display: none) file input'u seçip videoyu gönderiyoruz
            var videoInput = await page.WaitForSelectorAsync("input[type='file'][accept='video/*']",
                new PageWaitForSelectorOptions { State = WaitForSelectorState.Attached });
            await videoInput.SetInputFilesAsync(videoPath);

            // 2. AÇIKLAMA (CAPTION) YAZMA
            _logger.LogInformation("Açıklama ve hashtagler giriliyor...");
            // React Draft.js editör kutusunu seçiyoruz
            var captionBoxSelector = "div.public-DraftEditor-content[contenteditable='true']";
            var captionBox = await page.WaitForSelectorAsync(captionBoxSelector);
            await captionBox.ClickAsync();

            // TikTok'un JS eventlerini tetiklemek için InsertText yerine TypeAsync (klavye vuruşları) kullanmak daha sağlıklıdır
            await page.Keyboard.TypeAsync(caption, new KeyboardTypeOptions { Delay = 50 });
            await Task.Delay(2000);

            // 3. ÖZEL KAPAK FOTOĞRAFI YÜKLEME (Eğer kapak yolu verildiyse)
            if (!string.IsNullOrEmpty(coverPath) && File.Exists(coverPath))
            {
                _logger.LogInformation("Kapak fotoğrafı yükleniyor...");

                // "Kapağı düzenle" elementine tıkla
                var editCoverBtn = await page.WaitForSelectorAsync("div.edit-container");
                await editCoverBtn.ClickAsync();

                await Task.Delay(1000); // Pop-up animasyonunun açılmasını bekle

                // Kapak resmini seçmek için açılan gizli file input
                var coverInput = await page.WaitForSelectorAsync("input[type='file'][accept*='image']",
                    new PageWaitForSelectorOptions { State = WaitForSelectorState.Attached });
                await coverInput.SetInputFilesAsync(coverPath);

                await Task.Delay(1500); // Resmin işlenmesini bekle

                // "Kaydet" butonuna tıkla (has-text ile metin içeren butonu yakalıyoruz)
                var saveCoverBtn = await page.WaitForSelectorAsync("button:has-text('Kaydet')");
                await saveCoverBtn.ClickAsync();

                await Task.Delay(2000); // Pop-up'ın kapanmasını bekle
            }

            // 4. VİDEOYU PAYLAŞ (YAYINLA)
            _logger.LogInformation("Video paylaşılıyor (Post)...");
            // Video yüklemesinin tamamen bitmesi için biraz daha uzun bir güvenlik süresi
            await Task.Delay(5000);

            // Gönderdiğin DOM'daki 'data-e2e' özelliği en sağlam seçicidir
            var postButton = await page.WaitForSelectorAsync("button[data-e2e='post_video_button']");

            // NOT: Test aşamasında yanlışlıkla video paylaşmamak için aşağıdaki satırı yorum satırı yapabilirsin. 
            // Canlıya alırken yorumu kaldır.
            // await postButton.ClickAsync(); 

            _logger.LogInformation("TikTok yükleme simülasyonu başarıyla tamamlandı!");
            // Botu burada tamamen dondurur. Test için mükemmeldir!
            await page.PauseAsync();
        }
        private async Task UploadToInstagramAsync(IPage page, string videoPath, string caption, string coverPath)
        {
            _logger.LogInformation("Instagram anasayfasına gidiliyor...");
            await page.GotoAsync("https://www.instagram.com/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // 1. OLUŞTUR BUTONUNA TIKLA
            _logger.LogInformation("Yeni gönderi (Oluştur) ekranı açılıyor...");
            var createButton = page.Locator("svg[aria-label='Yeni Gönderi']").First;
            await createButton.ClickAsync();
            await Task.Delay(2000);

            // Gönderi ve Canlı menüsü çıkarsa
            var postOption = page.GetByText("Gönderi", new PageGetByTextOptions { Exact = true }).First;
            if (await postOption.IsVisibleAsync())
            {
                await postOption.ClickAsync();
                await Task.Delay(2000);
            }

            // 2. DOSYAYI YÜKLE
            _logger.LogInformation("Video dosyası Instagram'a yükleniyor...");
            var videoInput = page.Locator("input[type='file']").First;
            await videoInput.SetInputFilesAsync(videoPath);

            await Task.Delay(5000); // Videonun işlenmesini bekliyoruz

            // 3. KIRPMAYI DÜZELT (ORİJİNAL BOYUT)
            _logger.LogInformation("Videonun kırpılmaması için 'Orijinal' boyut seçiliyor...");
            var cropButton = page.Locator("svg[aria-label='Kırpmayı seç']").Locator("xpath=ancestor::button").First;
            if (await cropButton.IsVisibleAsync())
            {
                await cropButton.ClickAsync();
                await Task.Delay(1000);

                // TUZAK ÇÖZÜLDÜ: Sadece "Orijinal" kelimesini tam eşleşmeyle buluyoruz
                var originalOption = page.GetByText("Orijinal", new PageGetByTextOptions { Exact = true }).First;
                await originalOption.ClickAsync();
                await Task.Delay(1500);
            }

            // 4. İLERİ (1. Tıklama - Kapak/Filtre Ekranı)
            // TUZAK ÇÖZÜLDÜ: Sadece "İleri" kelimesini tam eşleşmeyle buluyoruz
            var nextButton = page.GetByText("İleri", new PageGetByTextOptions { Exact = true }).First;
            await nextButton.ClickAsync();
            await Task.Delay(2000);

            // 5. KAPAK FOTOĞRAFI YÜKLEME
            if (!string.IsNullOrEmpty(coverPath) && File.Exists(coverPath))
            {
                _logger.LogInformation("Kapak fotoğrafı sekmesine geçiliyor...");

                var coverTab = page.GetByText("Kapak", new PageGetByTextOptions { Exact = true }).First;
                if (await coverTab.IsVisibleAsync())
                {
                    await coverTab.ClickAsync();
                    await Task.Delay(1000);
                }

                _logger.LogInformation("Bilgisayardan kapak resmi yükleniyor...");
                var coverInput = page.Locator("input[type='file'][accept*='image']").First;
                await coverInput.SetInputFilesAsync(coverPath);

                await Task.Delay(2500);
            }

            // 6. İLERİ (2. Tıklama - Metin Ekranı)
            if (await nextButton.IsVisibleAsync())
            {
                await nextButton.ClickAsync();
                await Task.Delay(2000);
            }

            // 7. AÇIKLAMA YAZMA (Lexical Editor)
            _logger.LogInformation("Açıklama (caption) yazılıyor...");
            var captionBox = page.Locator("div[aria-label='Açıklama yaz...'][contenteditable='true']").First;
            await captionBox.ClickAsync();

            await page.Keyboard.TypeAsync(caption, new KeyboardTypeOptions { Delay = 30 });
            await Task.Delay(2000);

            // ================= YENİ EKLENEN KISIM BAŞLANGICI =================

            // ================= 7.5 AKILLI ŞALTER KONTROLÜ (GÜNCELLENDİ) =================

            _logger.LogInformation("Gelişmiş ayarlar menüsü kontrol ediliyor...");
            var advancedSettingsBtn = page.GetByText("Gelişmiş ayarlar", new PageGetByTextOptions { Exact = true }).First;

            if (await advancedSettingsBtn.IsVisibleAsync())
            {
                await advancedSettingsBtn.ClickAsync();
                await Task.Delay(1500);
            }

            _logger.LogInformation("Ayar şalterleri kontrol ediliyor ('İçeriği planla' hariç)...");
            var switches = await page.Locator("input[role='switch'][type='checkbox']").AllAsync();

            foreach (var toggle in switches)
            {
                // Playwright üzerinden JavaScript çalıştırarak, şalterin bulunduğu DOM satırının (3-4 div üstünün) metnini okuyoruz.
                string rowText = await toggle.EvaluateAsync<string>(@"el => {
            let node = el;
            // 4 seviye yukarı çıkıp o kapsayıcının metnini al
            for(let i=0; i<4; i++) { 
                if(node.parentElement) node = node.parentElement; 
            }
            return node.innerText || '';
        }");

                // Eğer bu şalter "İçeriği planla" satırındaysa TIKLAMADAN es geç!
                if (rowText.Contains("İçeriği planla"))
                {
                    _logger.LogInformation("'İçeriği planla' şalteri tespit edildi, atlanıyor...");
                    continue;
                }

                // Diğer şalterler kapalıysa açıyoruz
                var isChecked = await toggle.GetAttributeAsync("aria-checked");
                if (isChecked == "false")
                {
                    await toggle.ClickAsync(new LocatorClickOptions { Force = true });
                    await Task.Delay(500);
                }
            }

            await Task.Delay(1000);

            // ================= 7.5 BİTİŞ =================

            // ================= YENİ EKLENEN KISIM BİTİŞİ =================

            // 8. PAYLAŞ
            _logger.LogInformation("Instagram'da Paylaş (Share) butonuna basılıyor...");
            var shareButton = page.GetByText("Paylaş", new PageGetByTextOptions { Exact = true }).First;

            // await shareButton.ClickAsync(); // CANLIYA ALIRKEN YORUMU KALDIR

            _logger.LogInformation("Instagram yükleme simülasyonu başarıyla tamamlandı!");
            await Task.Delay(60000); // İzlemek için 1 dakika bekle
        }
    }
}