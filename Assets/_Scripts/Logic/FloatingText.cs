using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ProgressiveP.Logic
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float riseSpeed = 1.5f;
        [SerializeField] private float lifetime  = 1.2f;

        private static readonly Color WinColor  = new Color(0.2f, 0.9f, 0.2f);
        private static readonly Color LoseColor = new Color(0.9f, 0.2f, 0.2f);

        private Action<FloatingText> _onComplete;
    
      public void Setup(float amount, bool isWin, Action<FloatingText> onComplete = null)
        {
            _onComplete = onComplete;
            StopAllCoroutines();

            if (label != null)
            {
                // amount is the NET change 
                label.text  = amount >= 0 ? $"+{amount:F0}" : $"{amount:F0}";
                label.color = isWin ? WinColor : LoseColor;
            }

            gameObject.SetActive(true);
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
             // wait a frame for correct pos
            yield return null;

            float elapsed = 0f;
            float startY  = transform.position.y;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                transform.position = new Vector3(
                    transform.position.x,
                    startY + riseSpeed * elapsed,
                    transform.position.z);

                if (label != null)
                {
                    var c = label.color;
                    c.a = Mathf.Lerp(1f, 0f, t * t);
                    label.color = c;
                }
                yield return null;
            }

            if (_onComplete != null)
                _onComplete.Invoke(this);
            else
                Destroy(gameObject);
        }
    }
}
