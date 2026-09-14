# 🍓 ShakyFruits Dashboard UI (React 19 + Tauri v2)

[![React 19](https://img.shields.io/badge/React-19.0-61DAFB?style=for-the-badge&logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.8-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Vite](https://img.shields.io/badge/Vite-7.0-646CFF?style=for-the-badge&logo=vite&logoColor=white)](https://vitejs.dev/)
[![Tauri v2](https://img.shields.io/badge/Tauri-v2-FFC131?style=for-the-badge&logo=tauri&logoColor=black)](https://v2.tauri.app/)
[![Tailwind CSS v4](https://img.shields.io/badge/Tailwind_CSS-v4.0-06B6D4?style=for-the-badge&logo=tailwindcss&logoColor=white)](https://tailwindcss.com/)
[![TanStack Query](https://img.shields.io/badge/TanStack_Query-v5-FF4154?style=for-the-badge&logo=reactquery&logoColor=white)](https://tanstack.com/query)

The official desktop and web client for the **ShakyFruits Dashboard & Automation Center**. Built with **React 19**, **TypeScript**, **Tailwind CSS v4**, and **Tauri v2**, offering high-performance real-time monitoring, asset management, AI video pipeline control, and social media analytics.

---

## 🚀 Key Modules & Pages

- **📊 Dashboard (`/src/pages/Dashboard.tsx`)**: High-level command overview displaying active account metrics, quick stats, active generation jobs, and real-time social alerts.
- **🎬 Assets Manager (`/src/pages/Assets.tsx`)**:
  - Fruit character asset library management.
  - Reference dance video registry.
  - Character synergy prompt configuration (solo vs. multi-fruit rules).
- **⚡ Generation Pipeline (`/src/pages/Pipeline.tsx`)**:
  - Real-time video rendering queue management.
  - Interactive Kling AI bot orchestrator (Prepare & Cost estimation, Confirm execution).
  - Credit status monitor.
- **🎯 Strategy & Discovery (`/src/pages/Strategy.tsx`)**:
  - Live TikTok Creative Center trend feeds (breakout music and hashtags).
  - AI Caption Studio powered by Google Gemini.
  - Optimal posting time heatmaps (Golden Hours).
  - Character performance leaderboards.
- **📈 Analytics (`/src/pages/Analytics.tsx`)**:
  - Follower velocity and engagement rate breakdown across TikTok & Instagram.
  - Interactive performance charts powered by **Recharts**.
  - Solo vs. Group character ROI comparison.

---

## 🛠️ Tech Stack

- **Runtime & Bundler**: Vite 7 + TypeScript
- **UI Framework**: React 19
- **Desktop Shell**: Tauri v2
- **Styling**: Tailwind CSS v4, `@tailwindcss/postcss`, Radix UI primitives, shadcn
- **State & Server Cache**: `@tanstack/react-query` v5
- **Charts**: Recharts
- **Icons**: Lucide React & Hugeicons

---

## 📦 Getting Started

### Prerequisites

- **[Node.js](https://nodejs.org/)** (v18.0 or newer recommended)
- **npm** or **pnpm**
- *(For Tauri Desktop App only)*: [Tauri Prerequisites](https://v2.tauri.app/start/prerequisites/) (Rust, Cargo, and platform C++ build tools)

### Installation

```bash
# Navigate to UI directory
cd ShakyFruits.UI

# Install dependencies
npm install
```

### Development Scripts

| Command | Description |
|---|---|
| `npm run dev` | Runs the web app in development mode with Hot Module Replacement (HMR) at `http://localhost:5173` |
| `npm run tauri dev` | Launches the application as a native cross-platform desktop window using Tauri v2 |
| `npm run build` | Compiles TypeScript and builds production-ready static assets to `/dist` |
| `npm run preview` | Locally previews the production build |

---

## 🔌 API Backend Connection

The UI connects to the **ShakyFruits.API** backend (`http://localhost:5000` or configured URL).
CORS is configured via `TauriCorsPolicy` in the ASP.NET Core API to allow both browser origin and Tauri desktop protocols.

API client methods and types are organized under `src/api/`:
- `assets.api.ts`: Character sprites, reference videos, and synergy rules.
- `generation.api.ts`: Video generation queue, Kling bot triggers, and Gemini captions.
- `analytics.api.ts`: Growth charts, engagement stats, and scraper feeds.

---

## 🎨 Recommended IDE Setup

- [VS Code](https://code.visualstudio.com/)
- [Tauri Extension](https://marketplace.visualstudio.com/items?itemName=tauri-apps.tauri-vscode)
- [Tailwind CSS IntelliSense](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss)
- [ESLint](https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint)
