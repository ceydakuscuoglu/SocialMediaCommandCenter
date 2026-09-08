import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Trophy, Zap, TrendingUp, Users, Waves } from "lucide-react";
import { LeaderboardsTab } from "./LeaderboardsTab";
import { CastSynergyTab } from "./CastSynergyTab";
import { AlgoTriggersTab } from "./AlgoTriggersTab";
import { GoldenHoursTab } from "./GoldenHoursTab";

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

          <TabsContent value="leaderboards">
            <LeaderboardsTab />
          </TabsContent>

          <TabsContent value="synergy">
            <CastSynergyTab />
          </TabsContent>

          <TabsContent value="algorithm">
            <AlgoTriggersTab />
          </TabsContent>

          <TabsContent value="surf">
            <GoldenHoursTab />
          </TabsContent>
        </Tabs>
      </CardContent>
    </Card>
  );
}