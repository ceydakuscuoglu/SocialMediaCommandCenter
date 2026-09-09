import { useMemo } from "react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer
} from "recharts";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { VideoAnalyticsSnapshot } from "@/api/analytics.api";

interface VideoPerformanceChartProps {
  historyData: VideoAnalyticsSnapshot[];
  title?: string;
}

export function VideoPerformanceChart({
  historyData,
  title = "Growth Performance"
}: VideoPerformanceChartProps) {

  // 1. Verileri Grafiğin Anlayacağı ve Göstereceği Formata Çevir
  const formattedData = useMemo(() => {
    if (!historyData) return [];

    // Eski tarihten yeni tarihe doğru sırala (Grafiğin soldan sağa akması için)
    const sortedData = [...historyData].sort((a, b) =>
      new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime()
    );

    return sortedData.map(snap => {
      const date = new Date(snap.recordedAt);
      return {
        ...snap,
        // Ekranda çok yer kaplamaması için "Gün Ay, Saat:Dakika" formatı
        displayDate: date.toLocaleDateString("tu-TR", {
          month: "short",
          day: "numeric",
          hour: "2-digit",
          minute: "2-digit"
        })
      };
    });
  }, [historyData]);

  // 2. Veri Yoksa "Boş Durum" (Empty State) Göster
  if (formattedData.length === 0) {
    return (
      <Card className="bg-card/50 backdrop-blur-sm border-border/50 h-full flex flex-col items-center justify-center text-muted-foreground min-h-[350px]">
        No analytics data available for this timeline.
      </Card>
    );
  }

  // 3. Grafiği Çiz (Recharts & Shadcn UI)
  return (
    <Card className="bg-card/50 backdrop-blur-sm border-border/50 shadow-sm w-full h-full flex flex-col">
      <CardHeader>
        <CardTitle className="text-lg">{title}</CardTitle>
        <CardDescription>Views, Likes, and Favorites progression based on worker snapshots.</CardDescription>
      </CardHeader>
      <CardContent className="flex-1 min-h-[350px] w-full pb-4">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={formattedData} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>

            {/* Arka Plan Izgarası: Sadece yatay çizgiler, koyu temaya uygun hafiflikte */}
            <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" vertical={false} />

            {/* X Ekseni: Tarihler */}
            <XAxis
              dataKey="displayDate"
              stroke="hsl(var(--muted-foreground))"
              fontSize={12}
              tickLine={false}
              axisLine={false}
              dy={10}
            />

            {/* Y Ekseni: Sayılar (Binlik değerleri 'k' ile kısaltıyoruz, Örn: 1.5k) */}
            <YAxis
              stroke="hsl(var(--muted-foreground))"
              fontSize={12}
              tickLine={false}
              axisLine={false}
              tickFormatter={(value) => value >= 1000 ? `${(value / 1000).toFixed(1)}k` : value}
              dx={-10}
            />

            {/* Üzerine Gelindiğinde Çıkan Detay Baloncuğu */}
            <Tooltip
              contentStyle={{
                backgroundColor: "hsl(var(--card))",
                borderColor: "hsl(var(--border))",
                borderRadius: "8px",
                color: "hsl(var(--foreground))",
                boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)"
              }}
              itemStyle={{ fontSize: "14px", fontWeight: "500" }}
            />

            {/* Renk Göstergeleri (Legend) */}
            <Legend iconType="circle" wrapperStyle={{ paddingTop: "20px" }} />

            {/* 1. Çizgi: İzlenmeler (Gösterişli Mavi/Cyan) */}
            <Line
              type="monotone"
              name="Total Views"
              dataKey="views"
              stroke="#0ea5e9" // Tailwind sky-500
              strokeWidth={3}
              dot={{ r: 4, strokeWidth: 2, fill: "hsl(var(--background))" }}
              activeDot={{ r: 6, strokeWidth: 0, fill: "#0ea5e9" }}
            />

            {/* 2. Çizgi: Beğeniler (YENİ - Gösterişli Mor) */}
            <Line
              type="monotone"
              name="Likes"
              dataKey="likes"
              stroke="#8b5cf6" // Tailwind violet-500
              strokeWidth={3}
              dot={{ r: 4, strokeWidth: 2, fill: "hsl(var(--background))" }}
              activeDot={{ r: 6, strokeWidth: 0, fill: "#8b5cf6" }}
            />

            {/* 3. Çizgi: Favoriler (Gösterişli Pembe/Gül Rengi) */}
            <Line
              type="monotone"
              name="Favorites"
              dataKey="favorites"
              stroke="#f43f5e" // Tailwind rose-500
              strokeWidth={3}
              dot={{ r: 4, strokeWidth: 2, fill: "hsl(var(--background))" }}
              activeDot={{ r: 6, strokeWidth: 0, fill: "#f43f5e" }}
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}