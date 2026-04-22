using UnityEngine;

namespace ProgressiveP.Core
{
    public class DataKeeperLocal : MonoBehaviour
    {
       [SerializeField] private PlayerData playerData;
       [SerializeField] private GameData levelData;

       private void Awake()
       {
           DontDestroyOnLoad(gameObject);
       }
        private void OnEnable()
        {
           ServiceLocator.Register(this);
        }

        private void OnDisable()
        {
           ServiceLocator.Remove<DataKeeperLocal>();
        }

        public void SetPlayerData(PlayerData data)
         {
              
              
         }

    }

}
     