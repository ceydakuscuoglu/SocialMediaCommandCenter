import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './App.css' // CSS dosyamızın doğru şekilde import edildiğinden emin ol
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

// QueryClient örneğini oluşturuyoruz
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false, // Pencereye her odaklandığımızda API'ye tekrar istek atmasını engelliyoruz
      retry: 1, // Hata olursa 1 kez daha denesin
    },
  },
})

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </React.StrictMode>,
)