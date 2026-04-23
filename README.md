## Overview

I tried developed a scalable, server-authoritative architecture with a strong emphasis on decoupling and data integrity. The system leverages optimistic UI handling to ensure seamless transitions and responsive interface updates, while also utilizing local caching (PlayerPrefs) to preserve state in case of crashes. At the same time, it enforces strict backend validation for all session results and reward distribution. The design also supports straightforward migration and future extensibility.

## Project Architecture

### Assemblies
1. **Backend**: Contains the mock data services, interfaces, network simulation, and storage providers
2. **Core**: Manages the main game loop, data models, authentication flow, and backend communication (`BackendDialogPoint`, `DataKeeper`, `DataReader`,`Authenticator`, etc.)
3. **UI**: Handles user interface updates, visual drops, wallet balance displays, and target hit feedback
4. **Logic**: Gameplay related classes, game session manager

## Backend Services
The backend relies on three core interfaces and their concrete mock implementations

* **`MockPlayerService`**: Manages player-specific data, including IDs, balances, historical game lists and their last-known status.
* **`MockGameService`**: Handles strictly read-only game configurations, such as level config and global settings.
* **`MockSessionService`**: Manages active gameplay sessions. It provides level data, calculates the required balls to pass a level, and generates ball targets using configured probability values.

### Helper Classes
* **`StorageProvider`**: Routes data read/write requests. Read-only configuration data is fetched from Unity's `Resources` folder, while writable player data is saved to `Application.persistentDataPath`.
* **`NetworkSimulator`**: Introduces artificial delays to backend calls to accurately simulate live server latency.
***MockServices : Initializes and registers all mock service implementations.


# Data Structure

Data is structured locally to emulate a remote database schema. Read-only data is stored in Unity’s Resources directory, while writable data is persisted in Application.persistentDataPath.
### Global game rules
Resources/Games/[Game_ID]/global.json
### Levels, multipliers
Resources/Games/[Game_ID]/global.json/[Level_Configs].json   
### id, game history summary,balance
Data/Players/[Player_ID].json  
### aggregated stats (not implemented yet)
Data/PlayersGameData/[Player_ID]/[Game_ID]/summary.json 
### Detailed logs for active and expired sessions
Data/PlayersGameData/[Player_ID]/[Game_ID]/[Session_ID].json 

##  Optimization

- **Object Pooling**
  - Used for balls, UI, and popups  
  - Eliminates Instantiate/Destroy overhead  

- **Audio Optimization**
  - Limited audio sources  
  - Prevents CPU spikes  

- **Request Batching**
  - Sends data at intervals or thresholds  

- **Component Caching**
  - All references cached at initialization


## Flow

## Initialization (Scene 1)

When the player launches the game, the initialization flow begins:

- `Initializer` handles authentication (currently Unity Anonymous Sign-In)
- Using the retrieved player ID:
  - `MockServices` initializes and registers all services via a service locator
  - Network latency can be configured at this stage
  - `MockPlayerService` creates or retrieves the player profile
- After initialization completes, the main game scene is loaded

---

## Gameplay (Scene 2)

## Session Handling

On startup, the client checks for an active session:

- If an active session exists → session data is returned and resumed  
- If no session exists → a new game configuration is created  

When a session starts:

- The client updates the session start timestamp
- `SessionService` provides:
  - Level configuration
  - Required ball count to complete the level
  - Precomputed ball targets based on probability distributions

---

## Gameplay Flow

- The client consumes precomputed ball targets from the backend

When a ball is spawned:

- Target is predetermined
- Local wallet is immediately decremented
- Ball is added to a local queue

When the ball reaches its target:

- UI updates instantly with the reward

---

## Batch Processing

- Ball indices are accumulated locally
- A batch request is sent when:
  - A time threshold is reached, or
  - The queue limit is exceeded

Backend:

- Validates submitted balls
- Applies pre-recorded outcomes
- Returns the authoritative balance

---

## Crash Recovery

- Unsent ball indices are stored in `PlayerPrefs`

On next launch:

- Cached data is flushed as a recovery batch
- Session resumes without data loss

---

## Deterministic Logic

- Physics are visually dynamic but logically deterministic
- Backend precomputes:
  - Target buckets for each ball
  - Using inverse-probability weighting based on payout multipliers

Client responsibilities:

- Spawn and simulate balls visually
- Route each ball to its assigned target

---

## Wallet & Synchronization

- On spawn → bet is deducted locally  
- On landing → payout is applied instantly  

In the background:

- Ball indices are queued
- Sent in batches based on:
  - Time interval
  - Queue size

Server:

- Validates against precomputed results
- Returns authoritative balance
- Overwrites client state to prevent:
  - Desync
  - Double deductions

---

## Persistence & Resilience

- Session timing uses `DateTime.UtcNow.Ticks`

Local persistence:

- Unsent ball indices stored in `PlayerPrefs`

On relaunch after crash:

- Recovery batch is sent
- Active session is resumed
- UI (e.g., reward history) is rebuilt
