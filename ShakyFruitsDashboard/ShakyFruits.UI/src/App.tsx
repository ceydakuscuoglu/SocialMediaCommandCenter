import { useState } from "react";
import { 
  Card, 
  CardContent, 
  CardDescription, 
  CardHeader, 
  CardTitle 
} from "@/components/ui/card";
import { fetchCredits } from "@/api/jobs.api";
import { KlingCreditCard } from "@/components/dashboard/KlingCreditCard";
import { useQuery } from "@tanstack/react-query";
import { Clapperboard, BarChart3, ListVideo } from "lucide-react";
import { JobsTable } from "@/components/jobs/JobsTable";
import { NewJobDialog } from "@/components/jobs/NewJobDialog";
import { Button } from "@/components/ui/button";

// YENİ EKLENEN: Az önce yazdığımız Analytics sayfasını import ediyoruz
import { Analytics } from "@/pages/Analytics"; 

function App() {
  // Hangi sekmede olduğumuzu takip eden State
  const [activeTab, setActiveTab] = useState<"pipeline" | "analytics">("pipeline");

const { data: creditData, isLoading } = useQuery({
    queryKey: ["kling-credits"],
    queryFn: fetchCredits,
    // refetchInterval: 60000 satırını SİLDİK
    staleTime: 24 * 60 * 60 * 1000, // Veriyi 24 saat boyunca "taze" kabul et (tekrar çekme)
    refetchOnWindowFocus: false, // Kullanıcı başka sekmeye gidip gelince yenileme
  });

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

        {/* YENİ EKLENEN: Sayfalar Arası Geçiş Menüsü */}
        <div className="flex items-center gap-2 mt-4 border-b border-border/40 pb-4">
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

      {/* İÇERİK ALANI: Hangi sekme seçiliyse o bileşeni göster */}
      {activeTab === "pipeline" ? (
        <div className="space-y-6 animate-in fade-in duration-300">
          {/* İstatistik Kartları */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
            <KlingCreditCard data={creditData} isLoading={isLoading} />
          </div>

          {/* Tablo */}
          <Card className="shadow-sm border-border/40">
            <CardHeader className="bg-muted/20 border-b border-border/40 pb-4">
              <CardTitle className="text-xl">Production Pipeline</CardTitle>
              <CardDescription>Real-time generation tasks tracked by the local C# backend.</CardDescription>
            </CardHeader>
            <CardContent className="p-0">
              <JobsTable />
            </CardContent>
          </Card>
        </div>
      ) : (
        // Analitik Sayfasını Burada Render Ediyoruz!
        <div className="animate-in fade-in zoom-in-95 duration-300">
          <Analytics />
        </div>
      )}
      
    </div>
  );
}

export default App;