import { apiFetch } from "./client";

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
export const generateAiCaption = async ({ id, platform }: { id: number; platform: SocialPlatform }) => {
  return apiFetch<{ message: string; platform: string; caption: string }>(
    `/Generation/${id}/generate-caption?platform=${platform}`,
    { method: "POST" }
  );
};

// 2. VİDEO YAYINLAMA UCU
export const publishVideo = async (data: PublishRequest) => {
  return apiFetch<{ message: string; publishedAt: string }>("/Publish/publish-video", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
};