import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Trophy, Zap, TrendingUp, Users, Waves, Flame } from "lucide-react";
import { LeaderboardsTab } from "./LeaderboardsTab";
import { CastSynergyTab } from "./CastSynergyTab";
import { AlgoTriggersTab } from "./AlgoTriggersTab";
import { GoldenHoursTab } from "./GoldenHoursTab";
import { LiveTrendsTable } from "./LiveTrendsTable";

export function AdvancedInsights() {
  return (
    <Card className="w-full border-border/40 shadow-sm bg-card mt-8">
      <CardHeader className="border-b border-border/40 pb-4">
        <CardTitle className="text-xl flex items-center gap-2">
          <Zap className="w-5 h-5 text-amber-500 fill-current" />
          Business Intelligence & Strategy
        </CardTitle>
        <CardDescription>
          Deep dive into algorithmic triggers, audience psychology, and optimal posting times.
        </CardDescription>
      </CardHeader>

      <CardContent className="pt-6">
        <Tabs defaultValue="leaderboards" className="w-full">
          <TabsList className="grid w-full grid-cols-1 md:grid-cols-5 bg-muted/50 p-1 rounded-lg h-auto md:h-12">

            {/* Altın / Amber Rengi - Liderlik */}
            <TabsTrigger value="leaderboards" className="flex items-center justify-center gap-2 h-10">
              <Trophy className="w-4 h-4 text-amber-500" /> <span>Top Charts</span>
            </TabsTrigger>

            {/* Mor / Violet - Ekip Sinerjisi */}
            <TabsTrigger value="synergy" className="flex items-center justify-center gap-2 h-10">
              <Users className="w-4 h-4 text-violet-500" /> <span>Cast Synergy</span>
            </TabsTrigger>

            {/* Zümrüt Yeşili - Büyüme/Algoritma */}
            <TabsTrigger value="algorithm" className="flex items-center justify-center gap-2 h-10">
              <TrendingUp className="w-4 h-4 text-emerald-500" /> <span>Algo Triggers</span>
            </TabsTrigger>

            {/* Okyanus Mavisi - Dalga Sörfü */}
            <TabsTrigger value="surf" className="flex items-center justify-center gap-2 h-10">
              <Waves className="w-4 h-4 text-cyan-500" /> <span>Surf Strategy</span>
            </TabsTrigger>

            {/* Ateş Kırmızısı - Canlı Trendler */}
            <TabsTrigger value="trends" className="flex items-center justify-center gap-2 h-10">
              <Flame className="w-4 h-4 text-rose-500" /> <span>Live Trends</span>
            </TabsTrigger>

          </TabsList>

          <TabsContent value="leaderboards" className="mt-6">
            <LeaderboardsTab />
          </TabsContent>

          <TabsContent value="synergy" className="mt-6">
            <CastSynergyTab />
          </TabsContent>

          <TabsContent value="algorithm" className="mt-6">
            <AlgoTriggersTab />
          </TabsContent>

          <TabsContent value="surf" className="mt-6">
            <GoldenHoursTab />
          </TabsContent>

          <TabsContent value="trends" className="mt-6">
            <LiveTrendsTable />
          </TabsContent>
        </Tabs>
      </CardContent>
    </Card>
  );
}