import { 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableHeader, 
  TableRow 
} from "@/components/ui/table";
import { PublishedVideo } from "@/api/analytics.api";
import { ExternalLink, TrendingUp, VideoOff } from "lucide-react";

interface PublishedVideosTableProps {
  videos?: PublishedVideo[];
}

export function PublishedVideosTable({ videos = [] }: PublishedVideosTableProps) {
  
  if (videos.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-muted-foreground border border-dashed border-border/50 rounded-lg bg-card/30">
        <VideoOff className="w-8 h-8 mb-3 text-muted-foreground/50" />
        <p>Henüz takip edilen bir video bulunmuyor.</p>
        <p className="text-sm">Yukarıdaki "Track New Video" butonunu kullanarak listeye ekleme yapabilirsiniz.</p>
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
            <TableHead>Published Date</TableHead>
            <TableHead className="text-right">Views</TableHead>
            <TableHead className="text-right">Likes</TableHead>
            <TableHead className="text-right">Favorites</TableHead>
            <TableHead className="text-right">Comments</TableHead>
            <TableHead className="text-right">Shares</TableHead>
            <TableHead className="text-right">Status</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {videos.map((video) => {
            // En güncel analitik verisini (listenin son elemanını) alıyoruz
            const historyLength = video.analyticsHistory?.length || 0;
            const latestStats = historyLength > 0 
              ? video.analyticsHistory[historyLength - 1] 
              : null;

            return (
              <TableRow key={video.id} className="hover:bg-muted/20 transition-colors">
                <TableCell className="font-medium text-foreground">
                  #{video.videoGenerationId}
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
                <TableCell className="text-muted-foreground">
                  {new Date(video.publishedAt).toLocaleDateString("en-US", { 
                    month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" 
                  })}
                </TableCell>
                
                {/* İZLENMELER (Mavi) */}
                <TableCell className="text-right font-semibold text-sky-500">
                  {latestStats ? latestStats.views.toLocaleString() : "-"}
                </TableCell>
                
                {/* BEĞENİLER (Pembe/Kırmızı) */}
                <TableCell className="text-right font-semibold text-rose-500">
                  {latestStats ? latestStats.likes.toLocaleString() : "-"}
                </TableCell>
                
                {/* FAVORİLER (Sarı/Amber) */}
                <TableCell className="text-right font-semibold text-amber-500">
                  {latestStats ? latestStats.favorites.toLocaleString() : "-"}
                </TableCell>
                
                {/* YORUMLAR (Standart/Yeşilimsi) */}
                <TableCell className="text-right font-medium text-emerald-500">
                  {latestStats ? latestStats.comments.toLocaleString() : "-"}
                </TableCell>
                
                {/* PAYLAŞIMLAR (Standart) */}
                <TableCell className="text-right font-medium text-foreground">
                  {latestStats ? latestStats.shares.toLocaleString() : "-"}
                </TableCell>

                <TableCell className="text-right">
                  {latestStats ? (
                    <div className="flex items-center justify-end gap-1.5 text-xs text-emerald-500 font-medium">
                      <TrendingUp className="w-3.5 h-3.5" /> Tracking
                    </div>
                  ) : (
                    <span className="text-xs text-muted-foreground">Waiting for bot...</span>
                  )}
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}