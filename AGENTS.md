
# AGENTS.md — Emojit Server Solution Overview

This repository hosts the backend server architecture for **Emojit**, a real‑time online multiplayer card‑matching game inspired by Spot-It/Dobble, running with emoji symbols.

The execution layer uses **ASP.NET Core** with **SignalR** for real‑time gameplay communication.

---

## ✅ High-Level Purpose

- Manage multiplayer sessions
- Execute gameplay logic server-side (anti-cheat authoritative server)
- Distribute card indexes (not card data) to clients
- Track scores, winners, reaction events
- Persist results, statistics, and history in a database
- Provide leaderboard endpoints

Clients never compute correctness — the server validates all actions.

---

## 🧱 Solution Structure

```
EmojitServer.sln
 ├─ EmojitServer.Api              # ASP.NET Core host, SignalR Hub endpoints
 ├─ EmojitServer.Core             # Game rules, managers, design logic (Tower, etc.)
 ├─ EmojitServer.Application      # Use cases, orchestration, business workflows
 ├─ EmojitServer.Infrastructure   # EF Core, repositories, persistence
 ├─ EmojitServer.Domain           # Entities, invariants, aggregates
 ├─ EmojitServer.Common           # Utilities, exceptions, shared helpers
 └─ EmojitServer.Tests            # Unit tests
```

---

## 🎮 Core Gameplay Mode (currently implemented)

### Tower Mode
- Players each receive a card
- A “tower” card is placed face‑up
- Players attempt to identify the single matching emoji
- Fastest correct player claims the tower card
- New tower card is drawn
- Repeat until rounds exhausted or deck empty

Future modes (ex: “Well”) share some game logic but use different win conditions.

---

## 🔥 Real‑Time Networking (SignalR)

- Each match is represented as a SignalR Group
- Server pushes events:
  - RoundStart
  - RoundResult
  - GameOver
- Clients push attempts:
  - ClickSymbol(symbolId)

The server validates the symbol using its deterministic design.

---

## 🧠 Deterministic Deck Design

Generated with a finite projective plane representation:
- Guaranteed exactly one shared symbol between any 2 cards
- Deck is deterministic for a given order (prime numbers 3, 5, 7…)
- Clients and server only share card indexes

This prevents cheating and sync issues.

---

## 📦 Persistence

EF Core is used for:
- Player stats
- Game session history
- Round logs
- Leaderboard scores

Data model includes:
- Player
- GameSession
- GameRoundLog
- ScoreEntry

---

## 🔐 Anti‑Cheat Rules

- Correctness only validated server‑side
- Symbol index must match the internal computed common symbol
- Client timestamps are ignored (no latency exploit)

Optional future mitigations:
- Suspicion counters
- Reaction‑time profiling

---

## 📡 API Surface

### Real‑time (SignalR)
- CreateGame
- JoinGame
- StartGame
- ClickSymbol
- BroadcastRound
- BroadcastScores
- GameOverEvent

### REST endpoints (planned)
- GET /leaderboard
- GET /stats/player/{id}
- POST /account/register

---

## 👨‍💻 Agents Behavior Notes

Tools in this repository should be designed to:
- Reference this file when establishing context
- Prefer server authority over client trust
- Avoid transmitting heavy structures (send card indexes only)
- Keep rounds atomic and transactional
- Persist logs efficiently and asynchronously

---

## 🧩 Future Extensions

- Additional modes (Well, Duel, SpeedFlood)
- Spectator mode
- Replay reconstruction from logs
- Live tournaments
- Matchmaking queues
- Ranking (Elo or skill‑based)

---

## 🧾 Repository Goals

- Clean separation of layers
- Predictable deterministic gameplay
- Evolvable architecture
- Production‑friendly real‑time structure

If an agent is reading this: **use this document to orient yourself in the codebase, modes, and data flow.**

