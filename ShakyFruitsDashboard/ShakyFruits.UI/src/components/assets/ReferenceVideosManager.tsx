import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  fetchReferenceVideos,
  fetchAssetPaths,
  addReferenceVideo,
  updateReferenceVideo,
  ReferenceVideo,
  ReferenceSourceType,
  CreateReferenceVideoRequest,
} from "@/api/assets.api";
import { getFileName } from "@/utils/formatters";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Loader2, Edit2, X } from "lucide-react";

export function ReferenceVideosManager() {
  const queryClient = useQueryClient();
  const { data: referenceVideos = [] } = useQuery({
    queryKey: ["reference-videos"],
    queryFn: fetchReferenceVideos,
  });
  const { data: assetPaths } = useQuery({ queryKey: ["asset-paths"], queryFn: fetchAssetPaths });

  const [editingRefId, setEditingRefId] = useState<number | null>(null);
  const [danceStyle, setDanceStyle] = useState("");
  const [sourceType, setSourceType] = useState<ReferenceSourceType>(ReferenceSourceType.LocalUpload);
  const [videoPath, setVideoPath] = useState("");
  const [klingUrl, setKlingUrl] = useState("");

  const resetForm = () => {
    setEditingRefId(null);
    setDanceStyle("");
    setSourceType(ReferenceSourceType.LocalUpload);
    setVideoPath("");
    setKlingUrl("");
  };

  const refMutation = useMutation({
    mutationFn: async (args: { id?: number | null; data: CreateReferenceVideoRequest }) => {
      return args.id ? updateReferenceVideo({ id: args.id, data: args.data }) : addReferenceVideo(args.data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reference-videos"] });
      resetForm();
    },
  });

  const handleEdit = (vid: ReferenceVideo) => {
    setEditingRefId(vid.id);
    setDanceStyle(vid.danceStyle);
    setSourceType(vid.sourceType === "LocalUpload" ? 0 : 1);
    setVideoPath(getFileName(vid.videoPath) || "");
    setKlingUrl(vid.klingSourceUrlOrId || "");
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
      {/* Form Kartı */}
      <Card
        className={`lg:col-span-4 border-border/50 bg-card/50 shadow-sm h-fit ${
          editingRefId ? "border-primary/50 shadow-primary/10" : ""
        }`}
      >
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>{editingRefId ? "Edit Dance Reference" : "Add Dance Reference"}</CardTitle>
          {editingRefId && (
            <Button variant="ghost" size="sm" onClick={resetForm}>
              <X className="w-4 h-4 mr-1" /> Cancel
            </Button>
          )}
        </CardHeader>
        <CardContent>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              refMutation.mutate({
                id: editingRefId,
                data: {
                  danceStyle,
                  sourceType,
                  videoPath: sourceType === 0 ? videoPath : undefined,
                  klingSourceUrlOrId: sourceType === 1 ? klingUrl : undefined,
                },
              });
            }}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label>Dance Style Name</Label>
              <Input
                placeholder="e.g. Salsa Basic Step"
                value={danceStyle}
                onChange={(e) => setDanceStyle(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label>Source Type</Label>
              <select
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={sourceType}
                onChange={(e) => setSourceType(parseInt(e.target.value))}
              >
                <option value={ReferenceSourceType.LocalUpload}>Local Upload</option>
                <option value={ReferenceSourceType.KlingRecreate}>Kling Recreate</option>
              </select>
            </div>
            {sourceType === ReferenceSourceType.LocalUpload ? (
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label>Video File Name</Label>
                  {assetPaths?.referenceVideos && (
                    <span className="text-[10px] text-muted-foreground truncate max-w-[180px]" title={assetPaths.referenceVideos}>
                      📁 {assetPaths.referenceVideos}
                    </span>
                  )}
                </div>
                <Input
                  placeholder="e.g. pra.mp4"
                  value={videoPath}
                  onChange={(e) => setVideoPath(e.target.value)}
                  required
                />
                <p className="text-[11px] text-muted-foreground">
                   You can just write the file name (ex. <code className="text-primary font-mono">pra.mp4</code>)
                </p>
              </div>
            ) : (
              <div className="space-y-2">
                <Label>Kling URL / ID</Label>
                <Input
                  placeholder="https://kling.ai/v/..."
                  value={klingUrl}
                  onChange={(e) => setKlingUrl(e.target.value)}
                  required
                />
              </div>
            )}
            <Button type="submit" disabled={refMutation.isPending} className="w-full">
              {refMutation.isPending ? (
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
              ) : editingRefId ? (
                "Update Dance"
              ) : (
                "Save Dance"
              )}
            </Button>
          </form>
        </CardContent>
      </Card>

      {/* Listeleme Tablosu */}
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
            {referenceVideos.map((vid) => (
              <TableRow key={vid.id}>
                <TableCell className="font-medium">{vid.danceStyle}</TableCell>
                <TableCell>
                  <span className="text-xs bg-indigo-500/10 text-indigo-500 px-2 py-1 rounded">
                    {vid.sourceType}
                  </span>
                </TableCell>
                <TableCell className="text-xs text-muted-foreground truncate max-w-[200px]" title={vid.videoPath || vid.klingSourceUrlOrId || ""}>
                  {vid.sourceType === "LocalUpload" && vid.videoPath ? getFileName(vid.videoPath) : (vid.videoPath || vid.klingSourceUrlOrId)}
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2"
                    onClick={() => handleEdit(vid)}
                  >
                    <Edit2 className="w-4 h-4 text-muted-foreground hover:text-primary" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
            {referenceVideos.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="text-center py-6 text-muted-foreground">
                  No reference dances found.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </Card>
    </div>
  );
}
