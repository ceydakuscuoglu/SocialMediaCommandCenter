import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  fetchFruitAssets,
  fetchFruitTypes,
  addFruitAsset,
  updateFruitAsset,
  FruitAsset,
  CreateFruitAssetRequest
} from "@/api/assets.api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Loader2, Edit2, X } from "lucide-react";

export function FruitAssetsManager() {
  const queryClient = useQueryClient();
  const { data: fruitAssets = [] } = useQuery({ queryKey: ["fruit-assets"], queryFn: fetchFruitAssets });
  const { data: fruitTypes = [] } = useQuery({ queryKey: ["fruit-types"], queryFn: fetchFruitTypes });

  const [editingAssetId, setEditingAssetId] = useState<number | null>(null);
  const [assetTitle, setAssetTitle] = useState("");
  const [assetPath, setAssetPath] = useState("");
  const [isMultiple, setIsMultiple] = useState(false);
  const [selectedTypeIds, setSelectedTypeIds] = useState<number[]>([]);

  const resetForm = () => {
    setEditingAssetId(null);
    setAssetTitle("");
    setAssetPath("");
    setIsMultiple(false);
    setSelectedTypeIds([]);
  };

  const assetMutation = useMutation({
    mutationFn: async (args: { id?: number | null; data: CreateFruitAssetRequest }) => {
      return args.id ? updateFruitAsset({ id: args.id, data: args.data }) : addFruitAsset(args.data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["fruit-assets"] });
      resetForm();
    }
  });

  const handleEdit = (asset: FruitAsset) => {
    setEditingAssetId(asset.id);
    setAssetTitle(asset.title);
    setAssetPath(asset.imagePath);
    setIsMultiple(asset.isMultipleFruits);
    setSelectedTypeIds(asset.fruits.map((f) => f.id));
  };

  const toggleFruitType = (id: number) => {
    setSelectedTypeIds((prev) =>
      prev.includes(id) ? prev.filter((tId) => tId !== id) : [...prev, id]
    );
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
      {/* Form Kartı */}
      <Card
        className={`lg:col-span-4 border-border/50 bg-card/50 shadow-sm h-fit ${
          editingAssetId ? "border-primary/50 shadow-primary/10" : ""
        }`}
      >
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>{editingAssetId ? "Edit Character" : "Add Character Image"}</CardTitle>
          {editingAssetId && (
            <Button variant="ghost" size="sm" onClick={resetForm}>
              <X className="w-4 h-4 mr-1" /> Cancel
            </Button>
          )}
        </CardHeader>
        <CardContent>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              assetMutation.mutate({
                id: editingAssetId,
                data: {
                  title: assetTitle,
                  imagePath: assetPath,
                  isMultipleFruits: isMultiple,
                  fruitTypeIds: selectedTypeIds,
                },
              });
            }}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label>Title</Label>
              <Input
                placeholder="e.g. Angry Strawberry V1"
                value={assetTitle}
                onChange={(e) => setAssetTitle(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label>Local Image Path</Label>
              <Input
                placeholder="C:/assets/strawberry.png"
                value={assetPath}
                onChange={(e) => setAssetPath(e.target.value)}
                required
              />
            </div>

            <div className="space-y-3 pt-2">
              <Label>Included Fruits (Select tags)</Label>
              <div className="grid grid-cols-2 gap-2 max-h-32 overflow-y-auto p-2 border border-border/50 rounded-md bg-background/50">
                {fruitTypes.map((tag) => (
                  <div key={tag.id} className="flex items-center space-x-2">
                    <Checkbox
                      id={`tag-${tag.id}`}
                      checked={selectedTypeIds.includes(tag.id)}
                      onCheckedChange={() => toggleFruitType(tag.id)}
                    />
                    <label htmlFor={`tag-${tag.id}`} className="text-sm cursor-pointer">
                      {tag.name}
                    </label>
                  </div>
                ))}
              </div>
            </div>

            <div className="flex items-center space-x-2 pt-2">
              <Checkbox
                id="multiple"
                checked={isMultiple}
                onCheckedChange={(checked) => setIsMultiple(checked as boolean)}
              />
              <label htmlFor="multiple" className="text-sm font-medium text-amber-500 cursor-pointer">
                Contains multiple fruits
              </label>
            </div>

            <Button type="submit" disabled={assetMutation.isPending} className="w-full">
              {assetMutation.isPending ? (
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
              ) : editingAssetId ? (
                "Update Character"
              ) : (
                "Save Character"
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
              <TableHead>Title</TableHead>
              <TableHead>Path</TableHead>
              <TableHead>Tags</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {fruitAssets.map((asset) => (
              <TableRow key={asset.id}>
                <TableCell className="font-medium">{asset.title}</TableCell>
                <TableCell className="text-xs text-muted-foreground truncate max-w-[150px]">
                  {asset.imagePath}
                </TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-1">
                    {asset.fruits.map((f) => (
                      <span
                        key={f.id}
                        className="text-[10px] bg-secondary text-secondary-foreground px-2 py-0.5 rounded"
                      >
                        {f.name}
                      </span>
                    ))}
                  </div>
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2"
                    onClick={() => handleEdit(asset)}
                  >
                    <Edit2 className="w-4 h-4 text-muted-foreground hover:text-primary" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
            {fruitAssets.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="text-center py-6 text-muted-foreground">
                  No character assets found.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </Card>
    </div>
  );
}
