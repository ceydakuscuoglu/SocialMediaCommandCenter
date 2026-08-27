import { useState } from "react";
import { Clapperboard, BarChart3, ListVideo, LayoutDashboard } from "lucide-react";
import { NewJobDialog } from "@/components/jobs/NewJobDialog";
import { Button } from "@/components/ui/button";

// SAYFALARIMIZ (Pages)
import { Dashboard } from "@/pages/Dashboard";
import { Pipeline } from "@/pages/Pipeline";
import { Analytics } from "@/pages/Analytics";

function App() {
  const [activeTab, setActiveTab] = useState<"dashboard" | "pipeline" | "analytics">("dashboard");

  return (
    <div className="min-h-screen bg-background p-8 text-foreground font-sans">
      
      {/* Header & Action Area */}
      <div className="flex flex-col gap-4 mb-4">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <h1 className="text-4xl font-bold tracking-tight flex items-center gap-3">
            <Clapperboard className="w-9 h-9 text-primary" />
            <span>ShakyFruits Command Center</span>
          </h1>
          
          <NewJobDialog />
        </div>
        
        <p className="text-muted-foreground text-lg">
          Kling AI Video Automation & Content Management
        </p>

        {/* 3'LÜ MENÜ SİSTEMİ */}
        <div className="flex items-center gap-2 mt-4 border-b border-border/40 pb-4">
          <Button
            variant={activeTab === "dashboard" ? "default" : "ghost"}
            onClick={() => setActiveTab("dashboard")}
            className="gap-2"
          >
            <LayoutDashboard className="w-4 h-4" />
            Dashboard
          </Button>
          <Button
            variant={activeTab === "pipeline" ? "default" : "ghost"}
            onClick={() => setActiveTab("pipeline")}
            className="gap-2"
          >
            <ListVideo className="w-4 h-4" />
            Production Pipeline
          </Button>
          <Button
            variant={activeTab === "analytics" ? "default" : "ghost"}
            onClick={() => setActiveTab("analytics")}
            className="gap-2"
          >
            <BarChart3 className="w-4 h-4" />
            Analytics & Insights
          </Button>
        </div>
      </div>

      {/* İÇERİK ALANI: Hangi sayfa seçiliyse sadece onu render et */}
      <div className="mt-8">
        {activeTab === "dashboard" && <Dashboard />}
        {activeTab === "pipeline" && <Pipeline />}
        {activeTab === "analytics" && <Analytics />}
      </div>
      
    </div>
  );
}

export default App;