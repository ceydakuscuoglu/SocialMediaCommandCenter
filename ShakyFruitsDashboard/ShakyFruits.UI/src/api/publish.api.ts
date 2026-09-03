// src/api/publish.api.ts
const API_BASE_URL = "http://localhost:5290/api"; // Kendi portuna göre ayarla

export enum SocialPlatform {
  TikTok = 1,
  Instagram = 2
}

export interface PublishRequest {
  videoGenerationId: number;
  videoPath: string;
  caption: string;
  platform: SocialPlatform;
}

// 1. YAPAY ZEKA CAPTION ÜRETME UCU
export const generateAiCaption = async ({ id, platform }: { id: number, platform: SocialPlatform }) => {
  const res = await fetch(`${API_BASE_URL}/Generation/${id}/generate-caption?platform=${platform}`, {
    method: "POST"
  });
  
  if (!res.ok) {
    const errorData = await res.json().catch(() => ({}));
    throw new Error(errorData.Message || "Açıklama üretilirken bir hata oluştu.");
  }
  return res.json();
};

// 2. VİDEO YAYINLAMA UCU
export const publishVideo = async (data: PublishRequest) => {
  const res = await fetch(`${API_BASE_URL}/Publish/publish-video`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    const errorData = await res.json().catch(() => ({}));
    throw new Error(errorData.message || "Video yayınlanırken bir hata oluştu.");
  }
  return res.json();
};