using System;
using System.Threading.Tasks;
using UnityEngine;


namespace ProgressiveP.Backend
{
    
    public class MockGameService : IGameService
    {  
        private readonly NetworkSimulator _network;
         public MockGameService(NetworkSimulator network)
        {
            _network = network;
        }

        public async Task<BackendResult<BackendData>> GetGameConfigAsync(string gameId, Action<string> onError, Action<BackendData> onSuccess)
        {
            try
            {
                await _network.SimulateAsync();

                if (StorageProvider.LoadGame(gameId, out var config))
                {
                   var backendData = new BackendData(config);
                   onSuccess?.Invoke(backendData);
                   return BackendResult<BackendData>.Success(backendData);
                }
                else
                {
                    string errorMessage = $"Game config not found for gameId: {gameId}";
                    onError?.Invoke(errorMessage);
                    return BackendResult<BackendData>.Failure("404", errorMessage);
                }
       }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to fetch game config: {ex.Message}";
                onError?.Invoke(errorMessage);
                return BackendResult<BackendData>.Failure("500", errorMessage);
            }
        }
       

        




    public async Task<BackendResult<BackendData>> GetGameConfigGlobalAsync(string gameId, Action<string> onError, Action<BackendData> onSuccess)
        {
            try
            {
                await _network.SimulateAsync();

                if (StorageProvider.LoadGameGlobalConfig(gameId, out var config))
                {
                   var backendData = new BackendData(config);
                   onSuccess?.Invoke(backendData);
                   return BackendResult<BackendData>.Success(backendData);
                }
                else
                {
                    string errorMessage = $"Game config not found for gameId: {gameId}";
                    onError?.Invoke(errorMessage);
                    return BackendResult<BackendData>.Failure("404", errorMessage);
                }
           }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to fetch game config: {ex.Message}";
                onError?.Invoke(errorMessage);
                return BackendResult<BackendData>.Failure("500", errorMessage);
            }
        }
       

        
    }

}

