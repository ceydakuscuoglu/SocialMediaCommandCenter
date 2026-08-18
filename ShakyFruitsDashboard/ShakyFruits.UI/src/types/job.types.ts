//Projenin her yerinde kullanacağımız, backend'den gelen JSON'ın birebir yansıması olan sözleşmemiz (Interface).
export enum GenerationStatus {
  Pending = 0,
  Processing = 1,
  Completed = 2,
  Failed = 3
}

export interface VideoJob {
  id: number;
  fruitImagePath: string;
  referenceVideoPath: string;
  isRecreate: boolean;
  appliedPrompt: string;
  status: GenerationStatus; // Artık string değil, bu numaralı enum tipinde
  errorMessage: string | null;
  outputVideoPath: string | null;
  createdAt: string; // C# DateTime'ı JSON'da ISO string olarak gelir
}