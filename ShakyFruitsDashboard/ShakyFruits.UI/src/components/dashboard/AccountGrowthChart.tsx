import { useQuery } from "@tanstack/react-query";
import { fetchAccountHistory } from "@/api/analytics.api";
import {
    Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis, Legend
} from "recharts";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Loader2, TrendingUp } from "lucide-react";

export function AccountGrowthChart() {
    const { data: history, isLoading } = useQuery({
        queryKey: ["account-history"],
        queryFn: fetchAccountHistory,
    });

    if (isLoading) {
        return (
            <Card className="w-full h-80 flex items-center justify-center border-border/40 bg-card/30">
                <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
            </Card>
        );
    }

    if (!history || history.length === 0) return null;

    // Tarihleri UI için formatlıyoruz (Örn: "02 Sep") ve sıralıyoruz
    const chartData = [...history]
        .sort((a, b) => new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime())
        .map(item => {
            const date = new Date(item.recordedAt);
            return {
                ...item,
                displayDate: date.toLocaleDateString("en-US", { month: "short", day: "numeric" }),
            };
        });

    return (
        <Card className="w-full border-border/40 shadow-sm">
            <CardHeader className="pb-2">
                <div className="flex items-center justify-between">
                    <div>
                        <CardTitle className="text-lg font-bold flex items-center gap-2">
                            <TrendingUp className="w-5 h-5 text-primary" />
                            Account Growth & Reach
                        </CardTitle>
                        <CardDescription>Follower growth vs. 7-Day video view trends over time.</CardDescription>
                    </div>
                </div>
            </CardHeader>

            <CardContent>
                <div className="h-[300px] w-full mt-4">
                    <ResponsiveContainer width="100%" height="100%">
                        <AreaChart data={chartData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                            <defs>
                                <linearGradient id="colorViews" x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="5%" stopColor="#0ea5e9" stopOpacity={0.3} />
                                    <stop offset="95%" stopColor="#0ea5e9" stopOpacity={0} />
                                </linearGradient>
                                <linearGradient id="colorFollowers" x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="5%" stopColor="#8b5cf6" stopOpacity={0.3} />
                                    <stop offset="95%" stopColor="#8b5cf6" stopOpacity={0} />
                                </linearGradient>
                            </defs>

                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="hsl(var(--border))" opacity={0.5} />

                            <XAxis
                                dataKey="displayDate"
                                tickLine={false}
                                axisLine={false}
                                tick={{ fontSize: 12, fill: "hsl(var(--muted-foreground))" }}
                                dy={10}
                            />

                            {/* Sol Eksen (İzlenmeler - Mavi) */}
                            <YAxis
                                yAxisId="left"
                                tickLine={false}
                                axisLine={false}
                                tick={{ fontSize: 12, fill: "#0ea5e9" }}
                                tickFormatter={(val) => val >= 1000 ? `${(val / 1000).toFixed(0)}k` : val}
                            />

                            {/* Sağ Eksen (Takipçiler - Mor) */}
                            <YAxis
                                yAxisId="right"
                                orientation="right"
                                tickLine={false}
                                axisLine={false}
                                tick={{ fontSize: 12, fill: "#8b5cf6" }}
                                tickFormatter={(val) => val >= 1000 ? `${(val / 1000).toFixed(1)}k` : val}
                            />

                            <Tooltip
                                contentStyle={{
                                    backgroundColor: "hsl(var(--card))",
                                    borderColor: "hsl(var(--border))",
                                    borderRadius: "8px",
                                    boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)"
                                }}
                                itemStyle={{ fontWeight: 500 }}
                                // YENİ: value ve name parametreleri Recharts'ın beklediği gibi esnetildi (any)
                                formatter={(value: any, name: any) => {
                                    const formattedValue = typeof value === 'number' ? value.toLocaleString() : value;
                                    const formattedName = name === "totalVideoViews" ? "7-Day Views" : "Total Followers";
                                    return [formattedValue, formattedName];
                                }}
                                labelStyle={{ color: "hsl(var(--muted-foreground))", marginBottom: "4px" }}
                            />

                            <Legend
                                verticalAlign="top"
                                height={36}
                                iconType="circle"
                                formatter={(value) => <span className="text-sm font-medium text-foreground">{value === "totalVideoViews" ? "Video Views" : "Followers"}</span>}
                            />

                            <Area
                                yAxisId="left"
                                type="monotone"
                                dataKey="totalVideoViews"
                                stroke="#0ea5e9"
                                strokeWidth={2}
                                fillOpacity={1}
                                fill="url(#colorViews)"
                                activeDot={{ r: 6, strokeWidth: 0, fill: "#0ea5e9" }}
                            />

                            <Area
                                yAxisId="right"
                                type="monotone"
                                dataKey="totalFollowers"
                                stroke="#8b5cf6"
                                strokeWidth={2}
                                fillOpacity={1}
                                fill="url(#colorFollowers)"
                                activeDot={{ r: 6, strokeWidth: 0, fill: "#8b5cf6" }}
                            />
                        </AreaChart>
                    </ResponsiveContainer>
                </div>
            </CardContent>
        </Card>
    );
}