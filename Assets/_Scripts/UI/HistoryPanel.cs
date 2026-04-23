using System;
using System.Collections.Generic;
using UnityEngine;
using ProgressiveP.Core;
using ProgressiveP.Logic;


public class HistoryPanel : MonoBehaviour
{
    [SerializeField] private GameObject historyEntryPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private int maxVisibleEntries = 25;

    private readonly Queue<HistoryEntryUI> _entries = new Queue<HistoryEntryUI>(26);
    private readonly Stack<HistoryEntryUI> _entryPool = new Stack<HistoryEntryUI>(26);

    private void Awake()
    {
        // Pre-warm 
        if (historyEntryPrefab == null || contentParent == null) return;
        for (int i = 0; i < maxVisibleEntries; i++)
        {
            var go = Instantiate(historyEntryPrefab, contentParent);
            go.SetActive(false);
            var ui = go.GetComponent<HistoryEntryUI>();
            if (ui != null) _entryPool.Push(ui);
        }
    }

    private void OnEnable()
    {
        CollectionBasket.EarnedCoins       += HandleBasketHit;
        GameSessionManager.OnHistoryLoaded += HandleHistoryLoaded;
    }

    private void OnDisable()
    {
        CollectionBasket.EarnedCoins       -= HandleBasketHit;
        GameSessionManager.OnHistoryLoaded -= HandleHistoryLoaded;
    }



    private void HandleHistoryLoaded(RewardRecord[] records)
    {
       
        foreach (var r in records)
            AddEntry(r);
    }

    private void HandleBasketHit(object sender, CollectionBasket.OnBasketHit args)
    {
        var record = new RewardRecord
        {
            bucketIndex          = args.bucketIndex,
            betAmount            = args.betAmount,
            multiplier           = args.factor,
            payout               = (int)args.winnings,
            serverTimestampTicks = DateTime.UtcNow.Ticks
        };
        AddEntry(record);
    }

    private void AddEntry(RewardRecord record)
    {
        if (contentParent == null) return;

        // Get a pooled entry or instantiate a new one if the pool is empty.
        HistoryEntryUI entry;
        if (_entryPool.Count > 0)
        {
            entry = _entryPool.Pop();
        }
        else
        {
            if (historyEntryPrefab == null) return;
            var go = Instantiate(historyEntryPrefab, contentParent);
            entry = go.GetComponent<HistoryEntryUI>();
            if (entry == null) return;
        }

        entry.transform.SetParent(contentParent, false);
        entry.transform.SetAsFirstSibling();
        entry.gameObject.SetActive(true);
        entry.Setup(record);
        _entries.Enqueue(entry);

        // Return oldest entries to pool once the visible cap is exceeded.
        while (_entries.Count > maxVisibleEntries)
        {
            var evicted = _entries.Dequeue();
            evicted.gameObject.SetActive(false);
            _entryPool.Push(evicted);
        }
    }
}

