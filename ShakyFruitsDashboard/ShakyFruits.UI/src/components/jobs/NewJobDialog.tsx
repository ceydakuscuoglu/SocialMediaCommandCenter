import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Play, Sparkles, Loader2, Calculator, CheckCircle2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { prepareJob, confirmJob, PrepareResponse, ConfirmPayload } from "@/api/jobs.api";

export function NewJobDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const [step, setStep] = useState<1 | 2>(1);
  const [costData, setCostData] = useState<PrepareResponse | null>(null);

  // Form Verileri (Hazırlık aşaması için hala gerekliler)
  const [isRecreate, setIsRecreate] = useState("false");
  const [isMultipleFruits, setIsMultipleFruits] = useState("false");
  const [targetUrl, setTargetUrl] = useState("");
  const [fruitImage, setFruitImage] = useState<File | null>(null);
  const [referenceVideo, setReferenceVideo] = useState<File | null>(null);

  const [fruitTitle, setFruitTitle] = useState("");
  const [danceStyle, setDanceStyle] = useState("");

  const [targetModel, setTargetModel] = useState("VIDEO 2.6");
  const [targetResolution, setTargetResolution] = useState("720p");

  const prepareMutation = useMutation({
    mutationFn: prepareJob,
    onSuccess: (data) => {
      setCostData(data); // data artık sessionId içeriyor
      setStep(2);
    },
    onError: (err) => {
      alert(err.message);
    }
  });

  const confirmMutation = useMutation({
    mutationFn: confirmJob,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["jobs"] });
      resetForm();
    },
    onError: (err) => {
      alert(err.message);
    }
  });

  const resetForm = () => {
    setStep(1);
    setCostData(null);
    setIsRecreate("false");
    setIsMultipleFruits("false");
    setTargetUrl("");
    setFruitImage(null);
    setReferenceVideo(null);
    setFruitTitle(""); 
    setDanceStyle(""); 
    setTargetModel("VIDEO 2.6"); 
    setTargetResolution("720p"); 
    setOpen(false);
  };

  const handlePrepare = (e: React.FormEvent) => {
    e.preventDefault();
    if (!fruitImage) return alert("Fruit Image is required!");
    if (isRecreate === "false" && !referenceVideo) return alert("Reference Video is required for new jobs!");
    if (isRecreate === "true" && !targetUrl) return alert("Target URL is required for recreate jobs!");

    // Backend'in Cache'ine (Session) yazması için tüm verileri ilk aşamada gönderiyoruz
    const formData = new FormData();
    formData.append("FruitImage", fruitImage);
    formData.append("IsRecreate", isRecreate);
    formData.append("IsMultipleFruits", isMultipleFruits);
    formData.append("TargetModel", targetModel);
    formData.append("TargetResolution", targetResolution);

    if (fruitTitle) formData.append("FruitTitle", fruitTitle);
    if (danceStyle) formData.append("DanceStyle", danceStyle);

    if (isRecreate === "true") {
      formData.append("TargetUrl", targetUrl);
    } else {
      formData.append("ReferenceVideo", referenceVideo!);
    }

    prepareMutation.mutate(formData);
  };

  // --- GÜNCELLENEN KISIM: Sadece SessionId gönderiliyor ---
  const handleConfirm = () => {
    if (!costData || !costData.sessionId) {
      alert("Session bilgisi bulunamadı. Lütfen işlemi baştan başlatın.");
      return;
    }

    const payload: ConfirmPayload = {
      sessionId: costData.sessionId // Tüm form verileri backend RAM'inde olduğu için sadece anahtarı yolluyoruz
    };

    confirmMutation.mutate(payload);
  };
  // ---------------------------------------------------------

  return (
    <Dialog open={open} onOpenChange={(isOpen) => {
      if (!isOpen) resetForm();
      setOpen(isOpen);
    }}>
      <DialogTrigger asChild>
        <Button className="gap-2 shadow-md">
          <Play className="w-4 h-4 fill-current" />
          New Generation Job
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-[500px]">
        {step === 1 ? (
          <form onSubmit={handlePrepare}>
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <Sparkles className="w-5 h-5 text-primary" />
                Prepare Video Task
              </DialogTitle>
              <DialogDescription>
                Upload source files and define the character concepts.
              </DialogDescription>
            </DialogHeader>

            <div className="grid gap-4 py-4 max-h-[60vh] overflow-y-auto px-1">
              <div className="grid gap-2">
                <Label>Job Type</Label>
                <Select value={isRecreate} onValueChange={setIsRecreate}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="false">New Job (Upload Video & Image)</SelectItem>
                    <SelectItem value="true">Recreate (Upload Image & Provide URL)</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="grid gap-2">
                  <Label>Character Name</Label>
                  <Input
                    placeholder="e.g. Strawberry"
                    value={fruitTitle}
                    onChange={(e) => setFruitTitle(e.target.value)}
                  />
                </div>
                <div className="grid gap-2">
                  <Label>Fruit Quantity</Label>
                  <Select value={isMultipleFruits} onValueChange={setIsMultipleFruits}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="false">Singular</SelectItem>
                      <SelectItem value="true">Plural</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              {isRecreate === "false" && (
                <div className="grid gap-2 animate-in fade-in zoom-in duration-300">
                  <Label>Trend / Concept</Label>
                  <Input
                    placeholder="e.g. Hip-Hop Bounce"
                    value={danceStyle}
                    onChange={(e) => setDanceStyle(e.target.value)}
                  />
                </div>
              )}

              <div className="grid grid-cols-2 gap-4 mb-4">
                <div className="space-y-2">
                  <Label className="text-sm font-medium">Target Model</Label>
                  <Select value={targetModel} onValueChange={setTargetModel}>
                    <SelectTrigger className="bg-muted/50 border-0">
                      <SelectValue placeholder="Select model" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="VIDEO 2.6">VIDEO 2.6</SelectItem>
                      <SelectItem value="VIDEO 3.0">VIDEO 3.0</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label className="text-sm font-medium">Resolution</Label>
                  <Select value={targetResolution} onValueChange={setTargetResolution}>
                    <SelectTrigger className="bg-muted/50 border-0">
                      <SelectValue placeholder="Select resolution" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="720p">720p</SelectItem>
                      <SelectItem value="1080p">1080p</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="grid gap-2 mt-2 border-t border-border/50 pt-4">
                <Label>Fruit Image (Required)</Label>
                <Input
                  type="file"
                  accept="image/*"
                  onChange={(e) => setFruitImage(e.target.files?.[0] || null)}
                />
              </div>

              {isRecreate === "false" ? (
                <div className="grid gap-2 animate-in fade-in zoom-in duration-300">
                  <Label>Reference Video (Required)</Label>
                  <Input
                    type="file"
                    accept="video/*"
                    onChange={(e) => setReferenceVideo(e.target.files?.[0] || null)}
                  />
                </div>
              ) : (
                <div className="grid gap-2 animate-in fade-in zoom-in duration-300">
                  <Label>Target Bot URL</Label>
                  <Input
                    placeholder="https://..."
                    value={targetUrl}
                    onChange={(e) => setTargetUrl(e.target.value)}
                  />
                </div>
              )}
            </div>

            <DialogFooter>
              <Button type="submit" className="w-full gap-2" disabled={prepareMutation.isPending}>
                {prepareMutation.isPending ? (
                  <><Loader2 className="w-4 h-4 animate-spin" /> Calculating Cost...</>
                ) : (
                  <><Calculator className="w-4 h-4" /> Calculate Cost</>
                )}
              </Button>
            </DialogFooter>
          </form>
        ) : (
          <div className="flex flex-col gap-6 py-4">
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2 text-emerald-500">
                <CheckCircle2 className="w-5 h-5" />
                Cost Calculated
              </DialogTitle>
              <DialogDescription>
                Please review the Kling AI credit cost before starting the worker bot.
              </DialogDescription>
            </DialogHeader>

            <div className="bg-muted/30 p-4 rounded-lg border border-border flex flex-col gap-3">
              <p className="text-sm text-muted-foreground">{costData?.message}</p>
              <div className="flex justify-between items-center border-b border-border/50 pb-2">
                <span className="font-medium">Calculated Cost:</span>
                <span className="text-xl font-bold text-primary">{costData?.calculatedCredits} Credits</span>
              </div>
              <div className="flex justify-between items-center pt-1">
                <span className="text-sm font-medium">Video Duration:</span>
                <span className="text-sm">{costData?.videoDurationSeconds.toFixed(1)} seconds</span>
              </div>
            </div>

            <DialogFooter className="gap-2 sm:gap-0">
              <Button variant="outline" onClick={() => setStep(1)} disabled={confirmMutation.isPending}>
                Back to Edit
              </Button>
              <Button onClick={handleConfirm} className="gap-2" disabled={confirmMutation.isPending}>
                {confirmMutation.isPending ? (
                  <><Loader2 className="w-4 h-4 animate-spin" /> Starting Bot...</>
                ) : (
                  <><Play className="w-4 h-4 fill-current" /> Confirm & Start Job</>
                )}
              </Button>
            </DialogFooter>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}