import { VideoJob } from "@/types/job.types";

// CEYDAK sunucusundaki API adresimiz
// Not: Port numarasını (.NET API'nin çalıştığı port, örn: 5000, 5001 veya 7100 vb.) kendi yapına göre güncelle.
const API_BASE_URL = "http://localhost:5290/api";

export const fetchJobs = async (): Promise<VideoJob[]> => {
  const response = await fetch(`${API_BASE_URL}/BotTest/generations`);

  if (!response.ok) {
    throw new Error("Failed to fetch jobs from the .NET API");
  }

  return response.json();
};


// --- YENİ EKLENEN TİPLER ---

export interface PrepareResponse {
  message: string;
  calculatedCredits: number;
  videoDurationSeconds: number;
  fruitAssetId: number;       // <-- Eklendi
  referenceVideoId?: number;  // <-- Eklendi (Nullable)
}

export interface ConfirmPayload {
  fruitAssetId: number;
  referenceVideoId?: number; // Recreate ise boş olabilir
  isRecreate: boolean;
  targetUrl?: string;
  targetModel: string;
  targetResolution: string;
  isMultipleFruits: boolean;
}

// --- YENİ EKLENEN FONKSİYONLAR ---

// 1. AŞAMA: Dosyaları gönder ve maliyeti hesapla (FormData kullanılır)
export const prepareJob = async (formData: FormData): Promise<PrepareResponse> => {
  const response = await fetch(`${API_BASE_URL}/BotTest/prepare`, {
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
  const response = await fetch(`${API_BASE_URL}/BotTest/confirm`, {
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

// SİLME İŞLEMİ İÇİN YENİ METOT:
export const deleteJob = async (jobId: number): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/BotTest/generations/${jobId}`, {
    method: 'DELETE',
  });

  if (!response.ok) {
    // Backend'den dönen hata mesajını yakalamaya çalışalım
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || "Kayıt silinirken bir hata oluştu.");
  }
};