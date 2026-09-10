import { useState, useEffect } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateJob, UpdateHistoricalVideoRequest } from "@/api/jobs.api";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Loader2, Pencil } from "lucide-react";

interface UpdateJobModalProps {
    isOpen: boolean;
    onClose: () => void;
    job: any;
}

export function UpdateJobModal({ isOpen, onClose, job }: UpdateJobModalProps) {
    const queryClient = useQueryClient();
    
    // Varsayılan platform değerini 1 (TikTok) yaptık
    const [formData, setFormData] = useState<UpdateHistoricalVideoRequest>({
        title: "",
        fruitAssetId: 0,
        referenceVideoId: undefined,
        isRecreate: false,
        outputVideoPath: "",
        aiGeneratedCaption: "",
        isPublished: false,
        platform: 1, 
        postUrl: "",
    });

    useEffect(() => {
        if (isOpen && job) {
            console.log("✏️ Modal İçin Gelen Job Verisi:", job);

            setFormData({
                title: job.title ?? job.Title ?? "",
                fruitAssetId: job.fruitAssetId ?? job.FruitAssetId ?? 0,
                referenceVideoId: job.referenceVideoId ?? job.ReferenceVideoId,
                isRecreate: job.isRecreate ?? job.IsRecreate ?? false,
                outputVideoPath: job.outputVideoPath ?? job.OutputVideoPath ?? "",
                aiGeneratedCaption: job.aiGeneratedCaption ?? job.AiGeneratedCaption ?? "",
                isPublished: job.isPublished ?? job.IsPublished ?? false,
                platform: job.platform ?? job.Platform ?? 1, // Fallback olarak 1
                postUrl: job.postUrl ?? job.PostUrl ?? "",
            });
        }
    }, [isOpen, job]);

    const updateMutation = useMutation({
        mutationFn: updateJob,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["pipeline-jobs"] });
            onClose();
        },
        onError: (err) => alert(err.message)
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!job) return;

        // KESİN ÇÖZÜM: C#'ın 400 hatası atmaması için boş string'leri undefined'a çeviriyoruz
        const payload = {
            title: formData.title || undefined,
            fruitAssetId: formData.fruitAssetId,
            referenceVideoId: formData.referenceVideoId,
            isRecreate: formData.isRecreate,
            outputVideoPath: formData.outputVideoPath || undefined,
            aiGeneratedCaption: formData.aiGeneratedCaption || undefined,
            isPublished: formData.isPublished,
            platform: formData.platform,
            postUrl: formData.postUrl || undefined,
        };

        updateMutation.mutate({ id: job.id, data: payload });
    };

    return (
        <Dialog open={isOpen} onOpenChange={(open) => !updateMutation.isPending && !open && onClose()}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        <Pencil className="w-5 h-5 text-primary" />
                        Edit Job #{job?.id}
                    </DialogTitle>
                    <DialogDescription>
                        Update raw generation paths, caption, and publishing status manually.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="space-y-4 mt-2">
                    <div className="space-y-2">
                        <Label>Video Title</Label>
                        <Input
                            value={formData.title}
                            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <Label>Fruit Asset ID</Label>
                            <Input
                                type="number"
                                value={formData.fruitAssetId}
                                onChange={(e) => setFormData({ ...formData, fruitAssetId: parseInt(e.target.value) })}
                                required
                            />
                        </div>
                        <div className="space-y-2">
                            <Label>Ref Video ID (Optional)</Label>
                            <Input
                                type="number"
                                value={formData.referenceVideoId || ""}
                                onChange={(e) => setFormData({ ...formData, referenceVideoId: parseInt(e.target.value) || undefined })}
                            />
                        </div>
                    </div>

                    <div className="space-y-2">
                        <Label>Output Video Path</Label>
                        <Input
                            value={formData.outputVideoPath}
                            onChange={(e) => setFormData({ ...formData, outputVideoPath: e.target.value })}
                        />
                    </div>

                    <div className="space-y-2">
                        <Label>AI Generated Caption</Label>
                        <Textarea
                            className="resize-none h-20"
                            value={formData.aiGeneratedCaption}
                            onChange={(e) => setFormData({ ...formData, aiGeneratedCaption: e.target.value })}
                        />
                    </div>

                    <div className="p-4 border border-border/50 rounded-md bg-muted/20 space-y-4">
                        <div className="flex items-center justify-between">
                            <div className="space-y-0.5">
                                <Label>Published Status</Label>
                                <p className="text-[11px] text-muted-foreground">Sync this video's social media status.</p>
                            </div>
                            <Switch
                                checked={formData.isPublished}
                                onCheckedChange={(checked: any) => setFormData({ ...formData, isPublished: checked })}
                            />
                        </div>

                        {formData.isPublished && (
                            <div className="grid grid-cols-2 gap-4 pt-2 border-t border-border/50">
                                <div className="space-y-2">
                                    <Label>Platform</Label>
                                    <Select
                                        value={formData.platform.toString()}
                                        onValueChange={(val) => setFormData({ ...formData, platform: parseInt(val) })}
                                    >
                                        <SelectTrigger><SelectValue /></SelectTrigger>
                                        <SelectContent>
                                            {/* ENUM DEĞERLERİ 1 VE 2 OLARAK GÜNCELLENDİ */}
                                            <SelectItem value="1">TikTok</SelectItem>
                                            <SelectItem value="2">Instagram</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>
                                <div className="space-y-2">
                                    <Label>Post URL</Label>
                                    <Input
                                        value={formData.postUrl}
                                        onChange={(e) => setFormData({ ...formData, postUrl: e.target.value })}
                                    />
                                </div>
                            </div>
                        )}
                    </div>

                    <Button type="submit" className="w-full" disabled={updateMutation.isPending}>
                        {updateMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : "Save Changes"}
                    </Button>
                </form>
            </DialogContent>
        </Dialog>
    );
}