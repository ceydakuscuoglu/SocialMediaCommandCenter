import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { addHistoricalVideo, fetchFruitAssets, fetchReferenceVideos } from "@/api/assets.api";
import {
    Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
    Select, SelectContent, SelectItem, SelectTrigger, SelectValue
} from "@/components/ui/select";
import { Loader2, History, Link as LinkIcon } from "lucide-react";

export function AddHistoricalVideoModal() {
    const [open, setOpen] = useState(false);
    const queryClient = useQueryClient();

    const { data: fruits = [] } = useQuery({ queryKey: ["fruit-assets"], queryFn: fetchFruitAssets });
    const { data: references = [] } = useQuery({ queryKey: ["reference-videos"], queryFn: fetchReferenceVideos });

    // Form State (prompt silindi)
    const [fruitId, setFruitId] = useState("");
    const [refId, setRefId] = useState("none");
    const [videoPath, setVideoPath] = useState("");
    const [isPublished, setIsPublished] = useState(false);
    const [postUrl, setPostUrl] = useState("");
    const [publishedDate, setPublishedDate] = useState("");

    const historicalMutation = useMutation({
        mutationFn: addHistoricalVideo,
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ["jobs"] });
            queryClient.invalidateQueries({ queryKey: ["published-videos-latest"] });
            setOpen(false);
            resetForm();
            // YENİ: Backend'in otomatik ürettiği prompt'u konsolda veya alertte görebilirsin
            console.log("Sisteme eklenen video:", data);
        },
        onError: (err) => alert(err.message)
    });

    const resetForm = () => {
        setFruitId(""); setRefId("none"); setVideoPath("");
        setIsPublished(false); setPostUrl(""); setPublishedDate("");
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!fruitId) return alert("Lütfen bir meyve karakteri (Fruit Asset) seçin.");
        if (!videoPath) return alert("Lütfen bilgisayarınızdaki video yolunu girin.");
        if (isPublished && !postUrl) return alert("Yayınlanmış videolar için TikTok linki zorunludur.");

        historicalMutation.mutate({
            fruitAssetId: parseInt(fruitId),
            referenceVideoId: refId !== "none" ? parseInt(refId) : null,
            outputVideoPath: videoPath,
            // appliedPrompt payload'dan çıkarıldı, backend hallediyor.
            isPublished,
            postUrl: isPublished ? postUrl : undefined,
            publishedAt: isPublished && publishedDate ? new Date(publishedDate).toISOString() : null
        });
    };

    return (
        <Dialog open={open} onOpenChange={(isOpen) => { setOpen(isOpen); if (!isOpen) resetForm(); }}>
            <DialogTrigger asChild>
                <Button variant="secondary" className="gap-2 shadow-sm border border-border/50">
                    <History className="w-4 h-4" />
                    Import Legacy Video
                </Button>
            </DialogTrigger>

            <DialogContent className="sm:max-w-[500px] bg-card text-foreground max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Import Legacy Video</DialogTitle>
                    <DialogDescription>
                        Bypass the generation queue and directly register an existing video to the system.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="space-y-5 mt-4">

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <Label>Source Fruit Character <span className="text-destructive">*</span></Label>
                            <Select value={fruitId} onValueChange={setFruitId}>
                                <SelectTrigger><SelectValue placeholder="Select fruit..." /></SelectTrigger>
                                <SelectContent>
                                    {fruits.map(f => <SelectItem key={f.id} value={f.id.toString()}>{f.title}</SelectItem>)}
                                </SelectContent>
                            </Select>
                        </div>
                        <div className="space-y-2">
                            <Label>Dance Reference (Optional)</Label>
                            <Select value={refId} onValueChange={setRefId}>
                                <SelectTrigger><SelectValue placeholder="None" /></SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="none">None (Unknown)</SelectItem>
                                    {references.map(r => <SelectItem key={r.id} value={r.id.toString()}>{r.danceStyle}</SelectItem>)}
                                </SelectContent>
                            </Select>
                        </div>
                    </div>

                    <div className="space-y-2">
                        <Label>Local Video Path <span className="text-destructive">*</span></Label>
                        <Input
                            placeholder="D:\ShakyFruits\Outputs\viral_dans_1.mp4"
                            value={videoPath} onChange={e => setVideoPath(e.target.value)}
                        />
                    </div>

                    {/* Applied Prompt input alanı tamamen silindi */}

                    <div className="border-t border-border/50 pt-4 mt-2">
                        <div className="flex items-center space-x-2 mb-4">
                            <Checkbox id="isPublished" checked={isPublished} onCheckedChange={(c) => setIsPublished(c as boolean)} />
                            <label htmlFor="isPublished" className="text-sm font-medium leading-none cursor-pointer">
                                This video is already published on TikTok
                            </label>
                        </div>

                        {isPublished && (
                            <div className="space-y-4 animate-in fade-in slide-in-from-top-2 duration-300 bg-muted/20 p-4 rounded-lg border border-border/50">
                                <div className="space-y-2">
                                    <Label>TikTok Post URL <span className="text-destructive">*</span></Label>
                                    <div className="relative">
                                        <LinkIcon className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
                                        <Input
                                            placeholder="https://www.tiktok.com/@..."
                                            className="pl-9"
                                            value={postUrl} onChange={e => setPostUrl(e.target.value)}
                                        />
                                    </div>
                                    <p className="text-[10px] text-muted-foreground">Scraper will immediately start tracking this URL.</p>
                                </div>

                                <div className="space-y-2">
                                    <Label>Publish Date (Optional)</Label>
                                    <Input
                                        type="datetime-local"
                                        value={publishedDate} onChange={e => setPublishedDate(e.target.value)}
                                    />
                                </div>
                            </div>
                        )}
                    </div>

                    <Button type="submit" className="w-full" disabled={historicalMutation.isPending}>
                        {historicalMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : "Save Legacy Video"}
                    </Button>
                </form>
            </DialogContent>
        </Dialog>
    );
}