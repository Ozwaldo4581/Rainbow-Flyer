using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrailController : MonoBehaviour
{
    [Header("Trail Growth")]
    [SerializeField] private int trailCapPoints = 80;
    [SerializeField] private int pointsPerScore = 10; // trailMaxPoints = min(cap, score * pointsPerScore)
    [SerializeField] private float sampleInterval = 0.05f; // seconds between samples

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LineRenderer line;
    [Header("World Scroll (match GateSpawner)")]
    [SerializeField] private float worldScrollSpeed = 2.5f;



    private Vector3[] ring;
    private Vector3[] ordered; // reused array for SetPositions (no alloc)
    private int ringHead = 0;
    private int ringCount = 0;
    private float sampleTimer = 0f;
    private int targetMaxPoints = 0;

    private void Awake()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (player == null) player = transform;

        ring = new Vector3[trailCapPoints];
        ordered = new Vector3[trailCapPoints];

        line.positionCount = 0;
    }

    public void ResetTrail()
    {
        ringHead = 0;
        ringCount = 0;
        sampleTimer = 0f;
        targetMaxPoints = 0;
        line.positionCount = 0;
    }

    public void OnScoreChanged(int score)
    {
        targetMaxPoints = Mathf.Clamp(score * pointsPerScore, 0, trailCapPoints);
        if (targetMaxPoints == 0)
        {
            line.positionCount = 0;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        float dt = Time.deltaTime;

        // Drift all stored points left (ring buffer safe)
        if (ringCount > 0)
        {
            float dx = worldScrollSpeed * dt;
            for (int i = 0; i < trailCapPoints; i++)
            {
                ring[i].x -= dx;
            }
        }

        if (targetMaxPoints <= 0)
        {
            line.positionCount = 0;
            return;
        }

        // Sample new points on interval
        sampleTimer += dt;
        if (sampleTimer >= sampleInterval)
        {
            sampleTimer -= sampleInterval;
            AddPoint(player.position);
        }

        // Render every frame so drift is visible continuously
        RenderTrail();
    }


    private void AddPoint(Vector3 pos)
    {
        ring[ringHead] = pos;
        ringHead = (ringHead + 1) % trailCapPoints;
        if (ringCount < trailCapPoints) ringCount++;
    }

    private void RenderTrail()
    {
        int count = Mathf.Min(ringCount, targetMaxPoints);
        if (count <= 0)
        {
            line.positionCount = 0;
            return;
        }

        // Oldest-to-newest order for LineRenderer
        int start = (ringHead - count);
        while (start < 0) start += trailCapPoints;

        for (int i = 0; i < count; i++)
        {
            int idx = (start + i) % trailCapPoints;
            ordered[i] = ring[idx];
        }

        line.positionCount = count;
        line.SetPositions(ordered);
    }
}
