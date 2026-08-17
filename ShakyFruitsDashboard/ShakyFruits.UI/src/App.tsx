import { 
  Card, 
  CardContent, 
  CardDescription, 
  CardHeader, 
  CardTitle 
} from "@/components/ui/card";
import { 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableHeader, 
  TableRow 
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Activity, Clapperboard, CheckCircle2, Clock, AlertCircle, Play, Loader2 } from "lucide-react";
import { useQuery } from "@tanstack/react-query";

// .NET API'den gelecek verinin TypeScript arayüzü (Model)
interface VideoJob {
  id: string;
  character: string;
  trend: string;
  promptPreview: string;
  status: string;
  date: string;
}

// Fetch API ile veri çekme fonksiyonumuz
const fetchVideoJobs = async (): Promise<VideoJob[]> => {
  // CEYDAK sunucusundaki API yolunu kendi backend yapısına göre düzenleyebilirsin
  const response = await fetch("http://CEYDAK:5000/api/jobs");
  
  if (!response.ok) {
    throw new Error("Failed to fetch jobs from the local .NET API.");
  }
  
  return response.json();
};

const getStatusBadge = (status: string) => {
  switch (status) {
    case "Completed":
      return <Badge className="bg-emerald-500/10 text-emerald-500 hover:bg-emerald-500/20 border-emerald-500/20"><CheckCircle2 className="w-3 h-3 mr-1" /> Completed</Badge>;
    case "Processing":
      return <Badge variant="secondary" className="text-blue-500"><Activity className="w-3 h-3 mr-1 animate-pulse" /> Processing</Badge>;
    case "Error":
      return <Badge variant="destructive"><AlertCircle className="w-3 h-3 mr-1" /> Error</Badge>;
    case "Queued":
    default:
      return <Badge variant="outline"><Clock className="w-3 h-3 mr-1 text-muted-foreground" /> Queued</Badge>;
  }
};

function App() {
  // TanStack Query (React Query) kullanımı
  const { data: videoJobs, isLoading, isError, error } = useQuery({
    queryKey: ['videoJobs'],
    queryFn: fetchVideoJobs,
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
          
          <Button className="gap-2 shadow-md">
            <Play className="w-4 h-4 fill-current" />
            New Generation Job
          </Button>
        </div>
        
        <p className="text-muted-foreground text-lg">
          Kling AI Video Automation & Content Management
        </p>
      </div>

      {/* Main Content - Jobs Table */}
      <Card className="shadow-sm border-border/40">
        <CardHeader className="bg-muted/20 border-b border-border/40 pb-4">
          <CardTitle className="text-xl">Recent Production Jobs</CardTitle>
          <CardDescription>Latest video generation tasks executed by the Playwright bot.</CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          
          {/* Yükleme Durumu (Loading State) */}
          {isLoading && (
            <div className="flex flex-col items-center justify-center py-16 text-muted-foreground">
              <Loader2 className="w-8 h-8 animate-spin text-primary mb-4" />
              <p>Fetching data from CEYDAK server...</p>
            </div>
          )}

          {/* Hata Durumu (Error State) */}
          {isError && (
            <div className="flex flex-col items-center justify-center py-16 text-destructive">
              <AlertCircle className="w-8 h-8 mb-4" />
              <p className="font-medium">Connection Error</p>
              <p className="text-sm opacity-80">{error instanceof Error ? error.message : "Unknown error occurred"}</p>
            </div>
          )}

          {/* Başarılı Veri Durumu (Success State) */}
          {!isLoading && !isError && videoJobs && (
            <Table>
              <TableHeader className="bg-muted/10">
                <TableRow className="hover:bg-transparent">
                  <TableHead className="w-[100px] pl-6">Job ID</TableHead>
                  <TableHead>Character</TableHead>
                  <TableHead>Trend / Concept</TableHead>
                  <TableHead className="hidden md:table-cell">Prompt Preview</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right pr-6">Date</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {videoJobs.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} className="text-center py-12 text-muted-foreground">
                      No generation jobs found. Start a new one!
                    </TableCell>
                  </TableRow>
                ) : (
                  videoJobs.map((job) => (
                    <TableRow key={job.id} className="group transition-colors hover:bg-muted/50">
                      <TableCell className="font-medium pl-6">{job.id}</TableCell>
                      <TableCell>{job.character}</TableCell>
                      <TableCell>{job.trend}</TableCell>
                      <TableCell className="hidden md:table-cell text-muted-foreground truncate max-w-[250px]">
                        {job.promptPreview}
                      </TableCell>
                      <TableCell>{getStatusBadge(job.status)}</TableCell>
                      <TableCell className="text-right text-muted-foreground pr-6">
                        {job.date}
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
      
    </div>
  );
}

export default App;