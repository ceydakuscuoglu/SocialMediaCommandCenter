import { 
  Card, 
  CardContent, 
  CardDescription, 
  CardHeader, 
  CardTitle 
} from "@/components/ui/card";
import { Clapperboard } from "lucide-react";
import { JobsTable } from "@/components/jobs/JobsTable";
import { NewJobDialog } from "@/components/jobs/NewJobDialog"; // Yeni formumuzu import ettik

function App() {
  return (
    <div className="min-h-screen bg-background p-8 text-foreground font-sans">
      
      {/* Header & Action Area */}
      <div className="flex flex-col gap-4 mb-8">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <h1 className="text-4xl font-bold tracking-tight flex items-center gap-3">
            <Clapperboard className="w-9 h-9 text-primary" />
            <span>ShakyFruits Command Center</span>
          </h1>
          
          {/* Akıllı Modal Formumuz Buraya Geldi */}
          <NewJobDialog />
        </div>
        
        <p className="text-muted-foreground text-lg">
          Kling AI Video Automation & Content Management
        </p>
      </div>

      {/* Main Content */}
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

export default App;