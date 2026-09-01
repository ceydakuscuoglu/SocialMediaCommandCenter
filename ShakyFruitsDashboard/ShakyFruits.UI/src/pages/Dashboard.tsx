import { useQuery } from "@tanstack/react-query";
import { fetchCredits } from "@/api/jobs.api";
import { KlingCreditCard } from "@/components/dashboard/KlingCreditCard";
import { TikTokAccountOverview } from "@/components/analytics/TikTokAccountOverview";

export function Dashboard() {
  const { data: creditData, isLoading } = useQuery({
    queryKey: ["kling-credits"],
    queryFn: fetchCredits,
    refetchOnWindowFocus: false, 
  });

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      <div>
        <h2 className="text-2xl font-semibold tracking-tight mb-4">Executive Summary</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <KlingCreditCard data={creditData} isLoading={isLoading} />
          {/* İleride buraya başka özet kartları ekleyebilirsin */}
        </div>
      </div>
      
      <div>
        <TikTokAccountOverview />
      </div>
    </div>
  );
}