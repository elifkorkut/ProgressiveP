using System.Collections.Generic;
using ProgressiveP.Backend;
using UnityEngine;
using System.Threading.Tasks;


namespace ProgressiveP.Core
{
    public static class BackendDialogPoint 
    {
    
        public static async Task LoadPlayer(
            string playerId,
            System.Action<string>      onError,
            System.Action<string>      onSuccess)
        {
            await MockServices.Instance.AuthenticatePlayerAsync(
                playerId,
                onError:   onError,
                onSuccess: data => onSuccess?.Invoke(data.value));
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

                 
        public static async Task ActivateSession(
            string playerId, string gameId, string sessionId,
            System.Action<string> onError,
            System.Action         onSuccess)
        {
            await MockServices.Instance.ActivateSessionAsync(
                playerId, gameId, sessionId,
                onError: onError,
                onSuccess: _ => onSuccess?.Invoke());
        }

        
        public static async Task ValidateActiveSession(
            string playerId, string gameId, string sessionId,
            System.Action               onExpired,
            System.Action<NewSessionData> onValid)
        {
            await MockServices.Instance.ValidateActiveSessionAsync(
                playerId, gameId, sessionId,
                onError: _ => onExpired?.Invoke(),
                onSuccess: data =>
                {
                    var session = DataReader.LoadNewSessionData(data.value);
                    if (session.HasValue) onValid?.Invoke(session.Value);
                    else                 onExpired?.Invoke();
                });
        }

        
        public static async Task GetLevelBallTargets(
            string playerId, string gameId, string sessionId,
            int    levelIndex, int ballCount, float betPerBall,
            System.Action<string>        onError,
            System.Action<LevelBallData> onSuccess)
        {
            await MockServices.Instance.GetLevelBallTargetsAsync(
                playerId, gameId, sessionId, levelIndex, ballCount, betPerBall,
                onError: onError,
                onSuccess: data =>
                {
                    var parsed = DataReader.LoadLevelBallData(data.value);
                    if (parsed.HasValue) onSuccess?.Invoke(parsed.Value);
                    else                 onError?.Invoke("Failed to parse LevelBallData.");
                });
        }

      
        public static async Task SendBallBatch(
            string playerId, string gameId, string sessionId,
            int    levelIndex, int[] ballIndices, float betPerBall,
            System.Action<string>          onError,
            System.Action<BallBatchResult> onSuccess)
        {
            await MockServices.Instance.ProcessBallBatchAsync(
                playerId, gameId, sessionId, levelIndex, ballIndices, betPerBall,
                onError: onError,
                onSuccess: data =>
                {
                    var parsed = DataReader.LoadBallBatchResult(data.value);
                    if (parsed.HasValue) onSuccess?.Invoke(parsed.Value);
                    else                 onError?.Invoke("Failed to parse BallBatchResult.");
                });
        }

            public static async Task UpdateSessionLevel(
            string playerId, string gameId, string sessionId,
            int    newLevelIndex,
            System.Action<string> onError,
            System.Action         onSuccess)
        {
            await MockServices.Instance.UpdateSessionLevelAsync(
                playerId, gameId, sessionId, newLevelIndex,
                onError: onError,
                onSuccess: _ => onSuccess?.Invoke());
        }
    }

}
