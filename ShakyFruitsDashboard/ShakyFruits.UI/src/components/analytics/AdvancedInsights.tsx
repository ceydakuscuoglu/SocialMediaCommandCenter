import { useState } from "react";
import { useQuery, useMutation } from "@tanstack/react-query";
import {
    fetchLeaderboards, fetchSoloVsGroup, fetchEngagementMetrics,
    fetchFruitCombinations, fetchLifecycleInsights, generateGoldenHours
} from "@/api/analytics.api";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Trophy, Zap, TrendingUp, Users, Clock, Upload, Waves, Loader2 } from "lucide-react";
import {
    BarChart, Bar, Cell, XAxis, YAxis, Tooltip, ResponsiveContainer,
    ScatterChart, Scatter, ZAxis, CartesianGrid, Legend
} from "recharts";

export function AdvancedInsights() {
    const { data: leaderboards } = useQuery({ queryKey: ["leaderboards"], queryFn: fetchLeaderboards });
    const { data: soloVsGroup } = useQuery({ queryKey: ["solo-group"], queryFn: fetchSoloVsGroup });
    const { data: engagement } = useQuery({ queryKey: ["engagement"], queryFn: fetchEngagementMetrics });
    const { data: combinations } = useQuery({ queryKey: ["combinations"], queryFn: fetchFruitCombinations });
    const { data: lifecycle } = useQuery({ queryKey: ["lifecycle"], queryFn: fetchLifecycleInsights });

    const [csvFile, setCsvFile] = useState<File | null>(null);
    const [surfData, setSurfData] = useState<any>(null);

    const surfMutation = useMutation({
        mutationFn: generateGoldenHours,
        onSuccess: (data) => setSurfData(data),
        onError: (err) => alert(err.message)
    });

    const handleUpload = (e: React.FormEvent) => {
        e.preventDefault();
        if (csvFile) surfMutation.mutate(csvFile);
    };

    return (
        <Card className="w-full border-border/40 shadow-sm bg-card mt-8">
            <CardHeader className="border-b border-border/40 pb-4">
                <CardTitle className="text-xl flex items-center gap-2">
                    <Zap className="w-5 h-5 text-amber-500 fill-current" />
                    Business Intelligence & Strategy
                </CardTitle>
                <CardDescription>Deep dive into algorithmic triggers, audience psychology, and optimal posting times.</CardDescription>
            </CardHeader>

            <CardContent className="pt-6">
                <Tabs defaultValue="leaderboards" className="w-full">
                    <TabsList className="grid w-full grid-cols-1 md:grid-cols-4 bg-muted/50 p-1 rounded-lg h-auto md:h-12">
                        <TabsTrigger value="leaderboards" className="flex items-center justify-center gap-2 h-10">
                            <Trophy className="w-4 h-4" /> <span>Top Charts</span>
                        </TabsTrigger>
                        <TabsTrigger value="synergy" className="flex items-center justify-center gap-2 h-10">
                            <Users className="w-4 h-4" /> <span>Cast Synergy</span>
                        </TabsTrigger>
                        <TabsTrigger value="algorithm" className="flex items-center justify-center gap-2 h-10">
                            <TrendingUp className="w-4 h-4" /> <span>Algo Triggers</span>
                        </TabsTrigger>
                        <TabsTrigger value="surf" className="flex items-center justify-center gap-2 h-10">
                            <Waves className="w-4 h-4" /> <span>Surf Strategy</span>
                        </TabsTrigger>
                    </TabsList>

                    {/* 1. LİDERLİK TABLOLARI */}
                    <TabsContent value="leaderboards" className="mt-6 space-y-6">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {/* YENİ: Yatay BarChart (Top 5 Meyve) */}
                            <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
                                <h3 className="font-semibold text-foreground flex items-center gap-2">🍎 Fruit Champions (Top 5)</h3>
                                <div className="h-[250px] w-full">
                                    <ResponsiveContainer width="100%" height="100%">
                                        <BarChart
                                            data={leaderboards?.fruitLeaderboard.slice(0, 5)}
                                            layout="vertical"
                                            margin={{ top: 0, right: 30, left: 20, bottom: 0 }}
                                        >
                                            <XAxis type="number" hide />
                                            <YAxis dataKey="name" type="category" axisLine={false} tickLine={false} tick={{ fill: "hsl(var(--foreground))", fontSize: 12 }} width={80} />
                                            <Tooltip
                                                cursor={{ fill: 'var(--border)' }}
                                                contentStyle={{ borderRadius: '8px', border: 'none', backgroundColor: 'hsl(var(--card))', color: 'hsl(var(--foreground))' }}
                                                // value: number yerine value: any yapıldı ve güvenlik eklendi
                                                formatter={(value: any) => [typeof value === 'number' ? value.toLocaleString() : value, "Avg Views"]}
                                            />
                                            <Bar dataKey="averageViews" fill="#ec4899" radius={[0, 4, 4, 0]} barSize={24}>
                                                {leaderboards?.fruitLeaderboard.slice(0, 5).map((_, index) => (
                                                    <Cell key={`cell-${index}`} fill={index === 0 ? "#ec4899" : "var(--primary)"} opacity={index === 0 ? 1 : 0.6} />
                                                ))}
                                            </Bar>
                                        </BarChart>
                                    </ResponsiveContainer>
                                </div>
                            </div>

                            <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
                                <h3 className="font-semibold text-foreground flex items-center gap-2">💃 Trending Dances</h3>
                                <Table>
                                    <TableHeader><TableRow><TableHead>Style</TableHead><TableHead className="text-right">Avg Views</TableHead></TableRow></TableHeader>
                                    <TableBody>
                                        {leaderboards?.danceLeaderboard.slice(0, 5).map((d, i) => (
                                            <TableRow key={i}>
                                                <TableCell className="font-medium">{d.name}</TableCell>
                                                <TableCell className="text-right text-sky-500 font-semibold">{d.averageViews.toLocaleString()}</TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </div>
                        </div>
                    </TabsContent>

                    {/* 2. SİNERJİ & SOLO/GRUP */}
                    <TabsContent value="synergy" className="mt-6 space-y-8">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {soloVsGroup?.map((stat, i) => (
                                <Card key={i} className="bg-muted/20 border-border/50">
                                    <CardContent className="p-4 flex justify-between items-center">
                                        <div>
                                            <p className="text-sm font-medium text-muted-foreground">{stat.type}</p>
                                            <p className="text-2xl font-bold">{stat.averageViews.toLocaleString()} <span className="text-sm font-normal text-muted-foreground">avg views</span></p>
                                        </div>
                                        <Badge variant={stat.type.includes("Multiple") ? "default" : "secondary"}>
                                            {stat.videoCount} Videos
                                        </Badge>
                                    </CardContent>
                                </Card>
                            ))}
                        </div>

                        {/* YENİ: Sinerji Baloncuk Grafiği (Scatter Chart) */}
                        <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
                            <div className="flex justify-between items-center">
                                <h3 className="font-semibold text-foreground">Cast Synergy Sweet Spot</h3>
                                <span className="text-xs text-muted-foreground">Top right is the best (High Views + High K-Factor)</span>
                            </div>
                            <div className="h-[300px] w-full mt-4">
                                <ResponsiveContainer width="100%" height="100%">
                                    <ScatterChart margin={{ top: 10, right: 20, bottom: 10, left: 10 }}>
                                        <CartesianGrid strokeDasharray="3 3" opacity={0.2} vertical={false} />
                                        <XAxis type="number" dataKey="averageViews" name="Avg Views" tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }} tickFormatter={(val) => `${(val / 1000).toFixed(0)}k`} />
                                        <YAxis type="number" dataKey="averageViralFactor" name="Viral Factor" unit="%" tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }} axisLine={false} tickLine={false} />
                                        <ZAxis type="number" dataKey="videoCount" range={[100, 800]} name="Videos Published" />
                                        <Tooltip
                                            cursor={{ strokeDasharray: '3 3' }}
                                            contentStyle={{ borderRadius: '8px', backgroundColor: 'hsl(var(--card))', color: 'hsl(var(--foreground))', border: '1px solid var(--border)' }}
                                            // value: number, name: string yerine any kullanıldı
                                            formatter={(value: any, name: any) => [name === "Viral Factor" ? `${value}%` : (typeof value === 'number' ? value.toLocaleString() : value), name]}
                                        />
                                        <Scatter name="Combinations" data={combinations} fill="#8b5cf6" opacity={0.8} />
                                    </ScatterChart>
                                </ResponsiveContainer>
                            </div>
                        </div>
                    </TabsContent>

                    {/* 3. ALGORİTMA TETİKLEYİCİLERİ */}
                    <TabsContent value="algorithm" className="mt-6 space-y-8">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

                            <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
                                <h3 className="font-semibold text-emerald-500 flex items-center gap-2"><Clock className="w-4 h-4" /> Late Bloomers (Algorithm Revival)</h3>
                                <p className="text-xs text-muted-foreground mb-4">Videos that started slow but surged later.</p>
                                <Table>
                                    <TableHeader><TableRow><TableHead>Character</TableHead><TableHead className="text-right">Momentum</TableHead></TableRow></TableHeader>
                                    <TableBody>
                                        {lifecycle?.lateBloomers.slice(0, 5).map((lb, i) => (
                                            <TableRow key={i}>
                                                <TableCell className="font-medium text-xs">{lb.title}</TableCell>
                                                <TableCell className="text-right text-emerald-500 font-bold">{lb.momentumMultiplier}x Boost</TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </div>

                            {/* YENİ: K-Factor için dikey çubuk grafiği */}
                            <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
                                <h3 className="font-semibold text-indigo-500 flex items-center gap-2"><Trophy className="w-4 h-4" /> Highest Viral Factor (K-Factor)</h3>
                                <p className="text-xs text-muted-foreground">Shares / Views ratio indicating algorithmic push.</p>
                                <div className="h-[200px] w-full mt-2">
                                    <ResponsiveContainer width="100%" height="100%">
                                        <BarChart data={engagement?.topEngagingVideos.slice(0, 5)} margin={{ top: 10, right: 0, left: -20, bottom: 0 }}>
                                            <XAxis dataKey="title" tick={{ fontSize: 10, fill: "hsl(var(--muted-foreground))" }} tickFormatter={(val) => val.split('+')[0].trim()} axisLine={false} tickLine={false} />
                                            <YAxis axisLine={false} tickLine={false} tick={{ fontSize: 10, fill: "hsl(var(--muted-foreground))" }} tickFormatter={(val) => `${val}%`} />
                                            <Tooltip
                                                cursor={{ strokeDasharray: '3 3' }}
                                                contentStyle={{ borderRadius: '8px', backgroundColor: 'hsl(var(--card))', color: 'hsl(var(--foreground))', border: '1px solid var(--border)' }}
                                                // value: number, name: string yerine any kullanıldı
                                                formatter={(value: any, name: any) => [name === "Viral Factor" ? `${value}%` : (typeof value === 'number' ? value.toLocaleString() : value), name]}
                                            />
                                            <Bar dataKey="viralFactor" fill="#6366f1" radius={[4, 4, 0, 0]} barSize={32}>
                                                {engagement?.topEngagingVideos.slice(0, 5).map((_, index) => (
                                                    <Cell key={`cell-${index}`} fill={index === 0 ? "#6366f1" : "var(--primary)"} opacity={index === 0 ? 1 : 0.4} />
                                                ))}
                                            </Bar>
                                        </BarChart>
                                    </ResponsiveContainer>
                                </div>
                            </div>

                        </div>
                    </TabsContent>

                    {/* 4. SÖRF STRATEJİSİ (GOLDEN HOUR) */}
                    <TabsContent value="surf" className="mt-6">
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">

                            <Card className="col-span-1 border-dashed bg-muted/10 h-fit">
                                <CardHeader>
                                    <CardTitle className="text-base">Upload TikTok Activity</CardTitle>
                                    <CardDescription>Upload your FollowerActivity.csv to calculate the optimal surf time.</CardDescription>
                                </CardHeader>
                                <CardContent>
                                    <form onSubmit={handleUpload} className="space-y-4">
                                        <Input type="file" accept=".csv" onChange={e => setCsvFile(e.target.files?.[0] || null)} />
                                        <Button type="submit" className="w-full" disabled={!csvFile || surfMutation.isPending}>
                                            {surfMutation.isPending ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : <Upload className="w-4 h-4 mr-2" />}
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
                                                    <Waves className="w-6 h-6" /> Golden Hour: {surfData.goldenHour.recommendedPostingTime}
                                                </h3>
                                                <p className="text-sm text-muted-foreground leading-relaxed max-w-lg">
                                                    {surfData.goldenHour.recommendation}
                                                </p>
                                            </div>
                                            <div className="flex flex-col gap-2 min-w-[140px]">
                                                <Badge variant="outline" className="bg-sky-500/10 text-sky-500 border-sky-500/30 justify-center py-1">
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
                                                    <span className="flex items-center gap-1.5"><div className="w-3 h-3 rounded-sm bg-indigo-500"></div> Golden Hour</span>
                                                    <span className="flex items-center gap-1.5"><div className="w-3 h-3 rounded-sm bg-sky-500"></div> Peak Time</span>
                                                </div>
                                            </div>

                                            <div className="h-[200px] w-full">
                                                <ResponsiveContainer width="100%" height="100%">
                                                    <BarChart data={[...surfData.heatmap].sort((a, b) => parseInt(a.hour) - parseInt(b.hour))} margin={{ top: 10, right: 0, left: 0, bottom: 0 }}>
                                                        <XAxis dataKey="hour" tickFormatter={(val) => `${val}:00`} axisLine={false} tickLine={false} tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }} dy={10} />
                                                        <Tooltip
                                                            cursor={{ fill: 'var(--border)' }}
                                                            contentStyle={{ borderRadius: '8px', border: 'none', backgroundColor: 'hsl(var(--card))', color: 'hsl(var(--foreground))' }}
                                                            formatter={(value: any) => [value, "Active Followers"]}
                                                            labelFormatter={(label) => `Time: ${label}:00`}
                                                        />
                                                        <Bar dataKey="averageActiveFollowers" radius={[4, 4, 0, 0]}>
                                                            {[...surfData.heatmap].sort((a, b) => parseInt(a.hour) - parseInt(b.hour)).map((entry, index) => {
                                                                const isGolden = entry.hour === surfData.goldenHour.recommendedPostingTime.split(':')[0];
                                                                const isPeak = entry.hour === surfData.goldenHour.peakTime.split(':')[0];
                                                                const fillColor = isGolden ? "#6366f1" : (isPeak ? "#0ea5e9" : "var(--border)");
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
                    </TabsContent>

                </Tabs>
            </CardContent>
        </Card>
    );
}