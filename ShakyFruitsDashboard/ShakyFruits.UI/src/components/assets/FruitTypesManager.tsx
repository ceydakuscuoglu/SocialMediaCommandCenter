import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { fetchFruitTypes, addFruitType, updateFruitType, FruitType } from "@/api/assets.api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Loader2, Edit2, X } from "lucide-react";

export function FruitTypesManager() {
  const queryClient = useQueryClient();
  const { data: fruitTypes = [] } = useQuery({ queryKey: ["fruit-types"], queryFn: fetchFruitTypes });

  const [editingTypeId, setEditingTypeId] = useState<number | null>(null);
  const [fruitTypeName, setFruitTypeName] = useState("");

  const resetForm = () => {
    setEditingTypeId(null);
    setFruitTypeName("");
  };

  const typeMutation = useMutation({
    mutationFn: async (args: { id?: number | null; data: { name: string } }) => {
      return args.id ? updateFruitType({ id: args.id, data: args.data }) : addFruitType(args.data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["fruit-types"] });
      resetForm();
    }
  });

  const handleEdit = (tag: FruitType) => {
    setEditingTypeId(tag.id);
    setFruitTypeName(tag.name);
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
      {/* Form Kartı */}
      <Card className={`lg:col-span-4 border-border/50 bg-card/50 shadow-sm h-fit ${editingTypeId ? 'border-primary/50 shadow-primary/10' : ''}`}>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>{editingTypeId ? "Edit Fruit Tag" : "Create Fruit Tag"}</CardTitle>
          {editingTypeId && (
            <Button variant="ghost" size="sm" onClick={resetForm}>
              <X className="w-4 h-4 mr-1" /> Cancel
            </Button>
          )}
        </CardHeader>
        <CardContent>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              typeMutation.mutate({ id: editingTypeId, data: { name: fruitTypeName } });
            }}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label>Fruit Name</Label>
              <Input
                placeholder="e.g. Pineapple"
                value={fruitTypeName}
                onChange={(e) => setFruitTypeName(e.target.value)}
                required
              />
            </div>
            <Button type="submit" disabled={typeMutation.isPending} className="w-full">
              {typeMutation.isPending ? (
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
              ) : editingTypeId ? (
                "Update Tag"
              ) : (
                "Save Tag"
              )}
            </Button>
          </form>
        </CardContent>
      </Card>

      {/* Listeleme Kartı */}
      <Card className="lg:col-span-8 border-border/50 bg-card/50 shadow-sm h-fit">
        <CardHeader>
          <CardTitle>Existing Tags</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-2">
            {fruitTypes.map((tag) => (
              <div
                key={tag.id}
                className="flex items-center px-3 py-1 bg-primary/10 text-primary rounded-full text-sm font-medium border border-primary/20 group"
              >
                {tag.name}
                <button
                  type="button"
                  onClick={() => handleEdit(tag)}
                  className="ml-2 opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  <Edit2 className="w-3.5 h-3.5 hover:text-primary/70" />
                </button>
              </div>
            ))}
            {fruitTypes.length === 0 && (
              <span className="text-muted-foreground text-sm">No tags found.</span>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
