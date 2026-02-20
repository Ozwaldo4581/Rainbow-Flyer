using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject hudPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text hudScoreText;
    [SerializeField] private TMP_Text gameOverScoreText;

    [Header("Art Placeholders (drop future sprites here)")]
    public Image startArtImage;     // Drop your future start screen sprite here
    public Image gameOverArtImage;  // Drop your future game over sprite here

    [Header("Run Score / Best Score")]
    [SerializeField] private TMP_Text runScoreText;
    [SerializeField] private TMP_Text bestScoreText;

    public void SetState(GameManager.GameState state, int score)
    {
        

        if (startPanel != null) startPanel.SetActive(state == GameManager.GameState.Ready);
        if (gameOverPanel != null) gameOverPanel.SetActive(state == GameManager.GameState.GameOver);
        if (hudPanel != null) hudPanel.SetActive(state == GameManager.GameState.Playing);

        if (state == GameManager.GameState.GameOver)
        {
            
            if (gameOverScoreText != null)
                gameOverScoreText.text = $"{score}";

            if (runScoreText != null)
                runScoreText.text = $"{GameManager.Instance.LastRunScore}";

            if (bestScoreText != null)
                bestScoreText.text = $"{GameManager.Instance.BestScore}";
        }
    }

    public void SetScore(int score)
    {
        if (hudScoreText != null)
            hudScoreText.text = score.ToString();

        if (gameOverScoreText != null &&
            GameManager.Instance != null &&
            GameManager.Instance.State == GameManager.GameState.GameOver)
        {
            gameOverScoreText.text = $"Score: {score}";
        }
    }
}
