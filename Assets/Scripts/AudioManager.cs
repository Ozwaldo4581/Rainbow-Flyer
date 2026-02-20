using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public void StopSfx()
    {
        if (sfxSource != null) sfxSource.Stop();
    }

    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip musicLoop;
    [SerializeField] private AudioClip deathJingle;
    [SerializeField] private AudioClip flapSfx;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Play music on launch and keep looping until death.
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (musicSource == null || musicLoop == null) return;

        if (musicSource.clip != musicLoop)
            musicSource.clip = musicLoop;

        musicSource.loop = true;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    public void PlayDeath()
    {
        if (sfxSource == null || deathJingle == null) return;
        sfxSource.PlayOneShot(deathJingle);
    }

    public void PlayFlap()
    {
        if (sfxSource == null || flapSfx == null) return;
        sfxSource.PlayOneShot(flapSfx);
    }
}
