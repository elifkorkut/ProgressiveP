using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor.VersionControl;

namespace ProgressiveP.Backend
{
    public class StorageProvider : MonoBehaviour
    {
    // READ-ONLY
    // Assets/Resources/Data/Games/{gameId}/global.json - game rules & settings
    // Assets/Resources/Data/Games/{gameId}/games{id}.json - game and level definitions
    
    // WRITABLE 
    //  Assets/Resources/Data/Players/{playerId}.json - profile, balance, game list
  
    //GAME HISTORY
    // Assets/Resources/Data/PlayersGameData/{playerId}/{gameId}/{gameSessionId}.json - session history and data
    // Assets/Resources/Data/PlayersGameData/{playerId}/{gameId}/summary.json - aggregate stats



    private static string PersistentRoot=> Path.Combine(Application.persistentDataPath, "Data");
    private static string PlayersRoot=> Path.Combine(PersistentRoot, "Players");
    private static string PlayerGameDataRoot =>Path.Combine(PersistentRoot, "PlayersGameData");

    private static string GamesResourceRoot => "Games"; 

    public static bool LoadGameGlobalConfig(string gameId, out string config)
     {
            var asset = Resources.Load<TextAsset>($"{GamesResourceRoot}/{gameId}/global");
            if (asset == null)
            {
                Debug.LogWarning($"[Storage] Missing: Resources/{GamesResourceRoot}/{gameId}/global.json");
                config = null;
                return false;
            }
            config = asset.text;
            return true;
    }
    
     public static bool LoadGame(string gameId, out string config, string gameVersion = "1")
     {
           var asset = Resources.Load<TextAsset>($"{GamesResourceRoot}/{gameId}/game{gameVersion}");
            if (asset == null)
            {
                Debug.LogWarning($"[Storage] Missing: Resources/{GamesResourceRoot}/{gameId}/game{gameVersion}.json");
                config = null;
                return false;
            }
            
            config = asset.text;
            return true;
     }



    public static bool LoadPlayer(string playerId, out string data)
     {
          string path = Path.Combine(PlayersRoot, $"{playerId}.json");
            if (File.Exists(path))
            {
                data = File.ReadAllText(path);
                return true;
            }

            Debug.LogWarning($"[Storage] Player file not found at: {path}");
            data = null;
            return false;
     }
     


     public static string LoadPlayerGameData(string playerId,string gameId)
     {
            var asset = Resources.Load<TextAsset>($"{PlayerGameDataRoot}/{playerId}/{gameId}/data");
            if (asset == null)
            {
                Debug.LogWarning($"[Storage] Missing: Resources/{PlayerGameDataRoot}/{playerId}/{gameId}/data.json");
                return null;
            }
            return JsonUtility.FromJson<string>(asset.text);
     }


     public static string LoadPlayerGameSession(string playerId,string gameId,string sessionId)
     {
            var asset = Resources.Load<TextAsset>($"{PlayerGameDataRoot}/{playerId}/{gameId}/{sessionId}");
            if (asset == null)
            {
                Debug.LogWarning($"[Storage] Missing: Resources/{PlayerGameDataRoot}/{playerId}/{gameId}/{sessionId}.json");
                return null;
            }
            return JsonUtility.FromJson< string>(asset.text);
     }


        public static string LoadPlayerGameSummary(string playerId, string gameId)
     {
            var asset = Resources.Load<TextAsset>($"{PlayerGameDataRoot}/{playerId}/{gameId}/summary");
            if (asset == null)
            {   
                Debug.LogWarning($"[Storage] Missing: Resources/{PlayerGameDataRoot}/{playerId}/{gameId}/summary.json");
                return null;
            }
            return JsonUtility.FromJson<string>(asset.text);
     }



     public static string CreatePlayer(string playerId)
     {
         try
            {
             if (!Directory.Exists(PlayersRoot))
                {
                    Directory.CreateDirectory(PlayersRoot);
                }

                var playerData = new Dictionary<string, object>
                {
                    { "userId", playerId },
                    { "name", "Player" },
                    { "balance", 0 },
                    { "games", new List<object>() }
                };

                string jsonOutput = JsonConvert.SerializeObject(playerData, Formatting.Indented);
                string path = Path.Combine(PlayersRoot, $"{playerId}.json");
                
                File.WriteAllText(path, jsonOutput);
                Debug.Log($"[Storage] Mock Player saved to: {path}");

                return jsonOutput; 
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Storage] Error creating player: {ex}");
                return null;
            }
       }

  

    public static void SavePlayer(string playerId, string jsonData)
    {
        string path = Path.Combine(PlayersRoot, $"{playerId}.json");
        File.WriteAllText(path, jsonData);
        Debug.Log($"Player data saved to: {path}");
    }

    /// <summary>
    /// Writes a session document to:
    ///   {persistentDataPath}/Data/PlayersGameData/{playerId}/{gameId}/{sessionId}.json
    /// The sessionId is the formatted UTC start time so the filename is human-readable.
    /// </summary>
    public static void SavePlayerGameSession(string playerId, string gameId, string sessionId, string jsonData)
    {
        string dir = Path.Combine(PlayerGameDataRoot, playerId, gameId);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, $"{sessionId}.json");
        File.WriteAllText(path, jsonData);
        Debug.Log($"[Storage] Session saved: {path}");
    }
        
     
    }
}

