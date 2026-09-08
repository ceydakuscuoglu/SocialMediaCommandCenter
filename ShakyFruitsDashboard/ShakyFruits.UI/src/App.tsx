import { useState } from "react";
import { AppHeader } from "@/components/layout/AppHeader";
import { NavigationTabs, TabType } from "@/components/layout/NavigationTabs";

// SAYFALAR (Pages)
import { Dashboard } from "@/pages/Dashboard";
import { Pipeline } from "@/pages/Pipeline";
import { Analytics } from "@/pages/Analytics";
import { Assets } from "@/pages/Assets";
import { Strategy } from "@/pages/Strategy";

function App() {
  const [activeTab, setActiveTab] = useState<TabType>("dashboard");

  return (
    <div className="min-h-screen bg-background p-8 text-foreground font-sans">
      <div className="flex flex-col gap-4 mb-4">
        <AppHeader />
        <NavigationTabs activeTab={activeTab} onTabChange={setActiveTab} />
      </div>

      <main className="mt-8">
        {activeTab === "dashboard" && <Dashboard />}
        {activeTab === "pipeline" && <Pipeline />}
        {activeTab === "analytics" && <Analytics />}
        {activeTab === "strategy" && <Strategy />}
        {activeTab === "assets" && <Assets />}
      </main>
    </div>
  );
}

export default App;