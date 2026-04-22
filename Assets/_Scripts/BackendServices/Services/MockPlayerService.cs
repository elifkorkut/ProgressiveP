using System;
using System.Threading.Tasks;


namespace ProgressiveP.Backend
{
    public class MockPlayerService : IPlayerService
    { 
        private readonly NetworkSimulator     _network;
        
        public MockPlayerService(NetworkSimulator network)
        {
            _network = network;
        }
        
        public async Task<BackendResult<BackendData>> GetOrCreatePlayerAsync(string playerId)
        {
            try
            {
                await _network.SimulateAsync();

                if (StorageProvider.LoadPlayer(playerId, out var data))
                {
                   return BackendResult<BackendData>.Success( new BackendData (data));
                }

                
                var newPlayerData = StorageProvider.CreatePlayer(playerId);
               
                if (newPlayerData != null)
                {
                    return BackendResult<BackendData>.Success( new BackendData (newPlayerData));
                }

                return BackendResult<BackendData>.Failure("404", "Could not create or find player.");
            }
            catch (Exception ex)
            {
                return BackendResult<BackendData>.Failure("500", ex.Message);
            }
        }

        public async Task<BackendResult<BackendData>> GetPlayerAsync(string userId)
        {
            try
            {
                await _network.SimulateAsync();

                if (StorageProvider.LoadPlayer(userId, out var data))
                {
                   return BackendResult<BackendData>.Success( new BackendData (data));
                }

                return BackendResult<BackendData>.Failure("404", "Player not found.");
            }
            catch (Exception ex)
            {
                return BackendResult<BackendData>.Failure("500", ex.Message);
            }
        }

        public async Task<BackendResult<bool>> SavePlayerAsync(BackendData playerData)
        {
            // Placeholder for implementation
            await _network.SimulateAsync();
            return BackendResult<bool>.Success(true);
        }

        public async Task<BackendResult<bool>> UpdateCoinsAsync(string userId, double coins)
        {
            // Placeholder for implementation
            await _network.SimulateAsync();
            return BackendResult<bool>.Success(true);
        }
    }


 }


    
    
    