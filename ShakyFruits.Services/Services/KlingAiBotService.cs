using Microsoft.Playwright;

namespace ShakyFruits.Services
{
    public class KlingAiBotService
    {
        // === GLOBAL STATE (DURUM) DEĞİŞKENLERİ ===
        private IPlaywright? _playwright;
        private IBrowserContext? _context;
        private IPage? _page;
        private bool _isBusy = false;
        private CancellationTokenSource? _timeoutCts;

        // === HTML SEÇİCİLERİ (ESKİ + YENİ) ===
        private readonly string SELECTOR_LOADING_SPINNER = ".uploading";
        private readonly string SELECTOR_VIDEO_INPUT = ".motion-video-upload-wrapper input[type='file']";
        private readonly string SELECTOR_IMAGE_INPUT = ".human-image-upload-wrapper input[type='file']";
        private readonly string SELECTOR_DELETE_IMAGE_BTN = ".human-image-upload-wrapper [icon-name='IconDelete']";
        private readonly string SELECTOR_PROMPT_TEXTAREA = ".prompt-editor-wrapper .ProseMirror";

        private readonly string SELECTOR_SETTING_BTN = ".setting-select";
        private readonly string SELECTOR_MODEL_DROPDOWN = ".ai-web-select-model-version";
        private readonly string SELECTOR_RESOLUTION_TAB = ".option-tab-item";
        private readonly string SELECTOR_GENERATE_BTN = "button.button-pay";
        private readonly string SELECTOR_CREDIT_VALUE = "button.button-pay .price .value";



        // 1. AŞAMA: HAZIRLIK VE FİYAT ALMA
        // Parametrelere fruitImagePath, referenceVideoPath ve promptText eklendi
        public async Task<int> PrepareAndGetCostAsync(
            bool isRecreate,
            string fruitImagePath,
            string referenceVideoPath,
            string promptText,
            string? targetUrl = null,
            string targetModel = "VIDEO 2.6",
            string targetResolution = "720p")
        {
            if (_isBusy) throw new Exception("Bot şu anda başka bir işlem veya onay bekliyor!");
            _isBusy = true; // Botu diğer isteklere kilitliyoruz

            // Playwright ve Tarayıcıyı sadece ilk seferde veya kapanmışsa başlat
            if (_playwright == null || _context == null)
            {
                _playwright = await Playwright.CreateAsync();
                var userDataDir = Path.Combine(AppContext.BaseDirectory, "KlingSession");

                _context = await _playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
                {
                    Headless = false,
                    SlowMo = 500
                });
            }

            _page = _context.Pages.FirstOrDefault() ?? await _context.NewPageAsync();

            string urlToGo = isRecreate && !string.IsNullOrWhiteSpace(targetUrl)
                ? targetUrl
                : "https://kling.ai/app/video-motion-control/new";

            await _page.GotoAsync(urlToGo);
            await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            // --- TEST VERİLERİ (İleride bunları da parametre yapabilirsin) ---
            /* string fruitImagePath = @"D:\TempAssets\fruit_images\test_cilek.png";
             string referenceVideoPath = @"D:\TempAssets\reference_videos\ssstik.io_@aeedais_1786078923253.mp4";
             string promptText = "A cute anthropomorphic strawberry dancing salsa, highly detailed, 4k";*/
            // -----------------------------------------------------------------

            // === 1. DOSYA YÜKLEME VE BEKLEME MANTIĞI ===
            if (isRecreate)
            {
                // ... (Çöp kutusuna tıklama işlemleri) ...

                // Resmi yükle
                await _page.Locator(SELECTOR_IMAGE_INPUT).SetInputFilesAsync(fruitImagePath);
                await Task.Delay(500);

                // MÜKEMMEL BEKLEME: Resim yükleme animasyonunun kaybolmasını bekle!
                await _page.Locator(SELECTOR_LOADING_SPINNER).Last.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 120000 });
            }
            else
            {
                // Videoyu yükle
                await _page.Locator(SELECTOR_VIDEO_INPUT).SetInputFilesAsync(referenceVideoPath);
                await Task.Delay(500);

                // ... (Gerekirse Confirm butonuna basma işlemleri) ...

                // MÜKEMMEL BEKLEME 1: Video yükleme animasyonunun kaybolmasını bekle!
                await _page.Locator(SELECTOR_LOADING_SPINNER).First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 120000 });

                // Resmi yükle
                await _page.Locator(SELECTOR_IMAGE_INPUT).SetInputFilesAsync(fruitImagePath);
                await Task.Delay(500);

                // MÜKEMMEL BEKLEME 2: Fotoğraf yükleme animasyonunun kaybolmasını bekle!
                await _page.Locator(SELECTOR_LOADING_SPINNER).Last.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 120000 });
            }

            // Ortak Adım: Prompt (Yüklemeler tamamen bittikten sonra yazıyoruz)
            await _page.Locator(SELECTOR_PROMPT_TEXTAREA).FillAsync(promptText);


            // === 2. AYARLARI SEÇME (Ekran temizlendikten sonra) ===

            // 1. Model Seçimi (DÜZELTİLEN KISIM: SELECTOR_MODEL_DROPDOWN kullanıyoruz)
            await _page.Locator(SELECTOR_MODEL_DROPDOWN).ClickAsync();
            await Task.Delay(500);
            await _page.Locator(".el-select-dropdown__item").Filter(new LocatorFilterOptions { HasText = targetModel }).ClickAsync();

            // 2. Çözünürlük Seçimi (Burası Regex ile korumalı, zaten doğru çalışıyor)
            await _page.Locator(SELECTOR_SETTING_BTN)
                       .Filter(new LocatorFilterOptions { HasTextRegex = new System.Text.RegularExpressions.Regex("720p|1080p", System.Text.RegularExpressions.RegexOptions.IgnoreCase) })
                       .ClickAsync();
            await Task.Delay(500);

            await _page.Locator(SELECTOR_RESOLUTION_TAB).Filter(new LocatorFilterOptions { HasText = targetResolution }).ClickAsync();

            // Sitenin video süresine göre maliyeti hesaplaması için 1 saniye bekle
            await Task.Delay(1000);


            // === 3. MALİYETİ OKUMA ===
            string costText = await _page.Locator(SELECTOR_CREDIT_VALUE).InnerTextAsync();
            string costString = string.Join("", costText.Where(char.IsDigit));
            int requiredCredits = int.TryParse(costString, out int parsed) ? parsed : 0;

            // Arka planda 5 dakikalık geri sayımı başlat
            StartTimeoutTimer(TimeSpan.FromMinutes(5));

            return requiredCredits;
        }

        // 2. AŞAMA: ONAY VE ÜRETİM
        public async Task ConfirmAndGenerateAsync()
        {
            if (_page == null)
                throw new Exception("Aktif bir sayfa bulunamadı. Önce hazırlık aşaması yapılmalı.");

            /* // Yeşil Generate butonunun seçicisi (Bunu kendi projendeki sabite göre güncelle)
             string generateButtonSelector = ".generate-btn-class";

             // Butona tıkla
             await _page.Locator(generateButtonSelector).ClickAsync();*/

            // TEST KODU: Tıklamayı simüle ediyoruz
            Console.WriteLine("🟢 [TEST MİMARİSİ] Generate butonuna basıldı olarak kabul ediliyor...");
            await Task.Delay(2000);

            // Sayfayı güvenlice kapat
            Console.WriteLine("🟢 [TEST MİMARİSİ] Sayfa temizleniyor ve kapatılıyor...");
            await _page.CloseAsync();
            _page = null;

            // Tıklamayı algılaması için kısa bir bekleme
            await Task.Delay(2000);

            // İşlem bitti, Playwright sayfasını güvenlice kapatarak RAM'i temizle
            await _page.CloseAsync();
            _page = null;
        }

        // 3. AŞAMA: İPTAL
        public async Task CancelGenerationAsync()
        {
            // Kullanıcı vazgeçti, zaman aşımını iptal et ve sekmeyi kapat
            _timeoutCts?.Cancel();
            await CleanUpAsync();
        }

        // === YARDIMCI METOTLAR ===
        private void StartTimeoutTimer(TimeSpan timeout)
        {
            _timeoutCts?.Cancel();
            _timeoutCts?.Dispose();
            _timeoutCts = new CancellationTokenSource();

            var token = _timeoutCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(timeout, token);

                    if (!token.IsCancellationRequested)
                    {
                        Console.WriteLine("⌛ Zaman aşımı! Kullanıcıdan onay gelmedi, sekme temizleniyor...");
                        await CleanUpAsync();
                    }
                }
                catch (TaskCanceledException)
                {
                    // İptal edildiğinde buraya düşer, sorun yok.
                }
            });
        }

        private async Task CleanUpAsync()
        {
            if (_page != null)
            {
                await _page.CloseAsync();
                _page = null;
            }
            _isBusy = false; // Botu kilitten kurtar
        }
    }
}