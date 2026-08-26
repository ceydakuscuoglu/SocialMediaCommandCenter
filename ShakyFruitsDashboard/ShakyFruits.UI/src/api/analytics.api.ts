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

export interface ScraperTestResult {
  message: string;
  scrapedUrl: string;
  stats: {
    views: number;
    likes: number;
    comments: number;
    shares: number;
    favorites: number;
    recordedAt: string;
  };
}

// --- API SERVICES ---

// Backend portunu (örn: 5290) kendi sistemine göre güncelle
const API_BASE_URL = "http://localhost:5290/api/Analytics";

export const fetchInternalStats = async (): Promise<InternalStats> => {
  const response = await fetch(`${API_BASE_URL}/internal-stats`);
  if (!response.ok) throw new Error("İçgörü (Internal Stats) verileri alınamadı.");
  return response.json();
};

// GET isteği olduğu için URL'e query parameter olarak ekliyoruz
export const testTikTokScraper = async (url: string): Promise<ScraperTestResult> => {
  const response = await fetch(`${API_BASE_URL}/test-tiktok-scraper?url=${encodeURIComponent(url)}`);

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || "Kazıma işlemi başarısız oldu.");
  }

  return response.json();
};

export const fetchPublishedVideos = async (): Promise<PublishedVideo[]> => {
  const response = await fetch(`${API_BASE_URL}/published-videos`);
  if (!response.ok) throw new Error("Yayınlanan videolar alınamadı.");
  return response.json();
};

export const addPublishedVideo = async (payload: { videoGenerationId: number; postUrl: string }): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/publish-video`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const errData = await response.json().catch(() => null);
    throw new Error(errData?.message || "Video sisteme eklenirken bir hata oluştu.");
  }
};

