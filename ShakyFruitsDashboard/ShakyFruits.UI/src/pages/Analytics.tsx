import { useQuery } from "@tanstack/react-query";
import { VideoPerformanceChart } from "@/components/analytics/VideoPerformanceChart";
import { fetchInternalStats, fetchPublishedVideos } from "@/api/analytics.api";
import { ScraperTestModal } from "@/components/analytics/ScraperTestModal";
import { PublishVideoModal } from "@/components/analytics/PublishVideoModal";
import { PublishedVideosTable } from "@/components/analytics/PublishedVideosTable"; // YENİ IMPORT
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
  Loader2
} from "lucide-react";

export function Analytics() {
  const { data: stats, isLoading, isError } = useQuery({
    queryKey: ["internal-stats"],
    queryFn: fetchInternalStats,
  });

  const { data: publishedVideos } = useQuery({
    queryKey: ["published-videos"],
    queryFn: fetchPublishedVideos,
  });

  // En popüler meyve ve dansı hesaplamak için güvenli kontroller
  const topFruit = stats?.fruitStats?.sort((a, b) => b.usageCount - a.usageCount)[0];
  const topDance = stats?.danceStats?.sort((a, b) => b.usageCount - a.usageCount)[0];

  return (
    <div className="p-8 min-h-screen bg-background text-foreground space-y-8 animate-in fade-in duration-500">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Analytics & Insights</h1>
        <p className="text-muted-foreground mt-2">
          Monitor your TikTok performance, audience engagement, and ShakyFruits generation trends.
        </p>
      </div>

      {/* TIKTOK ACCOUNT OVERVIEW BURADAN SİLİNDİ */}

      {/* Yükleniyor / Hata Durumları ve Stat Kartları aynen kalıyor */}
      {/* ... */}
      {isLoading && (
        <div className="flex items-center justify-center h-32 text-muted-foreground">
          <Loader2 className="w-6 h-6 animate-spin mr-2" />
          <span>Analitik verileri toplanıyor...</span>
        </div>
      )}
      {isError && (
        <div className="p-4 bg-destructive/10 text-destructive border border-destructive/20 rounded-md">
          Veriler yüklenirken bir sorun oluştu. C# worker'ın çalıştığından emin olun.
        </div>
      )}

      {/* Özet Kartları (Stat Cards) */}
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

      {/* Alt Kısım: Grafik ve İşlem Alanı */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mt-8">

        {/* Sol Taraf (2 Kolon Genişliğinde): Performans Grafiği */}
        <div className="lg:col-span-2 min-h-[384px]">
          <VideoPerformanceChart
            historyData={publishedVideos?.[0]?.analyticsHistory || []}
            title="TikTok Growth Trend (Top Video)"
          />
        </div>

        {/* Sağ Taraf (1 Kolon Genişliğinde): Scraper ve Video Ekleme Modalları */}
        <div className="border border-dashed border-border/50 bg-card/30 rounded-lg h-96 flex flex-col items-center justify-center p-6 text-center space-y-6 shadow-sm">
          <div className="space-y-2">
            <h3 className="font-semibold text-foreground text-lg">Playwright Engine</h3>
            <p className="text-sm text-muted-foreground">
              Add a new TikTok URL to your tracking list or run a live test of the scraper.
            </p>
          </div>

          <div className="flex flex-col w-full max-w-xs gap-3">
            {/* Hem Video Ekleme hem de Test Modalı alt alta */}
            <PublishVideoModal />
            <ScraperTestModal />
          </div>

        </div>
      </div>

      {/* Adım D: Yayınlanan Videolar Tablosu */}
      <div className="mt-8 space-y-4">
        <div>
          <h3 className="text-lg font-semibold text-foreground">Tracked Portfolio</h3>
          <p className="text-sm text-muted-foreground">
            All published TikTok videos currently monitored by the ShakyFruits background engine.
          </p>
        </div>

        {/* Yer tutucu silindi, gerçek tablo bileşeni eklendi */}
        <PublishedVideosTable videos={publishedVideos} />
      </div>

    </div>
  );
}