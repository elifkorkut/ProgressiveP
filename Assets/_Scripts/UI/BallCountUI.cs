using TMPro;
using UnityEngine;
using ProgressiveP.Core;
using ProgressiveP.Logic;

public class BallCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ballCountText;
    [SerializeField] private string formatString = "{0} balls";

    private void OnEnable()
    {
        GameSessionManager.OnBallCountChanged += HandleBallCountChanged;
        GameSessionManager.OnSessionLoaded    += HandleSessionLoaded;
    }

    private void OnDisable()
    {
        GameSessionManager.OnBallCountChanged -= HandleBallCountChanged;
        GameSessionManager.OnSessionLoaded    -= HandleSessionLoaded;
    }

    private void HandleSessionLoaded()
    {
        // Use the actual remaining count from GameSessionManager (accounts for resume/spent balls).
        if (GameSessionManager.Instance != null)
            SetCount(GameSessionManager.Instance.RemainingBalls);
    }

    private void HandleBallCountChanged(int remaining) => SetCount(remaining);

    public void SetCount(int count)
    {
        if (ballCountText != null)
            ballCountText.text = string.Format(formatString, count);
    }
}

