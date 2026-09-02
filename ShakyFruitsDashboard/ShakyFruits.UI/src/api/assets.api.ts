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

export interface FruitType {
    id: number;
    name: string;
}

export interface FruitAsset {
    id: number;
    title: string;
    imagePath: string;
    isMultipleFruits: boolean;
    fruits: FruitType[]; // C# tarafındaki FruitsInImage listesinin karşılığı
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
}

// --- PUT (GÜNCELLEME) İSTEKLERİ ---
export const updateFruitType = async ({ id, data }: { id: number, data: CreateFruitTypeRequest }) => {
    const res = await fetch(`${API_BASE_URL}/fruit-types/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
    });
    if (!res.ok) throw new Error("Meyve türü güncellenemedi.");
    return res.json();
};

export const updateFruitAsset = async ({ id, data }: { id: number, data: CreateFruitAssetRequest }) => {
    const res = await fetch(`${API_BASE_URL}/fruits/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
    });
    if (!res.ok) throw new Error("Meyve görseli güncellenemedi.");
    return res.json();
};

export const updateReferenceVideo = async ({ id, data }: { id: number, data: CreateReferenceVideoRequest }) => {
    const res = await fetch(`${API_BASE_URL}/reference-videos/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
    });
    if (!res.ok) throw new Error("Referans video güncellenemedi.");
    return res.json();
};

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

export const fetchFruitTypes = async (): Promise<FruitType[]> => {
    const res = await fetch(`${API_BASE_URL}/fruit-types`);
    if (!res.ok) throw new Error("Meyve türleri alınamadı.");
    return res.json();
};

export const fetchFruitAssets = async (): Promise<FruitAsset[]> => {
    const res = await fetch(`${API_BASE_URL}/fruits`);
    if (!res.ok) throw new Error("Meyve görselleri alınamadı.");
    return res.json();
};

export const fetchReferenceVideos = async (): Promise<ReferenceVideo[]> => {
    const res = await fetch(`${API_BASE_URL}/reference-videos`);
    if (!res.ok) throw new Error("Referans videolar alınamadı.");
    return res.json();
};

export const addHistoricalVideo = async (data: AddHistoricalVideoRequest) => {
    const res = await fetch(`${API_BASE_URL}/historical-videos`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
    });

    if (!res.ok) {
        const errData = await res.json().catch(() => null);
        throw new Error(errData?.message || "Geçmiş video eklenirken bir hata oluştu.");
    }
    return res.json();
};