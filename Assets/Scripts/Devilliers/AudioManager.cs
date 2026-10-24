using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Super simple audio. Put this on ONE GameObject in your main menu scene (the first scene).
/// It survives scene changes, so don't put it in the gameplay scene.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    [Tooltip("Exact name of your main menu scene")]
    public string menuSceneName = "MainMenu";
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Sound Effects")]
    public AudioClip jumpSound;
    public AudioClip clickSound;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    AudioSource musicSource;
    AudioSource sfxSource;

    void Awake()
    {
        // Only one AudioManager ever
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Awake runs after the first scene loads, so handle the first scene manually
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Pick the right music for this scene
        AudioClip wanted = scene.name == menuSceneName ? menuMusic : gameMusic;
        if (musicSource.clip != wanted)
        {
            musicSource.clip = wanted;
            if (wanted != null) musicSource.Play();
        }

        // Auto-add the click sound to every button in the scene
        foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            b.onClick.AddListener(PlayClick);
        }
    }

    public void PlayJump()
    {
        if (jumpSound != null) sfxSource.PlayOneShot(jumpSound, sfxVolume);
    }

    public void PlayClick()
    {
        if (clickSound != null) sfxSource.PlayOneShot(clickSound, sfxVolume);
    }
}