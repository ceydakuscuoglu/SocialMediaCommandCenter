import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { fetchAccountHistory, SocialPlatform } from "@/api/analytics.api";
import {
    Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis, Legend
} from "recharts";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Loader2, TrendingUp } from "lucide-react";

export function AccountGrowthChart() {
    // Aktif platformu tutan state (Varsayılan: TikTok)
    const [activePlatform, setActivePlatform] = useState<SocialPlatform>(SocialPlatform.TikTok);

    // Aktif platforma göre veriyi çeken (ve undefined hatasını yok eden) kurşungeçirmez çağrı
    const { data: history, isLoading } = useQuery({
        queryKey: ["account-history", activePlatform],
        queryFn: () => fetchAccountHistory(activePlatform),
    });

    // Veri formatlama
    const chartData = history && history.length > 0 
        ? [...history]
            .sort((a, b) => new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime())
            .map(item => ({
                ...item,
                displayDate: new Date(item.recordedAt).toLocaleDateString("en-US", { month: "short", day: "numeric" }),
            }))
        : [];

    return (
        <Card className="w-full border-border/40 shadow-sm">
            <CardHeader className="pb-2">
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                    <div>
                        <CardTitle className="text-lg font-bold flex items-center gap-2">
                            <TrendingUp className="w-5 h-5 text-primary" />
                            Account Growth & Reach
                        </CardTitle>
                        <CardDescription>
                            Follower growth vs. 7-Day video performance trends over time.
                        </CardDescription>
                    </div>

                    {/* Platform Değiştirici (Toggle) */}
                    <div className="flex bg-muted/50 p-1 rounded-lg border border-border/50 w-fit">
                        <button
                            onClick={() => setActivePlatform(SocialPlatform.TikTok)}
                            className={`px-4 py-1.5 text-sm font-medium rounded-md transition-all ${
                                activePlatform === SocialPlatform.TikTok 
                                ? "bg-background text-foreground shadow-sm" 
                                : "text-muted-foreground hover:text-foreground"
                            }`}
                        >
                            TikTok
                        </button>
                        <button
                            onClick={() => setActivePlatform(SocialPlatform.Instagram)}
                            className={`px-4 py-1.5 text-sm font-medium rounded-md transition-all ${
                                activePlatform === SocialPlatform.Instagram 
                                ? "bg-background text-foreground shadow-sm" 
                                : "text-muted-foreground hover:text-foreground"
                            }`}
                        >
                            Instagram
                        </button>
                    </div>
                </div>
            </CardHeader>

            <CardContent>
                {isLoading ? (
                    <div className="w-full h-[300px] flex items-center justify-center mt-4">
                        <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
                    </div>
                ) : chartData.length === 0 ? (
                    <div className="w-full h-[300px] flex items-center justify-center mt-4 text-muted-foreground">
                        No historical data available for this platform yet.
                    </div>
                ) : (
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

                                {/* Sol Eksen (İzlenmeler/Erişim - Mavi) */}
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
                                    formatter={(value: any, name: any) => {
                                        const formattedValue = typeof value === 'number' ? value.toLocaleString() : value;
                                        // Platforma göre metni değiştir (Instagram için Reach, TikTok için Views)
                                        const viewLabel = activePlatform === SocialPlatform.Instagram ? "7-Day Reach" : "7-Day Views";
                                        const formattedName = name === "totalVideoViews" ? viewLabel : "Total Followers";
                                        return [formattedValue, formattedName];
                                    }}
                                    labelStyle={{ color: "hsl(var(--muted-foreground))", marginBottom: "4px" }}
                                />

                                <Legend
                                    verticalAlign="top"
                                    height={36}
                                    iconType="circle"
                                    formatter={(value) => (
                                        <span className="text-sm font-medium text-foreground">
                                            {value === "totalVideoViews" 
                                                ? (activePlatform === SocialPlatform.Instagram ? "Reach" : "Video Views") 
                                                : "Followers"}
                                        </span>
                                    )}
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
                )}
            </CardContent>
        </Card>
    );
}