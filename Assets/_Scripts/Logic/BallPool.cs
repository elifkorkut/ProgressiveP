using System.Collections.Generic;
using UnityEngine;

namespace ProgressiveP.Logic
{
    [DefaultExecutionOrder(-45)]
    public class BallPool : MonoBehaviour
    {
        public static BallPool Instance { get; private set; }

    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private int        initialPoolSize = 30;

    private readonly Stack<GameObject> _pool = new Stack<GameObject>(30);
    private Transform _poolRoot;

   
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance  = this;

        _poolRoot = new GameObject("[BallPool_Root]").transform;
        _poolRoot.SetParent(transform);

        Prewarm(initialPoolSize);
    }

     public GameObject Get()
    {
        if (_pool.Count == 0)
        {
            Debug.LogWarning("[BallPool] Pool exhausted — expanding by 1. " +
                             "Increase 'initialPoolSize' to prevent GC spikes.");
            Prewarm(1);
        }

        var ball = _pool.Pop();
        ball.SetActive(true);
        return ball;
    }

    
    public void Return(GameObject ball)
    {
        if (ball == null) return;

        ball.GetComponent<PlinkoBall>()?.ResetForPool();
        ball.SetActive(false);
        ball.transform.SetParent(_poolRoot);
        _pool.Push(ball);
    }


    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var ball = Instantiate(ballPrefab, _poolRoot);
            ball.SetActive(false);
            _pool.Push(ball);
        }
    }
}
}