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
import { Activity, Clapperboard, CheckCircle2, Clock, AlertCircle, Play } from "lucide-react";

const videoJobs = [
  {
    id: "SF-1042",
    character: "Strawberry",
    trend: "Hip-Hop Bounce",
    promptPreview: "hyper-realistic strawberry character, dancing hip-hop...",
    status: "Processing",
    date: "Today, 14:30",
  },
  {
    id: "SF-1041",
    character: "Banana",
    trend: "Moonwalk Slide",
    promptPreview: "tall banana character, detailed face, smooth moonwalk...",
    status: "Completed",
    date: "Today, 13:15",
  },
  {
    id: "SF-1040",
    character: "Pineapple",
    trend: "Salsa Spin",
    promptPreview: "pineapple wearing sunglasses, rapid spin motion...",
    status: "Queued",
    date: "Today, 14:45",
  },
  {
    id: "SF-1039",
    character: "Watermelon",
    trend: "Jumpstyle",
    promptPreview: "heavy watermelon character, anatomical proportions preserved...",
    status: "Error",
    date: "Yesterday, 18:20",
  },
];

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
  return (
    /* Karanlık modu test etmek istersen aşağıdaki div'in className'ine "dark" ekleyebilirsin: className="dark min-h-screen..." */
    <div className="min-h-screen bg-background p-8 text-foreground font-sans">
      
      {/* Header & Action Area */}
      <div className="flex flex-col gap-4 mb-8">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <h1 className="text-4xl font-bold tracking-tight flex items-center gap-3">
            {/* İkon rengi artık senin temanın ana rengi (Primary) */}
            <Clapperboard className="w-9 h-9 text-primary" />
            <span>ShakyFruits Command Center</span>
          </h1>
          
          {/* Yeni temanın rengini alacak Buton */}
          <Button className="gap-2 shadow-md">
            <Play className="w-4 h-4 fill-current" />
            New Generation Job
          </Button>
        </div>
        
        <p className="text-muted-foreground text-lg">
          Kling AI Video Automation & Content Management
        </p>
      </div>

      <Card className="shadow-sm border-border/40">
        <CardHeader className="bg-muted/20 border-b border-border/40 pb-4">
          <CardTitle className="text-xl">Recent Production Jobs</CardTitle>
          <CardDescription>Latest video generation tasks executed by the Playwright bot.</CardDescription>
        </CardHeader>
        <CardContent className="p-0">
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
              {videoJobs.map((job) => (
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
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
      
    </div>
  );
}

export default App;