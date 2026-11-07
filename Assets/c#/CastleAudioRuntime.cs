using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Shared 2D audio, loaded automatically in every gameplay scene.</summary>
public sealed class CastleAudioRuntime : MonoBehaviour
{
    static CastleAudioRuntime instance;
    static bool quitting;
    public static CastleAudioRuntime Instance
    {
        get
        {
            // Script reloads during Play can clear statics without replaying scene Awake.
            if (instance == null && Application.isPlaying && !quitting)
            {
                instance = FindObjectOfType<CastleAudioRuntime>();
                if (instance == null) new GameObject("[Castle Audio]").AddComponent<CastleAudioRuntime>();
                else instance.InitializeAudio();
            }
            return instance;
        }
    }
    public CastleAudioSettings Settings { get; private set; }
    readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    AudioSource fallbackMusic;
    Coroutine musicRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { instance = null; quitting = false; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        CastleAudioRuntime ready = Instance;
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        InitializeAudio();
    }

    void InitializeAudio()
    {
        DontDestroyOnLoad(gameObject);
        if (Settings == null)
        {
            CastleAudioSettings saved = Resources.Load<CastleAudioSettings>("CastleAudio/Settings");
            Settings = saved != null ? Instantiate(saved) : ScriptableObject.CreateInstance<CastleAudioSettings>();
        }
    }

    void OnEnable()
    {
        if (!Application.isPlaying || (instance != null && instance != this)) return;
        instance = this;
        InitializeAudio();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(ConnectMusic());
    }

    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    void OnApplicationQuit() { quitting = true; }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
        if (Settings != null) Destroy(Settings);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(ConnectMusic());
    }

    IEnumerator ConnectMusic()
    {
        // Let existing scene Awake / Start / LevelReady handlers finish first.
        yield return null;
        AudioClip bgm = LoadClip("BGM/MysteriousCastle_PianoStrings_Loop");
        if (bgm == null) yield break;
        MusicPlayer existing = FindObjectOfType<MusicPlayer>();
        if (existing != null && existing.Source != null)
        {
            if (fallbackMusic != null) fallbackMusic.Stop();
            bool replacePlaceholder = existing.Source.clip == null || existing.Source.clip.name == "Placeholder";
            if (replacePlaceholder)
            {
                existing.bgmVolume = Settings.musicVolume;
                existing.menuPlaylist = new[] { bgm };
                existing.Playlists = new[] { bgm };
                existing.ToPlay(existing.Playlists);
                existing.Source.volume = Settings.musicVolume;
            }
            if (existing.Source.clip == bgm)
            {
                if (!existing.Source.isPlaying && !AudioListener.pause) existing.ToPlay(new[] { bgm });
                existing.Source.loop = true;
            }
        }
        else
        {
            if (fallbackMusic == null)
            {
                fallbackMusic = gameObject.AddComponent<AudioSource>();
                fallbackMusic.playOnAwake = false;
                fallbackMusic.spatialBlend = 0f;
                fallbackMusic.loop = true;
                fallbackMusic.priority = 160;
            }
            fallbackMusic.volume = Settings.musicVolume;
            if (!fallbackMusic.isPlaying)
            {
                fallbackMusic.clip = bgm;
                fallbackMusic.Play();
            }
        }
        musicRoutine = null;
    }

    public AudioClip LoadClip(string relativePath)
    {
        AudioClip clip;
        if (!clips.TryGetValue(relativePath, out clip))
        {
            clip = Resources.Load<AudioClip>("CastleAudio/" + relativePath);
            clips[relativePath] = clip;
        }
        return clip;
    }

}
