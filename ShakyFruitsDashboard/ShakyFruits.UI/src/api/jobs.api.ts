import { apiFetch } from "./client";

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
  status: GenerationStatus;
  errorMessage: string | null;
  outputVideoPath: string | null;
  createdAt: string;
}

export interface UpdateHistoricalVideoRequest {
  fruitAssetId: number;
  referenceVideoId?: number;
  isRecreate: boolean;
  targetUrl?: string;
  outputVideoPath?: string;
  aiGeneratedCaption?: string;
  isPublished: boolean;
  platform: number;
  postUrl?: string;
  publishedAt?: string;
}

export interface PrepareResponse {
  message: string;
  calculatedCredits: number;
  videoDurationSeconds: number;
  sessionId: string;
}

export interface ConfirmPayload {
  sessionId: string;
}

export interface KlingCreditData {
  remainingCredits: number;
  membershipCredits: number;
  topUpCredits: number;
  bonusCredits: number;
}

export const fetchJobs = async (): Promise<VideoJob[]> => {
  return apiFetch<VideoJob[]>("/Generation");
};

export const prepareJob = async (formData: FormData): Promise<PrepareResponse> => {
  return apiFetch<PrepareResponse>("/KlingAIBot/prepare", {
    method: "POST",
    body: formData,
  });
};

export const confirmJob = async (payload: ConfirmPayload): Promise<{ message: string; jobId: number }> => {
  return apiFetch<{ message: string; jobId: number }>("/KlingAIBot/confirm", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
};

export const updateJob = async ({ id, data }: { id: number; data: UpdateHistoricalVideoRequest }) => {
  return apiFetch(`/Generation/historical-videos/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
};

export const deleteJob = async (jobId: number): Promise<void> => {
  return apiFetch(`/Generation/${jobId}`, {
    method: "DELETE",
  });
};

export const fetchCredits = async (): Promise<KlingCreditData> => {
  return apiFetch<KlingCreditData>("/KlingAIBot/credits");
};