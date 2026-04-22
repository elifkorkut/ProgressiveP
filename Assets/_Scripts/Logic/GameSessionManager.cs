using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System;
using ProgressiveP.Core;

namespace ProgressiveP.Logic
{

public class GameSessionManager : MonoBehaviour
{ 
    public static GameSessionManager Instance { get; private set; }
   [SerializeField] private string gameID;

    public static event System.Action<LevelConfig> OnLevelUp;
    public static event System.Action OnConfigLoaded;

    public event Action OnInactiveSession;
    public event Action OnActiveSession;

    public Action onSessionRequested;
    public Action onSessionLoaded;


     private void Awake()
    {
       ServiceLocator.Register(this);
       onSessionRequested+= RequestNewGame; 
    }

    private void OnDisable()
    {
        ServiceLocator.Remove<GameSessionManager>();
        onSessionRequested-= RequestNewGame;
    }

    public async Task StartNewSessionAsync(string playerId)
    {
        
    }

    public async Task EndCurrentSessionAsync()
    {
       
    }


   

    private async void OnApplicationQuit()
    {
       
    }



        private async void Start()
        {
            var config = await BackendDialogPoint.GetGlobalConfig(gameID);

            if (string.IsNullOrEmpty(config.gameId))
            {
                Debug.LogError($"[GameSessionManager] Global config failed to load for '{gameID}'. Session start aborted.");
                return;
            }

            Debug.Log($"[GameSessionManager] Config ready — initialCoins: {config.initialCoins}, bet: {config.defaultBet}, duration: {config.durationInSeconds}s");
            OnConfigLoaded?.Invoke();
            CheckGameStatus(gameID);
        }

        public void CheckGameStatus(string id)
        { 
            if(ServiceLocator.TryGet<DataKeeperServer>(out var dataKeeperServer))
            {
               List<GameData> games = dataKeeperServer.playerData.games;

               if( games == null || games.Count == 0 || games.Exists(g => g.gameId == id) == false)
               {     
                    StartWithInActiveSession();
                    Debug.Log("No games found for player.");
               }

               if(games.Exists(g => g.gameId == id && g.state == "active"))
               {
                    Debug.Log($"Game {id} is already active.");
                    BackendDialogPoint.GetActiveGame(id, dataKeeperServer.playerData.games.Find(g => g.gameId == id).sessionId);
               }
            
               
            }

            
        }

        public void StartWithInActiveSession()
        {
            OnInactiveSession?.Invoke();
        }

        public async void RequestNewGame()
        {
            if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks) ||
                string.IsNullOrEmpty(dks.playerData.userId))
            {
                Debug.LogError("[GameSessionManager] Cannot request new game: player data not available.");
                return;
            }

            var Id = dks.playerData.userId;
            
            await BackendDialogPoint.GetNewGame(gameID, Id);

            if (!string.IsNullOrEmpty(dks.activeSession.sessionId))
            {
                var sessionId = dks.activeSession.sessionId;
                Debug.Log($"[GameSessionManager] Session started: {sessionId}");
                
                StartBuildingGame();
            }
            else
            {
                Debug.LogError("[GameSessionManager] Session creation finished but no session ID found.");
            }
        }

        public void StartBuildingGame()
        {
            if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks))
            {
                Debug.LogError("[GameSessionManager] StartBuildingGame: DataKeeperServer not found.");
                return;
            }

            if (!ServiceLocator.TryGet<GameBuilder>(out var builder))
            {
                Debug.LogError("[GameSessionManager] StartBuildingGame: GameBuilder not found.");
                return;
            }

            
            builder.BuildGame(dks.activeSession);
             OnLevelUp -= builder.UpdateBaskets;
            OnLevelUp += builder.UpdateBaskets;
            // Notify UIs 
            onSessionLoaded?.Invoke();
        }
}
}