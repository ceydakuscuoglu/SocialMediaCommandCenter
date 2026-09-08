import { useQuery } from "@tanstack/react-query";
import { fetchLifecycleInsights, fetchEngagementMetrics } from "@/api/analytics.api";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Clock, Trophy } from "lucide-react";
import { BarChart, Bar, Cell, XAxis, YAxis, Tooltip, ResponsiveContainer } from "recharts";

export function AlgoTriggersTab() {
  const { data: lifecycle } = useQuery({ queryKey: ["lifecycle"], queryFn: fetchLifecycleInsights });
  const { data: engagement } = useQuery({ queryKey: ["engagement"], queryFn: fetchEngagementMetrics });

  return (
    <div className="mt-6 space-y-8">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Late Bloomers */}
        <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
          <h3 className="font-semibold text-emerald-500 flex items-center gap-2">
            <Clock className="w-4 h-4" /> Late Bloomers (Algorithm Revival)
          </h3>
          <p className="text-xs text-muted-foreground mb-4">
            Videos that started slow but surged later.
          </p>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Character</TableHead>
                <TableHead className="text-right">Momentum</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {lifecycle?.lateBloomers.slice(0, 5).map((lb, i) => (
                <TableRow key={i}>
                  <TableCell className="font-medium text-xs">{lb.title}</TableCell>
                  <TableCell className="text-right text-emerald-500 font-bold">
                    {lb.momentumMultiplier}x Boost
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        {/* Highest Viral Factor (K-Factor) */}
        <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
          <h3 className="font-semibold text-indigo-500 flex items-center gap-2">
            <Trophy className="w-4 h-4" /> Highest Viral Factor (K-Factor)
          </h3>
          <p className="text-xs text-muted-foreground">
            Shares / Views ratio indicating algorithmic push.
          </p>
          <div className="h-[200px] w-full mt-2">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={engagement?.topEngagingVideos.slice(0, 5)}
                margin={{ top: 10, right: 0, left: -20, bottom: 0 }}
              >
                <XAxis
                  dataKey="title"
                  tick={{ fontSize: 10, fill: "hsl(var(--muted-foreground))" }}
                  tickFormatter={(val) => val.split("+")[0].trim()}
                  axisLine={false}
                  tickLine={false}
                />
                <YAxis
                  axisLine={false}
                  tickLine={false}
                  tick={{ fontSize: 10, fill: "hsl(var(--muted-foreground))" }}
                  tickFormatter={(val) => `${val}%`}
                />
                <Tooltip
                  cursor={{ strokeDasharray: "3 3" }}
                  contentStyle={{
                    borderRadius: "8px",
                    backgroundColor: "hsl(var(--card))",
                    color: "hsl(var(--foreground))",
                    border: "1px solid var(--border)",
                  }}
                  formatter={(value: any, name: any) => [
                    name === "Viral Factor"
                      ? `${value}%`
                      : typeof value === "number"
                      ? value.toLocaleString()
                      : value,
                    name,
                  ]}
                />
                <Bar dataKey="viralFactor" fill="#6366f1" radius={[4, 4, 0, 0]} barSize={32}>
                  {engagement?.topEngagingVideos.slice(0, 5).map((_, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={index === 0 ? "#6366f1" : "var(--primary)"}
                      opacity={index === 0 ? 1 : 0.4}
                    />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>
    </div>
  );
}
