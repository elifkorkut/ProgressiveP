using System;
using TMPro;
using UnityEngine;
using ProgressiveP.Core;
using ProgressiveP.Logic.Effects;
using Random = UnityEngine.Random;

namespace ProgressiveP.Logic
{
public class CollectionBasket : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private float           spawnSpreadX = 0.3f;

    private float _displayMultiplier;
    private int   _bucketIndex;
    public  int   BucketIndex => _bucketIndex;

    public static event EventHandler<OnBasketHit> EarnedCoins;

    public class OnBasketHit : EventArgs
    {
        public float winnings;   // actual payout 
        public float betAmount;
        public float factor;     // multiplier shown on basket
        public int   bucketIndex;
    }

    private Animator _animator;

    [SerializeField] private SpriteRenderer main;
    [SerializeField] private SpriteRenderer shadow;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    

    public void Setup(int bucketIndex, float displayMultiplier, Color color, Color shadowColor)
    {
        _bucketIndex        = bucketIndex;
        _displayMultiplier  = displayMultiplier;

        if (multiplierText != null)
            multiplierText.text = displayMultiplier.ToString("F1").Replace(",", ".");

        if (color != Color.green)
        {
            if (main   != null) main.color   = color;
            if (shadow != null) shadow.color = shadowColor;
        }
    }

    

    private void OnCollisionEnter2D(Collision2D col)
    {
        SoundManager.Instance?.PlayBallLand(_displayMultiplier);

        if (_animator != null)
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= 1f || info.length <= 0f)
                _animator.SetTrigger("trigger");
        }

        var ballScript = col.gameObject.GetComponent<PlinkoBall>();
        if (ballScript == null) return;

        float bet = ballScript.GetBetValue();
        
        int   targetIdx   = ballScript.TargetBucketIndex >= 0
                            ? ballScript.TargetBucketIndex
                            : _bucketIndex;
        float multiplier  = GetMultiplierForBucket(targetIdx);
        int   payout      = Mathf.RoundToInt(bet * multiplier);
        bool  isWin       = payout >= (int)bet;

        EarnedCoins?.Invoke(this, new OnBasketHit
        {
            winnings    = payout,
            betAmount   = bet,
            factor      = multiplier,
            bucketIndex = targetIdx
        });


        Vector3 contact = col.contacts.Length > 0 ? (Vector3)col.contacts[0].point : transform.position;
        SpawnFloatingText(payout - Mathf.RoundToInt(bet), isWin, contact);

        StartCoroutine(ReturnNextFrame(col.gameObject));
    }

    
    private float GetMultiplierForBucket(int bucketIndex)
    {
        if (!ServiceLocator.TryGet<DataKeeperServer>(out var dks)) return _displayMultiplier;
        var levels = dks.activeSession.gameConfig.levels;
        if (levels == null || levels.Length == 0)                  return _displayMultiplier;

        int lvlIdx = 0;
        if (ServiceLocator.TryGet<GameSessionManager>(out var gsm))
            lvlIdx = gsm.CurrentLevelIndex;

        lvlIdx = Mathf.Clamp(lvlIdx, 0, levels.Length - 1);
        var mults = levels[lvlIdx].multipliers;
        if (mults == null || bucketIndex >= mults.Length) return _displayMultiplier;
        return mults[bucketIndex];
    }

    private void SpawnFloatingText(float amount, bool isWin, Vector3 contactPos)
    {
        if (!ServiceLocator.TryGet<FloatingTextPool>(out var pool)) return;

        float   jitter   = Random.Range(-spawnSpreadX, spawnSpreadX);
        Vector3 spawnPos = contactPos + new Vector3(jitter, 0.5f, 0f);

        var ft = pool.Get(spawnPos);
        if (ft == null) return;
        ft.Setup(amount, isWin, f => pool.Return(f));
    }

    private System.Collections.IEnumerator ReturnNextFrame(GameObject ballObj)
    {
        yield return null;
        if (BallPool.Instance != null) BallPool.Instance.Return(ballObj);
        else                            Destroy(ballObj);
    }
}
}
