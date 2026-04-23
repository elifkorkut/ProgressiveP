using System;
using System.Threading.Tasks;

namespace ProgressiveP.Backend
{
    public interface ISessionService
    {
        Task<BackendResult<BackendData>> CreateSessionAsync(
            string playerId, string gameId,
            Action<string>      onError,
            Action<BackendData> onSuccess);

       
        Task<BackendResult<BackendData>> ActivateSessionAsync(
            string playerId, string gameId, string sessionId,
            Action<string>      onError,
            Action<BackendData> onSuccess);

        //Verify the session hasn't expired
        Task<BackendResult<BackendData>> ValidateActiveSessionAsync(
            string playerId, string gameId, string sessionId,
            Action<string>      onError,
            Action<BackendData> onSuccess);

        // calculate target bucket for every ball in the level
        Task<BackendResult<BackendData>> GetLevelBallTargetsAsync(
            string playerId, string gameId, string sessionId,
            int levelIndex, int ballCount, float betPerBall,
            Action<string>      onError,
            Action<BackendData> onSuccess);

        //Resolve a batch of ball indices
        Task<BackendResult<BackendData>> ProcessBallBatchAsync(
            string playerId, string gameId, string sessionId,
            int levelIndex, int[] ballIndices, float betPerBall,
            Action<string>      onError,
            Action<BackendData> onSuccess);

        //Inform backend -> new level
        Task<BackendResult<BackendData>> UpdateSessionLevelAsync(
            string playerId, string gameId, string sessionId,
            int newLevelIndex,
            Action<string>      onError,
            Action<BackendData> onSuccess);
    }
}
