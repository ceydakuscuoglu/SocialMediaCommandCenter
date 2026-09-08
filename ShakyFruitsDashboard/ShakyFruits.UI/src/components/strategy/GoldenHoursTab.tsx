import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { generateGoldenHours } from "@/api/analytics.api";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Upload, Waves, Loader2 } from "lucide-react";
import { BarChart, Bar, Cell, XAxis, Tooltip, ResponsiveContainer } from "recharts";

export function GoldenHoursTab() {
  const [csvFile, setCsvFile] = useState<File | null>(null);
  const [surfData, setSurfData] = useState<any>(null);

  const surfMutation = useMutation({
    mutationFn: generateGoldenHours,
    onSuccess: (data) => setSurfData(data),
    onError: (err) => alert(err.message),
  });

  const handleUpload = (e: React.FormEvent) => {
    e.preventDefault();
    if (csvFile) surfMutation.mutate(csvFile);
  };

  return (
    <div className="mt-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="col-span-1 border-dashed bg-muted/10 h-fit">
          <CardHeader>
            <CardTitle className="text-base">Upload TikTok Activity</CardTitle>
            <CardDescription>
              Upload your FollowerActivity.csv to calculate the optimal surf time.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleUpload} className="space-y-4">
              <Input
                type="file"
                accept=".csv"
                onChange={(e) => setCsvFile(e.target.files?.[0] || null)}
              />
              <Button type="submit" className="w-full" disabled={!csvFile || surfMutation.isPending}>
                {surfMutation.isPending ? (
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                ) : (
                  <Upload className="w-4 h-4 mr-2" />
                )}
                Analyze Heatmap
              </Button>
            </form>
          </CardContent>
        </Card>

        {surfData && (
          <Card className="col-span-1 md:col-span-2 border-border/50 bg-card shadow-sm">
            <CardContent className="p-6">
              <div className="flex flex-col md:flex-row gap-6 items-start md:items-center justify-between mb-8 pb-6 border-b border-border/50">
                <div>
                  <h3 className="text-2xl font-bold text-indigo-500 mb-2 flex items-center gap-2">
                    <Waves className="w-6 h-6" /> Golden Hour:{" "}
                    {surfData.goldenHour.recommendedPostingTime}
                  </h3>
                  <p className="text-sm text-muted-foreground leading-relaxed max-w-lg">
                    {surfData.goldenHour.recommendation}
                  </p>
                </div>
                <div className="flex flex-col gap-2 min-w-[140px]">
                  <Badge
                    variant="outline"
                    className="bg-sky-500/10 text-sky-500 border-sky-500/30 justify-center py-1"
                  >
                    Peak: {surfData.goldenHour.peakTime}
                  </Badge>
                  <Badge variant="outline" className="bg-muted justify-center py-1">
                    Audience: {surfData.goldenHour.expectedAudienceAtPeak}
                  </Badge>
                </div>
              </div>

              <div>
                <div className="flex justify-between items-center mb-4">
                  <h4 className="text-sm font-semibold">24-Hour Audience Heatmap</h4>
                  <div className="flex items-center gap-4 text-xs font-medium">
                    <span className="flex items-center gap-1.5">
                      <div className="w-3 h-3 rounded-sm bg-indigo-500"></div> Golden Hour
                    </span>
                    <span className="flex items-center gap-1.5">
                      <div className="w-3 h-3 rounded-sm bg-sky-500"></div> Peak Time
                    </span>
                  </div>
                </div>

                <div className="h-[200px] w-full">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart
                      data={[...surfData.heatmap].sort(
                        (a, b) => parseInt(a.hour) - parseInt(b.hour)
                      )}
                      margin={{ top: 10, right: 0, left: 0, bottom: 0 }}
                    >
                      <XAxis
                        dataKey="hour"
                        tickFormatter={(val) => `${val}:00`}
                        axisLine={false}
                        tickLine={false}
                        tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }}
                        dy={10}
                      />
                      <Tooltip
                        cursor={{ fill: "var(--border)" }}
                        contentStyle={{
                          borderRadius: "8px",
                          border: "none",
                          backgroundColor: "hsl(var(--card))",
                          color: "hsl(var(--foreground))",
                        }}
                        formatter={(value: any) => [value, "Active Followers"]}
                        labelFormatter={(label) => `Time: ${label}:00`}
                      />
                      <Bar dataKey="averageActiveFollowers" radius={[4, 4, 0, 0]}>
                        {[...surfData.heatmap]
                          .sort((a, b) => parseInt(a.hour) - parseInt(b.hour))
                          .map((entry, index) => {
                            const isGolden =
                              entry.hour ===
                              surfData.goldenHour.recommendedPostingTime.split(":")[0];
                            const isPeak =
                              entry.hour === surfData.goldenHour.peakTime.split(":")[0];
                            const fillColor = isGolden
                              ? "#6366f1"
                              : isPeak
                              ? "#0ea5e9"
                              : "var(--border)";
                            return <Cell key={`cell-${index}`} fill={fillColor} />;
                          })}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
