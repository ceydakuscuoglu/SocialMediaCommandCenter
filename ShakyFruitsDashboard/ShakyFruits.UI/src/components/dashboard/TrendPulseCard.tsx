import { useQuery } from "@tanstack/react-query";
import { fetchLiveTrends } from "@/api/analytics.api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loader2, Flame } from "lucide-react";
import { Badge } from "@/components/ui/badge";

export function TrendPulseCard() {
    const { data: response, isLoading } = useQuery({
        queryKey: ["tiktok-live-trends"],
        queryFn: fetchLiveTrends,
    });

    if (isLoading) {
        return (
            <Card className="w-full h-[140px] flex items-center justify-center border-border/40">
                <Loader2 className="w-5 h-5 animate-spin text-muted-foreground" />
            </Card>
        );
    }

    const topTrends = response?.data?.trendingHashtags?.slice(0, 5) || [];

    return (
        <Card className="w-full border-border/40 shadow-sm bg-gradient-to-br from-card to-rose-500/5">
            <CardHeader className="pb-2 pt-4 px-4">
                <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                    <Flame className="w-4 h-4 text-rose-500" />
                    Today's TikTok Pulse
                </CardTitle>
            </CardHeader>
            <CardContent className="px-4 pb-4 pt-0">
                <div className="flex flex-wrap gap-1.5">
                    {topTrends.map((tag) => (
                        <Badge key={tag.rank} variant="secondary" className="bg-background border border-border/50 hover:bg-muted font-medium px-2 py-0.5 text-xs">
                            {tag.name}
                        </Badge>
                    ))}
                </div>
            </CardContent>
        </Card>
    );
}