# OpsFlow — Lightweight workflow & service request portal for small teams

A minimal, professional “service request/work order” portal:
- React + TypeScript UI
- ASP.NET Core Web API (.NET 8)
- SQL Server (Docker)
- EF Core migrations
- xUnit tests (unit + basic integration)
- GitHub Actions CI

## Stack
- Backend: ASP.NET Core Web API (.NET 8)
- DB: SQL Server (Docker)
- ORM: EF Core (SQL Server provider)
- Frontend: React + TypeScript
- CI: GitHub Actions
- Tests: xUnit (+ basic integration tests)

## Run (local)
### 1 Start SQL Server
```bash
cp .env.example .env
docker compose up -d

Run backend (later)
# placeholder

3 Run frontend (later)
# placeholder

Test (later)
# placeholder

Architecture

UI → API → SQL Server


### 0.4 Add .gitignore (root)
Create `.gitignore`:
```gitignore
# OS
.DS_Store

# Env
.env

# .NET
bin/
obj/
*.user
*.suo
*.cache
*.log

# Node / React
node_modules/
dist/
build/
.vite/
npm-debug.log*
yarn-debug.log*
yarn-error.log*

# IDE
.vscode/
.idea/