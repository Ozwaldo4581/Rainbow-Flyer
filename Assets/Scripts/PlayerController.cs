using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite ascendingSprite; // up.png
    [SerializeField] private Sprite descendingSprite; // Flying.png
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Collision (optional but recommended)")]
    [SerializeField] private Collider2D playerCollider;

    [Header("Physics")]
    [SerializeField] private float flapVelocity = 6.5f; // tune: 5-7
    [SerializeField] private float gravityScale = 1.6f; // tune for feel
    [SerializeField] private float maxUpwardVelocity = 10f; // clamp optional
    [SerializeField] private bool clampTopOfScreen = false;

    [Header("Bounds (optional)")]
    [SerializeField] private float topClampY = 5f;

    private Rigidbody2D rb;
    private Vector3 startPos;
    private Quaternion startRot;
    private bool simEnabled = false;
    private bool flapRequested = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        startRot = transform.rotation;

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        rb.gravityScale = gravityScale;
        rb.simulated = false; // controlled by GameManager state

        // Start hidden until GameState.Playing
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (playerCollider != null) playerCollider.enabled = false;
    }

    public void SetSimEnabled(bool enabled)
    {
        simEnabled = enabled;
        rb.simulated = enabled;

        // Show/hide player visuals + collisions based on sim state
        if (spriteRenderer != null) spriteRenderer.enabled = enabled;
        if (playerCollider != null) playerCollider.enabled = enabled;

        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            flapRequested = false;
        }
    }

    public void RequestFlap()
    {
        // Deferred to FixedUpdate so we stay physics-correct with Rigidbody2D
        if (!simEnabled) return;
        flapRequested = true;
    }

    public void ResetPlayer()
    {
        transform.position = startPos;
        transform.rotation = startRot;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        flapRequested = false;

        // Ensure hidden while not playing; GameManager will enable on Playing
        if (spriteRenderer != null) spriteRenderer.enabled = simEnabled;
        if (playerCollider != null) playerCollider.enabled = simEnabled;
    }

    private void Update()
    {
        if (!simEnabled) return;

        // Sprite swap based on vertical velocity
        if (spriteRenderer != null)
        {
            if (rb.linearVelocity.y > 0.01f && ascendingSprite != null) spriteRenderer.sprite = ascendingSprite;
            else if (descendingSprite != null) spriteRenderer.sprite = descendingSprite;
        }
    }

    private void FixedUpdate()
    {
        if (!simEnabled) return;

        if (flapRequested)
        {
            flapRequested = false;

            Vector2 v = rb.linearVelocity;
            v.y = flapVelocity;
            if (v.y > maxUpwardVelocity) v.y = maxUpwardVelocity;
            rb.linearVelocity = v;
        }

        if (clampTopOfScreen && transform.position.y > topClampY)
        {
            Vector3 p = transform.position;
            p.y = topClampY;
            transform.position = p;

            Vector2 v = rb.linearVelocity;
            if (v.y > 0) v.y = 0;
            rb.linearVelocity = v;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Any collision = death (flowers or ground)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
