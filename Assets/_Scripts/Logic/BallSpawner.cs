using UnityEngine;
using ProgressiveP.Core;

namespace ProgressiveP.Logic
{   
public class BallSpawner : MonoBehaviour
{
    [SerializeField] GameObject ball;

    private float increment;

    public static BallSpawner instance { get; private set; }

    public void AssignIncrement(float increment)
    {
        this.increment = increment;
    }

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        CancelInvoke();
    }

    public void SpawnBall(float betAmount)
    {
        // Require Active state 
        if (GameStateController.Instance == null || !GameStateController.Instance.IsActive)
        {
            Debug.Log("[BallSpawner] Spawn blocked: game not in Active state.");
            return;
        }

        // Require remaining session balls
        if (ServiceLocator.TryGet<GameSessionManager>(out var gsmCheck) && gsmCheck.RemainingBalls <= 0)
        {
            Debug.Log("[BallSpawner] Spawn blocked: no balls remaining.");
            return;
        }

        GameObject ballObj;
        if (BallPool.Instance != null)
        {
            ballObj = BallPool.Instance.Get();
            ballObj.transform.SetParent(transform);
        }
        else
        {
            ballObj = Instantiate(ball, transform);
        }

        ballObj.transform.localPosition = Vector3.zero;

        var plinkoBall = ballObj.GetComponent<PlinkoBall>();
        plinkoBall.Setup(increment, betAmount);

        // sequential index + pre-calculated target
        if (ServiceLocator.TryGet<GameSessionManager>(out var gsm))
        {
            int ballIndex    = gsm.GetAndIncrementBallIndex();
            int targetBucket = gsm.GetTargetBucketIndex(ballIndex);

            plinkoBall.AssignIndex(ballIndex);

            if (targetBucket >= 0 && ServiceLocator.TryGet<GameBuilder>(out var builder))
            {
                var basketTx = builder.GetBasketTransform(targetBucket);
                plinkoBall.AssignTarget(targetBucket, basketTx);
            }

            gsm.OnBallSpawned(ballIndex);
        }
    }
}
}
