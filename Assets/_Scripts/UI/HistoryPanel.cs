using System.Collections.Generic;
using UnityEngine;
using ProgressiveP.Core;
using ProgressiveP.Logic;


public class HistoryPanel : MonoBehaviour
{
    [SerializeField] private GameObject historyEntryPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private int maxVisibleEntries = 25;

    
    private readonly Queue<GameObject> _entries = new Queue<GameObject>(26);
    private void OnEnable()
    {
        //RewardBatchQueue.OnRewardsConfirmed += HandleRewardsConfirmed;
    }

    private void OnDisable()
    {
        //RewardBatchQueue.OnRewardsConfirmed -= HandleRewardsConfirmed;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void HandleRewardsConfirmed(RewardRecord[] rewards)
    {
        // Traverse in reverse so the last-confirmed reward ends up on top
        for (int i = rewards.Length - 1; i >= 0; i--)
            AddEntry(rewards[i]);
    }

    private void AddEntry(RewardRecord record)
    {
        if (historyEntryPrefab == null || contentParent == null) return;

        var entry = Instantiate(historyEntryPrefab, contentParent);
        entry.transform.SetAsFirstSibling();
        entry.GetComponent<HistoryEntryUI>()?.Setup(record);
        _entries.Enqueue(entry);

        // Trim oldest entries to keep the layout responsive
        while (_entries.Count > maxVisibleEntries)
            Destroy(_entries.Dequeue());
    }
}

