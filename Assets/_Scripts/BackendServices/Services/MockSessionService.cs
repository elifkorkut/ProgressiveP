using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ProgressiveP.Backend
{
    
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

                if (!StorageProvider.LoadGame(gameId, out var configJson))
                {
                    string err = $"Game config not found for '{gameId}'.";
                    onError?.Invoke(err);
                    return BackendResult<BackendData>.Failure("404", err);
                }

                var    now       = DateTime.UtcNow;
                string sessionId = now.ToString("yyyyMMdd_HHmmss_fff");

                var sessionDoc = new JObject
                {
                    ["sessionId"]      = sessionId,
                    ["playerId"]       = playerId,
                    ["gameId"]         = gameId,
                    ["startTime"]      = now.ToString("o"),
                    ["startTimeTicks"] = now.Ticks,
                    ["state"]          = "pending",
                    ["gameConfig"]     = JToken.Parse(configJson)
                };
                string sessionJson = sessionDoc.ToString(Formatting.Indented);
                StorageProvider.SavePlayerGameSession(playerId, gameId, sessionId, sessionJson);

                

               StorageProvider.LoadPlayer(playerId, out var playerJson);

                var playerObj = JObject.Parse(playerJson);
                var games     = playerObj["games"] as JArray ?? new JArray();
                for (int i = games.Count - 1; i >= 0; i--)
                    if ((string)games[i]["gameId"] == gameId) games.RemoveAt(i);

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

                Debug.Log($"[MockSession] Created: {sessionId}");
                var bd = new BackendData(sessionJson);
                onSuccess?.Invoke(bd);
                return BackendResult<BackendData>.Success(bd);
            }
            catch (Exception ex)
            {
                string err = $"Failed to create session: {ex.Message}";
                onError?.Invoke(err);
                return BackendResult<BackendData>.Failure("500", err);
            }
        }

               public async Task<BackendResult<BackendData>> ActivateSessionAsync(
            string playerId, string gameId, string sessionId,
            Action<string>      onError,
            Action<BackendData> onSuccess)
        {
            try
            {
                await _network.SimulateAsync();

                if (!StorageProvider.LoadPlayerGameSessionFile(playerId, gameId, sessionId, out string sessionJson))
                {
                    string err = $"Session '{sessionId}' not found.";
                    onError?.Invoke(err);
                    return BackendResult<BackendData>.Failure("404", err);
                }

                var sessionObj = JObject.Parse(sessionJson);
                sessionObj["state"]              = "active";
                sessionObj["activatedTimeTicks"] = DateTime.UtcNow.Ticks;

                string updated = sessionObj.ToString(Formatting.Indented);
                StorageProvider.SavePlayerGameSession(playerId, gameId, sessionId, updated);

                Debug.Log($"[MockSession] Activated: {sessionId}");
                var bd = new BackendData(updated);
                onSuccess?.Invoke(bd);
                return BackendResult<BackendData>.Success(bd);
            }
            catch (Exception ex)
            {
                string err = $"ActivateSession failed: {ex.Message}";
                onError?.Invoke(err);
                return BackendResult<BackendData>.Failure("500", err);
            }
        }

              public async Task<BackendResult<BackendData>> ValidateActiveSessionAsync(
            string playerId, string gameId, string sessionId,
            Action<string>      onError,
            Action<BackendData> onSuccess)
        {
            try
            {
                await _network.SimulateAsync();

                if (!StorageProvider.LoadPlayerGameSessionFile(playerId, gameId, sessionId, out string sessionJson))
                {
                    string err = "SESSION_EXPIRED";
                    onError?.Invoke(err);
                    return BackendResult<BackendData>.Failure("EXPIRED", err);
                }

                var sessionObj = JObject.Parse(sessionJson);
                long startTicks = (long)(sessionObj["startTimeTicks"] ?? 0L);

                // Read duration from global config
                int durationSeconds = 900;
                if (StorageProvider.LoadGameGlobalConfig(gameId, out string globalJson))
                {
                    var g = JObject.Parse(globalJson);
                    durationSeconds = (int)(g["durationInSeconds"] ?? 900);
                }

                double elapsed = (DateTime.UtcNow.Ticks - startTicks) / (double)TimeSpan.TicksPerSecond;
                if (elapsed > durationSeconds)
                {
                  
                    sessionObj["state"] = "expired";
                    StorageProvider.SavePlayerGameSession(playerId, gameId, sessionId, sessionObj.ToString(Formatting.Indented));

                    if (StorageProvider.LoadPlayer(playerId, out string playerJson))
                    {
                        var pObj  = JObject.Parse(playerJson);
                        var gArr  = pObj["games"] as JArray;
                        if (gArr != null)
                        {
                            foreach (var g in gArr)
                                if ((string)g["gameId"] == gameId && (string)g["sessionId"] == sessionId)
                                    g["state"] = "inactive";
                            pObj["games"] = gArr;
                            StorageProvider.SavePlayer(playerId, pObj.ToString(Formatting.Indented));
                        }
                    }

                    string err = "SESSION_EXPIRED";
                    onError?.Invoke(err);
                    return BackendResult<BackendData>.Failure("EXPIRED", err);
                }

                Debug.Log($"[MockSession] Valid session: {sessionId}, {durationSeconds - elapsed:F0}s remaining.");
                var bd = new BackendData(sessionJson);
                onSuccess?.Invoke(bd);
                return BackendResult<BackendData>.Success(bd);
            }
            catch (Exception ex)
            {
                string err = $"ValidateSession failed: {ex.Message}";
                onError?.Invoke(err);
                return BackendResult<BackendData>.Failure("500", err);
            }
        }

            public async Task<BackendResult<BackendData>> GetLevelBallTargetsAsync(
            string playerId, string gameId, string sessionId,
            int levelIndex, int ballCount, float betPerBall,
            Action<string>      onError,
            Action<BackendData> onSuccess)
        {
            try
            {
                await _network.SimulateAsync();

                if (ballCount <= 0)
                {
                    // MAX level
                    var empty = new JObject { ["sessionId"] = sessionId, ["levelIndex"] = levelIndex,
                                             ["ballCount"] = 0, ["betPerBall"] = betPerBall, ["targets"] = new JArray() };
                    var emptyBd = new BackendData(empty.ToString());
                    onSuccess?.Invoke(emptyBd);
                    return BackendResult<BackendData>.Success(emptyBd);
                }

                if (!StorageProvider.LoadGame(gameId, out string levelsJson))
                {
                    string err = $"Levels config not found for '{gameId}'.";
                    onError?.Invoke(err);
                    return BackendResult<BackendData>.Failure("404", err);
                }

                var levelsObj = JObject.Parse(levelsJson);
                var levelsArr = levelsObj["levels"] as JArray;
                if (levelsArr == null || levelIndex >= levelsArr.Count)
                {
                    string err = $"Level {levelIndex} not found in config.";
                    onError?.Invoke(err);
                    return BackendResult<BackendData>.Failure("404", err);
                }

                var levelCfg = levelsArr[levelIndex];
                var mults    = (levelCfg["multipliers"] as JArray)?.ToObject<float[]>() ?? new float[] { 1f };

                // Probability = 1/multiplier (high mult → rare), normalized
                float[] probs = new float[mults.Length];
                float   total = 0f;
                for (int i = 0; i < mults.Length; i++)
                {
                    probs[i] = mults[i] > 0f ? 1f / mults[i] : 0f;
                    total   += probs[i];
                }
                if (total > 0f)
                    for (int i = 0; i < probs.Length; i++) probs[i] /= total;

                var rng     = new System.Random();
                var targets = new JArray();
                for (int b = 0; b < ballCount; b++)
                {
                    float roll       = (float)rng.NextDouble();
                    float cumulative = 0f;
                    int   bucket     = probs.Length - 1;
                    for (int i = 0; i < probs.Length; i++)
                    {
                        cumulative += probs[i];
                        if (roll <= cumulative) { bucket = i; break; }
                    }
                    targets.Add(new JObject { ["ballIndex"] = b, ["bucketIndex"] = bucket });
                }

                var result = new JObject
                {
                    ["sessionId"]  = sessionId,
                    ["levelIndex"] = levelIndex,
                    ["ballCount"]  = ballCount,
                    ["betPerBall"] = betPerBall,
                    ["targets"]    = targets
                };
                string resultJson  = result.ToString(Formatting.Indented);
                string targetsFile = $"{sessionId}_L{levelIndex}_targets";
                StorageProvider.SavePlayerGameSession(playerId, gameId, targetsFile, resultJson);

                Debug.Log($"[MockSession] Pre-calc targets: {ballCount} balls, level {levelIndex}.");
                var bd = new BackendData(resultJson);
                onSuccess?.Invoke(bd);
                return BackendResult<BackendData>.Success(bd);
            }
            catch (Exception ex)
            {
                string err = $"GetLevelBallTargets failed: {ex.Message}";
                onError?.Invoke(err);
                return BackendResult<BackendData>.Failure("500", err);
            }
        }

        // ProcessBallBatch
        public async Task<BackendResult<BackendData>> ProcessBallBatchAsync(
            string playerId, string gameId, string sessionId,
            int levelIndex, int[] ballIndices, float betPerBall,
            Action<string>      onError,
            Action<BackendData> onSuccess)
        {
            try
            {
                await _network.SimulateAsync();

                // Load pre-calculated targets
                string targetsFile = $"{sessionId}_L{levelIndex}_targets";
                if (!StorageProvider.LoadPlayerGameSessionFile(playerId, gameId, targetsFile, out string targetsJson))
                {
                    string err = $"Targets file not found for session {sessionId}, level {levelIndex}.";
                    onError?.Invoke(err);
                    return BackendResult<BackendData>.Failure("404", err);
                }

                var targetsObj = JObject.Parse(targetsJson);
                var tarArr     = targetsObj["targets"] as JArray ?? new JArray();

                // Load level multipliers
                float[] mults = new float[0];
                if (StorageProvider.LoadGame(gameId, out string levelsJson))
                {
                    var levelsObj = JObject.Parse(levelsJson);
                    var levelsArr = levelsObj["levels"] as JArray;
                    if (levelsArr != null && levelIndex < levelsArr.Count)
                        mults = (levelsArr[levelIndex]["multipliers"] as JArray)?.ToObject<float[]>() ?? mults;
                }

                // Load player to update balance
                if (!StorageProvider.LoadPlayer(playerId, out string playerJson))
                    playerJson = StorageProvider.CreatePlayer(playerId);

                var playerObj = JObject.Parse(playerJson);
                double balance = (double)(playerObj["balance"] ?? 0);

                // Resolve each ball index
                var rewardsArr = new JArray();
                float totalPayout = 0f;
                long  nowTicks    = DateTime.UtcNow.Ticks;

                foreach (int idx in ballIndices)
                {
                    int bucketIdx = -1;
                    foreach (var t in tarArr)
                        if ((int)(t["ballIndex"] ?? -1) == idx) { bucketIdx = (int)(t["bucketIndex"] ?? 0); break; }

                    if (bucketIdx < 0 || mults.Length == 0)
                    {
                        // treat as loss
                        balance    -= betPerBall;
                        totalPayout += 0f;
                        continue;
                    }

                    float multiplier = bucketIdx < mults.Length ? mults[bucketIdx] : 1f;
                    int   payout     = Mathf.RoundToInt(betPerBall * multiplier);
                    int   net        = payout - Mathf.RoundToInt(betPerBall);   // negative = loss

                    balance    += net;
                    totalPayout += payout;

                    rewardsArr.Add(new JObject
                    {
                        ["bucketIndex"] = bucketIdx,
                        ["betAmount"]   = Mathf.RoundToInt(betPerBall),
                        ["multiplier"]  = multiplier,
                        ["payout"]      = payout,
                        ["ticks"]       = nowTicks
                    });
                }

                playerObj["balance"] = balance;
                StorageProvider.SavePlayer(playerId, playerObj.ToString(Formatting.Indented));

                // cumulative ball spend, level drop count,reward history
                if (StorageProvider.LoadPlayerGameSessionFile(playerId, gameId, sessionId, out string sessionJson))
                {
                    var sessionObj      = JObject.Parse(sessionJson);
                    int prevTotal       = (int)(sessionObj["totalBallsSpent"]      ?? 0);
                    int prevLevel       = (int)(sessionObj["ballsDroppedThisLevel"] ?? 0);
                    sessionObj["totalBallsSpent"]       = prevTotal + ballIndices.Length;
                    sessionObj["ballsDroppedThisLevel"] = prevLevel + ballIndices.Length;

                    var existingHistory = sessionObj["rewardHistory"] as JArray ?? new JArray();
                    foreach (JObject r in rewardsArr)
                        existingHistory.Add(r);
                    sessionObj["rewardHistory"] = existingHistory;

                    StorageProvider.SavePlayerGameSession(playerId, gameId, sessionId, sessionObj.ToString(Formatting.Indented));
                }

                var resultObj = new JObject
                {
                    ["totalPayout"] = (int)totalPayout,
                    ["newBalance"]  = balance,
                    ["rewards"]     = rewardsArr
                };
                string resultJson = resultObj.ToString(Formatting.Indented);

                Debug.Log($"[MockSession] Batch processed: {ballIndices.Length} balls, payout {totalPayout}, balance {balance}.");
                var bd = new BackendData(resultJson);
                onSuccess?.Invoke(bd);
                return BackendResult<BackendData>.Success(bd);
            }
            catch (Exception ex)
            {
                string err = $"ProcessBallBatch failed: {ex.Message}";
                onError?.Invoke(err);
                return BackendResult<BackendData>.Failure("500", err);
            }
        }

        // update session level
        public async Task<BackendResult<BackendData>> UpdateSessionLevelAsync(
            string playerId, string gameId, string sessionId,
            int newLevelIndex,
            Action<string>      onError,
            Action<BackendData> onSuccess)
        {
            try
            {
                await _network.SimulateAsync();

                if (!StorageProvider.LoadPlayerGameSessionFile(playerId, gameId, sessionId, out string sessionJson))
                {
                    string err = $"Session '{sessionId}' not found for level update.";
                    onError?.Invoke(err);
                    return BackendResult<BackendData>.Failure("404", err);
                }

                var sessionObj = JObject.Parse(sessionJson);
                sessionObj["currentLevelIndex"]       = newLevelIndex;
                // reset per-level counter
                sessionObj["ballsDroppedThisLevel"]   = 0;
                string updated = sessionObj.ToString(Formatting.Indented);
                StorageProvider.SavePlayerGameSession(playerId, gameId, sessionId, updated);

                Debug.Log($"[MockSession] Level updated to {newLevelIndex} for session {sessionId}.");
                var bd = new BackendData(updated);
                onSuccess?.Invoke(bd);
                return BackendResult<BackendData>.Success(bd);
            }
            catch (Exception ex)
            {
                string err = $"UpdateSessionLevel failed: {ex.Message}";
                onError?.Invoke(err);
                return BackendResult<BackendData>.Failure("500", err);
            }
        }
    }
}
