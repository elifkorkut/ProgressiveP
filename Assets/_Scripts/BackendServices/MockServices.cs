using System.Threading.Tasks;
using UnityEngine;

namespace ProgressiveP.Backend
{  
    [DefaultExecutionOrder(-100)]
    public class MockServices : MonoBehaviour
    {
         [SerializeField] private float minNetworkDelayMs = 80f;
         [SerializeField] private float maxNetworkDelayMs = 250f;

         public static MockServices Instance { get; private set; }

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _ = Initialize();
        }

        public async Task Initialize()
        {
           await RegisterServices();
        }

        private async Task  RegisterServices()
        { 
           
            var network = new NetworkSimulator(minNetworkDelayMs, maxNetworkDelayMs);
            ServiceLocatorBackend.Register<IPlayerService>(new MockPlayerService(network));
            ServiceLocatorBackend.Register<ISessionService>(new MockSessionService(network));
            ServiceLocatorBackend.Register<IGameService>(new MockGameService(network));
            
            try
            {
                await network.SimulateAsync();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MockServices] Initialization error: {ex}");
                return;
            }

         }

        public async Task<BackendData> AuthenticatePlayerAsync(string playerId, System.Action<string> onError, System.Action<BackendData> onSuccess)
        {
            if (!ServiceLocatorBackend.TryGet<IPlayerService>(out var playerService))
            {
                onError?.Invoke("Player service not available.");
                return null;
            }

            try
            {
                var data = await playerService.GetOrCreatePlayerAsync(playerId);
                if (data != null)
                {
                    onSuccess?.Invoke(data.Data);
                    return data.Data;
                }
                else
                {
                    onError?.Invoke("Failed to authenticate player.");
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                onError?.Invoke($"Authentication error: {ex.Message}");
                return null;
            }
        }

        
       
        public async Task<BackendData> GetGameConfigAsync(string gameId, System.Action<string> onError, System.Action<BackendData> onSuccess)
        {
            if (!ServiceLocatorBackend.TryGet<IGameService>(out var gameService))
            {
                onError?.Invoke("Game service not available.");
                return null;
            }

            try
            {
                var result = await gameService.GetGameConfigAsync(gameId, onError, onSuccess);
                if (result.IsSuccess)
                {
                    return result.Data;
                }
                else
                {
                    onError?.Invoke($"Failed to get game config: {result.ErrorMessage}");
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                onError?.Invoke($"Error fetching game config: {ex.Message}");
                return null;
            }
        }


        public async Task<BackendData> GetGameConfigGlobalAsync(string gameId, System.Action<string> onError, System.Action<BackendData> onSuccess)
        {
            if (!ServiceLocatorBackend.TryGet<IGameService>(out var gameService))
            {
                onError?.Invoke("Game service not available.");
                return null;
            }

            try
            {
                var result = await gameService.GetGameConfigGlobalAsync(gameId, onError, onSuccess);
                if (result.IsSuccess)
                {
                    return result.Data;
                }
                else
                {
                    onError?.Invoke($"Failed to get game config: {result.ErrorMessage}");
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                onError?.Invoke($"Error fetching game config: {ex.Message}");
                return null;
            }
        }

    
        public async Task<BackendData> CreateSessionAsync(
            string playerId, string gameId,
            System.Action<string>      onError,
            System.Action<BackendData> onSuccess)
        {
            if (!ServiceLocatorBackend.TryGet<ISessionService>(out var sessionService))
            {
                onError?.Invoke("Session service not available.");
                return null;
            }

            try
            {
                var result = await sessionService.CreateSessionAsync(playerId, gameId, onError, onSuccess);
                if (result.IsSuccess)
                    return result.Data;

                onError?.Invoke($"Failed to create session: {result.ErrorMessage}");
                return null;
            }
            catch (System.Exception ex)
            {
                onError?.Invoke($"Error creating session: {ex.Message}");
                return null;
            }
        }

        public async Task<BackendData> ActivateSessionAsync(
            string playerId, string gameId, string sessionId,
            System.Action<string>      onError,
            System.Action<BackendData> onSuccess)
        {
            if (!ServiceLocatorBackend.TryGet<ISessionService>(out var svc))
            { onError?.Invoke("Session service not available."); return null; }
            try
            {
                var r = await svc.ActivateSessionAsync(playerId, gameId, sessionId, onError, onSuccess);
                return r.IsSuccess ? r.Data : null;
            }
            catch (System.Exception ex) { onError?.Invoke(ex.Message); return null; }
        }

        public async Task<BackendData> ValidateActiveSessionAsync(
            string playerId, string gameId, string sessionId,
            System.Action<string>      onError,
            System.Action<BackendData> onSuccess)
        {
            if (!ServiceLocatorBackend.TryGet<ISessionService>(out var svc))
            { onError?.Invoke("Session service not available."); return null; }
            try
            {
                var r = await svc.ValidateActiveSessionAsync(playerId, gameId, sessionId, onError, onSuccess);
                return r.IsSuccess ? r.Data : null;
            }
            catch (System.Exception ex) { onError?.Invoke(ex.Message); return null; }
        }

        public async Task<BackendData> GetLevelBallTargetsAsync(
            string playerId, string gameId, string sessionId,
            int levelIndex, int ballCount, float betPerBall,
            System.Action<string>      onError,
            System.Action<BackendData> onSuccess)
        {
            if (!ServiceLocatorBackend.TryGet<ISessionService>(out var svc))
            { onError?.Invoke("Session service not available."); return null; }
            try
            {
                var r = await svc.GetLevelBallTargetsAsync(playerId, gameId, sessionId,
                                                           levelIndex, ballCount, betPerBall, onError, onSuccess);
                return r.IsSuccess ? r.Data : null;
            }
            catch (System.Exception ex) { onError?.Invoke(ex.Message); return null; }
        }

        public async Task<BackendData> ProcessBallBatchAsync(
            string playerId, string gameId, string sessionId,
            int levelIndex, int[] ballIndices, float betPerBall,
            System.Action<string>      onError,
            System.Action<BackendData> onSuccess)
        {
            if (!ServiceLocatorBackend.TryGet<ISessionService>(out var svc))
            { onError?.Invoke("Session service not available."); return null; }
            try
            {
                var r = await svc.ProcessBallBatchAsync(playerId, gameId, sessionId,
                                                        levelIndex, ballIndices, betPerBall, onError, onSuccess);
                return r.IsSuccess ? r.Data : null;
            }
            catch (System.Exception ex) { onError?.Invoke(ex.Message); return null; }
        }

        public async Task<BackendData> UpdateSessionLevelAsync(
            string playerId, string gameId, string sessionId,
            int newLevelIndex,
            System.Action<string>      onError,
            System.Action<BackendData> onSuccess)
        {
            if (!ServiceLocatorBackend.TryGet<ISessionService>(out var svc))
            { onError?.Invoke("Session service not available."); return null; }
            try
            {
                var r = await svc.UpdateSessionLevelAsync(playerId, gameId, sessionId, newLevelIndex, onError, onSuccess);
                return r.IsSuccess ? r.Data : null;
            }
            catch (System.Exception ex) { onError?.Invoke(ex.Message); return null; }
        }
    }
}
