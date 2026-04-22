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

        /// <summary>
        /// Creates a new game session:
        ///   - fetches game config (levels + multipliers)
        ///   - writes session JSON file
        ///   - updates the player profile with the new game entry
        /// Returns the session document JSON wrapped in BackendData.
        /// </summary>
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
    }
}
