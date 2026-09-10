import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  fetchInternalStats,
  fetchAllVideosLatestStats,
  fetchPublishedVideos,
  forceRefreshAllVideosStats
} from "@/api/analytics.api";
import { VideoPerformanceChart } from "@/components/analytics/VideoPerformanceChart";
import { TrackVideoModal } from "@/components/analytics/TrackVideoModal";
import { PublishedVideosTable } from "@/components/analytics/PublishedVideosTable";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import {
  Activity,
  Apple,
  Music,
  Video,
  Loader2,
  BarChart3,
  RefreshCw,
  Zap,
  CheckCircle2
} from "lucide-react";

export function Analytics() {
  const queryClient = useQueryClient();
  const [liveScrapeMessage, setLiveScrapeMessage] = useState<string | null>(null);

  const { data: stats, isLoading, isError } = useQuery({
    queryKey: ["internal-stats"],
    queryFn: fetchInternalStats,
  });

  const {
    data: latestVideos,
    refetch: refetchLatestVideos,
    isFetching: isFetchingLatestVideos
  } = useQuery({
    queryKey: ["published-videos-latest"],
    queryFn: fetchAllVideosLatestStats
  });

  const liveScrapeAllMutation = useMutation({
    mutationFn: forceRefreshAllVideosStats,
    onSuccess: (res) => {
      queryClient.invalidateQueries({ queryKey: ["published-videos-latest"] });
      queryClient.invalidateQueries({ queryKey: ["published-videos"] });
      setLiveScrapeMessage(res?.message || "All videos successfully updated.");
      setTimeout(() => setLiveScrapeMessage(null), 6000);
    },
    onError: (err: any) => {
      alert(err.message || "An error occurred while live scraping all videos.");
    }
  });

  const { data: publishedVideos } = useQuery({
    queryKey: ["published-videos"],
    queryFn: fetchPublishedVideos,
  });

  const topPerformingVideo = publishedVideos?.reduce((prev, current) => {
    const prevViews = prev.analyticsHistory?.[prev.analyticsHistory.length - 1]?.views || 0;
    const currViews = current.analyticsHistory?.[current.analyticsHistory.length - 1]?.views || 0;
    return currViews > prevViews ? current : prev;
  }, publishedVideos?.[0]);

  const topFruit = stats?.fruitStats?.sort((a, b) => b.usageCount - a.usageCount)[0];
  const topDance = stats?.danceStats?.sort((a, b) => b.usageCount - a.usageCount)[0];

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      <div className="flex flex-col gap-1">
        <h2 className="text-2xl font-semibold tracking-tight flex items-center gap-2">
          <BarChart3 className="w-6 h-6 text-primary" />
          Performance
        </h2>
        <p className="text-muted-foreground">
          Monitor your TikTok performance, audience engagement, and ShakyFruits generation trends.
        </p>
      </div>

      {isLoading && (
        <div className="flex items-center justify-center h-32 text-muted-foreground">
          <Loader2 className="w-6 h-6 animate-spin mr-2" />
          <span>Gathering analytics data...</span>
        </div>
      )}

      {isError && (
        <div className="p-4 bg-destructive/10 text-destructive border border-destructive/20 rounded-md">
          An error occurred while loading data. Please ensure the backend worker is running.
        </div>
      )}

      {stats && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <Card className="bg-card/50 backdrop-blur-sm border-border/50 shadow-sm">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Total Generations
              </CardTitle>
              <Video className="w-4 h-4 text-primary" />
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-foreground">
                {stats.overview?.totalGenerations || 0}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Videos rendered via Kling AI
              </p>
            </CardContent>
          </Card>

          <Card className="bg-card/50 backdrop-blur-sm border-border/50 shadow-sm">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Top Performing Fruit
              </CardTitle>
              <Apple className="w-4 h-4 text-emerald-500" />
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-foreground">
                {topFruit?.fruitName || "N/A"}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Used {topFruit?.usageCount || 0} times
              </p>
            </CardContent>
          </Card>

          <Card className="bg-card/50 backdrop-blur-sm border-border/50 shadow-sm">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Trending Dance Style
              </CardTitle>
              <Music className="w-4 h-4 text-amber-500" />
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-foreground capitalize">
                {topDance?.danceStyle || "N/A"}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Used {topDance?.usageCount || 0} times
              </p>
            </CardContent>
          </Card>

          <Card className="bg-card/50 backdrop-blur-sm border-border/50 shadow-sm">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                System Health
              </CardTitle>
              <Activity className="w-4 h-4 text-blue-500" />
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-foreground">
                {stats.systemHealth?.find(h => h.status === "Active")?.count || 0}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Active Playwright Scrapers
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      <div className="grid grid-cols-1 gap-6 mt-8">
        <div className="lg:col-span-3 min-h-[384px]">
          <VideoPerformanceChart
            historyData={topPerformingVideo?.analyticsHistory || []}
            title={
              topPerformingVideo
                ? `TikTok Growth Trend (Top Video: ${topPerformingVideo.title || '#' + topPerformingVideo.videoGenerationId})`
                : "TikTok Growth Trend"
            }
          />
        </div>
      </div>

      <div className="mt-8 space-y-4">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h3 className="text-lg font-semibold text-foreground">Tracked Portfolio</h3>
            <p className="text-sm text-muted-foreground">
              All published TikTok videos currently monitored by the ShakyFruits background engine.
            </p>
            {liveScrapeMessage && (
              <p className="text-xs text-emerald-500 font-medium flex items-center gap-1.5 mt-1.5 animate-in fade-in">
                <CheckCircle2 className="w-3.5 h-3.5" />
                {liveScrapeMessage}
              </p>
            )}
          </div>

          <div className="flex items-center gap-3 flex-wrap">
            <Button
              variant="outline"
              onClick={() => refetchLatestVideos()}
              disabled={isFetchingLatestVideos || liveScrapeAllMutation.isPending}
              // Tema renginin (Primary) yumuşak ve şık bir varyasyonunu uyguladık:
              className="h-10 px-5 rounded-full gap-2 border-primary/30 bg-primary/5 hover:bg-primary/15 text-primary font-medium shadow-sm transition-all"
              title="Instantly fetches the latest cached data from the database"
            >
              <RefreshCw className={`w-4 h-4 ${isFetchingLatestVideos ? "animate-spin" : ""}`} />
              <span>Refresh</span>
            </Button>

            <Button
              variant="outline"
              onClick={() => {
                if (window.confirm("Are you sure you want to trigger a live scrape for all videos? This process may take a while depending on the queue size.")) {
                  liveScrapeAllMutation.mutate();
                }
              }}
              disabled={liveScrapeAllMutation.isPending || isFetchingLatestVideos}
              // Amber rengini daha premium bir tona çektik ve kenarları yuvarladık:
              className="h-10 px-5 rounded-full gap-2 border-amber-500/30 bg-amber-500/5 hover:bg-amber-500/15 text-amber-600 dark:text-amber-500 font-medium shadow-sm transition-all"
              title="Sequentially connects to TikTok Studio to scrape live data for all videos"
            >
              {liveScrapeAllMutation.isPending ? (
                <>
                  <RefreshCw className="w-4 h-4 animate-spin" />
                  <span>Scraping...</span>
                </>
              ) : (
                <>
                  <Zap className="w-4 h-4" />
                  <span>Live Scrape All</span>
                </>
              )}
            </Button>

            {/* Bu modal içindeki buton zaten mor ve dolgun tasarıma sahip */}
            <TrackVideoModal />
          </div>
        </div>

        <PublishedVideosTable videos={latestVideos} />
      </div>

    </div>
  );
}