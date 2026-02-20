using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    [SerializeField] private Transform tileA;
    [SerializeField] private Transform tileB;
    [SerializeField] private float scrollSpeed = 2.5f;
    [SerializeField] private float tileWidthWorld = 10f; // set to sprite width in world units

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        float dt = Time.deltaTime;
        MoveLeft(tileA, dt);
        MoveLeft(tileB, dt);

        // Loop tiles: when one is fully off left, move it to the right of the other
        if (tileA.position.x <= -tileWidthWorld)
        {
            tileA.position = new Vector3(tileB.position.x + tileWidthWorld, tileA.position.y, tileA.position.z);
        }
        else if (tileB.position.x <= -tileWidthWorld)
        {
            tileB.position = new Vector3(tileA.position.x + tileWidthWorld, tileB.position.y, tileB.position.z);
        }
    }

    private void MoveLeft(Transform t, float dt)
    {
        if (t == null) return;
        Vector3 p = t.position;
        p.x -= scrollSpeed * dt;
        t.position = p;
    }
}
