import { useEffect, useState } from "react";
import {
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";

import {
  generateAiCaption,
  publishVideo,
  SocialPlatform,
} from "@/api/publish.api";

import { fetchLiveTrends } from "@/api/analytics.api";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { Badge } from "@/components/ui/badge";

import {
  Loader2,
  Send,
  Sparkles,
} from "lucide-react";

interface PublishModalProps {
  isOpen: boolean;
  onClose: () => void;
  videoGenerationId: number | null;
  videoPath: string;
  defaultCaption?: string;
}

export function PublishModal({
  isOpen,
  onClose,
  videoGenerationId,
  videoPath,
  defaultCaption,
}: PublishModalProps) {
  const queryClient = useQueryClient();

  const [platform, setPlatform] = useState<SocialPlatform>(
    SocialPlatform.TikTok
  );

  const [caption, setCaption] = useState("");

  /**
   * Canlı TikTok trend hashtag'lerini çeker.
   * Sadece modal açıkken çalışır.
   */
  const { data: trendsResponse } = useQuery({
    queryKey: ["tiktok-live-trends"],
    queryFn: fetchLiveTrends,
    enabled: isOpen,
  });

  /**
   * API response içerisinde trendingHashtags undefined gelebileceği için
   * her zaman güvenli bir array oluşturuyoruz.
   */
  const trendingHashtags =
    trendsResponse?.data?.trendingHashtags ?? [];

  /**
   * Modal açıldığında defaultCaption varsa caption alanına aktar.
   */
  useEffect(() => {
    if (isOpen) {
      setCaption(defaultCaption || "");
    }
  }, [isOpen, defaultCaption]);

  /**
   * AI Caption oluşturma.
   */
  const captionMutation = useMutation({
    mutationFn: generateAiCaption,

    onSuccess: (data) => {
      setCaption(data.caption);
    },

    onError: (err: Error) => {
      alert(err.message);
    },
  });

  /**
   * Videoyu seçilen sosyal medya platformuna yayınlama.
   */
  const publishMutation = useMutation({
    mutationFn: publishVideo,

    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ["pipeline-jobs"],
      });

      alert(
        data.message ||
        "Video başarıyla platforma yüklendi!"
      );

      onClose();
    },

    onError: (err: Error) => {
      alert(err.message);
    },
  });

  /**
   * Gemini ile caption oluştur.
   */
  const handleGenerateCaption = (
    e: React.MouseEvent<HTMLButtonElement>
  ) => {
    e.preventDefault();

    if (!videoGenerationId) {
      return;
    }

    captionMutation.mutate({
      id: videoGenerationId,
      platform,
    });
  };

  /**
   * Videoyu yayınla.
   */
  const handlePublish = (
    e: React.FormEvent<HTMLFormElement>
  ) => {
    e.preventDefault();

    if (!videoGenerationId) {
      return;
    }

    publishMutation.mutate({
      videoGenerationId,
      videoPath,
      caption,
      platform,
    });
  };

  /**
   * Trending hashtag'e tıklandığında caption sonuna ekler.
   */
  const handleHashtagClick = (
    tagName: string
  ) => {
    const currentText = caption || "";

    const spacer =
      currentText &&
        !currentText.endsWith(" ")
        ? " "
        : "";

    setCaption(
      currentText +
      spacer +
      tagName
    );
  };

  return (
    <Dialog
      open={isOpen}
      onOpenChange={(open) => {
        if (
          !publishMutation.isPending &&
          !open
        ) {
          onClose();
        }
      }}
    >
      <DialogContent
        className="
          w-[calc(100%-2rem)]
          sm:max-w-[500px]
          bg-card
          text-foreground
          overflow-hidden
        "
      >
        <DialogHeader className="min-w-0">
          <DialogTitle className="flex items-center gap-2">
            <Send className="h-5 w-5 shrink-0 text-primary" />

            <span className="min-w-0">
              Publish to Social Media
            </span>
          </DialogTitle>

          <DialogDescription>
            Select a platform and generate an
            AI-powered viral caption before
            publishing.
          </DialogDescription>
        </DialogHeader>

        <form
          onSubmit={handlePublish}
          className="
            mt-4
            w-full
            min-w-0
            space-y-6
          "
        >
          {/* ================================= */}
          {/* PLATFORM SEÇİMİ */}
          {/* ================================= */}

          <div className="w-full min-w-0 space-y-2">
            <Label>
              Target Platform
            </Label>

            <Select
              value={platform.toString()}
              onValueChange={(value) =>
                setPlatform(
                  parseInt(
                    value,
                    10
                  ) as SocialPlatform
                )
              }
              disabled={
                publishMutation.isPending
              }
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder="Select platform" />
              </SelectTrigger>

              <SelectContent>
                <SelectItem
                  value={SocialPlatform.TikTok.toString()}
                >
                  TikTok
                </SelectItem>

                <SelectItem
                  value={SocialPlatform.Instagram.toString()}
                >
                  Instagram (Reels)
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* ================================= */}
          {/* CAPTION */}
          {/* ================================= */}

          <div className="w-full min-w-0 space-y-2">
            <div
              className="
                flex
                min-w-0
                items-end
                justify-between
                gap-2
              "
            >
              <Label className="shrink-0">
                Caption & Hashtags
              </Label>

              <Button
                type="button"
                variant="secondary"
                size="sm"
                className="
                  h-7
                  shrink-0
                  border-amber-500/20
                  bg-amber-500/10
                  text-xs
                  text-amber-600
                  hover:bg-amber-500/20
                "
                onClick={
                  handleGenerateCaption
                }
                disabled={
                  captionMutation.isPending ||
                  publishMutation.isPending
                }
              >
                {captionMutation.isPending ? (
                  <Loader2
                    className="
                      mr-1.5
                      h-3
                      w-3
                      animate-spin
                    "
                  />
                ) : (
                  <Sparkles
                    className="
                      mr-1.5
                      h-3
                      w-3
                    "
                  />
                )}

                Generate with Gemini
              </Button>
            </div>

            <Textarea
              placeholder="Write your caption here or generate one..."
              className="
                min-h-[120px]
                w-full
                resize-none
              "
              value={caption}
              onChange={(e) =>
                setCaption(
                  e.target.value
                )
              }
              disabled={
                publishMutation.isPending
              }
              required
            />

            {/* ================================= */}
            {/* TRENDING HASHTAGS */}
            {/* ================================= */}

            {trendingHashtags.length >
              0 && (
                <div
                  className="
                  w-full
                  min-w-0
                  max-w-full
                  overflow-hidden
                "
                >
                  <div
                    className="
                    flex
                    w-full
                    min-w-0
                    max-w-full
                    gap-1.5
                    overflow-x-auto
                    overflow-y-hidden
                    pb-2
                  "
                  >
                    {trendingHashtags
                      .slice(0, 15)
                      .map((tag) => (
                        <Badge
                          key={tag.rank}
                          variant="outline"
                          className="
                          shrink-0
                          cursor-pointer
                          whitespace-nowrap
                          bg-muted/30
                          py-0.5
                          text-xs
                          transition-colors
                          hover:bg-primary
                          hover:text-white
                        "
                          onClick={() =>
                            handleHashtagClick(
                              tag.name
                            )
                          }
                          title={
                            tag.stats
                          }
                        >
                          + {tag.name}
                        </Badge>
                      ))}
                  </div>
                </div>
              )}
          </div>

          {/* ================================= */}
          {/* PUBLISH BUTTON */}
          {/* ================================= */}

          <Button
            type="submit"
            className="relative w-full"
            disabled={
              publishMutation.isPending ||
              captionMutation.isPending ||
              !caption.trim()
            }
          >
            {publishMutation.isPending ? (
              <>
                <Loader2
                  className="
                    mr-2
                    h-4
                    w-4
                    animate-spin
                  "
                />

                Robot is publishing...
                Do not close!
              </>
            ) : (
              "Publish Video"
            )}
          </Button>

          {/* ================================= */}
          {/* PLAYWRIGHT UYARISI */}
          {/* ================================= */}

          {publishMutation.isPending && (
            <p
              className="
                mt-2
                animate-pulse
                text-center
                text-[10px]
                text-amber-500
              "
            >
              Playwright browser automation
              is running in the background.
              This may take 1-3 minutes
              depending on network speed.
            </p>
          )}
        </form>
      </DialogContent>
    </Dialog>
  );
}