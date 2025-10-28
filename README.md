# Emojit Server

Emojit is a real‑time multiplayer game inspired by Spot‑It / Dobble, where each pair of cards share exactly one symbol. 
Players compete by identifying the common emoji between their card and the tower card as fast as possible.

This repository contains the backend server responsible for:
- Game session orchestration
- Real‑time multiplayer communication (SignalR)
- Deck generation (Projective Plane / Finite Fields)
- Round logging and result persistence
- Leaderboard updates
- Anti‑cheat validation
- Player scoring

---

## 🏗 Architecture Overview

The solution follows a clean layered architecture:

```
EmojitServer.Api          → SignalR Hub / REST endpoints / Hosting
EmojitServer.Application  → Use cases, coordinators, validation, DTO flow
EmojitServer.Core         → Game design & gameplay logic (deterministic)
EmojitServer.Domain       → Entities, value objects, invariants
EmojitServer.Infrastructure → Persistence, EF Core, repositories
EmojitServer.Common       → Shared utilities, exceptions, abstractions
EmojitServer.Tests        → Unit & integration tests
```

This separation allows clean CQRS‑like orchestration, testability, and future scalability.

---

## 🎮 Gameplay Mode (Tower)

Initial mode implemented:
- Each player holds a current card
- A shared tower card is visible
- Both cards share exactly one symbol
- First to click the correct emoji wins the round
- Winner claims the tower card
- Next card is revealed
- Game ends when `MaxRounds` is reached or deck is exhausted

Future planned modes include:
- Well (Hole)
- Tournaments
- Spectator mode
- Replay system

---

## 💻 Technologies

- **.NET 8**
- **ASP.NET Core Web API**
- **SignalR** (real‑time communication)
- **Entity Framework Core**
- **SQL Server**
- **XUnit** (tests)
- Optional:
  - Serilog
  - Mapster / AutoMapper
  - OpenTelemetry

---

## 📦 Database Schema (Simplified)

- `Players`
- `GameSessions`
- `RoundLogs`
- `LeaderboardEntries`

Rounds & sessions are logged for ranking, validation, analytics and anti‑cheat heuristics.

---

## 🔧 Development Environment

### Requirements:
- .NET 8 SDK
- SQL Server (local / Docker)
- Git

### Run locally

```
dotnet run --project EmojitServer.Api
```

Swagger UI available by default.

---

## 🚀 Docker

```
docker build -t emojit-server .
docker run -p 8080:8080 emojit-server
```

(Compose configuration coming soon)

---

## 🧪 Tests

```
dotnet test
```

Tests include:
- Symbol intersection validation
- Gameplay scoring progression
- Infrastructure persistence

---

## 🧠 Deterministic Deck Generation

Decks are generated using Projective Plane combinatorics (GF(n)).

Properties:
- `n + 1` symbols per card
- Every pair of cards share **exactly one** symbol
- Fully reproducible for multiplayer synchronization

Supported orders: 3, 5, 7 (primes only)

---

## 🧰 Repository Files Worth Reading

- `AGENTS.md` → Guidance for AI agents (Codex, ChatGPT, etc.)
- `CHECKLIST.md` → Development roadmap
- `*.Domain/*` → Entities / Value Objects
- `*.Core/Design` → Deck logic

---

## 🕸 SignalR Hub Events (server → client)

- `RoundStart`
- `RoundResult`
- `GameOver`

## SignalR Calls (client → server)

- `CreateGame`
- `JoinGame`
- `StartGame`
- `ClickSymbol`

---

## 🛡 Anti‑Cheat Goals

Future enhancements:
- Latency‑normalized timing
- Reaction time anomaly detection
- Device fingerprinting heuristics

---

## 🏆 Leaderboard

Rounds and sessions update global ranking based on:
- Reaction performance
- Win/loss ratio
- Aggregate scoring

Future plan: ELO variant

---

## 🤝 Contributing

Pull requests welcome!
Please follow:
- Meaningful commit messages (Conventional Commits recommended)
- Clean architecture boundaries
- Proper XML documentation

---

## 📜 License

Currently private and proprietary development sandbox.
Will update if open‑sourced.

---

## 👨‍💻 Author
Repository owner: BlinkSun  
Game designer / architect: Damien Villeneuve

Happy hacking! 🔥
