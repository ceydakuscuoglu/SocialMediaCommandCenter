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

export interface AccountAnalyticsHistory {
  lifetimeLikes: number;
  totalFollowers: number;
  followingCount: number;
  totalVideoViews: number;
  profileViews: number;
  totalLikes: number;
  totalComments: number;
  totalShares: number;
  estimatedRewards: number;
  recordedAt: string;
}

export interface LatestAccountStats {
  lifetimeLikes: number;
  totalFollowers: number;
  followingCount: number;
  totalVideoViews: number;
  profileViews: number;
  totalLikes: number;
  totalComments: number;
  totalShares: number;
  estimatedRewards: number;
  recordedAt: string;
  source: string;
}

export interface VideoStatsSnapshot {
  views: number;
  likes: number;
  comments: number;
  shares: number;
  favorites: number;
  recordedAt: string;
}

export interface PublishedVideoLatest {
  videoId: number;
  platform: string;
  postUrl: string;
  publishedAt: string;
  latestStats: VideoStatsSnapshot | null;
}

export interface LeaderboardItem {
  name: string;
  videoCount: number;
  averageViews: number;
  averageLikes: number;
}

export interface SoloVsGroupStats {
  type: string;
  videoCount: number;
  totalViews: number;
  averageViews: number;
  averageLikes: number;
  averageShares: number;
}

export interface EngagementVideo {
  videoId: number;
  title: string;
  url: string;
  views: number;
  engagementRate: number;
  viralFactor: number;
}

export interface FruitCombination {
  combination: string;
  videoCount: number;
  totalViews: number;
  averageViews: number;
  averageLikes: number;
  averageShares: number;
  averageViralFactor: number;
}

export interface LateBloomer {
  videoId: number;
  title: string;
  url: string;
  firstDaysIncrease: number;
  recentDaysIncrease: number;
  momentumMultiplier: number;
}

export interface LifespanItem {
  videoId: number;
  title: string;
  publishedAt: string;
  lastActiveDate: string;
  activeLifespanDays: number;
}

// --- GET İSTEKLERİ ---
export const fetchLeaderboards = async (): Promise<{ fruitLeaderboard: LeaderboardItem[], danceLeaderboard: LeaderboardItem[] }> => {
  const res = await fetch(`${API_BASE_URL}/leaderboards`);
  if (!res.ok) throw new Error("Liderlik tabloları alınamadı.");
  return res.json();
};

export const fetchSoloVsGroup = async (): Promise<SoloVsGroupStats[]> => {
  const res = await fetch(`${API_BASE_URL}/solo-vs-group`);
  if (!res.ok) throw new Error("Solo vs Grup verileri alınamadı.");
  return res.json();
};

export const fetchEngagementMetrics = async (): Promise<{ accountAverages: any, topEngagingVideos: EngagementVideo[] }> => {
  const res = await fetch(`${API_BASE_URL}/engagement-metrics`);
  if (!res.ok) throw new Error("Etkileşim metrikleri alınamadı.");
  return res.json();
};

export const fetchFruitCombinations = async (): Promise<FruitCombination[]> => {
  const res = await fetch(`${API_BASE_URL}/fruit-combinations`);
  if (!res.ok) throw new Error("Kombinasyon analizi alınamadı.");
  return res.json();
};

export const fetchLifecycleInsights = async (): Promise<{ lifespanLeaderboard: LifespanItem[], lateBloomers: LateBloomer[] }> => {
  const res = await fetch(`${API_BASE_URL}/lifecycle-insights`);
  if (!res.ok) throw new Error("Yaşam döngüsü içgörüleri alınamadı.");
  return res.json();
};

// --- POST İSTEĞİ (CSV YÜKLEME) ---
export const generateGoldenHours = async (file: File) => {
  const formData = new FormData();
  formData.append("file", file);

  const res = await fetch(`${API_BASE_URL}/golden-hours-heatmap`, {
    method: "POST",
    body: formData,
  });
  if (!res.ok) throw new Error("Isı haritası oluşturulamadı.");
  return res.json();
};

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

export const fetchAccountHistory = async (): Promise<AccountAnalyticsHistory[]> => {
  const response = await fetch(`${API_BASE_URL}/account-history`);
  if (!response.ok) throw new Error("Hesap verileri alınamadı.");
  return response.json();
};

// 1. Dashboard Tablosu İçin Hızlı Yükleme (Liste)
export const fetchAllVideosLatestStats = async (): Promise<PublishedVideoLatest[]> => {
  const response = await fetch(`${API_BASE_URL}/videos/latest-stats`);
  if (!response.ok) throw new Error("Video listesi alınamadı.");
  return response.json();
};

// 2. Tablo Satırındaki "Yenile" Butonu İçin (Zorla Yenileme)
export const forceRefreshVideoStats = async (videoId: number): Promise<any> => {
  const response = await fetch(`${API_BASE_URL}/videos/${videoId}/force-refresh`, {
    method: "POST"
  });
  if (!response.ok) throw new Error("Video verileri yenilenemedi.");
  return response.json();
};

// 3. Hesap Özeti İçin Akıllı Cache
export const fetchLatestAccountStats = async (): Promise<LatestAccountStats> => {
  const response = await fetch(`${API_BASE_URL}/latest-account-stats`);
  if (!response.ok) throw new Error("Hesap verileri alınamadı.");
  return response.json();
};

