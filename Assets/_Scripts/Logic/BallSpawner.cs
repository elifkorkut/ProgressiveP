using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        GameObject ballObj;

        if (BallPool.Instance != null)
        {
            // Pool path — zero allocation
            ballObj = BallPool.Instance.Get();
            ballObj.transform.SetParent(transform);
        }
        else
        {
            // Fallback for scenes without a BallPool component
            ballObj = Instantiate(ball, transform);
        }

        ballObj.transform.localPosition = Vector3.zero;
        ballObj.GetComponent<PlinkoBall>().Setup(increment, betAmount);
    }
}

}