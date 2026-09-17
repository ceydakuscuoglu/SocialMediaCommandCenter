import { LayoutDashboard, ListVideo, BarChart3, BrainCircuit, Layers } from "lucide-react";
import { Button } from "@/components/ui/button";
import { NewJobDialog } from "@/components/jobs/NewJobDialog";

export type TabType = "dashboard" | "pipeline" | "analytics" | "strategy" | "assets";

interface NavigationTabsProps {
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
}

const TABS: { id: TabType; label: string; icon: React.ComponentType<{ className?: string }> }[] = [
  { id: "dashboard", label: "Dashboard", icon: LayoutDashboard },
  { id: "pipeline", label: "Production Pipeline", icon: ListVideo },
  { id: "analytics", label: "Performance", icon: BarChart3 },
  { id: "strategy", label: "Strategy & BI", icon: BrainCircuit },
  { id: "assets", label: "Asset Library", icon: Layers },
];

export function NavigationTabs({ activeTab, onTabChange }: NavigationTabsProps) {
  return (
    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mt-4 border-b border-border/40 pb-4">
      <div className="flex flex-wrap items-center gap-2">
        {TABS.map(({ id, label, icon: Icon }) => (
          <Button
            key={id}
            variant={activeTab === id ? "default" : "ghost"}
            onClick={() => onTabChange(id)}
            className="gap-2"
          >
            <Icon className="w-4 h-4" />
            {label}
          </Button>
        ))}
      </div>

      <div className="flex items-center sm:ml-auto">
        <NewJobDialog />
      </div>
    </div>
  );
}
