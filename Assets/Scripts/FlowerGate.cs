using UnityEngine;

public class FlowerGate : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Transform topStem;
    [SerializeField] private Transform topHead;
    [SerializeField] private Transform bottomHead;
    [SerializeField] private Transform bottomStem;
    [SerializeField] private BoxCollider2D scoreZone;

    [SerializeField] private float bottomHeadYOffset = -0.15f; // negative = head lower


    [Header("Sprite Renderers (for sizing)")]
    [SerializeField] private SpriteRenderer topStemRenderer;
    [SerializeField] private SpriteRenderer topHeadRenderer;
    [SerializeField] private SpriteRenderer bottomHeadRenderer;
    [SerializeField] private SpriteRenderer bottomStemRenderer;

    [Header("Gap Randomization (NON-NEGOTIABLE)")]
    [SerializeField] private float gapMin = 1.2f; // GAP_MIN in world units
    [SerializeField] private float gapMax = 2.2f; // GAP_MAX in world units
    [SerializeField] private float margin = 0.6f; // safe margin from top and ground (world units)

    [Header("Score Zone")]
    [SerializeField] private float scoreZoneWidth = 0.6f;

    private bool scored = false;

    private Vector3 topStemBaseScale;
    private Vector3 bottomStemBaseScale;
    private Vector3 topStemOffsetFromHead;
    private Vector3 bottomStemOffsetFromHead;


    private void Awake()
    {
        if (topStem != null) topStemBaseScale = topStem.localScale;
        if (bottomStem != null) bottomStemBaseScale = bottomStem.localScale;

        if (topStem != null) topStemBaseScale = topStem.localScale;
        if (bottomStem != null) bottomStemBaseScale = bottomStem.localScale;

        // Cache rigid pair offsets from the authored prefab layout
        if (topStem != null && topHead != null)
            topStemOffsetFromHead = topStem.position - topHead.position;

        if (bottomStem != null && bottomHead != null)
            bottomStemOffsetFromHead = bottomStem.position - bottomHead.position;
    }

    private void Reset()
    {
        // Helpful if you drop script on prefab
        scoreZone = GetComponentInChildren<BoxCollider2D>();
    }

    public void Configure(float minY, float maxY, float groundTopY, float topSafeMargin)
    {
        
        scored = false;

        // Reset pooled transforms (fixed stems: never stretch)
        if (topStem != null) topStem.localScale = topStemBaseScale;
        if (bottomStem != null) bottomStem.localScale = bottomStemBaseScale;

        float m = Mathf.Max(margin, topSafeMargin);

        // 1) Random gap size in [gapMin, gapMax]
        float gap = Random.Range(gapMin, gapMax);

        // 2) Random vertical placement within safe bounds
        float halfGap = gap * 0.5f;
        float minCenterY = groundTopY + m + halfGap;
        float maxCenterY = (maxY - m) - halfGap;

        if (maxCenterY < minCenterY)
        {
            float mid = (minCenterY + maxCenterY) * 0.5f;
            minCenterY = mid;
            maxCenterY = mid;
        }

        float gapCenterY = Random.Range(minCenterY, maxCenterY);
        float gapTopY = gapCenterY + halfGap;
        float gapBottomY = gapCenterY - halfGap;

        // ============================
        // POSITION HEADS NEAR GAP
        // ============================
        float topHeadH = GetWorldHeight(topHeadRenderer);
        float bottomHeadH = GetWorldHeight(bottomHeadRenderer);

        float topHeadCenterY = gapTopY + (topHeadH * 0.5f);
        float bottomHeadCenterY = gapBottomY - (bottomHeadH * 0.5f);

        SetLocalY(topHead, topHeadCenterY);
        SetLocalY(bottomHead, bottomHeadCenterY);

        // ============================
        // STEMS MOVE AS RIGID PAIRS (NO SCALING, JUST REPOSITION)
        // ============================
        if (topStem != null && topHead != null)
            topStem.position = topHead.position + topStemOffsetFromHead;

        if (bottomStem != null && bottomHead != null)
            bottomStem.position = bottomHead.position + bottomStemOffsetFromHead + Vector3.up * bottomHeadYOffset;
        



        // ============================
        // SCORE ZONE (TRIGGER)
        // ============================
        if (scoreZone != null)
        {
            scoreZone.isTrigger = true;

            // Move the ScoreZone object to the gap center in WORLD space.
            Vector3 szPos = scoreZone.transform.position;
            szPos.y = gapCenterY;
            scoreZone.transform.position = szPos;

            scoreZone.offset = Vector2.zero;
            scoreZone.size = new Vector2(scoreZoneWidth, gap);
        }
    }

    private float GetWorldHeight(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null) return 1f;
        return sr.bounds.size.y;
    }

    private void SetLocalY(Transform t, float worldY)
    {
        if (t == null) return;
        Vector3 p = t.position;
        p.y = worldY;
        t.position = p;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        

        if (scored) return;
        if (!other.CompareTag("Player")) return;

        scored = true;
        

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(1);
        }
    }
}
