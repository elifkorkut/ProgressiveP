using System.Collections;
using UnityEngine;
using ProgressiveP.Core;


namespace ProgressiveP.Logic
{
[DefaultExecutionOrder(-35)]
public class GameBuilder : MonoBehaviour
{
    [SerializeField] private GameObject plinkoBallPrefab;
    [SerializeField] private Transform placedBalls;
    [SerializeField] private GameObject basket;
    [SerializeField] private Transform baskets;
    [SerializeField] private GameObject ballSpawner;

    [SerializeField] private Color red;
    [SerializeField] private Color redShadow;
    [SerializeField] private Color orange;
    [SerializeField] private Color orangeShadow;
    [SerializeField] private Color yellow;
    [SerializeField] private Color yellowShadow;

    private float _increment;

    // Cached so UpdateBaskets can reach them without finding children
    private CollectionBasket[] _currentBaskets;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnDisable()
    {
        ServiceLocator.Remove<GameBuilder>();
    }

   
    public void BuildGame(NewSessionData session)
    {
        var cfg = session.gameConfig;

        if (cfg.levels == null || cfg.levels.Length == 0)
        {
            Debug.LogError("[GameBuilder] Session has no level configs — cannot build board.");
            return;
        }

        var firstLevel = cfg.levels[0];
        BuildBoard(firstLevel.rows, firstLevel.multipliers);
        Debug.Log($"[GameBuilder] Board built — rows: {firstLevel.rows}, baskets: {_currentBaskets?.Length}");
    }

   
    public void UpdateBaskets(LevelConfig level)
    {
        if (_currentBaskets == null || _currentBaskets.Length == 0)
        {
            Debug.LogWarning("[GameBuilder] UpdateBaskets called but no baskets cached. Rebuilding board.");
            BuildBoard(level.rows, level.multipliers);
            return;
        }

        float[] mults = level.multipliers != null && level.multipliers.Length > 0
            ? level.multipliers
            : BuildFallbackMultipliers(level.rows);

        int rows = level.rows;
        for (int x = 0; x < _currentBaskets.Length; x++)
        {
            int   idx  = Mathf.Clamp(x, 0, mults.Length - 1);
            float mult = mults[idx];
            Color col, shadowCol;

            if (x == 0 || x == rows)
                (col, shadowCol) = (red, redShadow);
            else if (x == 1 || x == 2 || x == rows - 1 || x == rows - 2)
                (col, shadowCol) = (orange, orangeShadow);
            else
                (col, shadowCol) = (yellow, yellowShadow);

            _currentBaskets[x].Setup(idx, mult, col, shadowCol);
        }

        Debug.Log($"[GameBuilder] Baskets updated for level — rows: {rows}");
    }

    // ── Board construction ────────────────────────────────────────────────────

    private void BuildBoard(int rows, float[] multipliers)
    {
        foreach (Transform child in placedBalls.transform) Destroy(child.gameObject);
        foreach (Transform child in baskets.transform)     Destroy(child.gameObject);

        float startPosY = -0.4f * (Helpers.GetScreenHeight() / 2f);
        float startPosX = -Helpers.GetScreenWidth() / 2f + plinkoBallPrefab.transform.lossyScale.x / 2f;
        _increment      = 0.03f + 0.005f * (10 - rows);
        float gap       = (Helpers.GetScreenWidth() - plinkoBallPrefab.transform.lossyScale.x) / (rows + 1);
        float startBask = startPosX + gap / 2f;

        float guardSize = rows switch
        {
            8  => 0.375f, 9  => 0.425f, 10 => 0.500f,
            11 => 0.625f, 12 => 0.700f, 14 => 0.800f,
            16 => 0.875f, _  => 0.500f,
        };

        if (multipliers == null || multipliers.Length == 0)
            multipliers = BuildFallbackMultipliers(rows);

        int basketCount = rows + 1;
        _currentBaskets = new CollectionBasket[basketCount];

        for (int i = 0; i < rows; i++)
        {
            for (int x = 0; x < rows + 2 - i; x++)
            {
                var peg = Instantiate(plinkoBallPrefab, placedBalls.transform);
                peg.transform.localScale = new Vector2(
                    Helpers.GetScreenWidth() * _increment,
                    Helpers.GetScreenWidth() * _increment);
                peg.transform.position = new Vector2(
                    startPosX + gap * x + i * gap / 2f,
                    startPosY + gap * i);
                peg.name = "row" + i;

                peg.transform.GetChild(0).localScale    = new Vector2(0.1f, 1f);
                peg.transform.GetChild(0).localPosition = new Vector2( guardSize, -guardSize);
                peg.transform.GetChild(1).localScale    = new Vector2(0.1f, 1f);
                peg.transform.GetChild(1).localPosition = new Vector2(-guardSize, -guardSize);

                if (i == 0 && x < basketCount)
                    _currentBaskets[x] = SpawnBasket(x, rows, multipliers, startBask, gap, startPosY);

                if (i == rows - 1 && x == 1)
                {
                    var spawnerObj = Instantiate(ballSpawner, placedBalls.transform);
                    spawnerObj.GetComponent<BallSpawner>().AssignIncrement(_increment);
                    spawnerObj.transform.position = new Vector2(
                        startPosX + gap * x + i * gap / 2f,
                        startPosY + gap * (i + 2));
                }
            }
        }
    }

    private CollectionBasket SpawnBasket(int x, int rows, float[] multipliers,
                                          float startBask, float gap, float startPosY)
    {
        int   idx  = Mathf.Clamp(x, 0, multipliers.Length - 1);
        float mult = multipliers[idx];
        Color col, shadowCol;

        if (x == 0 || x == rows)
            (col, shadowCol) = (red, redShadow);
        else if (x == 1 || x == 2 || x == rows - 1 || x == rows - 2)
            (col, shadowCol) = (orange, orangeShadow);
        else
            (col, shadowCol) = (yellow, yellowShadow);

        var b = Instantiate(basket, baskets.transform);
        b.name = "basket " + x;
        var cb = b.GetComponent<CollectionBasket>();
        cb.Setup(idx, mult, col, shadowCol);
        b.transform.position   = new Vector2(startBask + x * gap, startPosY - gap);
        b.transform.localScale = new Vector2(gap * 0.9f, gap * 0.9f);
        return cb;
    }

    private static float[] BuildFallbackMultipliers(int rows)
    {
        int    count  = rows + 1;
        float[] mults = new float[count];
        for (int i = 0; i < count; i++)
            mults[i] = 1f;
        return mults;
    }
}
}
