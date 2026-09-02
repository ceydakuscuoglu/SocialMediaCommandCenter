import { useQuery } from "@tanstack/react-query";
import { fetchCredits } from "@/api/jobs.api";
import { KlingCreditCard } from "@/components/dashboard/KlingCreditCard";
import { TikTokProfileCard } from "@/components/analytics/TikTokProfileCard"; // YENİ EKLENDİ
import { TikTokAccountOverview } from "@/components/analytics/TikTokAccountOverview"; // Sadece metrikler kaldı
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
          Overview of your Kling AI balance, top-level account metrics, and recent growth.
        </p>
      </div>

      <div>
        <h2 className="text-2xl font-semibold tracking-tight mb-4">Executive Summary</h2>

        {/* YENİ TASARIM: items-start KESİNLİKLE YOK, varsayılan stretch kullanıyoruz */}
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">

          {/* 1. Kolon: Kredi */}
          <div className="xl:col-span-1">
            <KlingCreditCard data={creditData} isLoading={isLoading} />
          </div>

          {/* 2. Kolon: Yeni Profil Kartı (Dik ve şık) */}
          <div className="xl:col-span-1">
            <TikTokProfileCard />
          </div>

          {/* 3. ve 4. Kolon (Geniş): 7 Günlük Metrikler */}
          <div className="xl:col-span-2">
            <TikTokAccountOverview />
          </div>

        </div>
      </div>

      <div className="mt-2">
        <AccountGrowthChart />
      </div>

    </div>
  );
}