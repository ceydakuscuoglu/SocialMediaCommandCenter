import { useQuery } from "@tanstack/react-query";
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
import { fetchInternalStats } from "@/api/analytics.api";

export function Analytics() {
  const { data: stats, isLoading, isError } = useQuery({
    queryKey: ["internal-stats"],
    queryFn: fetchInternalStats,
    refetchInterval: 60000, // 1 dakikada bir otomatik yenile (opsiyonel)
  });

  // En popüler meyve ve dansı hesaplamak için güvenli kontroller
  const topFruit = stats?.fruitStats?.sort((a, b) => b.usageCount - a.usageCount)[0];
  const topDance = stats?.danceStats?.sort((a, b) => b.usageCount - a.usageCount)[0];

  return (
    <div className="p-8 min-h-screen bg-background text-foreground space-y-8 animate-in fade-in duration-500">
      
      {/* Sayfa Başlığı */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Analytics & Insights</h1>
        <p className="text-muted-foreground mt-2">
          Monitor your TikTok performance, audience engagement, and ShakyFruits generation trends.
        </p>
      </div>

      {/* Yükleniyor / Hata Durumları */}
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

      {/* Gelecek Adımlar için Yer Tutucular */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mt-8">
        <div className="lg:col-span-2 border border-dashed border-border/50 rounded-lg h-96 flex items-center justify-center text-muted-foreground">
          {/* Adım C: Recharts Grafiği Buraya Gelecek */}
          [Recharts Performance Graph Placeholder]
        </div>
        <div className="border border-dashed border-border/50 rounded-lg h-96 flex items-center justify-center text-muted-foreground">
          {/* Adım B: Yeni Video Ekleme Formu Buraya/Modala Gelecek */}
          [Add Published Video Action Area Placeholder]
        </div>
      </div>

      <div className="w-full border border-dashed border-border/50 rounded-lg h-64 flex items-center justify-center text-muted-foreground mt-8">
        {/* Adım D: Published Videos Data Table Buraya Gelecek */}
        [Data Table Placeholder]
      </div>

    </div>
  );
}