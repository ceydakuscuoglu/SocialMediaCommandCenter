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
import { Clapperboard } from "lucide-react";
import { JobsTable } from "@/components/jobs/JobsTable";
import { NewJobDialog } from "@/components/jobs/NewJobDialog";

function App() {
  // 1. Kredi verisini çeken React Query kancamızı ana App bileşenine taşıdık
  const { data: creditData, isLoading } = useQuery({
    queryKey: ["kling-credits"],
    queryFn: fetchCredits
  });

  return (
    <div className="min-h-screen bg-background p-8 text-foreground font-sans">
      
      {/* Header & Action Area */}
      <div className="flex flex-col gap-4 mb-8">
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
      </div>

      {/* YENİ EKLENEN: İstatistik Kartları Alanı (Tablonun hemen üstü) */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
        <KlingCreditCard data={creditData} isLoading={isLoading} />
        {/* İleride buraya başka istatistik kartları da ekleyebilirsin */}
      </div>

      {/* Main Content (Tablo) */}
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
  );
}

// 2. Fazlalık Dashboard fonksiyonunu tamamen sildik. Sadece App'i dışarı aktarıyoruz.
export default App;