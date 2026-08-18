import { VideoJob } from "@/types/job.types";

// CEYDAK sunucusundaki API adresimiz
// Not: Port numarasını (.NET API'nin çalıştığı port, örn: 5000, 5001 veya 7100 vb.) kendi yapına göre güncelle.
const API_BASE_URL = "http://localhost:5290/api";

export const fetchJobs = async (): Promise<VideoJob[]> => {
  const response = await fetch(`${API_BASE_URL}/BotTest/generations`);
  
  if (!response.ok) {
    throw new Error("Failed to fetch jobs from the .NET API");
  }
  
  return response.json();
};