# 🍓🕺 ShakyFruits Dashboard & Automation Center

![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQLServer-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Playwright](https://img.shields.io/badge/Playwright-2EAD33?style=for-the-badge&logo=playwright&logoColor=white)

An end-to-end automated Social Media Command Center built for the **ShakyFruits** brand. This project automates the generation of AI-driven character animation videos (using Kling AI) and manages assets, daily social media trends, and analytics within a robust N-Layer Clean Architecture.

## ✨ Features

- **🤖 Automated AI Video Generation:** Uses Microsoft Playwright to autonomously log into AI video platforms, upload assets, inject dynamic configurations (fix prompts), and download generated output videos.
- **📈 Trend Discovery:** Automated web scraping module to fetch and store daily TikTok/Instagram trends (music, hashtags) to drive content strategy.
- **📂 Smart Asset Management:** Manages dynamic relationships between reference dance videos and multi-fruit image assets, assigning specific AI text prompts based on single/multiple fruit detections.
- **⚙️ Background Processing:** Integrates **Hangfire** for scheduling automated publishing, scraping, and long-running generation tasks without blocking the main API thread.
- **🏛️ Clean Architecture:** Designed with strict separation of concerns (API, Core, Data, Services) using the Code-First approach.

## 🏗️ Architecture (N-Layer / Clean Architecture)

The solution is divided into four distinct projects:
- `ShakyFruits.API`: The presentation layer (REST API) serving data to the frontend.
- `ShakyFruits.Core`: The domain layer containing Entities (FruitAssets, ReferenceVideos, VideoGenerations), Enums, and business models.
- `ShakyFruits.Data`: The data access layer utilizing Entity Framework Core to communicate with SQL Server.
- `ShakyFruits.Services`: The business logic layer containing Playwright automation bots and Hangfire background jobs.

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0+ SDK](https://dotnet.microsoft.com/download)
- SQL Server (Local or Remote)
- Node.js (Optional, for Playwright dependencies)
