import { AdvancedInsights } from "@/components/strategy/AdvancedInsights";
import { BrainCircuit } from "lucide-react";

export function Strategy() {
  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      <div className="flex flex-col gap-1">
        <h2 className="text-2xl font-semibold tracking-tight flex items-center gap-2">
          <BrainCircuit className="w-6 h-6 text-primary" />
          Strategy & Intelligence
        </h2>
        <p className="text-muted-foreground">
          Analyze algorithmic triggers, cast synergy, and discover your golden posting hours.
        </p>
      </div>

      <AdvancedInsights />
    </div>
  );
}