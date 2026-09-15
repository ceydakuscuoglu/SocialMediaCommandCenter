import { useState, useMemo } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { forceRefreshVideoStats, PublishedVideoLatest } from "@/api/analytics.api";
import { ExternalLink, TrendingUp, VideoOff, RefreshCw, ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";

interface PublishedVideosTableProps {
  videos?: PublishedVideoLatest[];
}

type SortKey = "videoId" | "title" | "views" | "likes" | "favorites" | "comments";
type SortDirection = "asc" | "desc";

export function PublishedVideosTable({ videos = [] }: PublishedVideosTableProps) {
  const queryClient = useQueryClient();
  const [refreshingId, setRefreshingId] = useState<number | null>(null);

  // SIRALAMA (SORTING) STATE'İ
  // Varsayılan olarak Gen ID (videoId) azalan (desc) şeklinde ayarlandı (En yeniler üstte)
  const [sortConfig, setSortConfig] = useState<{ key: SortKey; direction: SortDirection }>({
    key: "videoId",
    direction: "desc",
  });

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

  // Kolon başlığına tıklandığında çalışacak fonksiyon
  const requestSort = (key: SortKey) => {
    let direction: SortDirection = "asc";
    if (sortConfig && sortConfig.key === key && sortConfig.direction === "asc") {
      direction = "desc";
    }
    setSortConfig({ key, direction });
  };

  // İkonları doğru yönde gösteren yardımcı bileşen
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
    <div className="rounded-md border border-border/50 bg-card/50 backdrop-blur-sm shadow-sm overflow-hidden">
      <Table>
        <TableHeader className="bg-muted/30">
          <TableRow>
            {/* 1. Gen ID (Tıklanabilir) */}
            <TableHead className="w-[100px]">
              <div
                className="flex items-center cursor-pointer select-none group hover:text-foreground transition-colors"
                onClick={() => requestSort("videoId")}
              >
                Gen ID <SortIcon columnKey="videoId" />
              </div>
            </TableHead>

            {/* 2. Video / Link (Tıklanabilir) */}
            <TableHead>
              <div
                className="flex items-center cursor-pointer select-none group hover:text-foreground transition-colors"
                onClick={() => requestSort("title")}
              >
                Video / Link <SortIcon columnKey="title" />
              </div>
            </TableHead>

            {/* 3. Views (Tıklanabilir - Sağa Dayalı) */}
            <TableHead className="text-right">
              <div
                className="flex items-center justify-end cursor-pointer select-none group hover:text-foreground transition-colors"
                onClick={() => requestSort("views")}
              >
                Views <SortIcon columnKey="views" />
              </div>
            </TableHead>

            {/* 4. Likes (Tıklanabilir - Sağa Dayalı) */}
            <TableHead className="text-right">
              <div
                className="flex items-center justify-end cursor-pointer select-none group hover:text-foreground transition-colors"
                onClick={() => requestSort("likes")}
              >
                Likes <SortIcon columnKey="likes" />
              </div>
            </TableHead>

            {/* 5. Favorites (Tıklanabilir - Sağa Dayalı) */}
            <TableHead className="text-right">
              <div
                className="flex items-center justify-end cursor-pointer select-none group hover:text-foreground transition-colors"
                onClick={() => requestSort("favorites")}
              >
                Favorites <SortIcon columnKey="favorites" />
              </div>
            </TableHead>

            {/* 6. Comments (Tıklanabilir - Sağa Dayalı) */}
            <TableHead className="text-right">
              <div
                className="flex items-center justify-end cursor-pointer select-none group hover:text-foreground transition-colors"
                onClick={() => requestSort("comments")}
              >
                Comments <SortIcon columnKey="comments" />
              </div>
            </TableHead>

            <TableHead className="text-right">Status</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {/* Orijinal videos dizisi yerine sortedVideos dizisini map'liyoruz */}
          {sortedVideos.map((video) => {
            const stats = video.latestStats;
            const isRefreshing = refreshingId === video.videoId;

            return (
              <TableRow key={video.videoId} className="hover:bg-muted/20 transition-colors">
                <TableCell className="font-medium text-foreground">
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