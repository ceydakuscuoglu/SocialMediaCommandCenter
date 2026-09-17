import { useQuery } from "@tanstack/react-query";
import { fetchSoloVsGroup, fetchFruitCombinations } from "@/api/analytics.api";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  ScatterChart,
  Scatter,
  XAxis,
  YAxis,
  ZAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell
} from "recharts";

// Özel Bilgi Baloncuğu (Tooltip) Tasarımı
const CustomTooltip = ({ active, payload }: any) => {
  if (active && payload && payload.length) {
    const data = payload[0].payload;
    return (
      <div className="bg-card border border-border/50 p-3 rounded-lg shadow-md text-sm min-w-[150px]">
        {/* ID yerine doğrudan Başlık (Combination) gösteriliyor */}
        <p className="font-semibold text-primary mb-2 pb-1 border-b border-border/50">
          {data.combination || "Unknown Title"}
        </p>
        <div className="flex flex-col gap-1">
          <p className="text-muted-foreground flex justify-between gap-4">
            <span>Avg Views:</span>
            <span className="font-medium text-foreground">{data.averageViews?.toLocaleString()}</span>
          </p>
          <p className="text-muted-foreground flex justify-between gap-4">
            <span>Viral Factor:</span>
            <span className="font-medium text-foreground">{data.averageViralFactor}%</span>
          </p>
          <p className="text-muted-foreground flex justify-between gap-4">
            <span>Videos:</span>
            <span className="font-medium text-foreground">{data.videoCount}</span>
          </p>
        </div>
      </div>
    );
  }
  return null;
};

export function CastSynergyTab() {
  const { data: soloVsGroup } = useQuery({ queryKey: ["solo-group"], queryFn: fetchSoloVsGroup });
  const { data: combinations } = useQuery({ queryKey: ["combinations"], queryFn: fetchFruitCombinations });

  return (
    <div className="mt-6 space-y-8">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {soloVsGroup?.map((stat, i) => (
          <Card key={i} className="bg-muted/20 border-border/50">
            <CardContent className="p-4 flex justify-between items-center">
              <div>
                <p className="text-sm font-medium text-muted-foreground">{stat.type}</p>
                <p className="text-2xl font-bold">
                  {stat.averageViews.toLocaleString()}{" "}
                  <span className="text-sm font-normal text-muted-foreground">avg views</span>
                </p>
              </div>
              <Badge variant={stat.type.includes("Multiple") ? "default" : "secondary"}>
                {stat.videoCount} Videos
              </Badge>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Sinerji Baloncuk Grafiği (Scatter Chart) */}
      <div className="space-y-4 border border-border/50 rounded-xl p-4 bg-muted/10">
        <div className="flex justify-between items-center">
          <h3 className="font-semibold text-foreground">Cast Synergy Sweet Spot</h3>
          <span className="text-xs text-muted-foreground">
            Top right is the best (High Views + High K-Factor)
          </span>
        </div>
        <div className="h-[300px] w-full mt-4">
          <ResponsiveContainer width="100%" height="100%">
            <ScatterChart margin={{ top: 10, right: 20, bottom: 10, left: 10 }}>
              <CartesianGrid strokeDasharray="3 3" opacity={0.2} vertical={false} />
              <XAxis
                type="number"
                dataKey="averageViews"
                name="Avg Views"
                tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }}
                tickFormatter={(val) => `${(val / 1000).toFixed(0)}k`}
              />
              <YAxis
                type="number"
                dataKey="averageViralFactor"
                name="Viral Factor"
                unit="%"
                tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }}
                axisLine={false}
                tickLine={false}
              />
              <ZAxis type="number" dataKey="videoCount" range={[100, 800]} name="Videos Published" />

              {/* Özel Tooltip Bileşenini Buraya Entegre Ettik */}
              <Tooltip
                cursor={{ strokeDasharray: "3 3", stroke: "hsl(var(--border))" }}
                content={<CustomTooltip />}
              />

              <Scatter name="Combinations" data={combinations} opacity={0.85}>
                {combinations?.map((entry, index) => {
                  // Temandaki chart-1'den chart-5'e kadar olan renkleri sırayla atıyoruz
                  const chartColor = `var(--chart-${(index % 5) + 1})`;
                  return (
                    <Cell
                      key={`scatter-cell-${index}`}
                      fill={chartColor}
                      className="hover:opacity-100 transition-opacity duration-200"
                    />
                  );
                })}
              </Scatter>
            </ScatterChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
}