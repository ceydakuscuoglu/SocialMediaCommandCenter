import { useQuery } from "@tanstack/react-query";
import { fetchAccountHistory, SocialPlatform } from "@/api/analytics.api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ArrowUpRight, ArrowDownRight, Activity, Eye, MessageCircle, Share2, CircleDollarSign, Minus, Heart } from "lucide-react";

const formatK = (num: number) => {
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toString();
};

export function InstagramAccountOverview() {
const { data: history, isLoading } = useQuery({
    queryKey: ["account-history", "instagram"],
    queryFn: () => fetchAccountHistory(SocialPlatform.Instagram),
  });

  if (isLoading) return null;
  if (!history || history.length === 0) return null;

  const sortedHistory = [...history].sort((a, b) => 
    new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime()
  );
  const latest = sortedHistory[sortedHistory.length - 1];
  const previous = sortedHistory.length > 1 ? sortedHistory[sortedHistory.length - 2] : null;

  const renderTrend = (current: number, prev?: number, isCurrency = false) => {
    if (prev === undefined || prev === null) return null;
    const diff = current - prev;
    const percent = prev > 0 ? (diff / prev) * 100 : 0;
    
    if (diff === 0) return (
      <div className="flex items-center gap-1 text-[11px] font-medium text-muted-foreground mt-2 bg-muted/50 px-2 py-0.5 rounded w-fit">
        <Minus className="w-3 h-3" />
      </div>
    );

    const isPositive = diff > 0;
    const colorClass = isPositive ? "text-emerald-600 bg-emerald-500/15" : "text-rose-600 bg-rose-500/15";
    const Icon = isPositive ? ArrowUpRight : ArrowDownRight;
    const formattedDiff = isCurrency ? `$${Math.abs(diff).toFixed(2)}` : formatK(Math.abs(diff));

    return (
      <div className={`flex items-center gap-1 text-[11px] font-bold mt-2 px-2 py-0.5 rounded w-fit ${colorClass}`}>
        <Icon className="w-3 h-3 stroke-[3]" />
        <span>{isPositive ? "+" : "-"}{formattedDiff} ({percent.toFixed(1)}%)</span>
      </div>
    );
  };

  return (
    <Card className="w-full h-full flex flex-col border-border/40 shadow-sm bg-card">
      <CardHeader className="pb-4">
        <CardTitle className="text-sm font-medium text-muted-foreground flex justify-between">
          <span>Instagram 7-Day Performance</span>
        </CardTitle>
      </CardHeader>
      
      <CardContent className="flex-1 flex flex-col">
        <div className="grid grid-cols-2 lg:grid-cols-3 gap-4 h-full">
          
          {/* Instagram'da İzlenme yerine Erişim (Reach) kullanılır */}
          <div className="flex flex-col justify-center p-4 rounded-xl border border-border/50 bg-muted/20">
            <div className="flex justify-between items-start mb-1">
              <span className="text-xs font-medium text-muted-foreground">Reach</span>
              <Activity className="w-4 h-4 text-orange-500" />
            </div>
            <span className="text-xl font-bold">{formatK(latest.totalVideoViews)}</span>
            {renderTrend(latest.totalVideoViews, previous?.totalVideoViews)}
          </div>

          <div className="flex flex-col justify-center p-4 rounded-xl border border-border/50 bg-muted/20">
            <div className="flex justify-between items-start mb-1">
              <span className="text-xs font-medium text-muted-foreground">Profile Visitors</span>
              <Eye className="w-4 h-4 text-purple-500" />
            </div>
            <span className="text-xl font-bold">{formatK(latest.profileViews)}</span>
            {renderTrend(latest.profileViews, previous?.profileViews)}
          </div>

          {/* Instagram'da Beğeniler yerine Etkileşim (Interactions) kapsamlıdır */}
          <div className="flex flex-col justify-center p-4 rounded-xl border border-border/50 bg-muted/20">
            <div className="flex justify-between items-start mb-1">
              <span className="text-xs font-medium text-muted-foreground">Interactions</span>
              <Heart className="w-4 h-4 text-pink-500" />
            </div>
            <span className="text-xl font-bold">{formatK(latest.totalLikes)}</span>
            {renderTrend(latest.totalLikes, previous?.totalLikes)}
          </div>

          <div className="flex flex-col justify-center p-4 rounded-xl border border-border/50 bg-muted/20">
            <div className="flex justify-between items-start mb-1">
              <span className="text-xs font-medium text-muted-foreground">Comments</span>
              <MessageCircle className="w-4 h-4 text-emerald-500" />
            </div>
            <span className="text-xl font-bold">{formatK(latest.totalComments)}</span>
            {renderTrend(latest.totalComments, previous?.totalComments)}
          </div>

          <div className="flex flex-col justify-center p-4 rounded-xl border border-border/50 bg-muted/20">
            <div className="flex justify-between items-start mb-1">
              <span className="text-xs font-medium text-muted-foreground">Shares</span>
              <Share2 className="w-4 h-4 text-amber-500" />
            </div>
            <span className="text-xl font-bold">{formatK(latest.totalShares)}</span>
            {renderTrend(latest.totalShares, previous?.totalShares)}
          </div>

          <div className="flex flex-col justify-center p-4 rounded-xl border border-border/50 bg-muted/20">
            <div className="flex justify-between items-start mb-1">
              <span className="text-xs font-medium text-muted-foreground">Earn</span>
              <CircleDollarSign className="w-4 h-4 text-green-500" />
            </div>
            <span className="text-xl font-bold">${latest.estimatedRewards.toFixed(2)}</span>
            {renderTrend(latest.estimatedRewards, previous?.estimatedRewards, true)}
          </div>

        </div>
      </CardContent>
    </Card>
  );
}