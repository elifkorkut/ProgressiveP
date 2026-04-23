using System;
using UnityEngine;
using ProgressiveP.Core;

namespace ProgressiveP.Logic
{
    
    public class SessionTimer : MonoBehaviour
    {
        public static SessionTimer Instance { get; private set; }

               public static event Action<float> OnTick;
        public static event Action OnExpired;

        public float TimeRemaining { get; private set; }
        public bool  IsRunning     { get; private set; }

        private long _startTimeTicks;
        private int  _durationSeconds;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Remove<SessionTimer>();
                Instance = null;
            }
        }

    
        public void Initialize(long startTimeTicks, int durationSeconds)
        {
            _startTimeTicks  = startTimeTicks;
            _durationSeconds = durationSeconds;
            TimeRemaining = ComputeRemaining();
            IsRunning = TimeRemaining > 0f;

            if (!IsRunning) OnExpired?.Invoke();
        }

        private float ComputeRemaining()
        {
            float elapsed = (float)((DateTime.UtcNow.Ticks - _startTimeTicks)
                                    / (double)TimeSpan.TicksPerSecond);
            return Mathf.Max(0f, _durationSeconds - elapsed);
        }

        private void Update()
        {
            if (!IsRunning) return;
            TimeRemaining = ComputeRemaining();
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                IsRunning     = false;
                OnExpired?.Invoke();
            }
            OnTick?.Invoke(TimeRemaining);
        }
    }
}
