import { useQuery } from "@tanstack/react-query";
import { fetchJobs } from "@/api/jobs.api";

export const useJobs = () => {
    return useQuery({
        queryKey: ["jobs"],
        queryFn: fetchJobs,
        // ShakyFruits otomasyonu arka planda çalıştığı için tabloyu belirli aralıklarla (örn: her 10 saniyede bir) 
        // sessizce güncelleyerek (polling) canlı bir Dashboard deneyimi sunabiliriz. 
        // İstersen aşağıdaki satırı aktif edebilirsin:
        refetchInterval: 60000,
    });
};