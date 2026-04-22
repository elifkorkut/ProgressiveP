using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using ProgressiveP.Core;

/// <summary>
/// Manages the player's coin balance with two write paths:
///
///   1. UseCoins(amount) — immediate optimistic deduction (bet placement).
///      Calls IPlayerRepository.UpdateCoinsAsync so the debit is persisted.
///
///   2. ApplyServerBalance(newBalance) — sync UI to the server-authoritative
///      balance after RewardBatchQueue confirms a batch. Does NOT write to
///      the repository (MockServerService already did that in ValidateHitBatchAsync).
///
/// This separation prevents double-counting: the server is the source of truth
/// for balance after payouts, while the client handles optimistic bet deductions.
///
/// Execution order: -50 (after SimulatedBackend/-100)
/// </summary>
namespace ProgressiveP.Logic
{
[DefaultExecutionOrder(-50)]
public class CoinManager : MonoBehaviour
{
   /* public static CoinManager Instance { get; private set; }

    // Lowercase alias keeps existing call sites compiling unchanged
    public static CoinManager instance => Instance;

    [SerializeField] private TextMeshProUGUI coinText;

    private double             _coins;
    private string             _playerId;
   

   

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private async void Start()
    {
        if (!ServiceLocator.TryGet<IPlayerRepository>(out _playerRepo))
        {
            Debug.LogWarning("[CoinManager] IPlayerRepository not found — using PlayerPrefs fallback.");
            _coins = PlayerPrefs.GetFloat("coins", 1000f);
            RefreshUI();
            return;
        }

        _playerId = SystemInfo.deviceUniqueIdentifier;
        string name = $"Player_{_playerId.Substring(0, Mathf.Min(6, _playerId.Length))}";

        var result = await _playerRepo.GetOrCreatePlayerAsync(_playerId, name);
        if (result.IsSuccess)
        {
            _coins = result.Data.coins;
        }
        else
        {
            Debug.LogWarning($"[CoinManager] Backend unavailable, using fallback: {result.ErrorMessage}");
            _coins = PlayerPrefs.GetFloat("coins", 1000f);
        }

        RefreshUI();

        if (GameSessionManager.Instance != null)
            await GameSessionManager.Instance.StartNewSessionAsync(_playerId);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Immediately deducts a bet from the local balance and persists via IPlayerRepository.
    /// </summary>
    public void UseCoins(float amount)
    {
        _coins = Math.Max(0, _coins - amount);
        RefreshUI();
        _ = _playerRepo?.UpdateCoinsAsync(_playerId, -amount);
    }

    /// <summary>
    /// Legacy direct credit — use only for non-server-validated awards (bonuses, etc.).
    /// For ball payouts, server calls through RewardBatchQueue → ApplyServerBalance.
    /// </summary>
    public void GainedCoins(float amount)
    {
        _coins += amount;
        RefreshUI();
        _ = _playerRepo?.UpdateCoinsAsync(_playerId, amount);
    }

    /// <summary>
    /// Syncs the local balance to the server-authoritative value returned by
    /// MockServerService.ValidateHitBatchAsync. Does NOT write to IPlayerRepository
    /// (the server already updated it). Purely a UI sync operation.
    /// </summary>
    public void ApplyServerBalance(double newBalance)
    {
        _coins = newBalance;
        RefreshUI();
    }

    public float getCoins() => (float)_coins;
    public float GetCoins() => (float)_coins;
    public string PlayerId  => _playerId;

    // ── Internal ──────────────────────────────────────────────────────────────

    private void RefreshUI()
    {
        if (coinText != null)
            coinText.text = _coins.ToString("F0");
    }

    private async void OnApplicationQuit()
    {
        if (GameSessionManager.Instance != null)
            await GameSessionManager.Instance.EndCurrentSessionAsync();
    }
}*/

}}