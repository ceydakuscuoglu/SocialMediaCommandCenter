import { useState, useMemo } from "react";
import { useMutation, useQueryClient, useQuery } from "@tanstack/react-query";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { forceRefreshVideoStats, fetchPublishedVideos, PublishedVideoLatest } from "@/api/analytics.api";
import { ExternalLink, TrendingUp, VideoOff, RefreshCw, ArrowUpDown, ArrowUp, ArrowDown, LineChart as LineChartIcon, Loader2 } from "lucide-react";
import { VideoPerformanceChart } from "@/components/analytics/VideoPerformanceChart";

interface PublishedVideosTableProps {
  videos?: PublishedVideoLatest[];
}

type SortKey = "videoId" | "title" | "views" | "likes" | "likeToViewRatio" | "favorites" | "comments";
type SortDirection = "asc" | "desc";

export function PublishedVideosTable({ videos = [] }: PublishedVideosTableProps) {
  const queryClient = useQueryClient();
  const [refreshingId, setRefreshingId] = useState<number | null>(null);

  // GRAFİK MODALI İÇİN STATE
  const [chartVideo, setChartVideo] = useState<PublishedVideoLatest | null>(null);

  // SIRALAMA (SORTING) STATE'İ
  const [sortConfig, setSortConfig] = useState<{ key: SortKey; direction: SortDirection }>({
    key: "videoId",
    direction: "desc",
  });

  // 1. ZORLA YENİLEME MUTATION'I
  const refreshMutation = useMutation({
    mutationFn: forceRefreshVideoStats,
    onMutate: (videoId) => setRefreshingId(videoId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["published-videos-latest"] });
      // Ana geçmiş verisini de tetikleyip güncelliyoruz
      queryClient.invalidateQueries({ queryKey: ["published-videos"] });
      setRefreshingId(null);
    },
    onError: (err) => {
      alert(err.message);
      setRefreshingId(null);
    }
  });

  // 2. GRAFİK İÇİN GEÇMİŞ VERİLERİ (Sadece modal açılınca çalışır)
  const { data: fullVideosData, isLoading: isHistoryLoading } = useQuery({
    queryKey: ["published-videos"],
    queryFn: fetchPublishedVideos,
    enabled: !!chartVideo, // Modal açılmadan istek atmaz
  });

  // Seçilen videonun geçmişini ana listeden bul
  const selectedHistory = useMemo(() => {
    if (!chartVideo || !fullVideosData) return [];
    const match = fullVideosData.find(
      (v) => v.id === chartVideo.videoId || v.videoGenerationId === chartVideo.videoId
    );
    return match?.analyticsHistory || [];
  }, [chartVideo, fullVideosData]);

  // SIRALAMA FONKSİYONU
  const sortedVideos = useMemo(() => {
    let sortableVideos = [...videos];
    if (sortConfig !== null) {
      sortableVideos.sort((a, b) => {
        let aValue: any = 0;
        let bValue: any = 0;

        switch (sortConfig.key) {
          case "videoId":
            aValue = a.videoId;
            bValue = b.videoId;
            break;
          case "title":
            aValue = (a.title || a.postUrl || "").toLowerCase();
            bValue = (b.title || b.postUrl || "").toLowerCase();
            break;
          case "views":
            aValue = a.latestStats?.views || 0;
            bValue = b.latestStats?.views || 0;
            break;
          case "likes":
            aValue = a.latestStats?.likes || 0;
            bValue = b.latestStats?.likes || 0;
            break;
          case "likeToViewRatio":
            aValue = a.likeToViewRatio ?? a.latestStats?.likeToViewRatio ?? (a.latestStats && a.latestStats.views > 0 ? (a.latestStats.likes / a.latestStats.views) * 100 : 0);
            bValue = b.likeToViewRatio ?? b.latestStats?.likeToViewRatio ?? (b.latestStats && b.latestStats.views > 0 ? (b.latestStats.likes / b.latestStats.views) * 100 : 0);
            break;
          case "favorites":
            aValue = a.latestStats?.favorites || 0;
            bValue = b.latestStats?.favorites || 0;
            break;
          case "comments":
            aValue = a.latestStats?.comments || 0;
            bValue = b.latestStats?.comments || 0;
            break;
        }

        if (aValue < bValue) {
          return sortConfig.direction === "asc" ? -1 : 1;
        }
        if (aValue > bValue) {
          return sortConfig.direction === "asc" ? 1 : -1;
        }
        return 0;
      });
    }
    return sortableVideos;
  }, [videos, sortConfig]);

  const requestSort = (key: SortKey) => {
    let direction: SortDirection = "asc";
    if (sortConfig && sortConfig.key === key && sortConfig.direction === "asc") {
      direction = "desc";
    }
    setSortConfig({ key, direction });
  };

  const SortIcon = ({ columnKey }: { columnKey: SortKey }) => {
    if (sortConfig?.key !== columnKey) {
      return <ArrowUpDown className="ml-1.5 h-3.5 w-3.5 opacity-40 transition-opacity group-hover:opacity-100" />;
    }
    return sortConfig.direction === "asc" ? (
      <ArrowUp className="ml-1.5 h-3.5 w-3.5 text-primary" />
    ) : (
      <ArrowDown className="ml-1.5 h-3.5 w-3.5 text-primary" />
    );
  };

  if (videos.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-muted-foreground border border-dashed border-border/50 rounded-lg bg-card/30">
        <VideoOff className="w-8 h-8 mb-3 text-muted-foreground/50" />
        <p>Henüz takip edilen bir video bulunmuyor.</p>
      </div>
    );
  }

  return (
    <>
      <div className="rounded-md border border-border/50 bg-card/50 backdrop-blur-sm shadow-sm overflow-hidden">
        <Table>
          <TableHeader className="bg-muted/30">
            <TableRow>
              <TableHead className="w-[90px] text-center">
                <div className="flex items-center justify-center cursor-pointer select-none group hover:text-foreground transition-colors" onClick={() => requestSort("videoId")}>
                  Gen ID <SortIcon columnKey="videoId" />
                </div>
              </TableHead>
              <TableHead>
                <div className="flex items-center cursor-pointer select-none group hover:text-foreground transition-colors" onClick={() => requestSort("title")}>
                  Video / Link <SortIcon columnKey="title" />
                </div>
              </TableHead>
              <TableHead className="text-center">
                <div className="flex items-center justify-center cursor-pointer select-none group hover:text-foreground transition-colors" onClick={() => requestSort("likeToViewRatio")}>
                  View / Like <SortIcon columnKey="likeToViewRatio" />
                </div>
              </TableHead>
              <TableHead className="text-center">
                <div className="flex items-center justify-center cursor-pointer select-none group hover:text-foreground transition-colors" onClick={() => requestSort("views")}>
                  Views <SortIcon columnKey="views" />
                </div>
              </TableHead>
              <TableHead className="text-center">
                <div className="flex items-center justify-center cursor-pointer select-none group hover:text-foreground transition-colors" onClick={() => requestSort("likes")}>
                  Likes <SortIcon columnKey="likes" />
                </div>
              </TableHead>
              <TableHead className="text-center">
                <div className="flex items-center justify-center cursor-pointer select-none group hover:text-foreground transition-colors" onClick={() => requestSort("favorites")}>
                  Favorites <SortIcon columnKey="favorites" />
                </div>
              </TableHead>
              <TableHead className="text-center">
                <div className="flex items-center justify-center cursor-pointer select-none group hover:text-foreground transition-colors" onClick={() => requestSort("comments")}>
                  Comments <SortIcon columnKey="comments" />
                </div>
              </TableHead>
              <TableHead className="text-center">Status</TableHead>
              <TableHead className="text-right pr-4">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {sortedVideos.map((video) => {
              const stats = video.latestStats;
              const ratio = video.likeToViewRatio ?? stats?.likeToViewRatio ?? (stats && stats.views > 0 ? (stats.likes / stats.views) * 100 : 0);
              const isRefreshing = refreshingId === video.videoId;

              return (
                <TableRow key={video.videoId} className="hover:bg-muted/20 transition-colors">
                  <TableCell className="font-medium text-foreground text-center">
                    #{video.videoId}
                  </TableCell>
                  <TableCell>
                    <div className="flex flex-col gap-0.5">
                      {video.title && (
                        <span className="font-semibold text-foreground text-sm truncate max-w-[240px] xl:max-w-[320px]" title={video.title}>
                          {video.title}
                        </span>
                      )}
                      <a
                        href={video.postUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-1.5 text-xs text-indigo-400 hover:text-indigo-300 transition-colors max-w-[240px] xl:max-w-[320px] truncate"
                      >
                        <ExternalLink className="w-3 h-3 flex-shrink-0" />
                        <span className="truncate">{video.postUrl}</span>
                      </a>
                    </div>
                  </TableCell>

                  <TableCell className="text-center font-semibold text-primary">
                    {stats ? (
                      <span
                        className="inline-block"
                        title={stats.views > 0 ? `${stats.likes.toLocaleString()} likes / ${stats.views.toLocaleString()} views` : "0 views"}
                      >
                        {ratio.toFixed(2)}%
                      </span>
                    ) : (
                      "-"
                    )}
                  </TableCell>
                  <TableCell className="text-center font-semibold text-sky-500">
                    {stats ? stats.views.toLocaleString() : "-"}
                  </TableCell>
                  <TableCell className="text-center font-semibold text-rose-500">
                    {stats ? stats.likes.toLocaleString() : "-"}
                  </TableCell>
                  <TableCell className="text-center font-semibold text-amber-500">
                    {stats ? stats.favorites.toLocaleString() : "-"}
                  </TableCell>
                  <TableCell className="text-center font-medium text-emerald-500">
                    {stats ? stats.comments.toLocaleString() : "-"}
                  </TableCell>

                  <TableCell className="text-center">
                    {stats ? (
                      <div className="flex items-center justify-center gap-1.5 text-xs text-emerald-500 font-medium">
                        <TrendingUp className="w-3.5 h-3.5" /> Tracked
                      </div>
                    ) : (
                      <span className="text-xs text-muted-foreground">Waiting...</span>
                    )}
                  </TableCell>

                  <TableCell className="text-right pr-4">
                    <div className="flex items-center justify-end gap-1">
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8 text-indigo-400 hover:text-indigo-500 hover:bg-indigo-500/10"
                        onClick={() => setChartVideo(video)}
                        title="View Growth Trend"
                      >
                        <LineChartIcon className="w-4 h-4" />
                      </Button>

                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8 text-muted-foreground hover:text-primary"
                        onClick={() => refreshMutation.mutate(video.videoId)}
                        disabled={isRefreshing}
                        title="Live Scrape (Force Refresh)"
                      >
                        {isRefreshing ? (
                          <RefreshCw className="w-4 h-4 animate-spin text-primary" />
                        ) : (
                          <RefreshCw className="w-4 h-4" />
                        )}
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </div>

      {/* GRAFİK MODALI (DIALOG) */}
      <Dialog open={!!chartVideo} onOpenChange={(open) => !open && setChartVideo(null)}>
        <DialogContent className="sm:max-w-[750px] bg-card border-border/50 text-foreground p-0 overflow-hidden">
          {chartVideo && (
            <div className="h-[450px] w-full bg-background/50 flex flex-col">
              {isHistoryLoading ? (
                <div className="flex-1 flex flex-col items-center justify-center text-muted-foreground">
                  <Loader2 className="w-8 h-8 animate-spin mb-4 text-primary" />
                  <p>Fetching performance history...</p>
                </div>
              ) : (
                <VideoPerformanceChart
                  historyData={selectedHistory}
                  title={`Growth Trend: ${chartVideo.title || `#${chartVideo.videoId}`}`}
                />
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </>
  );
}