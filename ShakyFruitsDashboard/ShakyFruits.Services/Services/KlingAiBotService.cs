using Microsoft.Playwright;

namespace ShakyFruits.Services
{
    public class KlingAiBotService
    {
        // === GLOBAL STATE (DURUM) DEĞİŞKENLERİ ===
        private IPlaywright? _playwright;
        private IBrowserContext? _context;
        private IPage? _page;

        // === HTML SEÇİCİLERİ ===
        // Her yükleme alanı için kendi yükleme barını (spinner) hedefliyoruz
        private readonly string SELECTOR_VIDEO_SPINNER = ".motion-video-upload-wrapper .uploading.all-center";
        private readonly string SELECTOR_IMAGE_SPINNER = ".human-image-upload-wrapper .uploading.all-center";
        private readonly string SELECTOR_VIDEO_INPUT = ".motion-video-upload-wrapper input[type='file']";
        private readonly string SELECTOR_IMAGE_INPUT = ".human-image-upload-wrapper input[type='file']";
        private readonly string SELECTOR_PROMPT_TEXTAREA = ".prompt-box .tiptap.ProseMirror"; // Güncellendi

        private readonly string SELECTOR_SETTING_BTN = ".setting-select";
        private readonly string SELECTOR_MODEL_DROPDOWN = ".ai-web-select-model-version";
        private readonly string SELECTOR_RESOLUTION_TAB = ".option-tab-item";
        private readonly string SELECTOR_GENERATE_BTN = "button.button-pay";
        private readonly string SELECTOR_CREDIT_VALUE = "button.button-pay .price .value";

        // 1. AŞAMA: HAZIRLIK VE YÜKLEME (Worker tarafından çağrılır)
        public async Task<int> PrepareAndGetCostAsync(
            bool isRecreate,
            string fruitImagePath,
            string referenceVideoPath,
            string promptText,
            string? targetUrl = null,
            string targetModel = "VIDEO 2.6",
            string targetResolution = "720p")
        {
            if (_playwright == null || _context == null)
            {
                _playwright = await Playwright.CreateAsync();
                var userDataDir = Path.Combine(AppContext.BaseDirectory, "KlingSession");

                _context = await _playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions
                {
                    // 1. GÖRÜNMEZLİK: Tarayıcı arayüzü çizilmez, RAM ve CPU kullanımı %70 oranında düşer.
                    Headless = true,

                    // 2. HIZ: İnsan gözü izlesin diye koyduğumuz "SlowMo = 500" gecikmesini sildik. 
                    // Bot artık elementleri bulduğu milisaniyede tıklayacak.

                    // 3. EKRAN BOYUTU KORUMASI: Headless tarayıcılar bazen varsayılan olarak 800x600 çözünürlükte açılır.
                    // Bu durum sitenin mobil görünüme geçmesine ve bizim aradığımız elementlerin (butonların) kaybolmasına sebep olabilir.
                    // Sabit bir Full HD çözünürlük vererek UI'ın her zaman doğru yüklenmesini garantiye alıyoruz.
                    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },

                    // 4. ANTI-BOT (Hayalet Modu): Headless tarayıcılar siteye bağlanırken "Ben Headless Chrome'um" diye bağırır.
                    // Kling AI'ın güvenlik duvarına (Cloudflare vb.) takılmamak için normal bir Windows kullanıcısı gibi davranıyoruz.
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",

                    // 5. GEREKSİZ YÜKLERİ ENGELLEME: Sunucu performansını artırmak için bildirimleri ve kamera izni gibi pop-up'ları baştan engelliyoruz.
                    Permissions = new[] { "notifications" }
                });
            }

            _page = _context.Pages.FirstOrDefault() ?? await _context.NewPageAsync();

            string urlToGo = isRecreate && !string.IsNullOrWhiteSpace(targetUrl)
                ? targetUrl
                : "https://kling.ai/app/video-motion-control/new";

            await _page.GotoAsync(urlToGo);
            await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            // === 0. POPUP ENGELİNİ AŞMA (Switch Spaces) ===
            try
            {
                // Ekranda "Confirm" yazan butonu ara ve en fazla 3 saniye bekle
                var confirmBtn = _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Confirm", Exact = true });

                await confirmBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
                await confirmBtn.ClickAsync();
                Console.WriteLine("🟢 Eski workspace uyarı popup'ı başarıyla kapatıldı.");
            }
            catch (TimeoutException)
            {
                // 3 saniye içinde popup çıkmazsa buraya düşer. Sistem hata vermeden normal akışına devam eder.
            }


            // === 1. DOSYA YÜKLEME VE AKILLI BEKLEME ===
            if (isRecreate)
            {
                // Sadece resmi yükle ve SADECE resim yükleme alanındaki spinner'ı bekle
                await _page.Locator(SELECTOR_IMAGE_INPUT).SetInputFilesAsync(fruitImagePath);
                await _page.Locator(SELECTOR_IMAGE_SPINNER).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 90000 });
            }
            else
            {
                // Önce videoyu yükle ve SADECE video yükleme alanındaki spinner'ı bekle
                await _page.Locator(SELECTOR_VIDEO_INPUT).SetInputFilesAsync(referenceVideoPath);
                await _page.Locator(SELECTOR_VIDEO_SPINNER).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 90000 });

                // Sonra resmi yükle ve SADECE resim yükleme alanındaki spinner'ı bekle
                await _page.Locator(SELECTOR_IMAGE_INPUT).SetInputFilesAsync(fruitImagePath);
                await _page.Locator(SELECTOR_IMAGE_SPINNER).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 90000 });
            }

            // === 2. PROMPT YAZIMI (TipTap Editör) ===
            var promptEditor = _page.Locator(SELECTOR_PROMPT_TEXTAREA);
            await promptEditor.ClickAsync();
            // İnsan gibi harf harf yazdırıyoruz
            await _page.Keyboard.TypeAsync(promptText, new KeyboardTypeOptions { Delay = 10 });


            // === 3. AYARLARI SEÇME ===
            await _page.Locator(SELECTOR_MODEL_DROPDOWN).ClickAsync();
            await Task.Delay(500);
            await _page.Locator(".el-select-dropdown__item").Filter(new LocatorFilterOptions { HasText = targetModel }).ClickAsync();

            await _page.Locator(SELECTOR_SETTING_BTN)
                       .Filter(new LocatorFilterOptions { HasTextRegex = new System.Text.RegularExpressions.Regex("720p|1080p", System.Text.RegularExpressions.RegexOptions.IgnoreCase) })
                       .ClickAsync();
            await Task.Delay(500);

            await _page.Locator(SELECTOR_RESOLUTION_TAB).Filter(new LocatorFilterOptions { HasText = targetResolution }).ClickAsync();
            await Task.Delay(1000); // Maliyetin güncellenmesi için kısa bir es


            // === 4. MALİYETİ OKUMA ===
            string costText = await _page.Locator(SELECTOR_CREDIT_VALUE).InnerTextAsync();
            string costString = string.Join("", costText.Where(char.IsDigit));

            return int.TryParse(costString, out int parsed) ? parsed : 0;
            // DİKKAT: Artık zamanlayıcı (Timer) yok, Worker hemen Confirm metodunu çağıracak!
        }

        // 2. AŞAMA: ONAY VE ÜRETİM (Worker tarafından hemen peşine çağrılır)
        public async Task ConfirmAndGenerateAsync()
        {
            if (_page == null)
                throw new Exception("Aktif bir sayfa bulunamadı. Önce hazırlık aşaması yapılmalı.");

            // TEST KODU: Gerçek butona basmak yerine log atıyoruz (Kredi gitmesin diye)
            Console.WriteLine("🟢 [TEST MİMARİSİ] Generate butonuna basıldı olarak kabul ediliyor...");
            await Task.Delay(2000);

            /* GERÇEK KOD (Canlıya çıkarken üstteki 2 satırı silip bunları açacağız)
            var generateBtn = _page.Locator(SELECTOR_GENERATE_BTN);
            await generateBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await generateBtn.ClickAsync();
            await Task.Delay(2000);
            */

            Console.WriteLine("🧹 Sekme temizleniyor ve kapatılıyor...");
            await _page.CloseAsync();
            _page = null;
        }
    }
}