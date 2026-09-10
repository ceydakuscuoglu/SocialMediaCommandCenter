import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Play, Sparkles, Loader2, Calculator, CheckCircle2, Image as ImageIcon, UploadCloud, Film } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
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
import { prepareJob, confirmJob, PrepareResponse } from "@/api/jobs.api";
import { fetchFruitAssets, fetchReferenceVideos } from "@/api/assets.api"; // YENİ IMPORT

export function NewJobDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  // --- API'den Sistemdeki Kaynakları Çekme ---
  const { data: fruitAssets = [], isLoading: isLoadingAssets } = useQuery({ 
    queryKey: ["fruit-assets"], 
    queryFn: fetchFruitAssets,
    enabled: open // Sadece modal açıldığında çek
  });
  
  const { data: referenceVideos = [], isLoading: isLoadingVideos } = useQuery({ 
    queryKey: ["reference-videos"], 
    queryFn: fetchReferenceVideos,
    enabled: open 
  });

  const [step, setStep] = useState<1 | 2>(1);
  const [costData, setCostData] = useState<PrepareResponse | null>(null);

  const [isRecreate, setIsRecreate] = useState("false");
  const [isMultipleFruits, setIsMultipleFruits] = useState("false");
  const [targetUrl, setTargetUrl] = useState("");
  
  // --- YENİ: Kaynak Seçim Modları ve Seçilen ID'ler ---
  const [imageMode, setImageMode] = useState<"existing" | "upload">("existing");
  const [selectedAssetId, setSelectedAssetId] = useState<string>("");
  const [fruitImage, setFruitImage] = useState<File | null>(null);

  const [videoMode, setVideoMode] = useState<"existing" | "upload">("existing");
  const [selectedVideoId, setSelectedVideoId] = useState<string>("");
  const [referenceVideo, setReferenceVideo] = useState<File | null>(null);

  const [videoTitle, setVideoTitle] = useState("");
  const [fruitTitle, setFruitTitle] = useState("");
  const [danceStyle, setDanceStyle] = useState("");

  const [targetModel, setTargetModel] = useState("VIDEO 2.6");
  const [targetResolution, setTargetResolution] = useState("720p");

  const prepareMutation = useMutation({
    mutationFn: prepareJob,
    onSuccess: (data) => {
      setCostData(data);
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
    setSelectedAssetId("");
    setSelectedVideoId("");
    setImageMode("existing");
    setVideoMode("existing");
    setVideoTitle("");
    setFruitTitle(""); 
    setDanceStyle(""); 
    setTargetModel("VIDEO 2.6"); 
    setTargetResolution("720p"); 
    setOpen(false);
  };

  const handlePrepare = (e: React.FormEvent) => {
    e.preventDefault();

    const formData = new FormData();
    formData.append("IsRecreate", isRecreate);
    formData.append("IsMultipleFruits", isMultipleFruits);
    formData.append("TargetModel", targetModel);
    formData.append("TargetResolution", targetResolution);

    if (videoTitle) formData.append("Title", videoTitle);
    if (fruitTitle) formData.append("FruitTitle", fruitTitle);
    if (danceStyle) formData.append("DanceStyle", danceStyle);

    // --- MEYVE GÖRSELİ MANTIĞI ---
    if (imageMode === "upload") {
      if (!fruitImage) return alert("Please upload a fruit image!");
      formData.append("FruitImage", fruitImage);
    } else {
      if (!selectedAssetId) return alert("Please select an existing fruit asset!");
      formData.append("ExistingFruitAssetId", selectedAssetId);
    }

    // --- REFERANS VİDEO VEYA RECREATE MANTIĞI ---
    if (isRecreate === "true") {
      if (!targetUrl) return alert("Target URL is required for recreate jobs!");
      formData.append("TargetUrl", targetUrl);
    } else {
      if (videoMode === "upload") {
        if (!referenceVideo) return alert("Please upload a reference video!");
        formData.append("ReferenceVideo", referenceVideo);
      } else {
        if (!selectedVideoId) return alert("Please select an existing reference video!");
        formData.append("ExistingReferenceVideoId", selectedVideoId);
      }
    }

    prepareMutation.mutate(formData);
  };

  const handleConfirm = () => {
    if (!costData || !costData.sessionId) {
      alert("Session bilgisi bulunamadı. Lütfen işlemi baştan başlatın.");
      return;
    }
    confirmMutation.mutate({ sessionId: costData.sessionId });
  };

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
                Select assets from your library or upload new source files.
              </DialogDescription>
            </DialogHeader>

            <div className="grid gap-4 py-4 max-h-[60vh] overflow-y-auto px-1">
              
              {/* Video Title */}
              <div className="grid gap-2">
                <Label>Video Title</Label>
                <Input placeholder="e.g. Dancing Strawberry Fiesta" value={videoTitle} onChange={(e) => setVideoTitle(e.target.value)} />
              </div>

              {/* Job Type */}
              <div className="grid gap-2">
                <Label>Job Type</Label>
                <Select value={isRecreate} onValueChange={setIsRecreate}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="false">New Generation</SelectItem>
                    <SelectItem value="true">Recreate from URL</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {/* Character Info */}
              <div className="grid grid-cols-2 gap-4">
                <div className="grid gap-2">
                  <Label>Character Name</Label>
                  <Input placeholder="e.g. Strawberry" value={fruitTitle} onChange={(e) => setFruitTitle(e.target.value)} />
                </div>
                <div className="grid gap-2">
                  <Label>Fruit Quantity</Label>
                  <Select value={isMultipleFruits} onValueChange={setIsMultipleFruits}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="false">Singular</SelectItem>
                      <SelectItem value="true">Plural</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4 mb-2">
                <div className="space-y-2">
                  <Label className="text-sm font-medium">Target Model</Label>
                  <Select value={targetModel} onValueChange={setTargetModel}>
                    <SelectTrigger className="bg-muted/50"><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="VIDEO 2.6">VIDEO 2.6</SelectItem>
                      <SelectItem value="VIDEO 3.0">VIDEO 3.0</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label className="text-sm font-medium">Resolution</Label>
                  <Select value={targetResolution} onValueChange={setTargetResolution}>
                    <SelectTrigger className="bg-muted/50"><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="720p">720p</SelectItem>
                      <SelectItem value="1080p">1080p</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              {/* --- YENİ: FRUIT ASSET SEÇİMİ --- */}
              <div className="grid gap-2 border-t border-border/50 pt-4">
                <Label className="mb-1">Source Fruit Image</Label>
                <Tabs value={imageMode} onValueChange={(val) => setImageMode(val as "existing" | "upload")} className="w-full">
                  <TabsList className="grid w-full grid-cols-2">
                    <TabsTrigger value="existing" className="gap-2"><ImageIcon className="w-4 h-4"/> Library</TabsTrigger>
                    <TabsTrigger value="upload" className="gap-2"><UploadCloud className="w-4 h-4"/> Upload New</TabsTrigger>
                  </TabsList>
                  
                  <TabsContent value="existing" className="pt-2">
                    <Select value={selectedAssetId} onValueChange={setSelectedAssetId} disabled={isLoadingAssets}>
                      <SelectTrigger>
                        <SelectValue placeholder={isLoadingAssets ? "Loading assets..." : "Select from existing assets..."} />
                      </SelectTrigger>
                      <SelectContent>
                        {fruitAssets.map((asset) => (
                          <SelectItem key={asset.id} value={asset.id.toString()}>
                            {asset.title}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </TabsContent>
                  
                  <TabsContent value="upload" className="pt-2">
                    <Input type="file" accept="image/*" onChange={(e) => setFruitImage(e.target.files?.[0] || null)} />
                  </TabsContent>
                </Tabs>
              </div>

              {/* --- YENİ: REFERENCE VIDEO VEYA URL SEÇİMİ --- */}
              {isRecreate === "false" ? (
                <div className="grid gap-2 border-t border-border/50 pt-4 animate-in fade-in zoom-in duration-300">
                  <Label className="mb-1">Reference Dance Video</Label>
                  <Tabs value={videoMode} onValueChange={(val) => setVideoMode(val as "existing" | "upload")} className="w-full">
                    <TabsList className="grid w-full grid-cols-2">
                      <TabsTrigger value="existing" className="gap-2"><Film className="w-4 h-4"/> Library</TabsTrigger>
                      <TabsTrigger value="upload" className="gap-2"><UploadCloud className="w-4 h-4"/> Upload New</TabsTrigger>
                    </TabsList>
                    
                    <TabsContent value="existing" className="pt-2">
                      <Select value={selectedVideoId} onValueChange={setSelectedVideoId} disabled={isLoadingVideos}>
                        <SelectTrigger>
                          <SelectValue placeholder={isLoadingVideos ? "Loading dances..." : "Select from existing dances..."} />
                        </SelectTrigger>
                        <SelectContent>
                          {referenceVideos.map((video) => (
                            <SelectItem key={video.id} value={video.id.toString()}>
                              {video.danceStyle} ({video.sourceType})
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </TabsContent>
                    
                    <TabsContent value="upload" className="pt-2">
                      <Input type="file" accept="video/*" onChange={(e) => setReferenceVideo(e.target.files?.[0] || null)} />
                    </TabsContent>
                  </Tabs>
                </div>
              ) : (
                <div className="grid gap-2 border-t border-border/50 pt-4 animate-in fade-in zoom-in duration-300">
                  <Label>Target Bot URL</Label>
                  <Input placeholder="https://..." value={targetUrl} onChange={(e) => setTargetUrl(e.target.value)} />
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
          /* STEP 2: ONAY EKRANI (Değişiklik Yok) */
          <div className="flex flex-col gap-6 py-4">
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2 text-emerald-500">
                <CheckCircle2 className="w-5 h-5" /> Cost Calculated
              </DialogTitle>
              <DialogDescription>
                Please review the Kling AI credit cost before starting the worker bot.
              </DialogDescription>
            </DialogHeader>

            <div className="bg-muted/30 p-4 rounded-lg border border-border flex flex-col gap-3">
              <p className="text-sm text-muted-foreground">{costData?.message}</p>
              {videoTitle && (
                <div className="flex justify-between items-center border-b border-border/50 pb-2">
                  <span className="text-sm font-medium">Video Title:</span>
                  <span className="text-sm font-semibold text-foreground truncate max-w-[250px]">{videoTitle}</span>
                </div>
              )}
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
              <Button variant="outline" onClick={() => setStep(1)} disabled={confirmMutation.isPending}>Back to Edit</Button>
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