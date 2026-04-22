using UnityEngine;


namespace ProgressiveP.Core
{
    public class DataKeeperServer : MonoBehaviour
    {
        public PlayerData       playerData;
        public GameData         levelData;
        public GlobalGameConfig globalGameConfig;
        public NewSessionData   activeSession;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            DataReader.OnPlayerDataLoaded   += SetPlayerData;
            DataReader.OnGlobalConfigLoaded += SetGlobalConfig;
            DataReader.OnNewSessionCreated  += SetActiveSession;
            ServiceLocator.Register(this);
        }

        private void OnDisable()
        {
            DataReader.OnPlayerDataLoaded   -= SetPlayerData;
            DataReader.OnGlobalConfigLoaded -= SetGlobalConfig;
            DataReader.OnNewSessionCreated  -= SetActiveSession;
            ServiceLocator.Remove<DataKeeperServer>();
        }

        public void SetPlayerData(PlayerData data)       => playerData       = data;
        public void SetGlobalConfig(GlobalGameConfig cfg) => globalGameConfig = cfg;
        public void SetActiveSession(NewSessionData s)    => activeSession    = s;
    }

}
     