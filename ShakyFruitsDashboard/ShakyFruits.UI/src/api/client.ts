// Merkezi API İstemcisi ve Yapılandırması
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5290/api";

export async function apiFetch<T = any>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
  const url = endpoint.startsWith("http") 
    ? endpoint 
    : `${API_BASE_URL}${endpoint.startsWith("/") ? "" : "/"}${endpoint}`;

  const response = await fetch(url, options);

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    const errorMessage =
      errorData.message ||
      errorData.Message ||
      errorData.error ||
      `API Error (${response.status}): ${response.statusText}`;
    throw new Error(errorMessage);
  }

  return response.json();
}
