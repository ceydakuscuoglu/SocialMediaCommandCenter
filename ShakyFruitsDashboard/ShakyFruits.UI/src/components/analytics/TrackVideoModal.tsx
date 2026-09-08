import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { addPublishedVideo } from "@/api/analytics.api";
import { 
  Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger 
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Link, Loader2, Plus } from "lucide-react";

export function TrackVideoModal() {
  const [open, setOpen] = useState(false);
  const [generationId, setGenerationId] = useState("");
  const [url, setUrl] = useState("");
  
  const queryClient = useQueryClient();

  const publishMutation = useMutation({
    mutationFn: addPublishedVideo,
    onSuccess: () => {
      // Tabloyu ve grafikleri yenile
      queryClient.invalidateQueries({ queryKey: ["published-videos"] });
      setOpen(false);
      setGenerationId("");
      setUrl("");
    },
    onError: (err) => {
      alert(err.message);
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!generationId || !url) return alert("Lütfen tüm alanları doldurun.");
    
    publishMutation.mutate({ 
      videoGenerationId: parseInt(generationId), 
      postUrl: url 
    });
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button className="gap-2 bg-indigo-600 hover:bg-indigo-700 text-white">
          <Plus className="w-4 h-4" />
          Add to Tracking
        </Button>
      </DialogTrigger>
      
      <DialogContent className="sm:max-w-[425px] bg-card text-foreground">
        <DialogHeader>
          <DialogTitle>Track New Published Video</DialogTitle>
          <DialogDescription>
            Link a generated video to a TikTok post. The background worker will start tracking its performance automatically.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4 mt-4">
          <div className="space-y-2">
            <Label>Video Generation ID</Label>
            <Input 
              type="number" 
              placeholder="e.g. 15" 
              value={generationId}
              onChange={(e) => setGenerationId(e.target.value)}
            />
            <p className="text-xs text-muted-foreground">The ID from your production pipeline table.</p>
          </div>

          <div className="space-y-2">
            <Label>TikTok Post URL</Label>
            <div className="relative">
              <Link className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input 
                placeholder="https://www.tiktok.com/@..." 
                className="pl-9"
                value={url}
                onChange={(e) => setUrl(e.target.value)}
              />
            </div>
          </div>

          <Button type="submit" className="w-full" disabled={publishMutation.isPending}>
            {publishMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : "Start Tracking"}
          </Button>
        </form>
      </DialogContent>
    </Dialog>
  );
}