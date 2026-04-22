using System.Collections.Generic;
using ProgressiveP.Backend;
using UnityEngine;
using System.Threading.Tasks;


namespace ProgressiveP.Core
{
    public static class BackendDialogPoint 
    {
        public static void AddOpenedGame(string gameId)
        {
            
        }

        public static async Task GetNewGame(string gameId, string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                Debug.LogError("[BackendDialogPoint] GetNewGame: playerId is empty. Is the player authenticated?");
                return;
            }

            await MockServices.Instance.CreateSessionAsync(
                playerId, gameId,
                onError: error =>
                {
                    Debug.LogError($"[BackendDialogPoint] Failed to create session for '{gameId}': {error}");
                },
                onSuccess: data =>
                {
                    var session = DataReader.LoadNewSessionData(data.value);
                    if (session.HasValue)
                    {
                        DataReader.OnNewSessionCreated?.Invoke(session.Value);
                        Debug.Log($"[BackendDialogPoint] New session ready: {session.Value.sessionId} " +
                                  $"| levels: {session.Value.gameConfig.levels?.Length ?? 0}");
                    }
                    else
                    {
                        Debug.LogError($"[BackendDialogPoint] Session created but JSON parse failed for '{gameId}'.");
                    }
                });
        }

        public static async Task<GlobalGameConfig> GetGlobalConfig(string gameId)
        {
            GlobalGameConfig result = default;

            await MockServices.Instance.GetGameConfigGlobalAsync(
                gameId,
                onError: error =>
                {
                    Debug.LogError($"[BackendDialogPoint] Failed to load global config for '{gameId}': {error}");
                },
                onSuccess: data =>
                {
                    var parsed = DataReader.LoadGlobalGameConfig(data.value);
                    if (parsed.HasValue)
                    {
                        result = parsed.Value;
                        DataReader.OnGlobalConfigLoaded?.Invoke(result);
                        Debug.Log($"[BackendDialogPoint] Global config loaded — game: {result.gameId}, v{result.version}");
                    }
                    else
                    {
                        Debug.LogError($"[BackendDialogPoint] Received data but failed to parse global config JSON for '{gameId}'.");
                    }
                });

            return result;
        }

        public static void GetActiveGame(string gameId, string sessionId)
        {
            
        }

         public static void SaveClosedGame(string gameId)
        {
            
        }

         public static void LoadClosedGame(string gameId, string sessionId)
         {
             
         }

         public static void SetGameConfig(string sessionId, Dictionary<string, object> data)
         {
             
         }
    }

}
