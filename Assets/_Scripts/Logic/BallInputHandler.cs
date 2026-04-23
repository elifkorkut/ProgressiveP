using UnityEngine;
using ProgressiveP.Core;
using UnityEngine.EventSystems;

namespace ProgressiveP.Logic
{
    
    public class BallInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float spawnInterval = 0.35f;

        private float _nextSpawnTime;
        private bool  _held;

        public void OnPointerDown(PointerEventData eventData)
        {
            _held = true;
            _nextSpawnTime = 0f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _held = false;
        }

        private void Update()
        {
            if (!_held || Time.time < _nextSpawnTime) return;

            _nextSpawnTime = Time.time + spawnInterval;

            float bet = 0f;
            if (ServiceLocator.TryGet<DataKeeperServer>(out var dks))
                bet = dks.globalGameConfig.defaultBet;

            BallSpawner.instance?.SpawnBall(bet);
        }
    }
}
