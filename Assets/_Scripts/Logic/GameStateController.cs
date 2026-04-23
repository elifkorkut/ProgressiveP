using System;
using UnityEngine;
using ProgressiveP.Core;

namespace ProgressiveP.Logic
{

    public class GameStateController : MonoBehaviour
    {
        public static GameStateController Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Idle;

        
        public static event Action<GameState, GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        private void OnDisable()
        {
            if (Instance == this)
                ServiceLocator.Remove<GameStateController>();
        }

        public void SetState(GameState newState)
        {
            if (newState == CurrentState) return;
            var prev = CurrentState;
            CurrentState = newState;
            Debug.Log($"[GameState] {prev} → {newState}");
            OnStateChanged?.Invoke(prev, newState);
        }

        public bool Is(GameState state) => CurrentState == state;
        public bool IsActive           => CurrentState == GameState.Active;
    }

    public enum GameState
    {
        Idle,
        Active,  
        LevelTransition,
        Resetting,
        Expired         
    }
}
