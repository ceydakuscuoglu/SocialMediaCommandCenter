import { Badge } from "@/components/ui/badge";
import { Activity, CheckCircle2, Clock, AlertCircle } from "lucide-react";
import { GenerationStatus } from "@/types/job.types";

interface JobStatusBadgeProps {
  status: GenerationStatus | string | number;
}

export function JobStatusBadge({ status }: JobStatusBadgeProps) {
  // Backend'den sayı veya metin gelse bile güvenli bir şekilde eşleştirmek için standartlaştırıyoruz
  const safeStatus = String(status).toLowerCase();

  // Completed (2)
  if (safeStatus === "2" || safeStatus === "completed") {
    return (
      <Badge className="bg-emerald-500/10 text-emerald-500 hover:bg-emerald-500/20 border-emerald-500/20">
        <CheckCircle2 className="w-3 h-3 mr-1" /> Completed
      </Badge>
    );
  }

  // Processing (1)
  if (safeStatus === "1" || safeStatus === "processing") {
    return (
      <Badge variant="secondary" className="text-blue-500">
        <Activity className="w-3 h-3 mr-1 animate-pulse" /> Processing
      </Badge>
    );
  }

  // Failed (3)
  if (safeStatus === "3" || safeStatus === "failed") {
    return (
      <Badge variant="destructive" className="text-red-500">
        <AlertCircle className="w-3 h-3 mr-1" /> Failed
      </Badge>
    );
  }

  // Pending (0) veya eşleşmeyen durumlar
  return (
    <Badge variant="outline" className="text-yellow-500">
      <Clock className="w-3 h-3 mr-1 text-muted-foreground" /> Pending
    </Badge>
  );
}