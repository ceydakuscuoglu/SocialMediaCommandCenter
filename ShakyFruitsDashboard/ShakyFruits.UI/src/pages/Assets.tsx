import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { 
  addFruitType, 
  addFruitAsset, 
  addReferenceVideo, 
  ReferenceSourceType 
} from "@/api/assets.api";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Loader2, Apple, Image as ImageIcon, Film } from "lucide-react";

export function Assets() {
  // --- FRUIT TYPE STATE ---
  const [fruitTypeName, setFruitTypeName] = useState("");

  // --- FRUIT ASSET STATE ---
  const [assetTitle, setAssetTitle] = useState("");
  const [assetPath, setAssetPath] = useState("");
  const [isMultiple, setIsMultiple] = useState(false);
  const [selectedTypeIds, setSelectedTypeIds] = useState<string>(""); // Virgülle ayırarak basitçe alacağız

  // --- REFERENCE VIDEO STATE ---
  const [danceStyle, setDanceStyle] = useState("");
  const [sourceType, setSourceType] = useState<ReferenceSourceType>(ReferenceSourceType.LocalUpload);
  const [videoPath, setVideoPath] = useState("");
  const [klingUrl, setKlingUrl] = useState("");

  // --- MUTATIONS ---
  const typeMutation = useMutation({
    mutationFn: addFruitType,
    onSuccess: () => {
      alert("Fruit Type added!");
      setFruitTypeName("");
    }
  });

  const assetMutation = useMutation({
    mutationFn: addFruitAsset,
    onSuccess: () => {
      alert("Fruit Asset added!");
      setAssetTitle(""); setAssetPath(""); setIsMultiple(false); setSelectedTypeIds("");
    }
  });

  const refMutation = useMutation({
    mutationFn: addReferenceVideo,
    onSuccess: () => {
      alert("Reference Video added!");
      setDanceStyle(""); setVideoPath(""); setKlingUrl("");
    }
  });

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      <div>
        <h2 className="text-2xl font-semibold tracking-tight">Asset Library</h2>
        <p className="text-muted-foreground mt-1">
          Manage raw materials for Kling AI generations (Fruit characters, tags, and dance references).
        </p>
      </div>

      <Tabs defaultValue="fruit-types" className="w-full">
        <TabsList className="grid w-full max-w-md grid-cols-3">
          <TabsTrigger value="fruit-types" className="gap-2"><Apple className="w-4 h-4"/> Tags</TabsTrigger>
          <TabsTrigger value="fruit-assets" className="gap-2"><ImageIcon className="w-4 h-4"/> Characters</TabsTrigger>
          <TabsTrigger value="reference-videos" className="gap-2"><Film className="w-4 h-4"/> Dances</TabsTrigger>
        </TabsList>

        {/* 1. FRUIT TYPES FORM */}
        <TabsContent value="fruit-types" className="mt-6">
          <Card className="max-w-xl border-border/50 bg-card/50 shadow-sm">
            <CardHeader>
              <CardTitle>Create Fruit Type (Tag)</CardTitle>
              <CardDescription>Define a new fruit category (e.g., Apple, Strawberry).</CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={(e) => { e.preventDefault(); typeMutation.mutate({ name: fruitTypeName }); }} className="space-y-4">
                <div className="space-y-2">
                  <Label>Fruit Name</Label>
                  <Input placeholder="e.g. Pineapple" value={fruitTypeName} onChange={e => setFruitTypeName(e.target.value)} required />
                </div>
                <Button type="submit" disabled={typeMutation.isPending}>
                  {typeMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : "Save Tag"}
                </Button>
              </form>
            </CardContent>
          </Card>
        </TabsContent>

        {/* 2. FRUIT ASSETS FORM */}
        <TabsContent value="fruit-assets" className="mt-6">
          <Card className="max-w-xl border-border/50 bg-card/50 shadow-sm">
            <CardHeader>
              <CardTitle>Add Fruit Character (Image)</CardTitle>
              <CardDescription>Register a new source image for generation.</CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={(e) => { 
                e.preventDefault(); 
                const ids = selectedTypeIds.split(",").map(id => parseInt(id.trim())).filter(id => !isNaN(id));
                assetMutation.mutate({ title: assetTitle, imagePath: assetPath, isMultipleFruits: isMultiple, fruitTypeIds: ids }); 
              }} className="space-y-4">
                <div className="space-y-2">
                  <Label>Title</Label>
                  <Input placeholder="e.g. Angry Strawberry V1" value={assetTitle} onChange={e => setAssetTitle(e.target.value)} required />
                </div>
                <div className="space-y-2">
                  <Label>Local Image Path</Label>
                  <Input placeholder="C:/assets/strawberry.png" value={assetPath} onChange={e => setAssetPath(e.target.value)} required />
                </div>
                <div className="space-y-2">
                  <Label>Fruit Type IDs (Comma separated)</Label>
                  <Input placeholder="e.g. 1, 4" value={selectedTypeIds} onChange={e => setSelectedTypeIds(e.target.value)} required />
                  <p className="text-xs text-muted-foreground">Tip: In the future, this will be a dropdown fetching from GET /fruit-types.</p>
                </div>
                <div className="flex items-center space-x-2 pt-2">
                  <Checkbox id="multiple" checked={isMultiple} onCheckedChange={(checked) => setIsMultiple(checked as boolean)} />
                  <label htmlFor="multiple" className="text-sm font-medium leading-none">Contains multiple fruits in one image</label>
                </div>
                <Button type="submit" disabled={assetMutation.isPending} className="mt-2">
                  {assetMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : "Save Character"}
                </Button>
              </form>
            </CardContent>
          </Card>
        </TabsContent>

        {/* 3. REFERENCE VIDEOS FORM */}
        <TabsContent value="reference-videos" className="mt-6">
          <Card className="max-w-xl border-border/50 bg-card/50 shadow-sm">
            <CardHeader>
              <CardTitle>Add Dance Reference</CardTitle>
              <CardDescription>Upload a local motion video or link an existing Kling generation.</CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={(e) => { 
                e.preventDefault(); 
                refMutation.mutate({ danceStyle, sourceType, videoPath: sourceType === 0 ? videoPath : undefined, klingSourceUrlOrId: sourceType === 1 ? klingUrl : undefined }); 
              }} className="space-y-4">
                <div className="space-y-2">
                  <Label>Dance Style Name</Label>
                  <Input placeholder="e.g. Salsa Basic Step" value={danceStyle} onChange={e => setDanceStyle(e.target.value)} required />
                </div>
                
                <div className="space-y-2">
                  <Label>Source Type</Label>
                  <select 
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                    value={sourceType} 
                    onChange={e => setSourceType(parseInt(e.target.value))}
                  >
                    <option value={ReferenceSourceType.LocalUpload}>Local Upload (Video Path)</option>
                    <option value={ReferenceSourceType.KlingRecreate}>Kling Recreate (URL / ID)</option>
                  </select>
                </div>

                {sourceType === ReferenceSourceType.LocalUpload ? (
                  <div className="space-y-2">
                    <Label>Local Video Path</Label>
                    <Input placeholder="C:/assets/dances/salsa.mp4" value={videoPath} onChange={e => setVideoPath(e.target.value)} required />
                  </div>
                ) : (
                  <div className="space-y-2">
                    <Label>Kling Source URL / ID</Label>
                    <Input placeholder="https://kling.ai/v/..." value={klingUrl} onChange={e => setKlingUrl(e.target.value)} required />
                  </div>
                )}

                <Button type="submit" disabled={refMutation.isPending} className="mt-2">
                  {refMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : "Save Dance"}
                </Button>
              </form>
            </CardContent>
          </Card>
        </TabsContent>

      </Tabs>
    </div>
  );
}