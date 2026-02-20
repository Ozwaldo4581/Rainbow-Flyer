using System.Collections.Generic;
using UnityEngine;

public class GateSpawner : MonoBehaviour
{
    [Header("Prefab & Pool")]
    [SerializeField] private FlowerGate gatePrefab;
    [SerializeField] private int initialPoolSize = 8;

    [Header("Spawn & Movement")]
    [SerializeField] private float scrollSpeed = 2.5f;
    [SerializeField] private float spawnSpacingX = 3.0f; // world units between gates (distance-based feel)
    [SerializeField] private float spawnX = 7.5f;        // where new gates appear (right side)
    [SerializeField] private float despawnX = -7.5f;     // when past left side, recycle

    [Header("Vertical Bounds")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform groundTopMarker; // place at top of ground visually
    [SerializeField] private float topMargin = 0.6f;     // safe margin from top (also used in gate)

    private readonly Queue<FlowerGate> pool = new Queue<FlowerGate>(32);
    private readonly List<FlowerGate> active = new List<FlowerGate>(32);
    private int spawnCount = 0;

    private bool spawningEnabled = false;
    private float spawnTimer = 0f;
    private float spawnInterval = 1.0f;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Start()
    {
        WarmPool();
        RecomputeInterval();
    }

    private void WarmPool()
    {
        if (gatePrefab == null) return;

        for (int i = 0; i < initialPoolSize; i++)
        {
            FlowerGate g = Instantiate(gatePrefab, transform);
            g.gameObject.SetActive(false);
            pool.Enqueue(g);
        }
    }

    private void RecomputeInterval()
    {
        // Time-based scheduler that matches a distance spacing:
        // distance = speed * time  => time = distance / speed
        spawnInterval = (scrollSpeed <= 0.0001f) ? 999f : (spawnSpacingX / scrollSpeed);
    }

    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;
        if (!enabled) spawnTimer = 0f;
    }

    public void ResetSpawner()
    {
        // Recycle all active gates
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Recycle(active[i]);
        }
        active.Clear();
        spawnTimer = 0f;
    }

    private void Update()
    {
        if (!spawningEnabled) return;

        float dt = Time.deltaTime;

        // Move active gates left
        for (int i = active.Count - 1; i >= 0; i--)
        {
            FlowerGate g = active[i];
            Vector3 p = g.transform.position;
            p.x -= scrollSpeed * dt;
            g.transform.position = p;

            if (p.x < despawnX)
            {
                Recycle(g);
                active.RemoveAt(i);
            }
        }

        spawnTimer += dt;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer -= spawnInterval;
            SpawnGate();
        }
    }

    private void SpawnGate()
    {
        spawnCount++; // ✅ increment once per spawn

        FlowerGate g = GetFromPool();
        g.transform.position = new Vector3(spawnX, 0f, 0f);
        g.gameObject.SetActive(true);

        // Visible vertical span from camera (fixed-vertical strategy)
        float camHalfH = mainCamera.orthographicSize;
        float minY = mainCamera.transform.position.y - camHalfH;
        float maxY = mainCamera.transform.position.y + camHalfH;

        float groundTopY = (groundTopMarker != null) ? groundTopMarker.position.y : (minY + 1.0f);

        g.Configure(minY, maxY, groundTopY, topMargin);

        active.Add(g);

            }


    private FlowerGate GetFromPool()
    {
        if (pool.Count > 0) return pool.Dequeue();

        // Expand pool if needed (still minimal allocations, but rare)
        FlowerGate g = Instantiate(gatePrefab, transform);
        g.gameObject.SetActive(false);
        return g;
    }

    private void Recycle(FlowerGate g)
    {
        g.gameObject.SetActive(false);
        pool.Enqueue(g);
    }
}
