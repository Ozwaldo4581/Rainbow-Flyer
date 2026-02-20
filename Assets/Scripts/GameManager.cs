using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { Ready, Playing, GameOver }

    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GateSpawner gateSpawner;
    [SerializeField] private TrailController trail;
    [SerializeField] private UIController ui;

    [Header("Gameplay")]
    [SerializeField] private float readyTimeScale = 0f;   // Freeze world in Ready (recommended)
    [SerializeField] private float playingTimeScale = 1f;

    // Best score persistence
    private const string BEST_SCORE_KEY = "BEST_SCORE";
    public int BestScore { get; private set; } = 0;

    public GameState State { get; private set; } = GameState.Ready;
    public int Score { get; private set; } = 0;

    // Score snapshot for the just-finished run
    public int LastRunScore { get; private set; } = 0;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        BestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
    }

    private void Start()
    {
        // Music plays on launch and persists until death
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic();

        SetState(GameState.Ready);
        ResetRun(); // ensures clean start
    }

    public void StartFromButton()
    {
        if (State != GameState.Ready) return;

        // Ensure music is on when a run begins (e.g., after returning to Ready)
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic();

        ResetRun();
        SetState(GameState.Playing);

        player.RequestFlap();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayFlap();
    }

    public void PlayAgainFromButton()
    {
        // from GameOver -> back to menu/ready (or change to Playing if you prefer)
        ResetRun();
        SetState(GameState.Ready);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic();

        if (AudioManager.Instance != null) AudioManager.Instance.StopSfx();

    }


    private void Update()
    {
        if (GetTapDown())
        {
            if (State == GameState.Playing)
            {
                player.RequestFlap();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayFlap();
            }

            // IMPORTANT: no tap-anywhere behavior in GameOver or Ready
        }
    }

    private bool GetTapDown()
    {
        if (Input.GetKeyDown(KeyCode.Space)) return true;
        if (Input.GetMouseButtonDown(0)) return true;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) return true;
        }

        return false;
    }

    private void StartPlayingAndFlap()
    {
        SetState(GameState.Playing);
        player.RequestFlap();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayFlap();
    }

    public void AddScore(int amount)
    {
        if (State != GameState.Playing) return;

        Score += amount;
        ui.SetScore(Score);
        trail.OnScoreChanged(Score);
    }

    public void GameOver()
    {
        if (State != GameState.Playing) return;

        // Capture final score immediately
        LastRunScore = Score;

        // Stop music + play death jingle
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayDeath();
        }

        // Commit best score before showing game over UI
        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, BestScore);
            PlayerPrefs.Save();
        }

        SetState(GameState.GameOver);
    }

    private void SetState(GameState newState)
    {
        State = newState;

        if (State == GameState.Ready) Time.timeScale = readyTimeScale;
        else Time.timeScale = playingTimeScale;

        ui.SetState(State, Score);
        gateSpawner.SetSpawningEnabled(State == GameState.Playing);
        player.SetSimEnabled(State == GameState.Playing);
    }

    public void ResetRun()
    {
        Time.timeScale = 1f;

        Score = 0;
        LastRunScore = 0;
        ui.SetScore(Score);

        gateSpawner.ResetSpawner();
        player.ResetPlayer();
        trail.ResetTrail();

        if (State == GameState.Ready) Time.timeScale = readyTimeScale;
        if (State == GameState.Playing) Time.timeScale = playingTimeScale;
    }
}
