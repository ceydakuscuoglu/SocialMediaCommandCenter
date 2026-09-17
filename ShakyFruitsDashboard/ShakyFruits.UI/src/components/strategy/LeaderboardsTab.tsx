import { useState, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { fetchLeaderboards } from "@/api/analytics.api";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { BarChart, Bar, Cell, XAxis, YAxis, Tooltip, ResponsiveContainer } from "recharts";
import { Flame, TrendingUp, Sparkles, Music } from "lucide-react";
import { cn } from "@/lib/utils";

export function LeaderboardsTab() {
  const { data: leaderboards, isLoading } = useQuery({
    queryKey: ["leaderboards"],
    queryFn: fetchLeaderboards,
  });

  const [metric, setMetric] = useState<"total" | "average">("total");

  // Meyveleri seçilen metriğe göre sırala ve ilk 5'i al
  const sortedFruits = useMemo(() => {
    if (!leaderboards?.fruitLeaderboard) return [];
    return [...leaderboards.fruitLeaderboard]
      .sort((a, b) =>
        metric === "total"
          ? (b.totalViews || 0) - (a.totalViews || 0)
          : (b.averageViews || 0) - (a.averageViews || 0)
      )
      .slice(0, 5);
  }, [leaderboards?.fruitLeaderboard, metric]);

  // Dans stillerini seçilen metriğe göre sırala ve ilk 5'i al
  const sortedDances = useMemo(() => {
    if (!leaderboards?.danceLeaderboard) return [];
    return [...leaderboards.danceLeaderboard]
      .sort((a, b) =>
        metric === "total"
          ? (b.totalViews || 0) - (a.totalViews || 0)
          : (b.averageViews || 0) - (a.averageViews || 0)
      )
      .slice(0, 5);
  }, [leaderboards?.danceLeaderboard, metric]);

  const activeDataKey = metric === "total" ? "totalViews" : "averageViews";

  return (
    <div className="mt-6 space-y-6">
      {/* Metrik Seçici (Total vs Average) */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-3.5 bg-muted/20 border border-border/50 rounded-xl">
        <div>
          <div className="flex items-center gap-2">
            <Sparkles className="w-4 h-4 text-primary" />
            <h4 className="text-sm font-semibold text-foreground">Ranking Metric</h4>
          </div>
          <p className="text-xs text-muted-foreground mt-0.5">
            {metric === "total"
              ? "Showing cumulative views across all published videos (highlights overall champions like Pineapple & Dragon Fruit)"
              : "Showing average views per video (highlights character efficiency and viral consistency)"}
          </p>
        </div>

        <div className="inline-flex items-center p-1 bg-muted/50 border border-border/50 rounded-lg self-start sm:self-auto shadow-inner">
          <button
            type="button"
            onClick={() => setMetric("total")}
            className={cn(
              "flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-md transition-all border",
              metric === "total"
                ? "bg-primary/10 text-primary border-primary/20 shadow-sm"
                : "border-transparent text-muted-foreground hover:bg-muted/50 hover:text-foreground"
            )}
          >
            <Flame className="w-3.5 h-3.5" />
            <span>Total Views</span>
          </button>
          <button
            type="button"
            onClick={() => setMetric("average")}
            className={cn(
              "flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-md transition-all border",
              metric === "average"
                ? "bg-primary/10 text-primary border-primary/20 shadow-sm"
                : "border-transparent text-muted-foreground hover:bg-muted/50 hover:text-foreground"
            )}
          >
            <TrendingUp className="w-3.5 h-3.5" />
            <span>Average Views</span>
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Yatay BarChart (Top 5 Meyve) */}
        <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
          <div className="flex items-center justify-between">
            <h3 className="font-semibold text-foreground flex items-center gap-2">
              🍎 Fruit Champions (Top 5)
            </h3>
            <Badge variant="outline" className="text-[11px] font-normal text-muted-foreground border-border/60">
              {metric === "total" ? "By Total Views" : "By Avg Views"}
            </Badge>
          </div>

          <div className="h-[270px] w-full">
            {isLoading ? (
              <div className="flex items-center justify-center h-full text-muted-foreground text-sm">
                Loading leaderboards...
              </div>
            ) : sortedFruits.length === 0 ? (
              <div className="flex items-center justify-center h-full text-muted-foreground text-sm">
                No video analytics data available yet.
              </div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart
                  data={sortedFruits}
                  layout="vertical"
                  margin={{ top: 10, right: 30, left: 10, bottom: 0 }}
                >
                  <XAxis type="number" hide />
                  <YAxis
                    dataKey="name"
                    type="category"
                    axisLine={false}
                    tickLine={false}
                    tick={{ fill: "hsl(var(--foreground))", fontSize: 12 }}
                    width={95}
                  />
                  <Tooltip
                    cursor={{ fill: "var(--border)", opacity: 0.2 }}
                    content={({ active, payload }) => {
                      if (active && payload && payload.length) {
                        const data = payload[0].payload;
                        return (
                          <div className="bg-card border border-border/50 p-3 rounded-lg shadow-lg text-xs min-w-[170px] space-y-1.5">
                            <div className="flex items-center justify-between border-b border-border/40 pb-1">
                              <span className="font-semibold text-foreground">{data.name}</span>
                              <Badge variant="secondary" className="text-[10px] px-1.5 py-0">
                                {data.videoCount} {data.videoCount === 1 ? "video" : "videos"}
                              </Badge>
                            </div>
                            <div className="flex justify-between gap-4 text-muted-foreground">
                              <span>Total Views:</span>
                              <span className="font-semibold text-foreground">
                                {(data.totalViews || 0).toLocaleString()}
                              </span>
                            </div>
                            <div className="flex justify-between gap-4 text-muted-foreground">
                              <span>Avg Views:</span>
                              <span className="font-semibold text-primary">
                                {(data.averageViews || 0).toLocaleString()}
                              </span>
                            </div>
                          </div>
                        );
                      }
                      return null;
                    }}
                  />
                  <Bar dataKey={activeDataKey} radius={[0, 4, 4, 0]} barSize={24}>
                    {sortedFruits.map((_, index) => {
                      // HATA DÜZELTİLDİ: Sadece var() kullanıyoruz çünkü teman zaten oklch içeriyor
                      const chartColor = `var(--chart-${(index % 5) + 1})`;

                      return (
                        <Cell
                          key={`cell-${index}`}
                          fill={chartColor}
                          className="hover:opacity-80 transition-opacity duration-200"
                        />
                      );
                    })}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        {/* Trending Dances Tablosu */}
        <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
          <div className="flex items-center justify-between">
            <h3 className="font-semibold text-foreground flex items-center gap-2">
              <Music className="w-4 h-4 text-sky-400" />
              <span>Trending Dances</span>
            </h3>
            <Badge variant="outline" className="text-[11px] font-normal text-muted-foreground border-border/60">
              {metric === "total" ? "By Total Views" : "By Avg Views"}
            </Badge>
          </div>

          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Style</TableHead>
                <TableHead className="text-center">Videos</TableHead>
                <TableHead className="text-right">
                  {metric === "total" ? "Total Views" : "Avg Views"}
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {isLoading ? (
                <TableRow>
                  <TableCell colSpan={3} className="text-center text-muted-foreground py-6">
                    Loading dances...
                  </TableCell>
                </TableRow>
              ) : sortedDances.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={3} className="text-center text-muted-foreground py-6">
                    No dance analytics data available.
                  </TableCell>
                </TableRow>
              ) : (
                sortedDances.map((d, i) => {
                  const primaryVal = metric === "total" ? d.totalViews : d.averageViews;
                  const secondaryVal = metric === "total" ? d.averageViews : d.totalViews;
                  const secondaryLabel = metric === "total" ? "Avg" : "Total";

                  return (
                    <TableRow key={i}>
                      <TableCell className="font-medium">{d.name}</TableCell>
                      <TableCell className="text-center">
                        <Badge variant="secondary" className="text-[10px] px-1.5 py-0 font-normal">
                          {d.videoCount}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <span className="font-semibold text-sky-500">
                          {(primaryVal || 0).toLocaleString()}
                        </span>
                        <span className="block text-[10px] text-muted-foreground font-normal">
                          {secondaryLabel}: {(secondaryVal || 0).toLocaleString()}
                        </span>
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </div>
      </div>
    </div>
  );
}
