import { useQuery } from "@tanstack/react-query";
import { fetchCredits } from "@/api/jobs.api";
import { KlingCreditCard } from "@/components/dashboard/KlingCreditCard";
import { TikTokProfileCard } from "@/components/dashboard/TikTokProfileCard";
import { InstagramProfileCard } from "@/components/dashboard/InstagramProfileCard";
import { TikTokAccountOverview } from "@/components/dashboard/TikTokAccountOverview";
import { InstagramAccountOverview } from "@/components/dashboard/InstagramAccountOverview";
import { AccountGrowthChart } from "@/components/dashboard/AccountGrowthChart";
import { LayoutDashboard } from "lucide-react";

export function Dashboard() {
  const { data: creditData, isLoading } = useQuery({
    queryKey: ["kling-credits"],
    queryFn: fetchCredits,
    staleTime: 24 * 60 * 60 * 1000,
    refetchOnWindowFocus: false,
  });

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      <div className="flex flex-col gap-1">
        <h2 className="text-2xl font-semibold tracking-tight flex items-center gap-2">
          <LayoutDashboard className="w-6 h-6 text-primary" />
          Dashboard
        </h2>
        <p className="text-muted-foreground">
          Overview of your Kling AI balance, top-level account metrics, and recent growth across platforms.
        </p>
      </div>

      <div>
        <h2 className="text-xl font-semibold tracking-tight mb-4 text-foreground/90">Executive Summary</h2>
        
        {/* ÜST SATIR: Bakiye ve Profil Kartları (3 Eşit Kolon) */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
          <KlingCreditCard data={creditData} isLoading={isLoading} />
          <TikTokProfileCard />
          <InstagramProfileCard />
        </div>

        {/* ORTA SATIR: 7 Günlük Platform Metrikleri (2 Geniş Kolon) */}
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
          <TikTokAccountOverview />
          <InstagramAccountOverview />
        </div>
      </div>

      {/* ALT SATIR: Kombine Büyüme Grafiği */}
      <div className="mt-2">
        <AccountGrowthChart />
      </div>

    </div>
  );
}