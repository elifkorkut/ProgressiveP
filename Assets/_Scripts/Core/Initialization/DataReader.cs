using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ProgressiveP.Core
{
    public class DataReader : MonoBehaviour
    {
        public static Action<PlayerData>       OnPlayerDataLoaded;
        public static Action<GlobalGameConfig> OnGlobalConfigLoaded;
     
       public static PlayerData? LoadPlayerData(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError("JSON content is empty!");
                return null;
            }

            try
            {
                JObject data = JObject.Parse(jsonContent);
                PlayerData player = new PlayerData();
                player.userId = (string)data["userId"];
                player.name = (string)data["name"] ?? "Unknown Player";
                player.balance = (int)(data["balance"] ?? 0);
                
                player.games = new List<GameData>();
              
                if (data["games"] != null)
                {
                    foreach (var game in data["games"])
                    {
                        var gameData = new GameData
                        {
                            gameId = (string)game["gameId"],
                            played = (int)(game["played"] ?? 0),
                            lastPlayedtick = (string)game["lastPlayed"],
                            startedTick = (string)game["startedTick"],
                            sessionId = (string)game["sessionId"],
                            state = (string)game["state"] ?? "inactive"
                         };
                        player.games.Add(gameData);
                    }
                }
                OnPlayerDataLoaded?.Invoke(player);
                return player;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse Player Data: {e.Message}");
                return null;
            }
        }

        public static GlobalGameConfig? LoadGlobalGameConfig(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError("[DataReader] Global config JSON is empty!");
                return null;
            }

            try
            {
                JObject data = JObject.Parse(jsonContent);
                return new GlobalGameConfig
                {
                    gameId = (string)data["gameId"]?? string.Empty,
                    version = (string)data["version"] ?? "1.0.0",
                    initialCoins = (int)(data["initialCoins"] ?? 0),
                    initialBalls = (int)(data["initialBalls"] ?? 200),
                    defaultBet = (int)(data["defaultBet"] ?? 100),
                    minBet = (int)(data["minBet"] ?? 10),
                    maxBet = (int)(data["maxBet"]?? 10000),
                    durationInSeconds = (int)(data["durationInSeconds"]  ?? 900),
                    defaultRows= (int)(data["defaultRows"]?? 8),
                    defaultRisk= (int)(data["defaultRisk"]?? 1)
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataReader] Failed to parse GlobalGameConfig: {e.Message}");
                return null;
            }
        }

        public static Action<NewSessionData> OnNewSessionCreated;

        
        public static NewSessionData? LoadNewSessionData(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError("[DataReader] Session JSON is empty!");
                return null;
            }

            try
            {
                JObject data = JObject.Parse(jsonContent);

                var session = new NewSessionData
                {
                    sessionId             = (string)data["sessionId"]           ?? string.Empty,
                    playerId              = (string)data["playerId"]             ?? string.Empty,
                    gameId                = (string)data["gameId"]               ?? string.Empty,
                    startTime             = (string)data["startTime"]            ?? string.Empty,
                    startTimeTicks        = (long)(data["startTimeTicks"]        ?? 0L),
                    state                 = (string)data["state"]                ?? "active",
                    currentLevelIndex     = (int)(data["currentLevelIndex"]      ?? 0),
                    totalBallsSpent       = (int)(data["totalBallsSpent"]        ?? 0),
                    ballsDroppedThisLevel = (int)(data["ballsDroppedThisLevel"] ?? 0),
                };

                // Parse reward history 
                if (data["rewardHistory"] is JArray histArr)
                {
                    var history = new RewardRecord[histArr.Count];
                    for (int i = 0; i < histArr.Count; i++)
                    {
                        history[i] = new RewardRecord
                        {
                            bucketIndex          = (int)(histArr[i]["bucketIndex"]   ?? 0),
                            betAmount            = (float)(histArr[i]["betAmount"]    ?? 0f),
                            multiplier           = (float)(histArr[i]["multiplier"]   ?? 0f),
                            payout               = Mathf.RoundToInt((float)(histArr[i]["payout"] ?? 0f)),
                            serverTimestampTicks = (long)(histArr[i]["ticks"]         ?? 0L),
                        };
                    }
                    session.rewardHistory = history;
                }

                if (data["gameConfig"] is JObject cfgObj)
                {
                    var cfg = new GameConfig
                    {
                        initialBalls      = (int)(cfgObj["initialBalls"]      ?? 0),
                        defaultBet        = (int)(cfgObj["defaultBet"]        ?? 0),
                        durationInSeconds = (int)(cfgObj["durationInSeconds"] ?? 0),
                        numberOfLevels    = (int)(cfgObj["numberOfLevels"]    ?? 0),
                    };

                    if (cfgObj["levels"] is JArray levelsArr)
                    {
                        cfg.levels = new LevelConfig[levelsArr.Count];
                        for (int i = 0; i < levelsArr.Count; i++)
                        {
                            var lv = levelsArr[i];
                            var mults = lv["multipliers"] as JArray;
                            var levelCfg = new LevelConfig
                            {
                                rows                = (int)(lv["rows"]                ?? 0),
                                // Support both field names -change this later when backend is finalized
                                numberOfBallsToPass = (int)(lv["ballsToNextLevel"] ?? lv["numberOfBallsToPass"] ?? 0),
                                multipliers         = mults != null
                                    ? System.Array.ConvertAll(mults.ToObject<float[]>(), m => m)
                                    : new float[0]
                            };
                            cfg.levels[i] = levelCfg;
                        }
                    }
                    session.gameConfig = cfg;
                }

                return session;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataReader] Failed to parse NewSessionData: {e.Message}");
                return null;
            }
        }

        public static LevelBallData? LoadLevelBallData(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var obj     = JObject.Parse(json);
                var tarArr  = obj["targets"] as JArray ?? new JArray();
                var targets = new BallTarget[tarArr.Count];
                for (int i = 0; i < tarArr.Count; i++)
                    targets[i] = new BallTarget
                    {
                        ballIndex   = (int)(tarArr[i]["ballIndex"]   ?? i),
                        bucketIndex = (int)(tarArr[i]["bucketIndex"] ?? 0)
                    };
                return new LevelBallData
                {
                    sessionId  = (string)obj["sessionId"]  ?? string.Empty,
                    levelIndex = (int)(obj["levelIndex"] ?? 0),
                    ballCount  = (int)(obj["ballCount"]  ?? 0),
                    betPerBall = (float)(obj["betPerBall"] ?? 0f),
                    targets    = targets
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataReader] Failed to parse LevelBallData: {e.Message}");
                return null;
            }
        }

       
        public static BallBatchResult? LoadBallBatchResult(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var obj      = JObject.Parse(json);
                var rewArr   = obj["rewards"] as JArray ?? new JArray();
                var rewards  = new RewardRecord[rewArr.Count];
                for (int i = 0; i < rewArr.Count; i++)
                    rewards[i] = new RewardRecord
                    {
                        bucketIndex          = (int)(rewArr[i]["bucketIndex"]    ?? 0),
                        betAmount            = (float)(rewArr[i]["betAmount"]    ?? 0f),
                        multiplier           = (float)(rewArr[i]["multiplier"]   ?? 1f),
                        payout               = Mathf.RoundToInt((float)(rewArr[i]["payout"] ?? 0f)),
                        serverTimestampTicks = (long)(rewArr[i]["ticks"]         ?? 0L)
                    };
                return new BallBatchResult
                {
                    totalPayout = Mathf.RoundToInt((float)(obj["totalPayout"] ?? 0f)),
                    newBalance  = (double)(obj["newBalance"]  ?? 0.0),
                    rewards     = rewards
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataReader] Failed to parse BallBatchResult: {e.Message}");
                return null;
            }
        }
    }
}
