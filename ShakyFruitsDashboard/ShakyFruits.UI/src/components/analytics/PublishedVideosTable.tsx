import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { forceRefreshVideoStats, PublishedVideoLatest } from "@/api/analytics.api";
import { ExternalLink, TrendingUp, VideoOff, RefreshCw } from "lucide-react";

interface PublishedVideosTableProps {
  videos?: PublishedVideoLatest[];
}

export function PublishedVideosTable({ videos = [] }: PublishedVideosTableProps) {
  const queryClient = useQueryClient();
  const [refreshingId, setRefreshingId] = useState<number | null>(null);

  const refreshMutation = useMutation({
    mutationFn: forceRefreshVideoStats,
    onMutate: (videoId) => setRefreshingId(videoId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["published-videos-latest"] });
      setRefreshingId(null);
    },
    onError: (err) => {
      alert(err.message);
      setRefreshingId(null);
    }
  });

  if (videos.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-muted-foreground border border-dashed border-border/50 rounded-lg bg-card/30">
        <VideoOff className="w-8 h-8 mb-3 text-muted-foreground/50" />
        <p>Henüz takip edilen bir video bulunmuyor.</p>
      </div>
    );
  }

  return (
    <div className="rounded-md border border-border/50 bg-card/50 backdrop-blur-sm shadow-sm overflow-hidden">
      <Table>
        <TableHeader className="bg-muted/30">
          <TableRow>
            <TableHead className="w-[80px]">Gen ID</TableHead>
            <TableHead>Platform URL</TableHead>
            <TableHead className="text-right">Views</TableHead>
            <TableHead className="text-right">Likes</TableHead>
            <TableHead className="text-right">Favorites</TableHead>
            <TableHead className="text-right">Comments</TableHead>
            <TableHead className="text-right">Status</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {videos.map((video) => {
            const stats = video.latestStats;
            const isRefreshing = refreshingId === video.videoId;

            return (
              <TableRow key={video.videoId} className="hover:bg-muted/20 transition-colors">
                <TableCell className="font-medium text-foreground">
                  #{video.videoId}
                </TableCell>
                <TableCell>
                  <a
                    href={video.postUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="flex items-center gap-2 text-indigo-400 hover:text-indigo-300 transition-colors max-w-[200px] truncate"
                  >
                    <ExternalLink className="w-3.5 h-3.5 flex-shrink-0" />
                    <span className="truncate">{video.postUrl}</span>
                  </a>
                </TableCell>

                <TableCell className="text-right font-semibold text-sky-500">
                  {stats ? stats.views.toLocaleString() : "-"}
                </TableCell>
                <TableCell className="text-right font-semibold text-rose-500">
                  {stats ? stats.likes.toLocaleString() : "-"}
                </TableCell>
                <TableCell className="text-right font-semibold text-amber-500">
                  {stats ? stats.favorites.toLocaleString() : "-"}
                </TableCell>
                <TableCell className="text-right font-medium text-emerald-500">
                  {stats ? stats.comments.toLocaleString() : "-"}
                </TableCell>

                <TableCell className="text-right">
                  {stats ? (
                    <div className="flex items-center justify-end gap-1.5 text-xs text-emerald-500 font-medium">
                      <TrendingUp className="w-3.5 h-3.5" /> Tracked
                    </div>
                  ) : (
                    <span className="text-xs text-muted-foreground">Waiting...</span>
                  )}
                </TableCell>

                {/* YENİ: SATIR İÇİ İŞLEM BUTONLARI (Refresh) */}
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2 text-muted-foreground hover:text-primary"
                    onClick={() => refreshMutation.mutate(video.videoId)}
                    disabled={isRefreshing}
                    title="Live Scrape (Zorla Yenile)"
                  >
                    {isRefreshing ? (
                      <RefreshCw className="w-4 h-4 animate-spin text-primary" />
                    ) : (
                      <RefreshCw className="w-4 h-4" />
                    )}
                  </Button>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}