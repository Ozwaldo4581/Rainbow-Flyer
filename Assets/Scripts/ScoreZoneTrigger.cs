using UnityEngine;

public class ScoreZoneTrigger : MonoBehaviour
{
    private bool scored;

    private void OnEnable() => scored = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        

        if (scored) return;
        if (!other.CompareTag("Player")) return;

        scored = true;
        

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(1);
    }
}
