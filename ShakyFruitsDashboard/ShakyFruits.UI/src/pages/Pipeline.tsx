import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { JobsTable } from "@/components/jobs/JobsTable";

export function Pipeline() {
  return (
    <div className="space-y-6 animate-in fade-in duration-300">
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