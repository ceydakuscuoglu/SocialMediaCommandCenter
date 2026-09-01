import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
    addFruitType, addFruitAsset, addReferenceVideo,
    fetchFruitTypes, fetchFruitAssets, fetchReferenceVideos,
    ReferenceSourceType
} from "@/api/assets.api";
import { AddHistoricalVideoModal } from "@/components/assets/AddHistoricalVideoModal";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Loader2, Apple, Image as ImageIcon, Film } from "lucide-react";

export function Assets() {
    const queryClient = useQueryClient();

    // --- QUERIES (Verileri Çekme) ---
    const { data: fruitTypes = [] } = useQuery({ queryKey: ["fruit-types"], queryFn: fetchFruitTypes });
    const { data: fruitAssets = [] } = useQuery({ queryKey: ["fruit-assets"], queryFn: fetchFruitAssets });
    const { data: referenceVideos = [] } = useQuery({ queryKey: ["reference-videos"], queryFn: fetchReferenceVideos });

    // --- STATES ---
    const [fruitTypeName, setFruitTypeName] = useState("");
    const [assetTitle, setAssetTitle] = useState("");
    const [assetPath, setAssetPath] = useState("");
    const [isMultiple, setIsMultiple] = useState(false);
    const [selectedTypeIds, setSelectedTypeIds] = useState<number[]>([]); // Artık string değil, array!
    const [danceStyle, setDanceStyle] = useState("");
    const [sourceType, setSourceType] = useState<ReferenceSourceType>(ReferenceSourceType.LocalUpload);
    const [videoPath, setVideoPath] = useState("");
    const [klingUrl, setKlingUrl] = useState("");

    // --- MUTATIONS (Veri Ekleme ve Listeleri Yenileme) ---
    const typeMutation = useMutation({
        mutationFn: addFruitType,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["fruit-types"] });
            setFruitTypeName("");
        }
    });

    const assetMutation = useMutation({
        mutationFn: addFruitAsset,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["fruit-assets"] });
            setAssetTitle(""); setAssetPath(""); setIsMultiple(false); setSelectedTypeIds([]);
        }
    });

    const refMutation = useMutation({
        mutationFn: addReferenceVideo,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["reference-videos"] });
            setDanceStyle(""); setVideoPath(""); setKlingUrl("");
        }
    });

    // Checkbox seçim mantığı
    const toggleFruitType = (id: number) => {
        setSelectedTypeIds(prev =>
            prev.includes(id) ? prev.filter(tId => tId !== id) : [...prev, id]
        );
    };

    return (
        <div className="space-y-6 animate-in fade-in duration-300">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div>
                    <h2 className="text-2xl font-semibold tracking-tight">Asset Library</h2>
                    <p className="text-muted-foreground mt-1">
                        Manage raw materials for Kling AI generations.
                    </p>
                </div>

                {/* YENİ EKLENEN ARŞİV MODALI */}
                <AddHistoricalVideoModal />
            </div>

            <Tabs defaultValue="fruit-types" className="w-full">
                <TabsList className="grid w-full max-w-md grid-cols-3 mb-6">
                    <TabsTrigger value="fruit-types" className="gap-2"><Apple className="w-4 h-4" /> Tags</TabsTrigger>
                    <TabsTrigger value="fruit-assets" className="gap-2"><ImageIcon className="w-4 h-4" /> Characters</TabsTrigger>
                    <TabsTrigger value="reference-videos" className="gap-2"><Film className="w-4 h-4" /> Dances</TabsTrigger>
                </TabsList>

                {/* 1. FRUIT TYPES (TAGS) SEKME */}
                <TabsContent value="fruit-types">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <Card className="border-border/50 bg-card/50 shadow-sm h-fit">
                            <CardHeader><CardTitle>Create Fruit Tag</CardTitle></CardHeader>
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

                        <Card className="border-border/50 bg-card/50 shadow-sm">
                            <CardHeader><CardTitle>Existing Tags</CardTitle></CardHeader>
                            <CardContent>
                                <div className="flex flex-wrap gap-2">
                                    {fruitTypes.map(tag => (
                                        <span key={tag.id} className="px-3 py-1 bg-primary/10 text-primary rounded-full text-sm font-medium border border-primary/20">
                                            {tag.name}
                                        </span>
                                    ))}
                                    {fruitTypes.length === 0 && <span className="text-muted-foreground text-sm">No tags found.</span>}
                                </div>
                            </CardContent>
                        </Card>
                    </div>
                </TabsContent>

                {/* 2. FRUIT ASSETS (CHARACTERS) SEKME */}
                <TabsContent value="fruit-assets">
                    <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
                        <Card className="lg:col-span-4 border-border/50 bg-card/50 shadow-sm h-fit">
                            <CardHeader><CardTitle>Add Character Image</CardTitle></CardHeader>
                            <CardContent>
                                <form onSubmit={(e) => { e.preventDefault(); assetMutation.mutate({ title: assetTitle, imagePath: assetPath, isMultipleFruits: isMultiple, fruitTypeIds: selectedTypeIds }); }} className="space-y-4">
                                    <div className="space-y-2">
                                        <Label>Title</Label>
                                        <Input placeholder="e.g. Angry Strawberry V1" value={assetTitle} onChange={e => setAssetTitle(e.target.value)} required />
                                    </div>
                                    <div className="space-y-2">
                                        <Label>Local Image Path</Label>
                                        <Input placeholder="C:/assets/strawberry.png" value={assetPath} onChange={e => setAssetPath(e.target.value)} required />
                                    </div>

                                    {/* Dinamik Checkbox Alanı (API'den geliyor) */}
                                    <div className="space-y-3 pt-2">
                                        <Label>Included Fruits (Select tags)</Label>
                                        <div className="grid grid-cols-2 gap-2 max-h-32 overflow-y-auto p-2 border border-border/50 rounded-md bg-background/50">
                                            {fruitTypes.map(tag => (
                                                <div key={tag.id} className="flex items-center space-x-2">
                                                    <Checkbox id={`tag-${tag.id}`} checked={selectedTypeIds.includes(tag.id)} onCheckedChange={() => toggleFruitType(tag.id)} />
                                                    <label htmlFor={`tag-${tag.id}`} className="text-sm cursor-pointer">{tag.name}</label>
                                                </div>
                                            ))}
                                        </div>
                                    </div>

                                    <div className="flex items-center space-x-2 pt-2">
                                        <Checkbox id="multiple" checked={isMultiple} onCheckedChange={(checked) => setIsMultiple(checked as boolean)} />
                                        <label htmlFor="multiple" className="text-sm font-medium text-amber-500">Contains multiple fruits</label>
                                    </div>
                                    <Button type="submit" disabled={assetMutation.isPending} className="w-full">
                                        {assetMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : "Save Character"}
                                    </Button>
                                </form>
                            </CardContent>
                        </Card>

                        <Card className="lg:col-span-8 border-border/50 bg-card/50 shadow-sm overflow-hidden">
                            <Table>
                                <TableHeader className="bg-muted/30">
                                    <TableRow>
                                        <TableHead>Title</TableHead>
                                        <TableHead>Path</TableHead>
                                        <TableHead>Tags</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {fruitAssets.map(asset => (
                                        <TableRow key={asset.id}>
                                            <TableCell className="font-medium">{asset.title}</TableCell>
                                            <TableCell className="text-xs text-muted-foreground truncate max-w-[150px]">{asset.imagePath}</TableCell>
                                            <TableCell>
                                                <div className="flex flex-wrap gap-1">
                                                    {asset.fruits.map(f => (
                                                        <span key={f.id} className="text-[10px] bg-secondary text-secondary-foreground px-2 py-0.5 rounded">{f.name}</span>
                                                    ))}
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </Card>
                    </div>
                </TabsContent>

                {/* 3. REFERENCE VIDEOS (DANCES) SEKME */}
                <TabsContent value="reference-videos">
                    <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
                        <Card className="lg:col-span-4 border-border/50 bg-card/50 shadow-sm h-fit">
                            <CardHeader><CardTitle>Add Dance Reference</CardTitle></CardHeader>
                            <CardContent>
                                <form onSubmit={(e) => { e.preventDefault(); refMutation.mutate({ danceStyle, sourceType, videoPath: sourceType === 0 ? videoPath : undefined, klingSourceUrlOrId: sourceType === 1 ? klingUrl : undefined }); }} className="space-y-4">
                                    <div className="space-y-2">
                                        <Label>Dance Style Name</Label>
                                        <Input placeholder="e.g. Salsa Basic Step" value={danceStyle} onChange={e => setDanceStyle(e.target.value)} required />
                                    </div>
                                    <div className="space-y-2">
                                        <Label>Source Type</Label>
                                        <select className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" value={sourceType} onChange={e => setSourceType(parseInt(e.target.value))}>
                                            <option value={ReferenceSourceType.LocalUpload}>Local Upload</option>
                                            <option value={ReferenceSourceType.KlingRecreate}>Kling Recreate</option>
                                        </select>
                                    </div>
                                    {sourceType === ReferenceSourceType.LocalUpload ? (
                                        <div className="space-y-2">
                                            <Label>Local Video Path</Label>
                                            <Input placeholder="C:/assets/dances/salsa.mp4" value={videoPath} onChange={e => setVideoPath(e.target.value)} required />
                                        </div>
                                    ) : (
                                        <div className="space-y-2">
                                            <Label>Kling URL / ID</Label>
                                            <Input placeholder="https://kling.ai/v/..." value={klingUrl} onChange={e => setKlingUrl(e.target.value)} required />
                                        </div>
                                    )}
                                    <Button type="submit" disabled={refMutation.isPending} className="w-full">
                                        {refMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : "Save Dance"}
                                    </Button>
                                </form>
                            </CardContent>
                        </Card>

                        <Card className="lg:col-span-8 border-border/50 bg-card/50 shadow-sm overflow-hidden">
                            <Table>
                                <TableHeader className="bg-muted/30">
                                    <TableRow>
                                        <TableHead>Dance Style</TableHead>
                                        <TableHead>Type</TableHead>
                                        <TableHead>Source Target</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {referenceVideos.map(vid => (
                                        <TableRow key={vid.id}>
                                            <TableCell className="font-medium">{vid.danceStyle}</TableCell>
                                            <TableCell><span className="text-xs bg-indigo-500/10 text-indigo-500 px-2 py-1 rounded">{vid.sourceType}</span></TableCell>
                                            <TableCell className="text-xs text-muted-foreground">{vid.videoPath || vid.klingSourceUrlOrId}</TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </Card>
                    </div>
                </TabsContent>

            </Tabs>
        </div>
    );
}