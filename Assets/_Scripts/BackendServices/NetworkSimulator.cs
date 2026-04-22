using UnityEngine;
using System.Threading.Tasks;


namespace ProgressiveP.Backend
{ public class NetworkSimulator
    {
        private readonly float _minDelayMs;
        private readonly float _maxDelayMs;
       
        public NetworkSimulator(float minDelayMs = 80f, float maxDelayMs = 250f)
        {
            _minDelayMs = minDelayMs;
            _maxDelayMs = maxDelayMs;
        }
        public async Task SimulateAsync()
        {
            int delayMs = Mathf.RoundToInt(UnityEngine.Random.Range(_minDelayMs, _maxDelayMs));
            await Task.Delay(delayMs);
        }

        /// cache-first reads
        public async Task SimulateCacheReadAsync()
        {
            int delayMs = Mathf.RoundToInt(_minDelayMs*0.5f);
            await Task.Delay(delayMs);
        }
    }
    
}
