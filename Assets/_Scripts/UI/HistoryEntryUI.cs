using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProgressiveP.Core;

/// <summary>
/// Displays one entry in the history list.
/// Reused by HistoryPanel. Setup() is called once after instantiation.
/// </summary>
public class HistoryEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI payoutText;
    [SerializeField] private Image           background;

    // Cached colors — avoids Color allocation on each call
    private static readonly Color ColorHighMult = new Color(1.00f, 0.27f, 0.27f, 0.90f);
    private static readonly Color ColorMedMult  = new Color(1.00f, 0.60f, 0.18f, 0.90f);
    private static readonly Color ColorLowMult  = new Color(0.95f, 0.92f, 0.23f, 0.90f);
    private static readonly Color ColorLoss     = new Color(0.45f, 0.45f, 0.45f, 0.75f);

    public void Setup(RewardRecord record)
    {
        if (multiplierText != null)
            multiplierText.text = record.multiplier.ToString("F1") + "x";

        if (payoutText != null)
            payoutText.text = record.payout >= record.betAmount
                ? "+" + record.payout.ToString("F0")
                : record.payout.ToString("F0");

        if (background != null)
            background.color = record.multiplier switch
            {
                >= 10f => ColorHighMult,
                >= 2f  => ColorMedMult,
                >= 1f  => ColorLowMult,
                _      => ColorLoss,
            };
    }
}
