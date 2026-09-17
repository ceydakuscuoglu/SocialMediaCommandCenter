import { Wand } from "lucide-react";

export function AppHeader() {
  return (
    <div className="flex flex-col gap-1">
      <h1 className="text-4xl font-bold tracking-tight flex items-center gap-3">
        <Wand className="w-9 h-9 text-primary" />
        <span>ShakyFruits Command Center</span>
      </h1>

      <p className="text-muted-foreground text-base">
        Social Media Command and Content Management Center
      </p>
    </div>
  );
}
