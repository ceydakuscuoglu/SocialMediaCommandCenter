import { Clapperboard } from "lucide-react";
import { NewJobDialog } from "@/components/jobs/NewJobDialog";

export function AppHeader() {
  return (
    <div className="flex flex-col gap-2">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <h1 className="text-4xl font-bold tracking-tight flex items-center gap-3">
          <Clapperboard className="w-9 h-9 text-primary" />
          <span>ShakyFruits Command Center</span>
        </h1>

        <NewJobDialog />
      </div>

      <p className="text-muted-foreground text-lg">
        Social Media Command and Content Managment Center
      </p>
    </div>
  );
}
