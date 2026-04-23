using TMPro;
using UnityEngine;
using ProgressiveP.Core;
using ProgressiveP.Logic;
public class BalanceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private string formatString = "{0}";

    private void OnEnable()
    {
        
        GameSessionManager.OnBalanceChanged += SetBalance;

        DataReader.OnPlayerDataLoaded       += HandlePlayerDataLoaded;
        GameSessionManager.OnSessionLoaded  += HandleSessionLoaded;
    }

    private void OnDisable()
    {
        GameSessionManager.OnBalanceChanged -= SetBalance;
        DataReader.OnPlayerDataLoaded       -= HandlePlayerDataLoaded;
        GameSessionManager.OnSessionLoaded  -= HandleSessionLoaded;
    }

    private void HandleSessionLoaded()
    {
        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks)) return;
        SetBalance(dks.playerData.balance);
    }

    private void HandlePlayerDataLoaded(PlayerData data)
    {
        SetBalance(data.balance);
    }

    public void SetBalance(int balance)
    {
        if (balanceText != null)
            balanceText.text = string.Format(formatString, balance);
    }
}
