import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
    addFruitType, updateFruitType,
    addFruitAsset, updateFruitAsset,
    addReferenceVideo, updateReferenceVideo,
    fetchFruitTypes, fetchFruitAssets, fetchReferenceVideos,
    ReferenceSourceType, FruitAsset, ReferenceVideo, FruitType
} from "@/api/assets.api";
import { AddHistoricalVideoModal } from "@/components/assets/AddHistoricalVideoModal";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Loader2, Apple, Image as ImageIcon, Film, Edit2, X } from "lucide-react";

export function Assets() {
    const queryClient = useQueryClient();

    const { data: fruitTypes = [] } = useQuery({ queryKey: ["fruit-types"], queryFn: fetchFruitTypes });
    const { data: fruitAssets = [] } = useQuery({ queryKey: ["fruit-assets"], queryFn: fetchFruitAssets });
    const { data: referenceVideos = [] } = useQuery({ queryKey: ["reference-videos"], queryFn: fetchReferenceVideos });

    // --- EDIT STATES (Hangi ID düzenleniyor?) ---
    const [editingTypeId, setEditingTypeId] = useState<number | null>(null);
    const [editingAssetId, setEditingAssetId] = useState<number | null>(null);
    const [editingRefId, setEditingRefId] = useState<number | null>(null);

    // --- FORM STATES ---
    const [fruitTypeName, setFruitTypeName] = useState("");
    const [assetTitle, setAssetTitle] = useState("");
    const [assetPath, setAssetPath] = useState("");
    const [isMultiple, setIsMultiple] = useState(false);
    const [selectedTypeIds, setSelectedTypeIds] = useState<number[]>([]);
    const [danceStyle, setDanceStyle] = useState("");
    const [sourceType, setSourceType] = useState<ReferenceSourceType>(ReferenceSourceType.LocalUpload);
    const [videoPath, setVideoPath] = useState("");
    const [klingUrl, setKlingUrl] = useState("");

    // --- RESET FONKSİYONLARI ---
    const resetTypeForm = () => { setEditingTypeId(null); setFruitTypeName(""); };
    const resetAssetForm = () => { setEditingAssetId(null); setAssetTitle(""); setAssetPath(""); setIsMultiple(false); setSelectedTypeIds([]); };
    const resetRefForm = () => { setEditingRefId(null); setDanceStyle(""); setSourceType(ReferenceSourceType.LocalUpload); setVideoPath(""); setKlingUrl(""); };

    // --- MUTATIONS ---
    const typeMutation = useMutation({
        mutationFn: async (args: { id?: number | null; data: any }) => {
            return args.id ? updateFruitType({ id: args.id, data: args.data }) : addFruitType(args.data);
        },
        onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["fruit-types"] }); resetTypeForm(); }
    });

    const assetMutation = useMutation({
        mutationFn: async (args: { id?: number | null; data: any }) => {
            return args.id ? updateFruitAsset({ id: args.id, data: args.data }) : addFruitAsset(args.data);
        },
        onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["fruit-assets"] }); resetAssetForm(); }
    });

    const refMutation = useMutation({
        mutationFn: async (args: { id?: number | null; data: any }) => {
            return args.id ? updateReferenceVideo({ id: args.id, data: args.data }) : addReferenceVideo(args.data);
        },
        onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["reference-videos"] }); resetRefForm(); }
    });

    // --- DÜZENLEME BAŞLATICI FONKSİYONLAR ---
    const handleEditType = (tag: FruitType) => {
        setEditingTypeId(tag.id);
        setFruitTypeName(tag.name);
    };

    const handleEditAsset = (asset: FruitAsset) => {
        setEditingAssetId(asset.id);
        setAssetTitle(asset.title);
        setAssetPath(asset.imagePath);
        setIsMultiple(asset.isMultipleFruits);
        setSelectedTypeIds(asset.fruits.map(f => f.id));
    };

    const handleEditRef = (vid: ReferenceVideo) => {
        setEditingRefId(vid.id);
        setDanceStyle(vid.danceStyle);
        setSourceType(vid.sourceType === "LocalUpload" ? 0 : 1);
        setVideoPath(vid.videoPath || "");
        setKlingUrl(vid.klingSourceUrlOrId || "");
    };

    const toggleFruitType = (id: number) => {
        setSelectedTypeIds(prev => prev.includes(id) ? prev.filter(tId => tId !== id) : [...prev, id]);
    };

    return (
        <div className="space-y-6 animate-in fade-in duration-300">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div>
                    <h2 className="text-2xl font-semibold tracking-tight">Asset Library</h2>
                    <p className="text-muted-foreground mt-1">Manage raw materials for Kling AI generations.</p>
                </div>
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
                    {/* YENİ: 2 kolon yerine 12 kolonluk grid sistemi kullanıldı */}
                    <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">

                        {/* Form Kartı: 12 kolonun sadece 4'ünü (1/3) kaplayacak */}
                        <Card className={`lg:col-span-4 border-border/50 bg-card/50 shadow-sm h-fit ${editingTypeId ? 'border-primary/50 shadow-primary/10' : ''}`}>
                            <CardHeader className="flex flex-row items-center justify-between">
                                <CardTitle>{editingTypeId ? "Edit Fruit Tag" : "Create Fruit Tag"}</CardTitle>
                                {editingTypeId && <Button variant="ghost" size="sm" onClick={resetTypeForm}><X className="w-4 h-4 mr-1" /> Cancel</Button>}
                            </CardHeader>
                            <CardContent>
                                <form onSubmit={(e) => {
                                    e.preventDefault();
                                    typeMutation.mutate({ id: editingTypeId, data: { name: fruitTypeName } });
                                }} className="space-y-4">
                                    <div className="space-y-2">
                                        <Label>Fruit Name</Label>
                                        <Input placeholder="e.g. Pineapple" value={fruitTypeName} onChange={e => setFruitTypeName(e.target.value)} required />
                                    </div>
                                    <Button type="submit" disabled={typeMutation.isPending} className="w-full">
                                        {typeMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : (editingTypeId ? "Update Tag" : "Save Tag")}
                                    </Button>
                                </form>
                            </CardContent>
                        </Card>

                        {/* Listeleme Kartı: Kalan 8 kolonu (2/3) kaplayacak */}
                        <Card className="lg:col-span-8 border-border/50 bg-card/50 shadow-sm h-fit">
                            <CardHeader><CardTitle>Existing Tags</CardTitle></CardHeader>
                            <CardContent>
                                <div className="flex flex-wrap gap-2">
                                    {fruitTypes.map(tag => (
                                        <div key={tag.id} className="flex items-center px-3 py-1 bg-primary/10 text-primary rounded-full text-sm font-medium border border-primary/20 group">
                                            {tag.name}
                                            <button type="button" onClick={() => handleEditType(tag)} className="ml-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                                <Edit2 className="w-3.5 h-3.5 hover:text-primary/70" />
                                            </button>
                                        </div>
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
                        <Card className={`lg:col-span-4 border-border/50 bg-card/50 shadow-sm h-fit ${editingAssetId ? 'border-primary/50 shadow-primary/10' : ''}`}>
                            <CardHeader className="flex flex-row items-center justify-between">
                                <CardTitle>{editingAssetId ? "Edit Character" : "Add Character Image"}</CardTitle>
                                {editingAssetId && <Button variant="ghost" size="sm" onClick={resetAssetForm}><X className="w-4 h-4 mr-1" /> Cancel</Button>}
                            </CardHeader>
                            <CardContent>
                                <form onSubmit={(e) => {
                                    e.preventDefault();
                                    assetMutation.mutate({
                                        id: editingAssetId,
                                        data: { title: assetTitle, imagePath: assetPath, isMultipleFruits: isMultiple, fruitTypeIds: selectedTypeIds }
                                    });
                                }} className="space-y-4">
                                    <div className="space-y-2">
                                        <Label>Title</Label>
                                        <Input placeholder="e.g. Angry Strawberry V1" value={assetTitle} onChange={e => setAssetTitle(e.target.value)} required />
                                    </div>
                                    <div className="space-y-2">
                                        <Label>Local Image Path</Label>
                                        <Input placeholder="C:/assets/strawberry.png" value={assetPath} onChange={e => setAssetPath(e.target.value)} required />
                                    </div>

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
                                        {assetMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : (editingAssetId ? "Update Character" : "Save Character")}
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
                                        <TableHead className="text-right">Actions</TableHead>
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
                                            <TableCell className="text-right">
                                                <Button variant="ghost" size="sm" className="h-8 px-2" onClick={() => handleEditAsset(asset)}>
                                                    <Edit2 className="w-4 h-4 text-muted-foreground hover:text-primary" />
                                                </Button>
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
                        <Card className={`lg:col-span-4 border-border/50 bg-card/50 shadow-sm h-fit ${editingRefId ? 'border-primary/50 shadow-primary/10' : ''}`}>
                            <CardHeader className="flex flex-row items-center justify-between">
                                <CardTitle>{editingRefId ? "Edit Dance Reference" : "Add Dance Reference"}</CardTitle>
                                {editingRefId && <Button variant="ghost" size="sm" onClick={resetRefForm}><X className="w-4 h-4 mr-1" /> Cancel</Button>}
                            </CardHeader>
                            <CardContent>
                                <form onSubmit={(e) => {
                                    e.preventDefault();
                                    refMutation.mutate({
                                        id: editingRefId,
                                        data: { danceStyle, sourceType, videoPath: sourceType === 0 ? videoPath : undefined, klingSourceUrlOrId: sourceType === 1 ? klingUrl : undefined }
                                    });
                                }} className="space-y-4">
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
                                        {refMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : (editingRefId ? "Update Dance" : "Save Dance")}
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
                                        <TableHead className="text-right">Actions</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {referenceVideos.map(vid => (
                                        <TableRow key={vid.id}>
                                            <TableCell className="font-medium">{vid.danceStyle}</TableCell>
                                            <TableCell><span className="text-xs bg-indigo-500/10 text-indigo-500 px-2 py-1 rounded">{vid.sourceType}</span></TableCell>
                                            <TableCell className="text-xs text-muted-foreground">{vid.videoPath || vid.klingSourceUrlOrId}</TableCell>
                                            <TableCell className="text-right">
                                                <Button variant="ghost" size="sm" className="h-8 px-2" onClick={() => handleEditRef(vid)}>
                                                    <Edit2 className="w-4 h-4 text-muted-foreground hover:text-primary" />
                                                </Button>
                                            </TableCell>
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