# Local Setup Guide (Visual Studio on Windows)

This guide walks through setting up the Emojit server solution end-to-end on a Windows workstation using Visual Studio 2022. Follow each step in order to ensure the API, SignalR hub, and persistence layer all work together.

---

## 1. Prerequisites

1. **Operating System**: Windows 10/11 with administrative privileges.
2. **Visual Studio**: Install Visual Studio 2022 (17.8 or newer) with the following workloads:
   - ASP.NET and web development
   - .NET desktop development (for tooling support)
   - Data storage and processing (installs SQL Server tooling)
3. **.NET SDK**: Visual Studio installs .NET 8 automatically with the workloads above. Verify from a Developer PowerShell window with:
   ```powershell
   dotnet --info
   ```
4. **SQL Server**: Install SQL Server Express LocalDB (bundled with Visual Studio) or an accessible SQL Server instance. The default development connection string expects **`(localdb)\MSSQLLocalDB`**.
5. **Git**: Install Git for Windows if you prefer cloning outside Visual Studio.
6. **Postman**: Version 10+ (supports REST and WebSocket testing). Swagger UI is bundled with the API for quick validation.

---

## 2. Clone or Fetch the Repository

### Option A – Visual Studio Start Window
1. Launch Visual Studio.
2. Choose **Clone a repository**.
3. Enter the repository URL and destination path (for example `C:\Repos\Emojit`).
4. Click **Clone**. Visual Studio automatically opens the solution after cloning.

### Option B – Git CLI
1. Open **Developer PowerShell for VS 2022**.
2. Run:
   ```powershell
   git clone https://<your-origin>/Emojit.git
   cd Emojit
   ```
3. Double-click `EmojitServer.sln` to open it in Visual Studio.

---

## 3. Trust HTTPS Development Certificate

Visual Studio prompts you to trust the ASP.NET Core developer certificate the first time you debug an HTTPS profile. Accept the prompt or run the following once from Developer PowerShell:
```powershell
dotnet dev-certs https --trust
```
This avoids browser and Postman TLS warnings when calling `https://localhost:7092`.

---

## 4. Restore NuGet Packages

1. In **Solution Explorer**, right-click the solution node and select **Restore NuGet Packages**.
2. Alternatively, run `dotnet restore` from Developer PowerShell at the repository root.
3. Confirm that the **Output** window shows `Restore completed` with no errors.

---

## 5. Configure the Startup Project

1. In Solution Explorer, right-click **EmojitServer.Api** and choose **Set as Startup Project**.
2. Pick the desired launch profile:
   - `https` (recommended) — runs on `https://localhost:7092` and `http://localhost:5139`.
   - `IIS Express` — runs on the ports shown in `launchSettings.json` (default `https://localhost:44364`).

---

## 6. Application Settings

All runtime configuration lives in `EmojitServer.Api/appsettings.Development.json` (used by Visual Studio debug profiles) and `appsettings.json` (fallback).

Key sections to review:

| Section | Purpose | Notes |
|---------|---------|-------|
| `ConnectionStrings:EmojitDatabase` | SQL Server connection string | Default points to `(localdb)\MSSQLLocalDB`. Change to a full SQL Server instance if preferred. |
| `Cors` | Allowed origins/headers/methods | Include your front-end origins during development (e.g. `http://localhost:5173`). |
| `GameDefaults` | Lobby defaults and validation bounds | Adjust player and round limits only if testing alternate scenarios. |
| `Jwt` | Token issuer, audience, signing key, and lifetimes | **Set a strong signing key**. You can override with user secrets or environment variables named `EMOJIT_Jwt__SigningKey`, etc. |
| `RateLimiting` | Global throttling guardrails | Defaults allow 100 requests per minute per user/IP in Development. |
| `SignalRLimits` | Max inbound SignalR payload size | Default 32 KiB. Increase only if you have large custom events. |

> 💡 **Environment overrides**: Any setting can be overridden with environment variables prefixed by `EMOJIT_` (for example `EMOJIT_Cors__AllowedOrigins__0=https://localhost:5173`). This matches the `builder.Configuration.AddEnvironmentVariables("EMOJIT_")` call in `Program.cs`.

---

## 7. Database Initialization

### 7.1 Create or Verify the Database

1. Ensure your SQL Server instance (LocalDB or full SQL Server) is running.
2. Confirm the connection string in `appsettings.Development.json` points to your instance.

### 7.2 Apply Entity Framework Core Migrations

1. Open **Tools ▸ NuGet Package Manager ▸ Package Manager Console**.
2. Ensure the **Default project** dropdown is set to `EmojitServer.Infrastructure`.
3. Run:
   ```powershell
   Update-Database
   ```
   This creates the `EmojitServer` database and applies the `InitialCreate` migration.
4. Verify the database now contains the tables `Players`, `GameSessions`, `RoundLogs`, and `LeaderboardEntries`.

### 7.3 Seed Sample Players (Manual SQL)

Authentication requires pre-existing players. Insert a few records directly using SQL Server Object Explorer (SSOX) or `sqlcmd`:

```sql
USE [EmojitServer];
GO
INSERT INTO dbo.Players (Id, DisplayName, CreatedAtUtc, LastActiveAtUtc, GamesPlayed, GamesWon)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'Alice', SYSUTCDATETIME(), SYSUTCDATETIME(), 0, 0),
    ('22222222-2222-2222-2222-222222222222', 'Bob',   SYSUTCDATETIME(), SYSUTCDATETIME(), 0, 0);
GO
```
You can add more players later with any unique GUID and display name (max length 32).

> 📌 If you prefer scripts in source control, create a `.sql` file in the `Docs` folder or use a migration-based seed.

---

## 8. Launching the Server

1. Press **F5** (Debug) or **Ctrl+F5** (Run without debugging).
2. Visual Studio builds the solution, starts `EmojitServer.Api`, and opens the Swagger UI (`https://localhost:7092/swagger`).
3. Watch the **Output ▸ Debug** window — structured Serilog logs now appear there because of the Serilog configuration in `Program.cs`.
4. Confirm the `/healthz` endpoint returns `Healthy` (Swagger ➜ `GET /healthz`).

If the application fails to start:
- Check the Output window for Serilog errors about missing configuration.
- Ensure the connection string is valid (SQL login errors appear as `SqlException`).
- Verify ports `7092`/`5139` are unused; adjust in `launchSettings.json` if needed.

---

## 9. Testing with Swagger UI (REST Endpoints)

### 9.1 Authentication (JWT)
1. Navigate to **POST `/api/authentication/token`**.
2. Click **Try it out** and enter a payload matching a seeded player:
   ```json
   {
     "playerId": "11111111-1111-1111-1111-111111111111",
     "displayName": "Alice"
   }
   ```
3. Execute the request. Expected `200 OK` response:
   ```json
   {
     "accessToken": "<JWT>",
     "tokenType": "Bearer",
     "expiresAtUtc": "2024-03-01T12:34:56.0000000Z"
   }
   ```
4. Copy the `accessToken` for SignalR testing.

### 9.2 Leaderboard & Stats
- **GET `/api/leaderboard/top?count=5`** → returns a collection of leaderboard entries. Initially empty until games complete.
- **GET `/api/stats/design?order=7`** → returns deterministic deck statistics for the configured order.

### 9.3 Health Check
- **GET `/healthz`** → returns `Healthy` when the API and database are reachable.

---

## 10. Testing Real-Time Gameplay with Postman (SignalR Hub)

Postman’s WebSocket client can speak the JSON SignalR protocol.

### 10.1 Prepare Tokens
- Obtain tokens for two players (e.g., Alice and Bob) using the authentication endpoint above.

### 10.2 Connect Player 1
1. In Postman, create a **New ▸ WebSocket Request**.
2. Enter the URL (match your HTTPS/HTTP choice):
   ```
   wss://localhost:7092/hubs/game?access_token=<Alice_JWT>
   ```
   - For HTTP profile use `ws://localhost:5139/...`.
3. Click **Connect**. Postman shows the socket is open.
4. Send the SignalR handshake (note the required `\u001e` record separator appended to every message):
   ```json
   {"protocol":"json","version":1}\u001e
   ```
   Expect a handshake acknowledgment: `{"type":6}\u001e`.

### 10.3 Create a Game
Send an invocation to create a Tower match:
```json
{"type":1,"target":"CreateGame","arguments":[{"mode":"Tower","maxPlayers":4,"maxRounds":5}]}\u001e
```
The hub responds with a completion message containing the new `gameId`.

### 10.4 Connect Player 2 and Join
1. Open a second WebSocket tab for Bob using his token.
2. Repeat the handshake.
3. Have Bob join the game (replace `<GAME_ID>` with the GUID returned above, `<BOB_ID>` with Bob’s GUID):
```json
{"type":1,"target":"JoinGame","arguments":[{"gameId":"<GAME_ID>","playerId":"<BOB_ID>"}]}\u001e
```
4. Have Alice also join if desired (players must join before start).

### 10.5 Start the Game and Play Rounds
1. With Alice’s connection, invoke `StartGame`:
```json
{"type":1,"target":"StartGame","arguments":["<GAME_ID>"]}\u001e
```
2. The hub broadcasts a `RoundStart` event to the group. Postman displays JSON payloads detailing card indexes.
3. Simulate a symbol click from Bob:
```json
{"type":1,"target":"ClickSymbol","arguments":[{"gameId":"<GAME_ID>","playerId":"<BOB_ID>","symbolId":42}]}\u001e
```
4. Observe `RoundResult` (and possibly `RoundStart` for next round) messages streaming back. When the game ends, a `GameOver` event is broadcast containing the final score snapshot.

> ✅ **Tip**: Postman automatically shows SignalR events in the timeline. Use the **Save Messages** option to archive test sessions.

### 10.6 Common SignalR Pitfalls
- Missing `\u001e` terminator → hub never processes the message.
- Expired JWT → connection closes immediately with `401 Unauthorized`.
- Player mismatch → hub returns a `HubException` stating the authenticated player does not match the payload.
- Too few players → `StartGame` throws a hub exception (`"At least two players are required to start the session."`).

---

## 11. Observing Logs

- **Visual Studio Output Window**: Shows Serilog-formatted entries, including request IDs and SignalR connection details.
- **Console Host Window** (Ctrl+F5): Streams the same logs. Use this to monitor rate limiter rejections or database errors during manual testing.

Key log markers:
- `Handling GET /api/...` / `Handled GET ...` from `RequestLoggingMiddleware`.
- `Connection <id> established` / `disconnected` for SignalR.
- `Processed attempt for player ...` for gameplay events.

---

## 12. Resetting Local State

- **Drop the database**: From Package Manager Console →
  ```powershell
  Drop-Database
  ```
  or execute `DROP DATABASE [EmojitServer];` in SQL Server Management Studio.
- **Clear migrations**: Remove generated tables and re-run `Update-Database` as needed.
- **Refresh players**: Re-run the SQL insert script with new GUIDs if you want clean statistics.
- **Reissue JWTs**: Tokens expire after the `Jwt:AccessTokenLifetimeInMinutes` (default 120 in Development). Simply call the authentication endpoint again to obtain fresh tokens.

---

## 13. Troubleshooting Checklist

| Symptom | Likely Cause | Resolution |
|---------|--------------|------------|
| `SqlException` on startup | SQL Server not reachable or wrong connection string | Verify `appsettings.Development.json` and ensure LocalDB instance exists (`sqllocaldb info`). |
| `401 Unauthorized` on SignalR connect | Missing/expired JWT | Request a new token and append `access_token` query parameter. |
| `429 Too Many Requests` | Rate limiter triggered | Default limit is 100 requests/minute per user/IP in Development; wait for window to reset or increase in settings. |
| CORS errors in browser client | Origin not listed | Update `Cors:AllowedOrigins` in development settings and restart the API. |
| No Swagger page after F5 | Launch profile set to `http` but hitting HTTPS | Confirm the URL in the browser matches the active profile (`http://localhost:5139/swagger`). |

---

## 14. Next Steps

- Add more sample data (players, leaderboard entries) as needed for demos.
- Adjust `GameDefaults` or `SignalRLimits` to exercise edge cases.
- Commit any local-only configuration changes using [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) instead of checking them into source control.

With these steps you can build, run, and validate the Emojit server locally using Visual Studio on Windows without relying on Docker containers.