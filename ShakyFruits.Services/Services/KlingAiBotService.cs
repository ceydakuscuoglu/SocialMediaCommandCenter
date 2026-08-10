using Microsoft.Playwright;

namespace ShakyFruits.Services
{
    public class KlingAiBotService
    {
        // === HTML SEÇİCİLERİ ===
        private readonly string SELECTOR_LOADING_SPINNER = ".uploading";
        private readonly string SELECTOR_VIDEO_INPUT = ".motion-video-upload-wrapper input[type='file']";
        private readonly string SELECTOR_IMAGE_INPUT = ".human-image-upload-wrapper input[type='file']";
        private readonly string SELECTOR_DELETE_IMAGE_BTN = ".human-image-upload-wrapper [icon-name='IconDelete']";
        private readonly string SELECTOR_PROMPT_TEXTAREA = ".prompt-editor-wrapper .ProseMirror";
        private readonly string SELECTOR_GENERATE_BTN = ""; // Kredileri korumak için şimdilik boş! :)

        // Metot imzasında targetUrl parametresini nullable (isteğe bağlı) yaptık
        public async Task RunTestAsync(bool isRecreate, string? targetUrl = null)
        {
            using var playwright = await Playwright.CreateAsync();
            var userDataDir = Path.Combine(AppContext.BaseDirectory, "KlingSession");

            await using var context = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = false,
                SlowMo = 500
            });

            var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();

            // Akıllı Yönlendirme Mantığı:
            // Recreate ise ve dışarıdan bir URL verildiyse ona git, değilse sitenin standart sayfasına git
            string urlToGo = isRecreate && !string.IsNullOrWhiteSpace(targetUrl)
                ? targetUrl
                : "https://kling.ai/app/video-motion-control/new";

            // 1. Belirlenen URL'e git
            await page.GotoAsync(urlToGo);
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);


            // --- TEST VERİLERİ ---
            // DİKKAT: Bilgisayarında gerçekten bu yollarda test dosyaları olduğundan emin ol, yoksa bot hata fırlatır!
            string fruitImagePath = @"D:\TempAssets\fruit_images\test_cilek.png";
            string referenceVideoPath = @"D:\TempAssets\reference_videos\ssstik.io_@aeedais_1786078923253.mp4";
            string promptText = "A cute anthropomorphic strawberry dancing salsa, highly detailed, 4k";
            // ---------------------

            if (isRecreate)
            {
                // Yönlendirme (GotoAsync) sonrası sitenin modalı çıkarması için biraz süre tanı
                await Task.Delay(1500);

                // 1. Ekranda o yeşil "Confirm" butonu belirdi mi diye kontrol et
                var confirmBtn = page.GetByRole(AriaRole.Button, new() { Name = "Confirm", Exact = true });

                if (await confirmBtn.IsVisibleAsync())
                {
                    // Buton varsa tıkla ve arayüzün yeni URL'e göre oturmasını bekle
                    await confirmBtn.ClickAsync();
                    await Task.Delay(1500);
                }

                // 2. Recreate (Görseli temizle ve yenisini yükle)
                var deleteBtn = page.Locator(SELECTOR_DELETE_IMAGE_BTN);

                // Eğer silme butonu ekranda varsa (yani resim yüklüyse) tıkla
                if (await deleteBtn.IsVisibleAsync())
                {
                    await deleteBtn.ClickAsync();
                    await Task.Delay(1000); // Silme animasyonunu bekle
                }

                // 3. Yeni resmi yükle
                await page.Locator(SELECTOR_IMAGE_INPUT).SetInputFilesAsync(fruitImagePath);

                // UI'ın tepki verip spinner'ı çıkarması için yarım saniye müsaade et
                await Task.Delay(500);

                // Fotoğraf yükleme animasyonunun kaybolmasını bekle
                await page.Locator(SELECTOR_LOADING_SPINNER).Last.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 120000
                });
            }
            else
            {
                // SENARYO 2: Local Upload (Her ikisini de sıfırdan yükle)

                // 1. Video dosyasını yüklemeye çalış
                await page.Locator(SELECTOR_VIDEO_INPUT).SetInputFilesAsync(referenceVideoPath);

                // Sitenin uyarı (Modal) animasyonunu çıkarması ihtimaline karşı yarım saniye bekle
                await Task.Delay(500);

                // 2. Ekranda o yeşil "Confirm" butonu belirdi mi diye kontrol et
                var confirmBtn = page.GetByRole(AriaRole.Button, new() { Name = "Confirm", Exact = true });

                if (await confirmBtn.IsVisibleAsync())
                {
                    // Buton varsa tıkla ve uyarının kapanmasını bekle
                    await confirmBtn.ClickAsync();
                    await Task.Delay(1000);

                    // Mod değiştiği ve arayüz sıfırlandığı için videoyu garanti olsun diye tekrar yükle
                    await page.Locator(SELECTOR_VIDEO_INPUT).SetInputFilesAsync(referenceVideoPath);

                    // Arayüzün spinner'ı çıkarması için yarım saniye bekle
                    await Task.Delay(500);
                }

                // Video yükleme animasyonunun (ekrandaki ilk spinner'ın) kaybolmasını bekle
                await page.Locator(SELECTOR_LOADING_SPINNER).First.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 120000
                });

                // 3. Son olarak meyve resmini yükle
                await page.Locator(SELECTOR_IMAGE_INPUT).SetInputFilesAsync(fruitImagePath);

                // Arayüzün fotoğraf spinner'ını çıkarması için yarım saniye bekle
                await Task.Delay(500);

                // Fotoğraf yükleme animasyonunun (ekrandaki son spinner'ın) kaybolmasını bekle
                await page.Locator(SELECTOR_LOADING_SPINNER).Last.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 120000
                });
            }

            // Ortak Adımlar: Prompt kutusuna metni yazdır
            await page.Locator(SELECTOR_PROMPT_TEXTAREA).FillAsync(promptText);

            // Kredi gitmemesi için tıklama kodu tamamen kapalı
            // await page.Locator(SELECTOR_GENERATE_BTN).ClickAsync(); 

            // İşlemler bittikten sonra sonucu rahatça izleyebilmen için tarayıcı 15 saniye açık kalacak
            await Task.Delay(15000);
        }
    }
}