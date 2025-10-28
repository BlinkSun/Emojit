# Emojit Server Development Checklist

## ✅ Étape 0 — Bootstrap de la solution (dotnet CLI)
- [x] Installer/valider .NET 8 SDK (`dotnet --info`)
- [x] Créer le dossier racine : `mkdir Emojit && cd Emojit`
- [x] Créer la solution : `dotnet new sln -n EmojitServer`
- [x] Créer les projets :
- [x] `dotnet new webapi -n EmojitServer.Api`
- [x] `dotnet new classlib -n EmojitServer.Domain`
- [x] `dotnet new classlib -n EmojitServer.Core`
- [x] `dotnet new classlib -n EmojitServer.Application`
- [x] `dotnet new classlib -n EmojitServer.Infrastructure`
- [x] `dotnet new classlib -n EmojitServer.Common`
- [x] `dotnet new xunit -n EmojitServer.Tests`
- [x] Ajouter les projets à la solution
- [x] Définir les références/dépendances
- [x] Structurer les dossiers internes
- [x] Ajouter packages NuGet
- [x] Commit initial `git init && git add . && git commit -m "chore: bootstrap solution"`

## ✅ Étape 1 — Domain (entités / invariants)
- [x] Créer entités :
  - [x] Player
  - [x] GameSession
  - [x] RoundLog
  - [x] LeaderboardEntry
- [x] Créer Value Objects / enums :
  - [x] GameMode enum (Tower, Well)
  - [x] GameId, PlayerId (optionnel)
- [x] Ajouter règles d’intégrité
- [x] Ajouter XML docs & noms significatifs

## ✅ Étape 2 — Core (moteur design & gameplay)
- [x] Implémenter EmojitDesign (équivalent SpotItDesign)
- [x] Ajouter GetCard, FindCommonSymbol, Validate, GetStats
- [x] Définir interface IGameMode
- [ ] Implémenter TowerGameManager
- [ ] Mélange deck / avancement rounds / scoring
- [ ] Gestion MaxRounds configurable
- [ ] Exceptions & XML docs

## ✅ Étape 3 — Infrastructure (EF Core + Repos)
- [ ] Créer EmojitDbContext + DbSet<>
- [ ] Configurer OnModelCreating
- [ ] Créer Repositories :
  - [ ] IPlayerRepository
  - [ ] IGameSessionRepository
  - [ ] IRoundLogRepository
  - [ ] ILeaderboardRepository
- [ ] ConnectionString appsettings.json
- [ ] Migrations EF :
  - [ ] `dotnet ef migrations add InitialCreate -p EmojitServer.Infrastructure -s EmojitServer.Api`
  - [ ] `dotnet ef database update -p EmojitServer.Infrastructure -s EmojitServer.Api`

## ✅ Étape 4 — Application (services logiques)
- [ ] GameService :
  - [ ] CreateGame
  - [ ] JoinGame
  - [ ] StartGame
  - [ ] ClickSymbol
  - [ ] GetScoresSnapshot
  - [ ] Persist endgame
- [ ] LeaderboardService
- [ ] LogService
- [ ] ValidationService (anti-cheat basic)

## ✅ Étape 5 — API (ASP.NET host)
- [ ] Config Program.cs (DI, Swagger, CORS)
- [ ] Ajouter services Application/Infrastructure/Core
- [ ] Ajouter SignalR
- [ ] REST Endpoints optionnels :
  - [ ] LeaderboardController
  - [ ] StatsController
  - [ ] HealthController

## ✅ Étape 6 — SignalR Hub (temps réel)
- [ ] Créer GameHub
- [ ] Méthodes :
  - [ ] CreateGame
  - [ ] JoinGame
  - [ ] StartGame
  - [ ] ClickSymbol
- [ ] Groups par gameId
- [ ] Broadcast :
  - [ ] RoundStart
  - [ ] RoundResult
  - [ ] GameOver
- [ ] Gestion erreurs/logs

## ✅ Étape 7 — Contracts (DTOs / messages)
- [ ] Créer Contracts/DTOs :
  - [ ] CreateGameRequest
  - [ ] JoinGameRequest
  - [ ] ClickSymbolRequest
- [ ] Events côté serveur :
  - [ ] RoundStartEvent
  - [ ] RoundResultEvent
  - [ ] GameOverEvent
- [ ] DTO leaderboard/stats
- [ ] Mapper automatique (Mapster/AutoMapper)

## ✅ Étape 8 — Config & Settings
- [ ] appsettings.json :
  - [ ] ConnectionStrings
  - [ ] CORS origins
  - [ ] GameDefaults
- [ ] Options binding IOptions<T>
- [ ] Env vars prod

## ✅ Étape 9 — Logging & Observabilité
- [ ] Serilog (ou ILogger)
- [ ] Healthchecks DB
- [ ] Metrics (OpenTelemetry optionnel)

## ✅ Étape 10 — Tests
- [ ] Domain/Core unit:
  - [ ] Validate
  - [ ] FindCommonSymbol
  - [ ] TowerGameManager
- [ ] Application tests :
  - [ ] GameService flux
- [ ] Integration tests :
  - [ ] Migrations DbContext

## ✅ Étape 11 — Sécurité
- [ ] CORS strict
- [ ] Limiter taille messages SignalR
- [ ] Rate limiting
- [ ] (Future) JWT Auth

## ✅ Étape 12 — Déploiement local
- [ ] `dotnet run --project EmojitServer.Api`
- [ ] Dockerfile API
- [ ] docker-compose SQL Server/Postgres
- [ ] Seed DB demo

## ✅ Étape 13 — CI/CD (GitHub Actions)
- [ ] Build + test
- [ ] Publish artifacts
- [ ] Docker build/push
- [ ] Env dev/staging/prod

## ✅ Étape 14 — Scalabilité & État
- [ ] Stocker parties : ConcurrentDictionary
- [ ] (Future) Redis Scale-out SignalR
- [ ] Timeouts AFK
- [ ] Cleanup sessions

## ✅ Étape 15 — Leaderboard & Logs (raffinements)
- [ ] UpdateLeaderboard (ELO ou score)
- [ ] Export logs / replays
- [ ] Spectator mode

## ✅ Étape 16 — Backlog Futur
- [ ] Implémenter Well mode
- [ ] Replay viewer
- [ ] Tournois
- [ ] Anti-cheat avancé
