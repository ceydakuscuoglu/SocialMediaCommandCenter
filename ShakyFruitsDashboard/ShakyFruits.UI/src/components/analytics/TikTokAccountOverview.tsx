import { useQuery } from "@tanstack/react-query";
import { fetchAccountHistory } from "@/api/analytics.api";
import { Card, CardContent } from "@/components/ui/card";
import { Loader2, ArrowUpRight, ArrowDownRight, Sparkles } from "lucide-react";

// TikTok stili sayı kısaltma (Örn: 124200 -> 124.2K)
const formatK = (num: number) => {
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toString();
};

export function TikTokAccountOverview() {
  const { data: history, isLoading } = useQuery({
    queryKey: ["account-history"],
    queryFn: fetchAccountHistory,
    refetchInterval: 60000,
  });

  if (isLoading) {
    return (
      <Card className="w-full h-32 flex items-center justify-center border-border/50 bg-card/30">
        <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
      </Card>
    );
  }

  if (!history || history.length === 0) return null;

  // En güncel veri (Listenin son elemanı)
  const latest = history[history.length - 1];
  // Bir önceki veri (Trend hesaplamak için sondan bir önceki eleman)
  const previous = history.length > 1 ? history[history.length - 2] : null;

  // Trend hesaplama yardımcı fonksiyonu
  const renderTrend = (current: number, prev?: number, isCurrency = false) => {
    if (prev === undefined || prev === null) return <span className="text-muted-foreground text-xs">--</span>;
    const diff = current - prev;
    const percent = prev > 0 ? (diff / prev) * 100 : 0;
    
    if (diff === 0) return <span className="text-muted-foreground text-xs">0.0 (0.0%)</span>;

    const isPositive = diff > 0;
    const colorClass = isPositive ? "text-emerald-500" : "text-rose-500";
    const Icon = isPositive ? ArrowUpRight : ArrowDownRight;
    const formattedDiff = isCurrency ? `$${Math.abs(diff).toFixed(2)}` : formatK(Math.abs(diff));

    return (
      <div className="flex items-center justify-center gap-1 text-xs font-medium mt-1">
        <Icon className={`w-3.5 h-3.5 ${colorClass}`} />
        <span className={colorClass}>{isPositive ? "+" : "-"}{formattedDiff}</span>
        <span className="text-muted-foreground">({percent.toFixed(1)}%)</span>
      </div>
    );
  };

  return (
    <Card className="w-full border-border/50 bg-card shadow-sm overflow-hidden animate-in fade-in duration-500">
      
      {/* ÜST KISIM: Profil Bilgileri */}
      <div className="flex items-center gap-4 p-5 border-b border-border/50 bg-muted/10">
        {/* Avatar Placeholder (Gerçek resim URL'si varsa buraya eklenebilir) */}
        <div className="w-14 h-14 rounded-full bg-gradient-to-tr from-orange-400 to-purple-500 flex items-center justify-center text-white font-bold text-xl shadow-inner">
          FD
        </div>
        
        <div className="flex flex-col">
          <div className="flex items-center gap-2">
            <h2 className="text-lg font-bold text-foreground">fruitssdancing</h2>
            <span className="text-sm font-medium text-muted-foreground flex items-center gap-1">
              Make Fruits Dancee <Sparkles className="w-3.5 h-3.5 text-amber-400" />
            </span>
          </div>
          <div className="flex items-center gap-3 text-sm text-foreground mt-0.5">
            <span>Beğeniler <strong className="font-bold">{formatK(latest.lifetimeLikes)}</strong></span>
            <span className="text-muted-foreground/50">•</span>
            <span>Takipçiler <strong className="font-bold">{formatK(latest.totalFollowers)}</strong></span>
            <span className="text-muted-foreground/50">•</span>
            <span>Takip ediliyor <strong className="font-bold">{latest.followingCount}</strong></span>
          </div>
        </div>
      </div>

      {/* ALT KISIM: Önemli Ölçütler (Key Metrics) */}
      <CardContent className="p-0">
        <div className="p-3 bg-muted/5 border-b border-border/30 flex justify-between items-center">
          <h3 className="text-sm font-bold pl-2">Önemli ölçütler <span className="text-muted-foreground font-normal ml-1">›</span></h3>
          <span className="text-xs text-muted-foreground border border-border/50 px-2 py-1 rounded-md bg-background">Son 7 gün</span>
        </div>
        
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 divide-y lg:divide-y-0 lg:divide-x divide-border/50">
          
          <div className="flex flex-col items-center justify-center p-4 py-6 hover:bg-muted/10 transition-colors">
            <span className="text-xs font-semibold text-foreground mb-2">Video Görüntülemeleri</span>
            <span className="text-2xl font-bold text-sky-500">{formatK(latest.totalVideoViews)}</span>
            {renderTrend(latest.totalVideoViews, previous?.totalVideoViews)}
          </div>

          <div className="flex flex-col items-center justify-center p-4 py-6 hover:bg-muted/10 transition-colors">
            <span className="text-xs font-semibold text-foreground mb-2">Profil Görüntülemeleri</span>
            <span className="text-2xl font-bold">{formatK(latest.profileViews)}</span>
            {renderTrend(latest.profileViews, previous?.profileViews)}
          </div>

          <div className="flex flex-col items-center justify-center p-4 py-6 hover:bg-muted/10 transition-colors">
            <span className="text-xs font-semibold text-foreground mb-2">Beğeniler</span>
            <span className="text-2xl font-bold">{formatK(latest.totalLikes)}</span>
            {renderTrend(latest.totalLikes, previous?.totalLikes)}
          </div>

          <div className="flex flex-col items-center justify-center p-4 py-6 hover:bg-muted/10 transition-colors">
            <span className="text-xs font-semibold text-foreground mb-2">Yorumlar</span>
            <span className="text-2xl font-bold">{formatK(latest.totalComments)}</span>
            {renderTrend(latest.totalComments, previous?.totalComments)}
          </div>

          <div className="flex flex-col items-center justify-center p-4 py-6 hover:bg-muted/10 transition-colors">
            <span className="text-xs font-semibold text-foreground mb-2">Paylaşımlar</span>
            <span className="text-2xl font-bold">{formatK(latest.totalShares)}</span>
            {renderTrend(latest.totalShares, previous?.totalShares)}
          </div>

          <div className="flex flex-col items-center justify-center p-4 py-6 hover:bg-muted/10 transition-colors">
            <span className="text-xs font-semibold text-foreground mb-2">Tahmini ödüller</span>
            <span className="text-2xl font-bold">${latest.estimatedRewards.toFixed(2)}</span>
            {renderTrend(latest.estimatedRewards, previous?.estimatedRewards, true)}
          </div>

        </div>
      </CardContent>
    </Card>
  );
}