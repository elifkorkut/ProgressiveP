using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System;
using ProgressiveP.Core;
using ProgressiveP.Logic.Effects;

namespace ProgressiveP.Logic
{



public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [SerializeField] private string gameID;
    [SerializeField] private int    batchSize         = 5;
    [SerializeField] private float  batchFlushSeconds = 10f;  // second or size
   
    public static event Action<LevelConfig> OnLevelUp;
    public static event Action              OnConfigLoaded;
    public static event Action<int>         OnBallCountChanged;
   
    public static event Action<int>         OnBalanceChanged;
   
    public static event Action<RewardRecord[]> OnHistoryLoaded;

    public event Action OnInactiveSession;
    public event Action OnActiveSession;

    public Action onSessionRequested;
    public static event Action OnSessionLoaded;

    
    private int _currentLevelIndex;
    private int _ballsDroppedThisLevel;
    private int _nextBallIndex;  
    private int _ballsRemainingInSession;
    private int _localBalance;          // optimistic balance — updated immediately
    private float _timeSinceLastBatch;    // seconds since last batch 
    private string _currentPlayerId;
    private LevelBallData _currentLevelBallData;
    private List<int> _unsentBallIndices = new List<int>();

    public int RemainingBalls => _ballsRemainingInSession;

    private const string CacheKeyPrefix = "ball_cache_";


    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        ServiceLocator.Register(this);
        onSessionRequested += RequestNewGame;
    }

    private void OnDisable()
    {
        if (Instance == this)
            ServiceLocator.Remove<GameSessionManager>();
        onSessionRequested             -= RequestNewGame;
        GameStateController.OnStateChanged -= OnGameStateChanged;
        SessionTimer.OnExpired             -= OnSessionExpired;
        CollectionBasket.EarnedCoins       -= OnBasketEarnedCoins;
    }

    private void Update()
    {
        // Time-based batch flush — 
        if (_unsentBallIndices.Count > 0 && GameStateController.Instance != null && GameStateController.Instance.IsActive)
        {
            _timeSinceLastBatch += Time.deltaTime;
            if (_timeSinceLastBatch >= batchFlushSeconds)
                _ = SendBatchAsync();
        }
    }

    private async void Start()
    {
        var config = await BackendDialogPoint.GetGlobalConfig(gameID);
        if (string.IsNullOrEmpty(config.gameId))
        {
            Debug.LogError($"[GSM] Global config failed for '{gameID}'.");
            return;
        }
        Debug.Log($"[GSM] Config ready — bet:{config.defaultBet} dur:{config.durationInSeconds}s");
        OnConfigLoaded?.Invoke();

        string authId = ProgressiveP.Core.Authenticator.playerId;
        if (string.IsNullOrEmpty(authId))
        {
            Debug.LogError("[GSM] No authenticated player ID available. Aborting.");
            return;
        }

        await BackendDialogPoint.LoadPlayer(
            authId,
            onError:   err  => Debug.LogError($"[GSM] Player load failed: {err}"),
            onSuccess: data => DataReader.LoadPlayerData(data));

         if (ServiceLocator.TryGet<DataKeeperServer>(out var dks) &&
            !string.IsNullOrEmpty(dks.playerData.userId))
            _currentPlayerId = dks.playerData.userId;
        else
            _currentPlayerId = authId;

        CheckGameStatus(gameID);
    }

    
    public void CheckGameStatus(string id)
    {
        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks))
        {
            StartWithInActiveSession();
            return;
        }

        var games = dks.playerData.games;
        if (games == null || games.Count == 0 || !games.Exists(g => g.gameId == id))
        {
            Debug.Log("[GSM] No game record found — starting fresh.");
            StartWithInActiveSession();
            return;
        }

        var activeGame = games.Find(g => g.gameId == id && g.state == "active");
        if (string.IsNullOrEmpty(activeGame.gameId))
        {
            StartWithInActiveSession();
            return;
        }

        Debug.Log($"[GSM] Found active session {activeGame.sessionId} — validating...");
        _currentPlayerId = dks.playerData.userId;
        _ = BackendDialogPoint.ValidateActiveSession(
            _currentPlayerId, id, activeGame.sessionId,
            onExpired: () =>
            {
                Debug.Log("[GSM] Session expired — starting fresh.");
                StartWithInActiveSession();
            },
            onValid: session =>
            {
                DataReader.OnNewSessionCreated?.Invoke(session);
                _ = RecoverAndBuildAsync();
            });
    }

    private async Task RecoverAndBuildAsync()
    {
        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks)) return;

        string sessionId = dks.activeSession.sessionId;
        var    cache     = LoadLocalCache(sessionId);

        if (cache.HasValue && cache.Value.indices.Count > 0)
        {
            Debug.Log($"[GSM] Recovering {cache.Value.indices.Count} unsent balls...");
            await BackendDialogPoint.SendBallBatch(
                _currentPlayerId, gameID, sessionId,
                cache.Value.levelIndex,
                cache.Value.indices.ToArray(),
                dks.globalGameConfig.defaultBet,
                onError: err  => Debug.LogError($"[GSM] Recovery batch failed: {err}"),
                onSuccess: res =>
                {
                    ApplyBatchResult(res, dks);
                    ClearLocalCache(sessionId);
                });
            await BackendDialogPoint.ValidateActiveSession(
                _currentPlayerId, gameID, sessionId,
                onExpired: () => { },
                onValid: freshSession =>
                {
                    DataReader.OnNewSessionCreated?.Invoke(freshSession);
                });
        }
        StartBuildingGame();
    }

    public void StartWithInActiveSession() => OnInactiveSession?.Invoke();

    //New game 
    public async void RequestNewGame()
    {
        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks) ||
            string.IsNullOrEmpty(dks.playerData.userId))
        {
            Debug.LogError("[GSM] Cannot request new game: no player data.");
            return;
        }
        _currentPlayerId = dks.playerData.userId;
        await BackendDialogPoint.GetNewGame(gameID, _currentPlayerId);

        if (!string.IsNullOrEmpty(dks.activeSession.sessionId))
            StartBuildingGame();
        else
            Debug.LogError("[GSM] Session creation finished but no session ID found.");
    }

    //Board building 
    public void StartBuildingGame()
    {
        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks))     { Debug.LogError("[GSM] No DataKeeperServer."); return; }
        if (!ServiceLocator.TryGet<GameBuilder>(out var builder))       { Debug.LogError("[GSM] No GameBuilder.");      return; }

        // Restore 
        var session = dks.activeSession;
        _currentLevelIndex = session.currentLevelIndex;      
        _ballsDroppedThisLevel = session.ballsDroppedThisLevel; 
        _nextBallIndex = 0;
        _currentPlayerId = dks.playerData.userId;
        _unsentBallIndices.Clear();

        // Remaining balls
        int initialBalls = dks.globalGameConfig.initialBalls > 0 ? dks.globalGameConfig.initialBalls : 200;
        _ballsRemainingInSession = Mathf.Max(0, initialBalls - session.totalBallsSpent);
        OnBallCountChanged?.Invoke(_ballsRemainingInSession);

        // Seed optimistic balance
        _localBalance       = dks.playerData.balance;
        _timeSinceLastBatch = 0f;
        OnBalanceChanged?.Invoke(_localBalance);

        // Subscribe for optimistic balance
        CollectionBasket.EarnedCoins -= OnBasketEarnedCoins;
        CollectionBasket.EarnedCoins += OnBasketEarnedCoins;

        builder.BuildGame(dks.activeSession);
        OnLevelUp -= builder.UpdateBaskets;
        OnLevelUp += builder.UpdateBaskets;

        // update the board visuals
      
        if (_currentLevelIndex > 0 && dks.activeSession.gameConfig.levels != null &&
            _currentLevelIndex < dks.activeSession.gameConfig.levels.Length)
        {
            builder.UpdateBaskets(dks.activeSession.gameConfig.levels[_currentLevelIndex]);
        }

        // Initialize countdown 
        if (ServiceLocator.TryGet<SessionTimer>(out var timer))
            timer.Initialize(dks.activeSession.startTimeTicks, dks.globalGameConfig.durationInSeconds);
        else
            Debug.LogError("[GSM] SessionTimer not found in ServiceLocator — add a SessionTimer component to the scene.");

        SessionTimer.OnExpired             -= OnSessionExpired;
        SessionTimer.OnExpired             += OnSessionExpired;
        GameStateController.OnStateChanged -= OnGameStateChanged;
        GameStateController.OnStateChanged += OnGameStateChanged;

        _ = ActivateAndLoadAsync(dks);
    }

    private async Task ActivateAndLoadAsync(DataKeeperServer dks)
    {
        // the board is ready
        await BackendDialogPoint.ActivateSession(
            _currentPlayerId, gameID, dks.activeSession.sessionId,
            onError:   err => Debug.LogError($"[GSM] Activation failed: {err}"),
            onSuccess: ()  => Debug.Log("[GSM] Session activated."));

        // Fetch pre-calculated ball targets 
        await LoadLevelBallTargetsAsync(_currentLevelIndex, dks);

        // Active state 
        if (ServiceLocator.TryGet<GameStateController>(out var gsc))
            gsc.SetState(GameState.Active);

        OnActiveSession?.Invoke();
        OnSessionLoaded?.Invoke();

      var history = dks.activeSession.rewardHistory;
        if (history != null && history.Length > 0)
            OnHistoryLoaded?.Invoke(history);
    }

    private async Task LoadLevelBallTargetsAsync(int levelIndex, DataKeeperServer dks = null)
    {
        if (dks == null && !ServiceLocator.TryGet(out dks)) return;

        var levels = dks.activeSession.gameConfig.levels;
        if (levels == null || levelIndex >= levels.Length) return;

        int   ballCount = levels[levelIndex].numberOfBallsToPass;
        float bet       = dks.globalGameConfig.defaultBet;

        if (ballCount <= 0)
        {
            _currentLevelBallData = default;
            return;
        }

        await BackendDialogPoint.GetLevelBallTargets(
            _currentPlayerId, gameID, dks.activeSession.sessionId,
            levelIndex, ballCount, bet,
            onError:   err  => Debug.LogError($"[GSM] Ball targets failed: {err}"),
            onSuccess: data =>
            {
                _currentLevelBallData = data;
                _nextBallIndex        = 0;
                Debug.Log($"[GSM] Level {levelIndex} targets loaded ({ballCount} balls).");
            });
    }

    //Ball tracking

   
    public int GetAndIncrementBallIndex() => _nextBallIndex++;

   
    public int GetTargetBucketIndex(int ballIndex)
    {
        if (_currentLevelBallData.targets == null) return -1;
        foreach (var t in _currentLevelBallData.targets)
            if (t.ballIndex == ballIndex) return t.bucketIndex;
        return -1;
    }

    public int CurrentLevelIndex => _currentLevelIndex;

    public void OnBallSpawned(int ballIndex)
    {
        _ballsDroppedThisLevel++;
        _unsentBallIndices.Add(ballIndex);

      
        _ballsRemainingInSession = Mathf.Max(0, _ballsRemainingInSession - 1);
        OnBallCountChanged?.Invoke(_ballsRemainingInSession);

       
        if (ServiceLocator.TryGet<DataKeeperServer>(out var dksBalance))
        {
            int bet = (int)dksBalance.globalGameConfig.defaultBet;
            _localBalance -= bet;
           
            OnBalanceChanged?.Invoke(_localBalance);
        }

        //  crash recovery
        if (ServiceLocator.TryGet<DataKeeperServer>(out var dks))
            SaveLocalCache(dks.activeSession.sessionId, _currentLevelIndex, _unsentBallIndices);

        // Send batch 
        if (_unsentBallIndices.Count >= batchSize)
            _ = SendBatchAsync();

        // Check level-up
        _ = CheckLevelUpAsync();
    }

    private async Task CheckLevelUpAsync()
    {
        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks)) return;
        var levels = dks.activeSession.gameConfig.levels;
        if (levels == null || _currentLevelIndex >= levels.Length) return;

        int threshold = levels[_currentLevelIndex].numberOfBallsToPass;
        if (threshold > 0 && _ballsDroppedThisLevel >= threshold)
            await TriggerLevelUpAsync();
    }

    private async Task SendBatchAsync()
    {
        if (_unsentBallIndices.Count == 0) return;

        _timeSinceLastBatch = 0f;   // reset the time-based flush timer

        var batch = _unsentBallIndices.ToArray();
        _unsentBallIndices.Clear();

        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks)) return;

        await BackendDialogPoint.SendBallBatch(
            _currentPlayerId, gameID, dks.activeSession.sessionId,
            _currentLevelIndex, batch, dks.globalGameConfig.defaultBet,
            onError: err => Debug.LogError($"[GSM] Batch failed: {err}"),
            onSuccess: res =>
            {
                ClearLocalCache(dks.activeSession.sessionId);
                ApplyBatchResult(res, dks);
            });
    }

    private void ApplyBatchResult(BallBatchResult res, DataKeeperServer dks)
    {
        var pd = dks.playerData;
        pd.balance = (int)res.newBalance;
        dks.SetPlayerData(pd);
       
        _localBalance = pd.balance;
        OnBalanceChanged?.Invoke(_localBalance);
        if (res.rewards != null && res.rewards.Length > 0)
        {
            var session = dks.activeSession;
            int prevLen = session.rewardHistory?.Length ?? 0;
            var merged  = new RewardRecord[prevLen + res.rewards.Length];
            if (prevLen > 0) System.Array.Copy(session.rewardHistory, merged, prevLen);
            System.Array.Copy(res.rewards, 0, merged, prevLen, res.rewards.Length);
            session.rewardHistory = merged;
            dks.SetActiveSession(session);
        }
    }

    private void OnBasketEarnedCoins(object sender, CollectionBasket.OnBasketHit args)
    {
        
        _localBalance += (int)args.winnings;
        OnBalanceChanged?.Invoke(_localBalance);
    }

    // Level up
    private async Task TriggerLevelUpAsync()
    {
        if (!ServiceLocator.TryGet<GameStateController>(out var gsc)) return;
        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks))    return;

       
        if (gsc.CurrentState == GameState.LevelTransition || gsc.CurrentState == GameState.Idle) return;

       
        if (_unsentBallIndices.Count > 0) await SendBatchAsync();

        gsc.SetState(GameState.LevelTransition);

        var levels    = dks.activeSession.gameConfig.levels;
        int nextLevel = _currentLevelIndex + 1;
        if (nextLevel >= levels.Length) nextLevel = levels.Length - 1; 

        _currentLevelIndex     = nextLevel;
        _ballsDroppedThisLevel = 0;
        _nextBallIndex         = 0;

        
        OnLevelUp?.Invoke(levels[nextLevel]);

        // Notify backend
        await BackendDialogPoint.UpdateSessionLevel(
            _currentPlayerId, gameID, dks.activeSession.sessionId, nextLevel,
            onError:   err => Debug.LogError($"[GSM] Level update failed: {err}"),
            onSuccess: ()  => Debug.Log($"[GSM] Level updated to {nextLevel}."));

       
        await LoadLevelBallTargetsAsync(nextLevel, dks);

        gsc.SetState(GameState.Active);

        SoundManager.Instance?.CoinsEarned();
    }

    //State callbacks
    private void OnGameStateChanged(GameState from, GameState to) { }

    private void OnSessionExpired()
    {
        Debug.Log("[GSM] Session timer expired.");
        if (ServiceLocator.TryGet<GameStateController>(out var gsc))
            gsc.SetState(GameState.Expired);
        OnInactiveSession?.Invoke();
    }

    private void SaveLocalCache(string sessionId, int levelIndex, List<int> indices)
    {
        if (string.IsNullOrEmpty(sessionId)) return;
        PlayerPrefs.SetString(CacheKeyPrefix + sessionId,
            levelIndex + "|" + string.Join(",", indices));
    }

    private (int levelIndex, List<int> indices)? LoadLocalCache(string sessionId)
    {
        string raw = PlayerPrefs.GetString(CacheKeyPrefix + sessionId, null);
        if (string.IsNullOrEmpty(raw)) return null;

        var parts   = raw.Split('|');
        if (parts.Length < 2) return null;

        if (!int.TryParse(parts[0], out int lvl)) return null;
        var indices = new List<int>();
        if (!string.IsNullOrEmpty(parts[1]))
            foreach (var s in parts[1].Split(','))
                if (int.TryParse(s, out int i)) indices.Add(i);

        return (lvl, indices);
    }

    private void ClearLocalCache(string sessionId) =>
        PlayerPrefs.DeleteKey(CacheKeyPrefix + sessionId);

    
    private void OnApplicationQuit()
    {
      
        PlayerPrefs.Save();
        Debug.Log("[GSM] PlayerPrefs flushed on quit — unsent balls cached for recovery.");
    }

    private void OnApplicationPause(bool paused)
    {
    
        if (paused) PlayerPrefs.Save();
    }
}
}
