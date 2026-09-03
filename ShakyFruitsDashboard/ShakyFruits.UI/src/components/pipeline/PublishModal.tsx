import { useState, useEffect } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { generateAiCaption, publishVideo, SocialPlatform } from "@/api/publish.api";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Loader2, Sparkles, Send, ImageIcon, CheckCircle2 } from "lucide-react";

interface PublishModalProps {
  isOpen: boolean;
  onClose: () => void;
  videoGenerationId: number | null;
  videoPath: string;
  defaultCaption?: string;
}

export function PublishModal({ isOpen, onClose, videoGenerationId, videoPath, defaultCaption }: PublishModalProps) {
  const queryClient = useQueryClient();

  const [platform, setPlatform] = useState<SocialPlatform>(SocialPlatform.TikTok);
  const [caption, setCaption] = useState("");

  // Modal açıldığında varsayılan caption varsa doldur
  useEffect(() => {
    if (isOpen) {
      setCaption(defaultCaption || "");
    }
  }, [isOpen, defaultCaption]);

  // 1. YAPAY ZEKA CAPTION MUTATION
  const captionMutation = useMutation({
    mutationFn: generateAiCaption,
    onSuccess: (data) => {
      setCaption(data.caption);
    },
    onError: (err) => alert(err.message)
  });

  // 2. YAYINLAMA (PLAYWRIGHT) MUTATION
  const publishMutation = useMutation({
    mutationFn: publishVideo,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["pipeline-jobs"] });
      alert(data.message || "Video başarıyla platforma yüklendi!");
      onClose();
    },
    onError: (err) => alert(err.message)
  });

  const handleGenerateCaption = (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    if (!videoGenerationId) return;
    captionMutation.mutate({ id: videoGenerationId, platform });
  };

  const handlePublish = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!videoGenerationId) return;

    publishMutation.mutate({
      videoGenerationId,
      videoPath,
      caption,
      platform
      // Kapak fotoğrafı backend tarafından ekleneceği için sildik
    });
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !publishMutation.isPending && !open && onClose()}>
      <DialogContent className="sm:max-w-[500px] bg-card text-foreground">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Send className="w-5 h-5 text-primary" />
            Publish to Social Media
          </DialogTitle>
          <DialogDescription>
            Select a platform and generate an AI-powered viral caption before publishing.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handlePublish} className="space-y-6 mt-4">

          {/* Platform Seçimi */}
          <div className="space-y-2">
            <Label>Target Platform</Label>
            <Select
              value={platform.toString()}
              onValueChange={(val) => setPlatform(parseInt(val) as SocialPlatform)}
              disabled={publishMutation.isPending}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select platform" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={SocialPlatform.TikTok.toString()}>TikTok</SelectItem>
                <SelectItem value={SocialPlatform.Instagram.toString()}>Instagram (Reels)</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Caption Alanı ve AI Butonu */}
          <div className="space-y-2">
            <div className="flex justify-between items-end">
              <Label>Caption & Hashtags</Label>
              <Button
                type="button"
                variant="secondary"
                size="sm"
                className="h-7 text-xs bg-amber-500/10 text-amber-600 hover:bg-amber-500/20 border-amber-500/20"
                onClick={handleGenerateCaption}
                disabled={captionMutation.isPending || publishMutation.isPending}
              >
                {captionMutation.isPending ? (
                  <Loader2 className="w-3 h-3 mr-1.5 animate-spin" />
                ) : (
                  <Sparkles className="w-3 h-3 mr-1.5" />
                )}
                Generate with Gemini
              </Button>
            </div>
            <Textarea
              placeholder="Write your caption here or generate one..."
              className="min-h-[120px] resize-none"
              value={caption}
              onChange={(e) => setCaption(e.target.value)}
              disabled={publishMutation.isPending}
              required
            />
          </div>
          
          {/* Yayınla Butonu */}
          <Button
            type="submit"
            className="w-full relative"
            disabled={publishMutation.isPending || captionMutation.isPending || !caption}
          >
            {publishMutation.isPending ? (
              <>
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                Robot is publishing... Do not close!
              </>
            ) : (
              "Publish Video"
            )}
          </Button>

          {/* Uzun Süren İşlem Uyarısı */}
          {publishMutation.isPending && (
            <p className="text-[10px] text-amber-500 text-center animate-pulse mt-2">
              Playwright browser automation is running in the background. This may take 1-3 minutes depending on network speed.
            </p>
          )}

        </form>
      </DialogContent>
    </Dialog>
  );
}