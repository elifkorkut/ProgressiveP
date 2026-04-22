using TMPro;
using UnityEngine;
using ProgressiveP.Core;
using ProgressiveP.Logic;

/// <summary>
/// Displays remaining ball count.
/// On session load: sets count to initialBalls from the session's game config.
/// </summary>
public class BallCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ballCountText;
    [SerializeField] private string formatString = "{0} balls";

    private void OnEnable()
    {
        if (ServiceLocator.TryGet<GameSessionManager>(out var mgr))
            mgr.onSessionLoaded += HandleSessionLoaded;
    }

    private void OnDisable()
    {
        if (ServiceLocator.TryGet<GameSessionManager>(out var mgr))
            mgr.onSessionLoaded -= HandleSessionLoaded;
    }

    private void HandleSessionLoaded()
    {
        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks)) return;

        int initialBalls = dks.activeSession.gameConfig.initialBalls;
        SetCount(initialBalls);
    }

    public void SetCount(int count)
    {
        if (ballCountText != null)
            ballCountText.text = string.Format(formatString, count);
    }

    private void HandleBallCountChanged(int remaining, int total)
    {
        SetCount(remaining);
    }
}
