import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { fetchLiveTrends } from "@/api/analytics.api";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Loader2, Copy, Check, TrendingUp } from "lucide-react";

export function LiveTrendsTable() {
    const [copiedTag, setCopiedTag] = useState<string | null>(null);

    const { data: response, isLoading } = useQuery({
        queryKey: ["tiktok-live-trends"],
        queryFn: fetchLiveTrends,
    });

    const handleCopy = (tag: string) => {
        navigator.clipboard.writeText(tag);
        setCopiedTag(tag);
        setTimeout(() => setCopiedTag(null), 2000);
    };

    if (isLoading) {
        return (
            <Card className="w-full h-64 flex items-center justify-center border-border/40">
                <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
            </Card>
        );
    }

    const trends = response?.data?.trendingHashtags || [];

    return (
        <Card className="w-full border-border/40 shadow-sm">
            <CardHeader>
                <CardTitle className="flex items-center gap-2">
                    <TrendingUp className="w-5 h-5 text-primary" />
                    TikTok Daily Trending Hashtags
                </CardTitle>
                <CardDescription>Top 50 performing hashtags updated daily from TikTok Creative Center.</CardDescription>
            </CardHeader>
            <CardContent>
                <div className="rounded-md border border-border/50">
                    <Table>
                        <TableHeader className="bg-muted/30">
                            <TableRow>
                                <TableHead className="w-16 text-center">Rank</TableHead>
                                <TableHead>Hashtag</TableHead>
                                <TableHead>Performance Stats</TableHead>
                                <TableHead className="text-right">Action</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {trends.map((tag) => (
                                <TableRow key={tag.rank} className="hover:bg-muted/20">
                                    <TableCell className="text-center font-bold text-muted-foreground">#{tag.rank}</TableCell>
                                    <TableCell className="font-medium text-foreground">{tag.name}</TableCell>
                                    <TableCell className="text-sm text-muted-foreground">{tag.stats}</TableCell>
                                    <TableCell className="text-right">
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={() => handleCopy(tag.name)}
                                            className="h-8 text-xs text-muted-foreground hover:text-primary"
                                        >
                                            {copiedTag === tag.name ? <Check className="w-4 h-4 text-emerald-500" /> : <Copy className="w-4 h-4" />}
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </div>
            </CardContent>
        </Card>
    );
}