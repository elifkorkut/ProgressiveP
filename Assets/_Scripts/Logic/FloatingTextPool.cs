using System.Collections.Generic;
using UnityEngine;
using ProgressiveP.Core;

namespace ProgressiveP.Logic
{

    public class FloatingTextPool : MonoBehaviour
    {
        [SerializeField] private GameObject floatingTextPrefab;
        [SerializeField] private int        prewarmCount = 20;

        private readonly Stack<FloatingText> _pool = new Stack<FloatingText>(20);

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            Prewarm();
        }

        private void OnDestroy()
        {
            ServiceLocator.Remove<FloatingTextPool>();
        }

        private void Prewarm()
        {
            if (floatingTextPrefab == null) return;
            for (int i = 0; i < prewarmCount; i++)
            {
                var go = Instantiate(floatingTextPrefab);
                go.SetActive(false);
                var ft = go.GetComponent<FloatingText>();
                if (ft != null) _pool.Push(ft);
            }
        }

        public FloatingText Get(Vector3 position)
        {
            FloatingText ft;
            if (_pool.Count > 0)
            {
                ft = _pool.Pop();
            }
            else
            {
                if (floatingTextPrefab == null) return null;
                var go = Instantiate(floatingTextPrefab);
                ft = go.GetComponent<FloatingText>();
                if (ft == null) return null;
            }
            // before activating 
            ft.transform.position = position;
            return ft;
        }

        public void Return(FloatingText ft)
        {
            if (ft == null) return;
            ft.gameObject.SetActive(false);
            _pool.Push(ft);
        }
    }
}
