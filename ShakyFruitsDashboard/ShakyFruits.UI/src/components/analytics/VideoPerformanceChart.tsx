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

  const formattedData = useMemo(() => {
    if (!historyData) return [];

    const sortedData = [...historyData].sort((a, b) =>
      new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime()
    );

    return sortedData.map(snap => {
      const date = new Date(snap.recordedAt);
      return {
        ...snap,
        displayDate: date.toLocaleDateString("tr-TR", {
          month: "short",
          day: "numeric",
          hour: "2-digit",
          minute: "2-digit"
        })
      };
    });
  }, [historyData]);

  if (formattedData.length === 0) {
    return (
      <Card className="bg-card/50 backdrop-blur-sm border-border/50 h-full flex flex-col items-center justify-center text-muted-foreground min-h-[350px]">
        No analytics data available for this timeline.
      </Card>
    );
  }

  // --- MATEMATİKSEL NOKTA HESAPLAMASI ---
  const dataLength = formattedData.length;
  const step = Math.max(1, Math.floor(dataLength / 10));

  const renderCustomDot = (props: any, color: string) => {
    const { cx, cy, index } = props;

    if (index % step === 0 || index === dataLength - 1) {
      return (
        <circle
          key={`dot-${index}`}
          cx={cx}
          cy={cy}
          r={3.2}
          strokeWidth={2.5}
          stroke={color}
          /* 
            SVG'nin HSL okuyamama sorununu çözmek için sabit beyaz verdik. 
            Eğer karanlık tema kullanırsan Tailwind'in 'fill-background' 
            sınıfı bunu otomatik ezecek ve karta uyum sağlayacaktır. 
          */
          fill="#ffffff"
          className="fill-background"
        />
      );
    }
    return null;
  };

  return (
    <Card className="bg-card/50 backdrop-blur-sm border-border/50 shadow-sm w-full h-full flex flex-col">
      <CardHeader>
        <CardTitle className="text-lg">{title}</CardTitle>
        <CardDescription>Views, Likes, and Favorites progression based on worker snapshots.</CardDescription>
      </CardHeader>
      <CardContent className="flex-1 min-h-[350px] w-full pb-4">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={formattedData} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>

            <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" vertical={false} opacity={0.5} />

            <XAxis
              dataKey="displayDate"
              stroke="hsl(var(--muted-foreground))"
              fontSize={11}
              tickLine={false}
              axisLine={false}
              dy={10}
              minTickGap={40}
            />

            <YAxis
              stroke="hsl(var(--muted-foreground))"
              fontSize={11}
              tickLine={false}
              axisLine={false}
              tickFormatter={(value) => value >= 1000 ? `${(value / 1000).toFixed(1)}k` : value}
              dx={-10}
            />

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

            <Legend iconType="circle" wrapperStyle={{ paddingTop: "20px" }} />

            <Line
              type="monotone" // Yeniden pürüzsüz kavisli formata döndük
              name="Total Views"
              dataKey="views"
              stroke="#0ea5e9"
              strokeWidth={3}
              // Fonksiyonu çalıştırıp sadece belirlediğimiz indexlere nokta basıyoruz
              dot={(props) => renderCustomDot(props, "#0ea5e9")}
              activeDot={{ r: 6, strokeWidth: 0, fill: "#0ea5e9" }}
            />

            <Line
              type="monotone"
              name="Likes"
              dataKey="likes"
              stroke="#8b5cf6"
              strokeWidth={3}
              dot={(props) => renderCustomDot(props, "#8b5cf6")}
              activeDot={{ r: 6, strokeWidth: 0, fill: "#8b5cf6" }}
            />

            <Line
              type="monotone"
              name="Favorites"
              dataKey="favorites"
              stroke="#f43f5e"
              strokeWidth={3}
              dot={(props) => renderCustomDot(props, "#f43f5e")}
              activeDot={{ r: 6, strokeWidth: 0, fill: "#f43f5e" }}
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}