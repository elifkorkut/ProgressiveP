using TMPro;
using UnityEngine;
using ProgressiveP.Core;
using ProgressiveP.Logic;

/// <summary>
/// Displays the player's coin balance.
/// On session load: reads balance from DataKeeperServer.playerData.
/// Listens to DataReader.OnPlayerDataLoaded for any subsequent updates
/// (e.g. after a save/reload cycle).
/// </summary>
public class BalanceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private string formatString = "{0}";

    private void OnEnable()
    {
        DataReader.OnPlayerDataLoaded += HandlePlayerDataLoaded;

        if (ServiceLocator.TryGet<GameSessionManager>(out var mgr))
            mgr.onSessionLoaded += HandleSessionLoaded;
    }

    private void OnDisable()
    {
        DataReader.OnPlayerDataLoaded -= HandlePlayerDataLoaded;

        if (ServiceLocator.TryGet<GameSessionManager>(out var mgr))
            mgr.onSessionLoaded -= HandleSessionLoaded;
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
