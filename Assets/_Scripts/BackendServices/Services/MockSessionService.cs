using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ProgressiveP.Backend
{
    /// <summary>
    /// Simulates a Firebase "sessions" service.
    ///
    /// CreateSessionAsync flow:
    ///   1. Load game config (multipliers, levels) from Resources via StorageProvider.
    ///   2. Generate sessionId = UTC start time formatted as "yyyyMMdd_HHmmss_fff".
    ///   3. Write session JSON to:
    ///        {persistentDataPath}/Data/PlayersGameData/{playerId}/{gameId}/{sessionId}.json
    ///   4. Update player profile JSON to add/replace this game's entry (state = "active").
    /// </summary>
    public class MockSessionService : ISessionService
    {
        private readonly NetworkSimulator _network;

        public MockSessionService(NetworkSimulator network)
        {
            _network = network;
        }

        public async Task<BackendResult<BackendData>> CreateSessionAsync(
            string playerId, string gameId,
            Action<string>      onError,
            Action<BackendData> onSuccess)
        {
            try
            {
                await _network.SimulateAsync();

                // ── 1. Load game config ───────────────────────────────────────
                if (!StorageProvider.LoadGame(gameId, out var configJson))
                {
                    string err = $"Game config not found for '{gameId}'.";
                    onError?.Invoke(err);
                    return BackendResult<BackendData>.Failure("404", err);
                }

                // ── 2. Generate sessionId from start time ─────────────────────
                var    now       = DateTime.UtcNow;
                string sessionId = now.ToString("yyyyMMdd_HHmmss_fff");

                // ── 3. Build session document ─────────────────────────────────
                var sessionDoc = new JObject
                {
                    ["sessionId"]      = sessionId,
                    ["playerId"]       = playerId,
                    ["gameId"]         = gameId,
                    ["startTime"]      = now.ToString("o"),   // ISO-8601
                    ["startTimeTicks"] = now.Ticks,
                    ["state"]          = "active",
                    ["gameConfig"]     = JToken.Parse(configJson)
                };
                string sessionJson = sessionDoc.ToString(Formatting.Indented);

                // ── 4. Persist session file ───────────────────────────────────
                StorageProvider.SavePlayerGameSession(playerId, gameId, sessionId, sessionJson);

                // ── 5. Update player profile: add/replace game entry ──────────
                if (!StorageProvider.LoadPlayer(playerId, out var playerJson))
                    playerJson = StorageProvider.CreatePlayer(playerId);

                var playerObj = JObject.Parse(playerJson);
                var games     = playerObj["games"] as JArray ?? new JArray();

                // Remove any existing entry for this gameId
                for (int i = games.Count - 1; i >= 0; i--)
                    if ((string)games[i]["gameId"] == gameId)
                        games.RemoveAt(i);

                games.Add(new JObject
                {
                    ["gameId"]      = gameId,
                    ["played"]      = 0,
                    ["startedTick"] = sessionId,
                    ["lastPlayed"]  = now.ToString("o"),
                    ["sessionId"]   = sessionId,
                    ["state"]       = "active"
                });

                playerObj["games"] = games;
                StorageProvider.SavePlayer(playerId, playerObj.ToString(Formatting.Indented));

                Debug.Log($"[MockSessionService] Session created: {sessionId} for player '{playerId}', game '{gameId}'.");

                var result = new BackendData(sessionJson);
                onSuccess?.Invoke(result);
                return BackendResult<BackendData>.Success(result);
            }
            catch (Exception ex)
            {
                string err = $"Failed to create session: {ex.Message}";
                onError?.Invoke(err);
                return BackendResult<BackendData>.Failure("500", err);
            }
        }
    }
}

       