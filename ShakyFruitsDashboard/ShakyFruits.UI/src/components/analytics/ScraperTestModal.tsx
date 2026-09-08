import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { testTikTokScraper, ScraperTestResult } from "@/api/analytics.api";
import { 
  Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger 
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Eye, Heart, MessageCircle, Bookmark, Search, Loader2 } from "lucide-react";

export function ScraperTestModal() {
  const [url, setUrl] = useState("");
  const [result, setResult] = useState<ScraperTestResult | null>(null);

  const scrapeMutation = useMutation({
    mutationFn: testTikTokScraper,
    onSuccess: (data) => {
      setResult(data);
    },
    onError: (err) => {
      alert(err.message);
    }
  });

  const handleTest = (e: React.FormEvent) => {
    e.preventDefault();
    if (!url) return alert("Lütfen bir TikTok linki girin.");
    
    setResult(null); // Yeni arama yaparken eski sonucu temizle
    scrapeMutation.mutate(url);
  };

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button className="gap-2 bg-indigo-600 hover:bg-indigo-700 text-white">
          <Search className="w-4 h-4" />
          Test Playwright Scraper
        </Button>
      </DialogTrigger>
      
      <DialogContent className="sm:max-w-[500px] bg-card text-foreground">
        <DialogHeader>
          <DialogTitle>TikTok Studio Scraper Test</DialogTitle>
          <DialogDescription>
            Enter a TikTok video URL to test the C# Playwright background worker instantly.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleTest} className="flex gap-2 mt-4">
          <Input 
            placeholder="https://www.tiktok.com/@user/video/..." 
            value={url}
            onChange={(e) => setUrl(e.target.value)}
            className="flex-1"
          />
          <Button type="submit" disabled={scrapeMutation.isPending}>
            {scrapeMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : "Scrape"}
          </Button>
        </form>

        {/* Sonuç Alanı */}
        {result && (
          <div className="mt-6 space-y-4 animate-in fade-in zoom-in duration-300">
            <div className="p-3 bg-emerald-500/10 text-emerald-500 border border-emerald-500/20 rounded-md text-sm text-center font-medium">
              {result.message}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="flex items-center gap-3 p-3 bg-muted/50 rounded-lg border border-border/50">
                <div className="p-2 bg-blue-500/10 rounded-md text-blue-500"><Eye className="w-4 h-4" /></div>
                <div>
                  <div className="text-xs text-muted-foreground">Views</div>
                  <div className="font-bold">{result.stats.views.toLocaleString()}</div>
                </div>
              </div>

              <div className="flex items-center gap-3 p-3 bg-muted/50 rounded-lg border border-border/50">
                <div className="p-2 bg-rose-500/10 rounded-md text-rose-500"><Heart className="w-4 h-4" /></div>
                <div>
                  <div className="text-xs text-muted-foreground">Likes</div>
                  <div className="font-bold">{result.stats.likes.toLocaleString()}</div>
                </div>
              </div>

              <div className="flex items-center gap-3 p-3 bg-muted/50 rounded-lg border border-border/50">
                <div className="p-2 bg-amber-500/10 rounded-md text-amber-500"><Bookmark className="w-4 h-4" /></div>
                <div>
                  <div className="text-xs text-muted-foreground">Favorites</div>
                  <div className="font-bold">{result.stats.favorites.toLocaleString()}</div>
                </div>
              </div>

              <div className="flex items-center gap-3 p-3 bg-muted/50 rounded-lg border border-border/50">
                <div className="p-2 bg-emerald-500/10 rounded-md text-emerald-500"><MessageCircle className="w-4 h-4" /></div>
                <div>
                  <div className="text-xs text-muted-foreground">Comments</div>
                  <div className="font-bold">{result.stats.comments.toLocaleString()}</div>
                </div>
              </div>
            </div>
            
            <div className="text-xs text-center text-muted-foreground pt-2">
              Snapshot taken at: {new Date(result.stats.recordedAt).toLocaleString()}
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}