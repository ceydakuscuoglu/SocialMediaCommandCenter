import { Badge } from "@/components/ui/badge";
import { CheckCircle2, CircleDashed, Loader2, XCircle, AlertCircle } from "lucide-react";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { GenerationStatus } from "@/api/jobs.api";

interface JobStatusBadgeProps {
  status: GenerationStatus | string | number;
  errorMessage?: string | null;
}

export function JobStatusBadge({ status, errorMessage }: JobStatusBadgeProps) {
  // TypeScript ve C# enum karmaşasını önlemek için string'e çevirip kontrol ediyoruz
  const statusStr = String(status).toLowerCase();
  
  if (statusStr === "completed" || statusStr === "2") {
    return (
      <Badge variant="outline" className="text-emerald-500 border-emerald-500/20 bg-emerald-500/5">
        <CheckCircle2 className="w-3 h-3 mr-1" /> Completed
      </Badge>
    );
  }
  
  if (statusStr === "processing" || statusStr === "1") {
    return (
      <Badge variant="outline" className="text-blue-500 border-blue-500/20 bg-blue-500/5">
        <Loader2 className="w-3 h-3 mr-1 animate-spin" /> Processing
      </Badge>
    );
  }
  
  if (statusStr === "failed" || statusStr === "3") {
    const badge = (
      <Badge variant="outline" className="text-red-500 border-red-500/20 bg-red-500/5">
        <XCircle className="w-3 h-3 mr-1" /> Failed
      </Badge>
    );

    // Eğer hata mesajı varsa yanına info ikonu ve Tooltip ekle
    if (errorMessage) {
      return (
        <TooltipProvider delayDuration={200}>
          <Tooltip>
            <TooltipTrigger asChild>
              <div className="inline-flex items-center gap-1.5 cursor-help">
                {badge}
                <AlertCircle className="w-4 h-4 text-red-500/70 hover:text-red-500 transition-colors" />
              </div>
            </TooltipTrigger>
            {/* Hata baloncuğu kırmızı tonlarında olacak */}
            <TooltipContent side="top" className="max-w-[280px] bg-destructive text-destructive-foreground border-destructive-foreground/10 shadow-lg">
              <p className="text-xs leading-relaxed font-medium">{errorMessage}</p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      );
    }

    return badge;
  }
  
  // Varsayılan / Pending (0)
  return (
    <Badge variant="outline" className="text-muted-foreground border-border bg-muted/50">
      <CircleDashed className="w-3 h-3 mr-1 animate-[spin_3s_linear_infinite]" /> Pending
    </Badge>
  );
}