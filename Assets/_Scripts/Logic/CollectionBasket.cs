using System;
using TMPro;
using UnityEngine;
using ProgressiveP.Logic.Effects;

namespace ProgressiveP.Logic
{
public class CollectionBasket : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI multiplierText;
    private float _displayMultiplier;
    private int _bucketIndex;

    public static event EventHandler<OnBasketHit> EarnedCoins;

    public class OnBasketHit : EventArgs
    {
        public float winnings;  // always 0 here; UI should listen to RewardBatchQueue
        public float factor;    // display multiplier shown on basket
        public int   bucketIndex;
    }

    private Animator _animator;

    [SerializeField] private SpriteRenderer main;
    [SerializeField] private SpriteRenderer shadow;

  

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by GameBuilder when constructing or rebuilding the board.
    /// bucketIndex is the server-side index used to look up the multiplier.
    /// </summary>
    public void Setup(int bucketIndex, float displayMultiplier, Color color, Color shadowColor)
    {
        _bucketIndex = bucketIndex;
        _displayMultiplier = displayMultiplier;

        if (multiplierText != null)
            multiplierText.text = displayMultiplier.ToString("F1").Replace(",", ".");

        if (color != Color.green)
        {
            if (main != null) main.color   = color;
            if (shadow != null) shadow.color = shadowColor;
        }
    }

    // ── Collision ─────────────────────────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D col)
    {
        // ── Audio — delegated to SoundManager (polyphony-capped) ──────────────
        SoundManager.Instance?.PlayBallLand(_displayMultiplier);

        // ── Animator trigger (skip if already animating to same state) ─────────
        if (_animator != null)
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= 1f || info.length <= 0f)
                _animator.SetTrigger("trigger");
        }

        var ballScript = col.gameObject.GetComponent<PlinkoBall>();
        if (ballScript == null) return;

        float betAmount = ballScript.GetBetValue();

        // ── Route hit to server via batch queue (SERVER calculates reward) ─────
      //  RewardBatchQueue.Instance?.EnqueueHit(_bucketIndex, betAmount);

        // ── Visual-only event (payout is 0 — actual confirmed rewards come later) ─
        EarnedCoins?.Invoke(this, new OnBasketHit
        {
            winnings    = 0f,          // do not use for wallet — listen to RewardBatchQueue
            factor      = _displayMultiplier,
            bucketIndex = _bucketIndex
        });

        // ── Return ball to pool (zero GC vs Destroy) ──────────────────────────
        if (BallPool.Instance != null)
            BallPool.Instance.Return(col.gameObject);
        else
            Destroy(col.gameObject);
    }
}

}