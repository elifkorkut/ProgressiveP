using TMPro;
using UnityEngine;


public class FpsCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float           updateInterval = 0.25f;

    private float _elapsed;
    private int   _frameCount;
    private float _fps;

    private void Update()
    {
        _frameCount++;
        _elapsed += Time.unscaledDeltaTime;

        if (_elapsed < updateInterval) return;

        _fps        = _frameCount / _elapsed;
        _elapsed    = 0f;
        _frameCount = 0;

        if (fpsText != null)
            fpsText.text = $"{_fps:F0} FPS";
    }
}
