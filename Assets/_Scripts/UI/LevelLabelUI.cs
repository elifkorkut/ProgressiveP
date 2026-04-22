using TMPro;
using UnityEngine;
using ProgressiveP.Logic;
/// <summary>
/// Displays the current level label (e.g. "Level 3") from the server schema.
/// Subscribes to LevelProgressionTracker.OnLevelUp — no Update() cost.
/// </summary>
public class LevelLabelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;

    private void OnEnable()
    {
        //LevelProgressionTracker.OnLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        //LevelProgressionTracker.OnLevelUp -= HandleLevelUp;
    }

    private void Start()
    {
        // Populate on start if tracker is already ready
       
    }

    //private void HandleLevelUp(ProgressiveP.Core.LevelSchema schema)
       // => UpdateLabel(schema.label);

    private void UpdateLabel(string label)
    {
        if (levelText != null)
            levelText.text = label;
    }
}
