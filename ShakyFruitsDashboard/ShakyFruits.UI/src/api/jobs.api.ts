export enum GenerationStatus {
  Pending = 0,
  Processing = 1,
  Completed = 2,
  Failed = 3
}

export interface VideoJob {
  id: number;
  fruitImagePath: string;
  referenceVideoPath: string;
  isRecreate: boolean;
  appliedPrompt: string;
  status: GenerationStatus; // Artık string değil, bu numaralı enum tipinde
  errorMessage: string | null;
  outputVideoPath: string | null;
  createdAt: string; // C# DateTime'ı JSON'da ISO string olarak gelir
}

export interface UpdateHistoricalVideoRequest {
  fruitAssetId: number;
  referenceVideoId?: number;
  isRecreate: boolean;
  targetUrl?: string;
  outputVideoPath?: string;
  aiGeneratedCaption?: string;
  isPublished: boolean;
  platform: number; // 0: TikTok, 1: Instagram (Backend enum yapına göre)
  postUrl?: string;
  publishedAt?: string;
}
// CEYDAK sunucusundaki API adresimiz
// Not: Port numarasını (.NET API'nin çalıştığı port, örn: 5000, 5001 veya 7100 vb.) kendi yapına göre güncelle.
const API_BASE_URL = "http://localhost:5290/api/KlingAIBot";

export const fetchJobs = async (): Promise<VideoJob[]> => {
  const response = await fetch(`${API_BASE_URL}/generations`);

  if (!response.ok) {
    throw new Error("Failed to fetch jobs from the .NET API");
  }

  return response.json();
};

// --- GÜNCELLENEN TİPLER (SESSION MİMARİSİ) ---

export interface PrepareResponse {
  message: string;
  calculatedCredits: number;
  videoDurationSeconds: number;
  sessionId: string; // <-- YENİ: RAM'deki verinin (Cache) anahtarı
}

export interface ConfirmPayload {
  sessionId: string; // <-- YENİ: Artık form datalarına gerek yok, sadece bu anahtar yeterli
}

export interface KlingCreditData {
  remainingCredits: number;
  membershipCredits: number;
  topUpCredits: number;
  bonusCredits: number;
}

// --- FONKSİYONLAR ---

// 1. AŞAMA: Dosyaları gönder ve maliyeti hesapla (FormData kullanılır)
export const prepareJob = async (formData: FormData): Promise<PrepareResponse> => {
  const response = await fetch(`${API_BASE_URL}/prepare`, {
    method: "POST",
    // NOT: FormData gönderirken 'Content-Type' başlığını BİZ BELİRLEMİYORUZ. 
    // Tarayıcı otomatik olarak 'multipart/form-data; boundary=...' ekler.
    body: formData,
  });

  if (!response.ok) {
    const err = await response.text();
    throw new Error(err || "Failed to prepare job");
  }

  return response.json();
};

// 2. AŞAMA: Onayla ve Kuyruğa At (JSON kullanılır)
export const confirmJob = async (payload: ConfirmPayload): Promise<{ message: string; jobId: number }> => {
  const response = await fetch(`${API_BASE_URL}/confirm`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const err = await response.text();
    throw new Error(err || "Failed to confirm job");
  }

  return response.json();
};

export const updateJob = async ({ id, data }: { id: number; data: UpdateHistoricalVideoRequest }) => {
  const res = await fetch(`https://localhost:7159/api/KlingAIBot/historical-videos/${id}`, { 
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    const errorData = await res.json().catch(() => null);
    
    // ASP.NET Core'un fırlattığı detaylı Validation (Doğrulama) hatalarını ekrana yazdırır
    let errorMsg = errorData?.title || errorData?.message || "Kayıt güncellenirken bir hata oluştu.";
    if (errorData?.errors) {
        errorMsg += "\n\nDetaylar:\n" + JSON.stringify(errorData.errors, null, 2);
    }
    
    throw new Error(errorMsg);
  }
  return res.json();
};

// SİLME İŞLEMİ İÇİN METOT:
export const deleteJob = async (jobId: number): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/generations/${jobId}`, {
    method: 'DELETE',
  });

  if (!response.ok) {
    // Backend'den dönen hata mesajını yakalamaya çalışalım
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || "Kayıt silinirken bir hata oluştu.");
  }
};

export const fetchCredits = async (): Promise<KlingCreditData> => {
  // API_BASE_URL zaten 'http://localhost:5290/api/KlingAIBot' olarak ayarlıydı
  const response = await fetch(`${API_BASE_URL}/credits`);

  if (!response.ok) {
    throw new Error("Kredi bilgisi alınırken bir hata oluştu.");
  }

  return response.json();
};