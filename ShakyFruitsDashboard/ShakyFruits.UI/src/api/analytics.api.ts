// --- TYPES ---

export interface InternalStats {
  overview: { totalGenerations: number };
  fruitStats: { fruitName: string; usageCount: number }[];
  danceStats: { danceStyle: string; usageCount: number }[];
  systemHealth: { status: string; count: number }[];
}

export interface VideoAnalyticsSnapshot {
  views: number;
  likes: number;
  comments: number;
  shares: number;
  favorites: number;
  recordedAt: string;
}

export interface PublishedVideo {
  id: number;
  videoGenerationId: number;
  platform: number; // 1: TikTok
  postUrl: string;
  publishedAt: string;
  latestStats: VideoAnalyticsSnapshot;
  analyticsHistory: VideoAnalyticsSnapshot[];
}

// --- API SERVICES ---

// Backend portunu (örn: 5290) kendi sistemine göre güncelle
const API_BASE_URL = "http://localhost:5290/api/Analytics";

export const fetchInternalStats = async (): Promise<InternalStats> => {
  const response = await fetch(`${API_BASE_URL}/internal-stats`);
  if (!response.ok) throw new Error("İçgörü (Internal Stats) verileri alınamadı.");
  return response.json();
};

export const fetchPublishedVideos = async (): Promise<PublishedVideo[]> => {
  const response = await fetch(`${API_BASE_URL}/published-videos`);
  if (!response.ok) throw new Error("Yayınlanan videolar alınamadı.");
  return response.json();
};

export const addPublishedVideo = async (payload: { videoGenerationId: number; postUrl: string }): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/published-videos`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  if (!response.ok) throw new Error("Video sisteme eklenirken bir hata oluştu.");
};