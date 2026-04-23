using UnityEngine;

namespace ProgressiveP.Logic.Effects
{

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public static SoundManager instance => Instance;
   
    [Header("Clips — UI")]
    [SerializeField] private AudioClip clipUIClick;
    [SerializeField] private AudioClip clipCoinsEarned;

    [Header("Clips — Ball")]
    [SerializeField] private AudioClip clipBallHit;
    [SerializeField] private AudioClip clipBallLand;

  
    [SerializeField, Range(4, 20)] private int poolSize = 12;
    [SerializeField, Range(1, 8)]  private int maxConcurrentHits  = 5;
    [SerializeField, Range(1, 4)]  private int maxConcurrentLands = 3;
    [SerializeField, Range(0f, 0.2f)] private float hitDebounceSeconds = 0.05f;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float volumeUI = 1.00f;
    [SerializeField, Range(0f, 1f)] private float volumeCoins = 0.90f;
    [SerializeField, Range(0f, 1f)] private float volumeHit = 0.55f;
    [SerializeField, Range(0f, 1f)] private float volumeLand = 0.80f;
   
    private AudioSource[] _sources;
    private int[] _priorities;   
    private bool[] _isBallHit;
    private bool[] _isBallLand;

    private float _lastHitTime = -999f;

   
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildPool();
    }

    private void BuildPool()
    {
        _sources = new AudioSource[poolSize];
        _priorities = new int[poolSize];
        _isBallHit  = new bool[poolSize];
        _isBallLand = new bool[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"SFX_{i:D2}");
            go.transform.SetParent(transform);
            _sources[i] = go.AddComponent<AudioSource>();
            _sources[i].playOnAwake = false;
            _priorities[i] = 99;
        }
    }

   
    public void CoinsEarned()  => PlayUI(clipCoinsEarned, volumeCoins);
     public void PlayClick()  => PlayUI(clipUIClick, volumeUI);
    public void PlayBallHit()
    {
        float now = Time.realtimeSinceStartup;
        if (now - _lastHitTime < hitDebounceSeconds) return;
        _lastHitTime = now;
         int active = CountActive(_isBallHit);
         float vol = active >= maxConcurrentHits
            ? Mathf.Max(volumeHit * Mathf.Lerp(1f, 0.1f, (float)active / maxConcurrentHits), 0.07f)
            : volumeHit;

        PlayInternal(clipBallHit, vol, priority: 2, pitchJitter: true,
                     markHit: true, markLand: false);
    }
    public void PlayBallLand(float multiplier = 1f)
    {
        if (CountActive(_isBallLand) >= maxConcurrentLands) return;

        float pitch = multiplier switch
        {
            >= 10f => 1.30f,
            >= 2f  => 1.00f,
            _      => 0.80f,
        };

        PlayInternal(clipBallLand, volumeLand, priority: 1, pitchJitter: false,
                     customPitch: pitch, markHit: false, markLand: true);
    }

    
    private void PlayUI(AudioClip clip, float volume)
        => PlayInternal(clip, volume, priority: 0, pitchJitter: false,
                        markHit: false, markLand: false);

    private void PlayInternal(AudioClip clip, float volume, int priority,
                               bool pitchJitter,
                               bool markHit    = false,
                               bool markLand   = false,
                               float customPitch = 1f)
    {
        if (clip == null) return;

        int idx = AcquireSlot(priority);
        if (idx < 0) return;

        _isBallHit[idx]  = markHit;
        _isBallLand[idx] = markLand;
        _priorities[idx] = priority;

        AudioSource src = _sources[idx];
        src.clip   = clip;
        src.volume = volume;
        src.pitch  = pitchJitter ? UnityEngine.Random.Range(0.85f, 1.15f) : customPitch;
        src.Play();
    }

    
    private int AcquireSlot(int requestPriority)
    {
       
        for (int i = 0; i < _sources.Length; i++)
        {
            if (!_sources[i].isPlaying)
            {
                _isBallHit[i]  = false;
                _isBallLand[i] = false;
                return i;
            }
        }

      
        int worstIdx = -1;
        int worstPri = requestPriority;
        for (int i = 0; i < _sources.Length; i++)
        {
            if (_priorities[i] > worstPri)
            {
                worstPri = _priorities[i];
                worstIdx = i;
            }
        }

        if (worstIdx >= 0)
        {
            _sources[worstIdx].Stop();
            _isBallHit[worstIdx]  = false;
            _isBallLand[worstIdx] = false;
            return worstIdx;
        }

        return -1; // pool full 
    }

    private int CountActive(bool[] flags)
    {
        int count = 0;
        for (int i = 0; i < _sources.Length; i++)
            if (flags[i] && _sources[i].isPlaying) count++;
        return count;
    }
}
}