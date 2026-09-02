import { useQuery } from "@tanstack/react-query";
import { fetchAccountHistory } from "@/api/analytics.api";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Loader2, Sparkles, Heart, Users, UserPlus } from "lucide-react";

const formatK = (num: number) => {
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toString();
};

export function TikTokProfileCard() {
  const { data: history, isLoading } = useQuery({
    queryKey: ["account-history"],
    queryFn: fetchAccountHistory,
  });

  if (isLoading) {
    return (
      <Card className="w-full h-full min-h-[200px] animate-pulse bg-card/50 flex items-center justify-center">
        <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
      </Card>
    );
  }

  if (!history || history.length === 0) return null;

  // En güncel veriyi al
  const sortedHistory = [...history].sort((a, b) => 
    new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime()
  );
  const latest = sortedHistory[sortedHistory.length - 1];

  return (
    <Card className="w-full h-full flex flex-col border-border/40 shadow-sm relative overflow-hidden bg-card">
      {/* Şık arka plan bandı */}
      <div className="absolute top-0 left-0 w-full h-16 bg-gradient-to-r from-orange-500/10 via-pink-500/10 to-purple-500/10"></div>
      
      <CardContent className="flex-1 flex flex-col items-center justify-center text-center pt-8 pb-4 px-4 z-10">
        
        {/* Avatar */}
        <div className="w-16 h-16 rounded-full bg-gradient-to-tr from-orange-500 via-pink-500 to-purple-500 flex items-center justify-center text-white font-bold text-2xl shadow-md ring-4 ring-background mb-3">
          FD
        </div>
        
        {/* İsim ve Rozet */}
        <h2 className="text-xl font-bold text-foreground tracking-tight mb-1">fruitssdancing</h2>
        <Badge variant="secondary" className="bg-amber-500/10 text-amber-600 hover:bg-amber-500/20 border-0 gap-1 mb-6">
          <Sparkles className="w-3 h-3" /> Make Fruits Dancee
        </Badge>
        
        {/* Alt Kısım: İstatistikler (mt-auto ile en alta yapışır) */}
        <div className="grid grid-cols-3 gap-2 w-full mt-auto border-t border-border/50 pt-4">
          <div className="flex flex-col items-center gap-1">
            <Heart className="w-4 h-4 text-rose-500 fill-current" />
            <span className="font-bold text-sm text-foreground">{formatK(latest.lifetimeLikes)}</span>
            <span className="text-[10px] text-muted-foreground uppercase tracking-wider">Beğeni</span>
          </div>
          
          <div className="flex flex-col items-center gap-1 border-x border-border/50">
            <Users className="w-4 h-4 text-sky-500 fill-current" />
            <span className="font-bold text-sm text-foreground">{formatK(latest.totalFollowers)}</span>
            <span className="text-[10px] text-muted-foreground uppercase tracking-wider">Takipçi</span>
          </div>
          
          <div className="flex flex-col items-center gap-1">
            <UserPlus className="w-4 h-4 text-muted-foreground" />
            <span className="font-bold text-sm text-foreground">{latest.followingCount}</span>
            <span className="text-[10px] text-muted-foreground uppercase tracking-wider">Takip</span>
          </div>
        </div>

      </CardContent>
    </Card>
  );
}