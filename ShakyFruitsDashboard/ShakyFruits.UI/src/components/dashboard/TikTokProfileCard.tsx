import { useQuery } from "@tanstack/react-query";
import { fetchTikTokStats } from "@/api/analytics.api";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Loader2, Sparkles, Heart, Users, UserPlus } from "lucide-react";

const formatK = (num: number) => {
  if (!num) return "0";
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toString();
};

export function TikTokProfileCard() {
  const { data: response, isLoading } = useQuery({
    queryKey: ["tiktok-latest-stats"],
    queryFn: fetchTikTokStats,
  });

  if (isLoading) {
    return (
      <Card className="w-full h-full min-h-[200px] animate-pulse bg-card/50 flex items-center justify-center">
        <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
      </Card>
    );
  }

  // Backend { source: "...", data: { ... } } veya direkt obje dönüyor
  const latest = response?.data ?? response;
  const lifetimeLikes = latest?.lifetimeLikes ?? 0;
  const totalFollowers = latest?.totalFollowers ?? 0;
  const followingCount = latest?.followingCount ?? 0;

  return (
    <Card className="w-full h-full flex flex-col border-border/40 shadow-sm relative overflow-hidden bg-card">
      <div className="absolute top-0 left-0 w-full h-16 bg-gradient-to-r from-cyan-500/15 via-background to-pink-500/15"></div>
      
      <CardContent className="flex-1 flex flex-col items-center justify-center text-center pt-8 pb-4 px-4 z-10">
        <div className="w-16 h-16 rounded-full bg-gradient-to-tr from-cyan-500 to-pink-500 flex items-center justify-center text-white font-bold text-2xl shadow-md ring-4 ring-background mb-3">
          FD
        </div>
        
        <h2 className="text-xl font-bold text-foreground tracking-tight mb-1">fruitssdancing</h2>
        <Badge variant="secondary" className="bg-cyan-500/10 text-cyan-600 hover:bg-cyan-500/20 border-0 gap-1 mb-6">
          <Sparkles className="w-3 h-3" /> TikTok Creator
        </Badge>
        
        <div className="grid grid-cols-3 gap-2 w-full mt-auto border-t border-border/50 pt-4">
          <div className="flex flex-col items-center gap-1">
            <Heart className="w-4 h-4 text-rose-500 fill-current" />
            <span className="font-bold text-sm text-foreground">{formatK(lifetimeLikes)}</span>
            <span className="text-[10px] text-muted-foreground uppercase tracking-wider">Likes</span>
          </div>
          
          <div className="flex flex-col items-center gap-1 border-x border-border/50">
            <Users className="w-4 h-4 text-cyan-500 fill-current" />
            <span className="font-bold text-sm text-foreground">{formatK(totalFollowers)}</span>
            <span className="text-[10px] text-muted-foreground uppercase tracking-wider">Followers</span>
          </div>
          
          <div className="flex flex-col items-center gap-1">
            <UserPlus className="w-4 h-4 text-muted-foreground" />
            <span className="font-bold text-sm text-foreground">{followingCount}</span>
            <span className="text-[10px] text-muted-foreground uppercase tracking-wider">Followed</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}