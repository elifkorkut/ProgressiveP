using TMPro;
using UnityEngine;
using ProgressiveP.Logic;
using ProgressiveP.Core;

public class LevelLabelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;

    private void OnEnable()
    {
        GameSessionManager.OnLevelUp       += HandleLevelUp;
        GameSessionManager.OnSessionLoaded += HandleSessionLoaded;
    }

    private void OnDisable()
    {
        GameSessionManager.OnLevelUp       -= HandleLevelUp;
        GameSessionManager.OnSessionLoaded -= HandleSessionLoaded;
    }

    private void HandleSessionLoaded()
    {
        int idx = GameSessionManager.Instance != null
                  ? GameSessionManager.Instance.CurrentLevelIndex + 1
                  : 1;
        UpdateLabel($"Level {idx}");
    }

    private void HandleLevelUp(LevelConfig level)
    {
        int idx = GameSessionManager.Instance != null
                  ? GameSessionManager.Instance.CurrentLevelIndex + 1
                  : 1;
        UpdateLabel($"Level {idx}");
    }

    private void UpdateLabel(string label)
    {
        if (levelText != null)
            levelText.text = label;
    }
}
