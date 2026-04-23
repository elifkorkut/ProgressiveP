using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProgressiveP.Core;


public class HistoryEntryUI : MonoBehaviour
{
   
    [SerializeField] private TextMeshProUGUI payoutText;
    
    private static readonly Color ColorHighMult = new Color(1.00f, 0.27f, 0.27f, 0.90f);
    private static readonly Color ColorMedMult = new Color(1.00f, 0.60f, 0.18f, 0.90f);
    private static readonly Color ColorLowMult = new Color(0.95f, 0.92f, 0.23f, 0.90f);
    private static readonly Color ColorLoss = new Color(0.45f, 0.45f, 0.45f, 0.75f);

    public void Setup(RewardRecord record)
    {
        if (payoutText != null)
            payoutText.text = record.payout >= record.betAmount
                ? "+" + record.payout.ToString("F0")
                : record.payout.ToString("F0");

      payoutText.color= record.multiplier switch
            {
                >= 10f => ColorHighMult,
                >= 2f  => ColorMedMult,
                >= 1f  => ColorLowMult,
                _      => ColorLoss,
            };
    }
}
