// Extracts the file name from a full local path (e.g., D:\TempAssets\...)
export const getFileName = (path: string | null): string => {
  if (!path) return "-";
  return path.split('\\').pop()?.split('/').pop() || path;
};

// Yeni: Tarih formatlayıcı (Örn: "Aug 18, 14:30")
export const formatDate = (dateString: string | null): string => {
  if (!dateString) return "-";
  
  const date = new Date(dateString);
  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    hour12: false // 24 saat dilimi kullanmak istersen (isteğe bağlı)
  }).format(date);
};