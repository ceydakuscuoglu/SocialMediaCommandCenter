import { useQuery } from "@tanstack/react-query";
import { fetchLeaderboards } from "@/api/analytics.api";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { BarChart, Bar, Cell, XAxis, YAxis, Tooltip, ResponsiveContainer } from "recharts";

export function LeaderboardsTab() {
  const { data: leaderboards } = useQuery({ queryKey: ["leaderboards"], queryFn: fetchLeaderboards });

  return (
    <div className="mt-6 space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Yatay BarChart (Top 5 Meyve) */}
        <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
          <h3 className="font-semibold text-foreground flex items-center gap-2">
            🍎 Fruit Champions (Top 5)
          </h3>
          <div className="h-[250px] w-full">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={leaderboards?.fruitLeaderboard.slice(0, 5)}
                layout="vertical"
                margin={{ top: 0, right: 30, left: 20, bottom: 0 }}
              >
                <XAxis type="number" hide />
                <YAxis
                  dataKey="name"
                  type="category"
                  axisLine={false}
                  tickLine={false}
                  tick={{ fill: "hsl(var(--foreground))", fontSize: 12 }}
                  width={80}
                />
                <Tooltip
                  cursor={{ fill: "var(--border)" }}
                  contentStyle={{
                    borderRadius: "8px",
                    border: "none",
                    backgroundColor: "hsl(var(--card))",
                    color: "hsl(var(--foreground))",
                  }}
                  formatter={(value: any) => [
                    typeof value === "number" ? value.toLocaleString() : value,
                    "Avg Views",
                  ]}
                />
                <Bar dataKey="averageViews" fill="#ec4899" radius={[0, 4, 4, 0]} barSize={24}>
                  {leaderboards?.fruitLeaderboard.slice(0, 5).map((_, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={index === 0 ? "#ec4899" : "var(--primary)"}
                      opacity={index === 0 ? 1 : 0.6}
                    />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Trending Dances Tablosu */}
        <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
          <h3 className="font-semibold text-foreground flex items-center gap-2">
            💃 Trending Dances
          </h3>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Style</TableHead>
                <TableHead className="text-right">Avg Views</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {leaderboards?.danceLeaderboard.slice(0, 5).map((d, i) => (
                <TableRow key={i}>
                  <TableCell className="font-medium">{d.name}</TableCell>
                  <TableCell className="text-right text-sky-500 font-semibold">
                    {d.averageViews.toLocaleString()}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </div>
    </div>
  );
}
