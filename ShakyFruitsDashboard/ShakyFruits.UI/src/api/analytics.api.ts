import { apiFetch } from "./client";

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
  followerGrowth?: number;
  viewsGrowth?: number;
}

export type VideoStatsSnapshot = VideoAnalyticsSnapshot;

export interface PublishedVideoLatest {
  id?: number;
  videoId: number;
  videoGenerationId?: number;
  platform: string | number;
  postUrl: string;
  publishedAt: string;
  fruitTitle?: string;
  danceStyle?: string;
  latestStats?: VideoAnalyticsSnapshot | null;
  views?: number;
  likes?: number;
  comments?: number;
  shares?: number;
  favorites?: number;
  lastScrapedAt?: string;
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

// Platform Enum
export enum SocialPlatform {
  TikTok = 1,
  Instagram = 2
}

// --- TIKTOK LIVE TRENDS ---
export interface TrendingHashtag {
  name: string;
  rank: string;
  stats: string;
}

export interface LiveTrendsResponse {
  source: string;
  message: string;
  data: {
    trendingHashtags: TrendingHashtag[];
  };
}

// --- GET & POST İSTEKLERİ ---

export const fetchInternalStats = async (): Promise<InternalStats> => {
  return apiFetch<InternalStats>("/Analytics/internal-stats");
};

export const testTikTokScraper = async (url: string): Promise<ScraperTestResult> => {
  return apiFetch<ScraperTestResult>(`/Analytics/test-tiktok-scraper?url=${encodeURIComponent(url)}`);
};

export const fetchPublishedVideos = async (): Promise<PublishedVideo[]> => {
  return apiFetch<PublishedVideo[]>("/Analytics/published-videos");
};

export const addPublishedVideo = async (payload: { videoGenerationId: number; postUrl: string }): Promise<void> => {
  return apiFetch<void>("/Analytics/track-video", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
};

export const fetchAllVideosLatestStats = async (): Promise<PublishedVideoLatest[]> => {
  return apiFetch<PublishedVideoLatest[]>("/Analytics/videos/latest-stats");
};

export const forceRefreshVideoStats = async (videoId: number): Promise<any> => {
  return apiFetch(`/Analytics/videos/${videoId}/force-refresh`, {
    method: "POST",
  });
};

export const forceRefreshAllVideosStats = async (): Promise<any> => {
  return apiFetch("/Analytics/videos/force-refresh-all", {
    method: "POST",
  });
};

export const fetchAccountHistory = async (platform?: SocialPlatform | any) => {
  let endpoint = "/Analytics/account-history";
  if (typeof platform === "number") {
    const platformString = SocialPlatform[platform];
    if (platformString) {
      endpoint += `?platform=${platformString}`;
    }
  }
  return apiFetch(endpoint);
};

export const fetchTikTokStats = async () => {
  return apiFetch("/Analytics/tiktok/latest-account-stats");
};

export const fetchInstagramStats = async () => {
  return apiFetch("/Analytics/instagram/latest-account-stats");
};

export const fetchLatestAccountStats = async (): Promise<LatestAccountStats> => {
  return apiFetch<LatestAccountStats>("/Analytics/latest-account-stats");
};

export const fetchLeaderboards = async (): Promise<{ fruitLeaderboard: LeaderboardItem[]; danceLeaderboard: LeaderboardItem[] }> => {
  return apiFetch("/Analytics/leaderboards");
};

export const fetchSoloVsGroup = async (): Promise<SoloVsGroupStats[]> => {
  return apiFetch<SoloVsGroupStats[]>("/Analytics/solo-vs-group");
};

export const fetchEngagementMetrics = async (): Promise<{ accountAverages: any; topEngagingVideos: EngagementVideo[] }> => {
  return apiFetch("/Analytics/engagement-metrics");
};

export const fetchFruitCombinations = async (): Promise<FruitCombination[]> => {
  return apiFetch<FruitCombination[]>("/Analytics/fruit-combinations");
};

export const fetchLifecycleInsights = async (): Promise<{ lifespanLeaderboard: LifespanItem[]; lateBloomers: LateBloomer[] }> => {
  return apiFetch("/Analytics/lifecycle-insights");
};

export const generateGoldenHours = async (file: File) => {
  const formData = new FormData();
  formData.append("file", file);

  return apiFetch("/Analytics/golden-hours-heatmap", {
    method: "POST",
    body: formData,
  });
};

export const fetchLiveTrends = async (): Promise<LiveTrendsResponse> => {
  return apiFetch<LiveTrendsResponse>("/Analytics/tiktok/live-trends");
};