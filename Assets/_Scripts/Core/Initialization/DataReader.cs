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
                    initialCoins = (int)(data["initialCoins"] ?? 1000),
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

        /// <summary>
        /// Parses a session document JSON (produced by MockSessionService.CreateSessionAsync)
        /// into a <see cref="NewSessionData"/> struct.
        /// The embedded "gameConfig" object is deserialized into <see cref="GameConfig"/>
        /// including all level definitions and multipliers.
        /// Returns null on empty input or parse failure.
        /// </summary>
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
                    sessionId      = (string)data["sessionId"]      ?? string.Empty,
                    playerId       = (string)data["playerId"]        ?? string.Empty,
                    gameId         = (string)data["gameId"]          ?? string.Empty,
                    startTime      = (string)data["startTime"]       ?? string.Empty,
                    startTimeTicks = (long)(data["startTimeTicks"]   ?? 0L),
                    state          = (string)data["state"]           ?? "active",
                };

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
                                numberOfBallsToPass = (int)(lv["numberOfBallsToPass"] ?? 0),
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
    }


}