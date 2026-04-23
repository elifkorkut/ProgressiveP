using TMPro;
using UnityEngine;
using ProgressiveP.Logic;

public class SessionTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void OnEnable()
    {
        SessionTimer.OnTick                += HandleTick;
        SessionTimer.OnExpired             += HandleExpired;
        GameSessionManager.OnSessionLoaded += HandleSessionLoaded;
    }

    private void OnDisable()
    {
        SessionTimer.OnTick                -= HandleTick;
        SessionTimer.OnExpired             -= HandleExpired;
        GameSessionManager.OnSessionLoaded -= HandleSessionLoaded;
    }

    private void HandleSessionLoaded()
    {
        if (SessionTimer.Instance != null)
            HandleTick(SessionTimer.Instance.TimeRemaining);
    }

    private void HandleTick(float remaining)
    {
        if (timerText == null) return;
        int minutes = (int)(remaining / 60f);
        int seconds = (int)(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void HandleExpired()
    {
        if (timerText != null)
            timerText.text = "00:00";
    }
}
