import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { AddHistoricalVideoModal } from "@/components/assets/AddHistoricalVideoModal";
import { FruitTypesManager } from "@/components/assets/FruitTypesManager";
import { FruitAssetsManager } from "@/components/assets/FruitAssetsManager";
import { ReferenceVideosManager } from "@/components/assets/ReferenceVideosManager";
import { Apple, Image as ImageIcon, Film, Layers } from "lucide-react";

export function Assets() {
  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h2 className="text-2xl font-semibold tracking-tight flex items-center gap-2">
            <Layers className="w-6 h-6 text-primary" />
            Asset Library
          </h2>
          <p className="text-muted-foreground">
            Manage raw materials for Kling AI generations including tags, characters, and dances.
          </p>
        </div>
        {/* Legacy Video Import Modal */}
        <AddHistoricalVideoModal />
      </div>

      <Tabs defaultValue="fruit-types" className="w-full">
        <TabsList className="grid w-full max-w-md grid-cols-3 mb-6">
          <TabsTrigger value="fruit-types" className="gap-2">
            <Apple className="w-4 h-4" /> Tags
          </TabsTrigger>
          <TabsTrigger value="fruit-assets" className="gap-2">
            <ImageIcon className="w-4 h-4" /> Characters
          </TabsTrigger>
          <TabsTrigger value="reference-videos" className="gap-2">
            <Film className="w-4 h-4" /> Dances
          </TabsTrigger>
        </TabsList>

        <TabsContent value="fruit-types">
          <FruitTypesManager />
        </TabsContent>

        <TabsContent value="fruit-assets">
          <FruitAssetsManager />
        </TabsContent>

        <TabsContent value="reference-videos">
          <ReferenceVideosManager />
        </TabsContent>
      </Tabs>
    </div>
  );
}