import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Coins, Sparkles, Wallet, Crown } from "lucide-react";

export interface KlingCreditData {
  remainingCredits: number;
  membershipCredits: number;
  topUpCredits: number;
  bonusCredits: number;
}

interface KlingCreditCardProps {
  data?: KlingCreditData | null;
  isLoading?: boolean;
}

export function KlingCreditCard({ data, isLoading }: KlingCreditCardProps) {
  // 1. Yükleniyor Durumu (Skeleton)
  if (isLoading) {
    return (
      <Card className="w-full max-w-sm animate-pulse border-border/50 shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between pb-2">
          <CardTitle className="text-sm font-medium text-muted-foreground">Kling AI Balance</CardTitle>
          <Coins className="h-4 w-4 text-muted-foreground/50" />
        </CardHeader>
        <CardContent>
          <div className="h-10 w-32 bg-muted rounded mb-4"></div>
          <div className="space-y-3 pt-4 border-t border-border/50">
            <div className="h-3 w-full bg-muted rounded"></div>
            <div className="h-3 w-full bg-muted rounded"></div>
          </div>
        </CardContent>
      </Card>
    );
  }

  // 2. Veri Yoksa Güvenlik Çıkışı
  if (!data) return null;

  return (
    <Card className="w-full max-w-sm relative overflow-hidden border-primary/20 bg-gradient-to-br from-primary/5 to-transparent shadow-sm">
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium">Kling AI Balance</CardTitle>
        <Coins className="h-4 w-4 text-primary" />
      </CardHeader>
      <CardContent>
        {/* Ana Kredi Miktarı */}
        <div className="text-3xl font-bold text-primary tracking-tight">
          {data.remainingCredits.toFixed(2)}
        </div>
        <p className="text-xs text-muted-foreground mb-4 font-medium">Total available credits</p>

        {/* Alt Kırılımlar */}
        <div className="flex flex-col gap-2.5 pt-4 border-t border-border/50">
          
          <div className="flex justify-between items-center text-xs">
            <div className="flex items-center gap-1.5 text-muted-foreground">
              <Sparkles className="w-3.5 h-3.5 text-emerald-500" />
              <span>Bonus Credits</span>
            </div>
            <span className="font-semibold">{data.bonusCredits.toFixed(2)}</span>
          </div>
          
          <div className="flex justify-between items-center text-xs">
            <div className="flex items-center gap-1.5 text-muted-foreground">
              <Wallet className="w-3.5 h-3.5 text-blue-500" />
              <span>Top-Up Credits</span>
            </div>
            <span className="font-semibold">{data.topUpCredits.toFixed(2)}</span>
          </div>

          <div className="flex justify-between items-center text-xs">
            <div className="flex items-center gap-1.5 text-muted-foreground">
              <Crown className="w-3.5 h-3.5 text-amber-500" />
              <span>Membership</span>
            </div>
            <span className="font-semibold">{data.membershipCredits.toFixed(2)}</span>
          </div>

        </div>
      </CardContent>
    </Card>
  );
}