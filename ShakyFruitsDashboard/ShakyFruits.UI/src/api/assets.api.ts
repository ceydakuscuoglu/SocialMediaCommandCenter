const API_BASE_URL = "http://localhost:5290/api/Assets";

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

// --- API CALLS ---
export const addFruitType = async (data: CreateFruitTypeRequest) => {
  const res = await fetch(`${API_BASE_URL}/fruit-types`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error("Meyve türü eklenemedi.");
  return res.json();
};

export const addFruitAsset = async (data: CreateFruitAssetRequest) => {
  const res = await fetch(`${API_BASE_URL}/fruits`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error("Meyve görseli eklenemedi.");
  return res.json();
};

export const addReferenceVideo = async (data: CreateReferenceVideoRequest) => {
  const res = await fetch(`${API_BASE_URL}/reference-videos`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error("Referans video eklenemedi.");
  return res.json();
};