import { apiFetch } from "./client";

// --- ENUMS & TYPES ---
export enum ReferenceSourceType {
  LocalUpload = 0,
  KlingRecreate = 1
}

export interface CreateFruitTypeRequest {
  name: string;
}

export interface CreateFruitAssetRequest {
  title: string;
  imagePath: string;
  isMultipleFruits: boolean;
  fruitTypeIds: number[];
}

export interface CreateReferenceVideoRequest {
  danceStyle: string;
  sourceType: ReferenceSourceType;
  videoPath?: string;
  klingSourceUrlOrId?: string;
}

export interface FruitType {
  id: number;
  name: string;
}

export interface FruitAsset {
  id: number;
  title: string;
  imagePath: string;
  isMultipleFruits: boolean;
  fruits: FruitType[];
}

export interface ReferenceVideo {
  id: number;
  danceStyle: string;
  sourceType: string;
  videoPath: string | null;
  klingSourceUrlOrId: string | null;
}

export interface AddHistoricalVideoRequest {
  fruitAssetId: number;
  referenceVideoId?: number | null;
  outputVideoPath: string;
  aiGeneratedCaption?: string;
  isPublished: boolean;
  postUrl?: string;
  publishedAt?: string | null;
  platform?: number;
  isRecreate: boolean;
  targetUrl?: string;
}

// --- PUT (GÜNCELLEME) İSTEKLERİ ---
export const updateFruitType = async ({ id, data }: { id: number; data: CreateFruitTypeRequest }) => {
  return apiFetch(`/Assets/fruit-types/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
};

export const updateFruitAsset = async ({ id, data }: { id: number; data: CreateFruitAssetRequest }) => {
  return apiFetch(`/Assets/fruits/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
};

export const updateReferenceVideo = async ({ id, data }: { id: number; data: CreateReferenceVideoRequest }) => {
  return apiFetch(`/Assets/reference-videos/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
};

// --- API CALLS ---
export const addFruitType = async (data: CreateFruitTypeRequest) => {
  return apiFetch(`/Assets/fruit-types`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
};

export const addFruitAsset = async (data: CreateFruitAssetRequest) => {
  return apiFetch(`/Assets/fruits`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
};

export const addReferenceVideo = async (data: CreateReferenceVideoRequest) => {
  return apiFetch(`/Assets/reference-videos`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
};

export const fetchFruitTypes = async (): Promise<FruitType[]> => {
  return apiFetch<FruitType[]>("/Assets/fruit-types");
};

export const fetchFruitAssets = async (): Promise<FruitAsset[]> => {
  return apiFetch<FruitAsset[]>("/Assets/fruits");
};

export const fetchReferenceVideos = async (): Promise<ReferenceVideo[]> => {
  return apiFetch<ReferenceVideo[]>("/Assets/reference-videos");
};

export const addHistoricalVideo = async (data: AddHistoricalVideoRequest) => {
  return apiFetch("/Generation/historical-videos", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
};